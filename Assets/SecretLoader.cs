using UnityEngine; // Unityの基本機能を使うための宣言
using System.IO; // ファイルの読み込み
using System.Runtime.InteropServices; // DllImport を使うために必要
using System.Collections; // コルーチンを使うために必要

// 外部の設定ファイルを読み込み、Firebase等の初期化を行う
public class SecretLoader : MonoBehaviour 
{
    // ブラウザ側（JavaScript）で定義されたsetupFirebase関数を呼ぶ
    [DllImport("__Internal")]
    private static extern void setupFirebase(string configJson);

    void Start()
    {
        /* 
        【注記】
        当初は以下の処理でStreamingAssetsから機密情報を読み込んでいたが、
        FirebaseのAPIキーは公開前提の識別子であること、およびWebGLでの
        初期化タイミングの安定性を考慮し、現在は index.html 側での直接初期化に移行している。
        今後何かの参考になる可能性を秘めている為、以下に初期仕様の残骸を残す。
        */

        /* 
        // 秘密の設定ファイルsecret_config.jsonが置いてある場所（パス）を特定する
        string path = Path.Combine(Application.streamingAssetsPath, "secret_config.json");
        
        // ちゃんとファイルが存在するかチェック
        if (File.Exists(path))
        {
            // ファイルの中身（JSON形式のテキスト）を丸ごと読み込む
            string jsonText = File.ReadAllText(path);
            
            // プラットフォームに応じた条件分岐
            // Unityエディタ上ではなく、かつWebブラウザ（WebGL）で動いている時だけ実行する
            #if !UNITY_EDITOR && UNITY_WEBGL
                // ブラウザ側の準備が整うまで少し待ってから初期化を呼ぶ工夫
                StartCoroutine(WaitAndSetup(jsonText));
            #else
                // 開発中のエディタ上では、初期化はせずに読み込みが成功したことだけを確認する
                Debug.Log("エディタ上では読み込みだけ確認: " + jsonText);
            #endif
        }
        else
        {
            // ファイルがないと、オンライン機能が動かなくなる
            Debug.LogError("secret_config.json が見つからない");
        }
        */
    }

    // ブラウザ側のJSがロードされるのを少し待つためのコルーチン
    IEnumerator WaitAndSetup(string json)
    {
        yield return new WaitForSeconds(0.5f); // 0.5秒だけ待機
        setupFirebase(json);
    }
}