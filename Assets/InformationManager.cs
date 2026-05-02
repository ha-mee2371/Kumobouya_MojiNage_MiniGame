using UnityEngine; // Unityの基本クラスを使用するための宣言
using UnityEngine.SceneManagement; // シーン遷移を管理するための宣言
using System.Runtime.InteropServices; // [DllImport]など、外部（JavaScript）と通信するための宣言
using TMPro; // TextMeshProを操作するための宣言

// お問い合わせ送信やフィードバックを管理するクラス
public class InformationManager : MonoBehaviour 
{
    // JavaScript側の関数を呼び出す
    [DllImport("__Internal")]
    private static extern string OpenPromptJS(string title, string defaultText); // ブラウザの入力プロンプトを出す

    [DllImport("__Internal")]
    private static extern void SendFeedbackJS(string userName, string comment); // DB送信用のJSを叩く

    // インスペクター上での操作を分かりやすくグループ化する工夫
    [Header("UI Panels (Canvases)")] 
    public GameObject sentCanvas; // 入力画面
    public GameObject thanksCanvas; // 送信完了画面
    public GameObject errorCanvas; // 送信エラー（上限到達）

    [Header("Input Fields")]
    public TMP_InputField contentInput; // テキスト入力欄

    // 入力画面を表示する関数
    public void ShowSentCanvas() {
        sentCanvas.SetActive(true);
    }

    // 入力画面を閉じる関数
    public void CloseSentCanvas() {
        sentCanvas.SetActive(false);
    }

    // 入力欄がクリックされた時の処理
    public void OnClickInputField() 
    {
        // エディタ上ではなく、WebGLで動いている時だけJSを呼び出す
        #if !UNITY_EDITOR && UNITY_WEBGL
            string input = OpenPromptJS("お問い合わせ内容を入力してください", ""); 
            if (!string.IsNullOrEmpty(input)) {
                contentInput.text = input; // 入力された文字をUnity側に反映
            }
        #endif
    }

    // 実際にデータを送信するメインロジック
    public void SendToDB() {
        string content = contentInput.text; // 入力された内容を取得

        // 中身が空っぽなら送信しない
        if (string.IsNullOrEmpty(content)) {
            Debug.Log("中身が空っぽ！");
            return;
        }

        // スパム・連続送信防止ロジック
        int sendCount = PlayerPrefs.GetInt("SendCount", 0); // これまでの送信回数をロード
        string lastSendTimeStr = PlayerPrefs.GetString("LastSendTime", ""); // 最後に送った時間をロード

        if (!string.IsNullOrEmpty(lastSendTimeStr)) {
            System.DateTime lastSendTime = System.DateTime.Parse(lastSendTimeStr); // 文字列を時間に変換
            // 1時間以上経過していたらカウントをリセットする
            if ((System.DateTime.Now - lastSendTime).TotalHours >= 1) {
                sendCount = 0;
            }
        }

        // 1時間に5回以上送ろうとしたらブロックする
        if (sendCount >= 5) {
            Debug.Log("1時間の送信上限に達しました");
            OnSendError(); 
            return;
        }

        CloseSentCanvas(); // 入力窓を閉じる

        // 実際に送信を実行する
        #if !UNITY_EDITOR && UNITY_WEBGL
            SendFeedbackJS("Guest", content); // 本番環境ではJS経由でDBへ飛ばす
            OnSendSuccess(); 
        #else
            Debug.Log($"【エディタ確認】DB送信内容: {content}"); // 開発中はログに出すだけ
            OnSendSuccess(); 
        #endif

        // 送信記録を保存して、次回の制限に備える
        PlayerPrefs.SetInt("SendCount", sendCount + 1);
        PlayerPrefs.SetString("LastSendTime", System.DateTime.Now.ToString());
        PlayerPrefs.Save(); // 保存

        contentInput.text = ""; // 送信後に中身を空にしておく
    }

    // 送信成功時の表示切り替え
    public void OnSendSuccess() {
        thanksCanvas.SetActive(true);
        errorCanvas.SetActive(false);
    }

    // 送信失敗（制限）時の表示切り替え
    public void OnSendError() {
        thanksCanvas.SetActive(false);
        errorCanvas.SetActive(true);
    }

    // タイトルシーンへ戻る
    public void GoToTitle() {
        SceneManager.LoadScene("TitleScene");
    }
}