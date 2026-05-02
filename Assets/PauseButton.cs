using UnityEngine; // Unityの基本クラスを使用するための宣言
using TMPro; // TextMeshProUGUIを操作するための宣言

public class PauseButton : MonoBehaviour // ポーズ画面のボタンなどの外見を管理するクラス
{
    // オブジェクトが生成された時に一度だけ実行
    void Start()
    {
        // 全体管理役のGameManagerが存在するかチェック
        if (GameManager.instance != null)
        {
            // このボタン自身に付いているTextMeshProUGUIコンポーネントを取得
            var text = GetComponent<TextMeshProUGUI>();
            
            // テキストコンポーネントが無事に見つかったら処理を続ける
            if (text != null)
            {
                // GameManagerが持っているプレイヤーが選んだ色をボタンの文字色に上書き
                text.color = GameManager.instance.selectedColor;
            }
        }
    }
}