using UnityEngine; // Unityの基本機能を使用するための宣言

// ゴールへの接触を検知し、スコア加算や演出のトリガーを引くクラス
public class GoalSensor : MonoBehaviour 
{
    private void OnTriggerEnter2D(Collider2D other) // 2Dのトリガーコライダーに何かが触れた瞬間に呼ばれる
    {
        // 既に処理済みのオブジェクトは無視する
        if (other.CompareTag("Untagged"))
        {
            return; 
        }

        //　触れたものが文字（MojiController）であるかを確認する
        MojiController moji = other.GetComponent<MojiController>();
        if (moji == null)
        {
            return; 
        }

        // 処理前に元のタグ（TargetやOther）を記憶
        string originalTag = other.tag; 

        // フリーモード用のポイント管理クラスを探して、外部に加算を依頼する
        var nanyateManager = Object.FindAnyObjectByType<FreeModeNanyateManager>();
        if (nanyateManager != null)
        {
            nanyateManager.CollectPoint(originalTag); // タグに応じたポイントを計算
        }

        // ゴールに入った直後にタグを「Untagged」に変えることで、1フレームに何度も判定が出るのを防ぐ
        other.tag = "Untagged"; 

        // 子供のコンポーネントから文字情報を取得する
        TMPro.TextMeshPro textMesh = other.GetComponentInChildren<TMPro.TextMeshPro>();
        string mojiText = (textMesh != null) ? textMesh.text : "不明";

        // 開発時に役立つログ出力
        Debug.Log($"くもぼうやは {mojiText} をゴールに入れた！");
        
        // スコア管理に、何の文字をどの種類で入れたか報告
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(mojiText, originalTag);
        }
    }
}