using UnityEngine; // Unityの基本機能を使用するための宣言
using TMPro; // InputField（文字入力欄）を扱うためのTextMeshProの宣言
using System.Runtime.InteropServices; // [DllImport]など、外部ライブラリ（JS）と連携するための宣言

// Firebase（外部DB）との橋渡しを管理するクラスの定義
public class FirebaseManager : MonoBehaviour
{
    public TMP_InputField commentInput; // ユーザーがコメントを入力するUI要素の参照

    // WebGLビルド時に、ブラウザ側のJavaScript関数を呼び出すための特殊な命令
    [DllImport("__Internal")]
    private static extern void SendFeedbackJS(string userName, string comment); 
// 最終的にuserNameは使わない仕様とすることにしたが、将来気が変わったときの為にプログラム上では残しておく。

// 送信ボタンが押されたときに実行されるイベント関数
public void OnClickSend()
    {
        string comment = commentInput.text; // 入力されたテキストを取得して変数に格納

        if (string.IsNullOrEmpty(comment)) // 入力内容が空（またはnull）でないかチェックする
        {
            Debug.Log("くもぼうやは中身が空っぽなのを見て、しょんぼりしました。");
            return; // 空っぽなら、以降の送信処理を中断
        }

// 「エディタ上ではなく、かつWebGLビルドの時だけ」実行するためのプリプロセッサディレクティブ
        #if !UNITY_EDITOR && UNITY_WEBGL
            SendFeedbackJS("Player", comment);
        #else
            Debug.Log($"[Firebase] (Mock) Feedback sent from Editor: {comment}");
        #endif

        // 送信後、次の入力のためにテキストボックスを空にする
        commentInput.text = "";
        Debug.Log("[Firebase] Comment input cleared after submission.");
    }
}