using UnityEngine; // Unityの基本機能を使用するための宣言
using UnityEngine.UI; // ButtonやImageなど、UI要素を制御するための宣言
using System.Collections; // コルーチン（IEnumerator）を使用するための宣言
using TMPro; // TextMeshProを操作するための宣言
using UnityEngine.SceneManagement; // Scene移動の宣言

// フリーモード全体の進行と設定を管理するクラス
public class FreeModeManager : MonoBehaviour 
{
    // インスペクター上での操作を分かりやすくグループ化する工夫
    [Header("UIの設定")] 
    public GameObject startImage; // 開始時に表示する演出用画像の参照
    
    [Header("ボタン画像の設定")] // デバフのON/OFF状態を視覚的に示すための設定
    public Image gravityButtonImage; // 重力切り替えボタンの見た目を管理する参照
    public Image invisibleButtonImage; // 隠蔽切り替えボタンの見た目を管理する参照

    [Header("プレイヤー参照")] 
    public PlayerController player; // プレイヤー制御スクリプトの参照

    [Header("なんやてポイント設定")] 
    public TextMeshProUGUI nanyateCountText; // ポイントを表示するテキストUIの参照

    private bool isGravityEnabled = true; // 現在重力ギミックが有効かどうかのフラグ
    private bool isInvisibleEnabled = true; // 現在隠蔽ギミックが有効かどうかのフラグ

    // ゲーム開始時に呼ばれる初期化処理
    void Start()
    {
        InitFreeMode(); // フリーモードのセットアップを開始
    }

    void InitFreeMode() // 内部的な初期設定を行う関数
    {
        player = FindFirstObjectByType<PlayerController>(); // シーン内からプレイヤーを自動検索して紐付ける
        StartCoroutine(FreeModeStartSequence()); // 開始時の演出シーケンスをコルーチンで実行
    }

    public void ToggleGravity() // 重力ギミックの有効・無効を切り替えるボタンイベント
    {
        isGravityEnabled = !isGravityEnabled; // フラグを反転させる
        if (gravityButtonImage != null) // ボタン画像の見た目を変更してユーザーに伝える
            gravityButtonImage.color = isGravityEnabled ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.8f); // 無効時は暗くする

        FreeModeGravity fmg = Object.FindAnyObjectByType<FreeModeGravity>(); // シーン内の重力制御スクリプトを探す
        if (fmg != null) fmg.SetPause(!isGravityEnabled); // 重力スクリプトへ停止・再開を命令する他クラス連携

        Debug.Log("重力切り替え！ 今は: " + isGravityEnabled);
    }

    public void ToggleInvisible() // 隠蔽ギミックの有効・無効を切り替える
    {
        isInvisibleEnabled = !isInvisibleEnabled; // フラグを反転
        
        if (invisibleButtonImage != null) // ボタンの見た目を更新
        {
            invisibleButtonImage.color = isInvisibleEnabled ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.8f); // 視覚的なON/OFF表現
        }

        FreeModeInvisible fmi = Object.FindAnyObjectByType<FreeModeInvisible>(); // 隠蔽制御スクリプトを探す
        if (fmi != null)
        {
            fmi.SetPause(!isInvisibleEnabled); // 隠蔽スクリプトへ命令
        }

        Debug.Log("隠蔽切り替え！ 今は: " + isInvisibleEnabled); 
    }

    // 「ポヨン」と跳ねるような登場アニメーション
    IEnumerator PoyonAnimation(Transform Target) 
    {
        Target.localScale = Vector3.zero; // 最初は大きさを0
        float duration = 0.5f; // アニメーションにかける時間
        float time = 0; // 経過時間の記録
        while (time < duration) // 指定時間の間、計算を繰り返す
        {
            time += Time.deltaTime; // 実時間を加算
            float t = time / duration; // 0から1への進捗率を計算
            // Sin関数と減衰を使って「弾むような動き」を作る
            float scale = Mathf.Sin(t * Mathf.PI * 2.5f) * (1 - t) + 1; 
            Target.localScale = new Vector3(scale, scale, 1); // 計算したスケールを適用
            yield return null; // 1フレーム待機
        }
        Target.localScale = Vector3.one; // 最終的に正しい大きさに固定
    }

    // 開始時の演出の流れを管理するコルーチン
    IEnumerator FreeModeStartSequence() 
    {
        if (startImage != null) // 開始ロゴがある場合
        {
            startImage.SetActive(true); // 画像を表示
            yield return StartCoroutine(PoyonAnimation(startImage.transform)); // 弾むアニメーションが終わるまで待機する
            yield return new WaitForSeconds(0.8f); // 表示したまま少し待つ
            startImage.SetActive(false); // 画像を消す
        }
    }

    public void BackToTitle() // タイトル画面へ戻るボタンのフラグ
    {
        SceneManager.LoadScene("TitleScene"); 
    }
}