using UnityEngine; // Unityの基本機能を使用するための宣言
using TMPro; // TextMeshProを使用するための宣言
using UnityEngine.UI; // ButtonやImageなどのUI部品を制御するための宣言
using Unity.Services.Leaderboards; // Unityのオンラインランキング機能を使用するための宣言
using Unity.Services.Core; // Unity Servicesの基本システムを制御するための宣言
using Unity.Services.Authentication; // 匿名ログインなどの認証機能を使用するための宣言
using System.Collections.Generic; // リストなどのコレクション機能を使うための宣言
using Newtonsoft.Json; // JSON形式のデータを扱うための宣言
using UnityEngine.SceneManagement;  // シーンの切り替えを制御するための宣言
using System.Threading.Tasks; // 非同期処理を扱うための宣言
using System.Text.RegularExpressions; // 正規表現による文字列チェック（ひらがな判定）を行うための宣言

public class ArcadeScoreManager : MonoBehaviour // アーケード式でゲームのスコア管理を行うクラスの定義
{
    // インスペクター上での操作を分かりやすくグループ化する工夫
    [Header("UI Elements")]
    public TMP_InputField nameInputField;
    public TMP_Text scoreDisplayText; 
    public Button submitButton; 
    public GameObject uploadPanel; 

    // スコアデータをグループ化
    [Header("Score Data")]
    public int scoreToUpload;

    private bool isSubmitting = false; // 二重送信を防ぐ。連打防止を狙う工夫

    void OnEnable() // オブジェクトが有効になった時に呼ぶイベント関数
    {
        UpdateScoreDisplay(); // スコア表示を最新の状態に更新
    }

    // スコアテキストを更新する独自の関数
    public void UpdateScoreDisplay()
    {
        if (scoreDisplayText != null) // UIがセットされていない場合のエラーを防ぐ
        {
            scoreDisplayText.text = "今回のスコア: " + scoreToUpload.ToString(); // 数値を文字列に変換して結合
        }
    }

    // ゲーム開始時に非同期で初期化を行う
    async void Start()
    {
        UpdateScoreDisplay(); // 念のため開始時にもスコアを表示する
        try // ネットワーク初期化は失敗する可能性があるので、例外処理の工夫
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized) // 既に初期化済みでないかチェック
            {
                await UnityServices.InitializeAsync(); // Unity Servicesの全体的な初期化を待機
                
                if (!AuthenticationService.Instance.IsSignedIn) // 未ログインの場合にログイン処理を行う条件分岐
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync(); // 匿名ログイン
                }
                Debug.Log("くもぼうやはスコアの通信準備を完了させた");
            }
        }
        catch (System.Exception e) // エラーが発生した場合
        {
            Debug.LogError("くもぼうやはネットワークの初期化に失敗: " + e);
        }
    }

    // 送信ボタンが押された時の非同期処理
    public async void OnClickSubmit()
    {
        if (isSubmitting) return; // 送信中なら何もしない。サーバー負荷と重複登録を防ぐ。

        // 名前の取得
        string baseName = "くも"; // 名前の入力がなかったら「くも」を自動で設定
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text)) // 入力欄が存在&空でないか確認
        {
            baseName = nameInputField.text; // 入力された名前を代入
        }

        // ひらがなチェック
        if (!Regex.IsMatch(baseName, @"^[ぁ-んー]+$")) // 正規表現で「ひらがなと長音」のみを許可
        {
            Debug.LogWarning("ひらがな以外が入力されたので、くもぼうやは読むことが出来ない。");
            if (scoreDisplayText != null) // フィードバック用のテキストがあるか確認
            {
                scoreDisplayText.text = "<color=red>ひらがなのみ</color>"; //警告
            }
            return; // 不適切な文字があればここで処理を中断
        }

        isSubmitting = true; // 送信開始フラグを立てる
        if (submitButton != null) submitButton.interactable = false; // ボタンを無効化して連打を防ぐ。UI制御。
            Debug.Log($"[Submission] Start. Name: {baseName}, Score: {scoreToUpload}");

        try // 通信エラーに備えた例外処理
        {
            AuthenticationService.Instance.SignOut(true); // 確実に新しいプレイヤーとして登録するためのクリアログイン
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // 再度ログインして最新のセッションを確保

            string playerColor = "#FFFFFF"; // デフォルトの色(デバッグ時にのみ適用予定)
            if (GameManager.instance != null) // シングルトンのGameManagerから情報を取得する
            {
                playerColor = "#" + ColorUtility.ToHtmlStringRGB(GameManager.instance.selectedColor); // 色データをHTML形式の文字列に変換する
            }

            string finalCombinedName = $"{playerColor}|{baseName}ぼうや"; // 色情報+名前+固定称を結合して1つのデータにする。ランキングの表現力を高める仕組み。

            await AuthenticationService.Instance.UpdatePlayerNameAsync(finalCombinedName); // Unity Gaming Services上のプレイヤー名を設定した名前に更新
            await LeaderboardsService.Instance.AddPlayerScoreAsync("KUMO_RANKING_01", scoreToUpload); // 指定したIDのランキングにスコアを送信
            
            Debug.Log("くもぼうやはスコア送信をして喜んでいる: " + finalCombinedName);
            SceneManager.LoadScene("RankingScene"); // 送信完了後、即座にRankingSceneへ移動
        }
        catch (System.Exception e) // 通信失敗時の処理
        {
            Debug.LogError("くもぼうやはスコア送信に失敗して悲しんでいる: " + e.Message);
            isSubmitting = false;// 失敗したら再試行できるようにフラグを戻す
            if (submitButton != null) submitButton.interactable = true; // ボタンも再度押せるように戻す
        }
    }

    public void OnClickCancel() // キャンセルボタンが押された時の処理
    {
        if (uploadPanel != null) // パネルが存在するか確認
        {
            uploadPanel.SetActive(false); // 送信パネルを閉じてゲームへ戻る
            Debug.Log("くもぼうやはランキングに参加しないことにしました。");
        }
    }
}