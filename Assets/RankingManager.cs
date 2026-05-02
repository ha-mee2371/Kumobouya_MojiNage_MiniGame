using UnityEngine; // Unityの基本クラスを使用するための宣言
using UnityEngine.SceneManagement; // シーン移動
using TMPro; // テキスト表示
using System.Threading.Tasks; // 非同期処理
using Unity.Services.Core; // Unityサービス全体の初期化
using Unity.Services.Leaderboards; // ランキング機能
using Unity.Services.Authentication; // ログイン機能
using System.Collections.Generic; // リスト操作
using Newtonsoft.Json; // JSONデータの扱いに必要

// オンラインランキングの取得と表示を管理するクラス
public class RankingManager : MonoBehaviour 
{
    public GameObject thanksForPlayerPanel; // 感謝パネルのオンオフ用
    
    [Header("1位〜8位")]
    public TextMeshProUGUI[] topRankTexts; // 上位入賞者用の個別のテキスト枠

    [Header("9位以降")]
    public TextMeshProUGUI rankingText; // 9位以下の人をまとめて流すテキスト枠
    public string leaderboardId = "KUMO_RANKING_01"; // Unityの管理画面で設定したID

    [Header("AutoScroll連携")]
    public AutoScroll autoScrollScript; // ランキングを自動で動かすスクリプトへの参照

    async void Start() // 非同期で動く開始処理
    {
        try 
        {
            // Unityのオンラインサービスを準備
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            // 匿名サインイン
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("くもぼうやはランキング閲覧用のサインインに成功した");
            }

            // 準備ができたらランキングデータを取ってくる
            await FetchLeaderboard();
        }
        catch (System.Exception e)
        {
            Debug.LogError("ランキング開始時のエラー: " + e);
        }
    }

    // データを取ってきて画面に並べる
    public async Task FetchLeaderboard()
    {
        try
        {
            // 最大50件のスコアを取得
            var scores = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, new GetScoresOptions { Limit = 50 });
            
            // 表示をリセット
            if (rankingText != null) rankingText.text = ""; 
            foreach (var t in topRankTexts) if (t != null) t.text = "---";

            int rank = 1;
            foreach (var entry in scores.Results)
            {
                // 名前処理
                string rawName = entry.PlayerName;
                string finalDisplayName = "えらー";
                string colorHtml = "#FFFFFF"; 

                // 「#FF0000|くもぼうや#1234」のような形式を分解
                if (rawName.Contains("|"))
                {
                    string[] parts = rawName.Split('|');
                    colorHtml = parts[0]; // 前半分が色コード

                    if (parts.Length > 1)
                    {
                        string nameAndId = parts[1];
                        finalDisplayName = nameAndId.Split('#')[0]; // 名前だけ抜く
                    }
                }

                // 順位に応じて表示場所を変える
                if (rank <= 8)
                {
                    int index = rank - 1;
                    if (index < topRankTexts.Length && topRankTexts[index] != null)
                    {
                        // 1〜3位は改行して目立たせる
                        if (rank <= 3)
                        {
                            topRankTexts[index].text = $"<color={colorHtml}>{rank}. {finalDisplayName}\n{entry.Score}点</color>";
                        }
                        else
                        {
                            topRankTexts[index].text = $"<color={colorHtml}>{rank}. {finalDisplayName} {entry.Score}点</color>";
                        }
                    }
                }
                else
                {
                    // 9位以降は一つのテキストにどんどん追加
                    if (rankingText != null)
                    {
                        rankingText.text += $"<color={colorHtml}>{finalDisplayName} {entry.Score}点</color>\n";
                    }
                }
                rank++;
            }

            // 全て書き終わったら自動スクロールを開始
            if (autoScrollScript != null) 
            {
                autoScrollScript.StartScroll(); 
            }    
        }
        catch (System.Exception e)
        {
            if (rankingText != null) rankingText.text = "Error！";
            Debug.LogError("ランキング取得エラー: " + e);
        }
    }

    // Sceneなど
    public void GoToTitleScene() { 
        SceneManager.LoadScene("TitleScene"); 
        }

    public void GoToReadme() { 
        SceneManager.LoadScene("ReadmeWarning"); 
        }

    public void OpenThanksForPlayer() { 
        if (thanksForPlayerPanel != null) thanksForPlayerPanel.SetActive(true); 
        }

    public void CloseThanksForPlayer() { 
        if (thanksForPlayerPanel != null) thanksForPlayerPanel.SetActive(false); 
        }
}