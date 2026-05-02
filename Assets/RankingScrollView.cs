using UnityEngine; // Unityの基本クラスを使用するための宣言
using UnityEngine.UI; // ScrollRectを操作するための宣言

// ランキング画面などを自動でスクロールさせるためのクラス
public class AutoScroll : MonoBehaviour 
{
    public ScrollRect scrollRect; // スクロールさせるUIの本体への参照
    public float scrollSpeed = 150f; // スクロールする速さの設定
    private bool isReady = false; // ランキングデータの読み込みが終わって、準備ができたかどうかのフラグ

    // ランキングの取得が終わったタイミングで、他のスクリプトから呼ばれる
    public void StartScroll()
    {
        isReady = true; // スクロールを開始してOK
    }

    // UIの座標更新
    void LateUpdate()
    {
        // 準備ができていない、あるいは必要なコンポーネントが足りない時は何もしない
        if (!isReady || scrollRect == null || scrollRect.content == null) return;

        // 現在のコンテンツの座標を取得
        Vector2 pos = scrollRect.content.anchoredPosition;
        
        // Y座標を時間経過に合わせて増やしていくことで、スクロールさせる
        pos.y += scrollSpeed * Time.deltaTime; 

        // ループ処理：一番上まで行き過ぎたら、下の方に戻して無限ループさせる
        if (pos.y > scrollRect.content.rect.height + 500f)
        {
            pos.y = -500f; 
        }

        // 計算した新しい座標をセットして、実際に動かす
        scrollRect.content.anchoredPosition = pos;

        // 速度をほぼゼロに固定することで、自動スクロール中にガタつくのを防ぐ
        scrollRect.velocity = new Vector2(0, 0.0001f);
    }
}