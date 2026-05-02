using UnityEngine; // Unityの基本クラスを使用するための宣言
using TMPro; // TextMeshProを操作するための宣言

// 画面に現れる文字の動きと種類を管理するクラス
public class MojiController : MonoBehaviour 
{
    // 出現する可能性のある「ひらがな」のリスト
    string[] allMoji = {
        "あ","い","う","え","お",
        "か","き","く","け","こ",
        "さ","し","す","せ","そ",
        "た","ち","つ","て","と",
        "な","に","ぬ","ね","の",
        "は","ひ","ふ","へ","ほ",
        "ま","み","む","め","も",
        "や","ゆ","よ",
        "ら","り","る","れ","ろ",
        "わ","を","ん"
    };

    // 文字が生成された瞬間に一度だけ呼ばれる初期設定
    void Start() 
    {
        // 文字を表示するためのTextMeshProコンポーネントを子供のオブジェクトから探して取得
        TextMeshPro textMesh = GetComponentInChildren<TextMeshPro>();

        // GameManagerに保存されているプレイヤーが選んだ色を文字に塗る
        if (textMesh != null && GameManager.instance != null)
        {
            textMesh.color = GameManager.instance.selectedColor;
        }
        // -------------------------------------------------------

        // 50%の確率で「な,ん,や,て」という正解の文字セットから選ぶ
        string selectedMoji;
        string[] targetMoji = { "な", "ん", "や", "て" }; 

        if (Random.value < 0.5f) 
        {
            // 正解グループからランダムに1文字選んで、タグをTargetにする
            selectedMoji = targetMoji[Random.Range(0, targetMoji.Length)];
            this.tag = "Target"; 
        }
        else 
        {
            // 全ての文字リストからランダムに1文字選んで、タグをOtherにする
            selectedMoji = allMoji[Random.Range(0, allMoji.Length)];
            this.tag = "Other"; 

            foreach(string t in targetMoji)
            {
                if(selectedMoji == t)
                {
                    this.tag = "Target";
                    break;
                }
            }
        }

        // 実際に画面に表示される文字を選ばれた文字に書き換える
        if (textMesh != null)
        {
            textMesh.text = selectedMoji;
        }
    }
    
    void Update() // 毎フレーム、文字を下に動かす処理
    {
        // もし何かの子供になっている（プレイヤーに掴まれている）なら、勝手に動かないよう中断
        if (transform.parent != null) 
        {
            return; 
        }

        // 文字を毎秒2.0のスピードで下に移動
        transform.Translate(Vector3.down * 2.0f * Time.deltaTime);

        // 画面外に消えていった文字は破棄
        if (transform.position.y < -6.0f)
        {
            Destroy(gameObject);
        }
    }
}