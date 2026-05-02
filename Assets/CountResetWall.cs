using UnityEngine; // Unityの基本クラスを使用するための宣言

public class CountResetWall : MonoBehaviour // 壁への衝突をトリガーにデバフギミックをリセットするクラス
{
    // インスペクター上での操作を分かりやすくグループ化する工夫
    public PeriodicGravity gravityScript; 
    public InvisibleGoal invisibleScript;

    // どっちをリセットするかを覚えておくフラグ、trueなら重力・falseなら隠蔽
    private bool isGravityTurn = true; 

    // Unityが「何かがぶつかったと判断したときに自動で呼ばれる関数
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("壁にヒット！ 名前: " + collision.gameObject.name + " / タグ: " + collision.gameObject.tag);

        if (collision.gameObject.tag == "Other") // ぶつかってきた相手のタグを確認
        {
            ExecuteReset(); // 交互リセットの実行。条件を満たした場合にリセット処理を呼び出す。
        }
        else if (collision.gameObject.tag == "Target")
        {
            // 何もしない
        }
    }

    // 交互にデバフギミックをリセットするロジック
    void ExecuteReset()
    {
        if (isGravityTurn) // 重力リセットの番かどうかを判定
        {
            // 重力のリセット
            if (gravityScript != null) // スクリプトがセットされているか確認する
            {
                gravityScript.ResetTimer(); // 重力ギミックのタイマーを初期化するメソッドを叩く
                Debug.Log("くもぼうやが重力デバフのクールダウンをリセット！");
            }
            isGravityTurn = false; // 次は隠蔽の番にする
        }
        else
        {
            // 隠蔽のリセット
            if (invisibleScript != null) // スクリプトがセットされているか確認する
            {
                invisibleScript.ResetTimer(); // ゴール隠蔽ギミックのタイマーを初期化する
                Debug.Log("くもぼうやがゴール隠蔽デバフのクールダウンをリセット！");
            }
            isGravityTurn = true; // 次は重力の番にする
        }
    }
}