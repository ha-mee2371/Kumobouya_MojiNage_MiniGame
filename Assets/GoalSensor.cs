using UnityEngine; // Unityの基本機能を使用するための宣言

public class GoalSensor : MonoBehaviour 
{
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Untagged")) return; 

        MojiController moji = other.GetComponent<MojiController>();
        if (moji == null) return; 

        string originalTag = other.tag; 

        // ポイント管理クラスを探して報告
        var nanyateManager = Object.FindAnyObjectByType<FreeModeNanyateManager>();
        if (nanyateManager != null)
        {
            nanyateManager.CollectPoint(originalTag); 
        }

        other.tag = "Untagged"; 

        TMPro.TextMeshPro textMesh = other.GetComponentInChildren<TMPro.TextMeshPro>();
        string mojiText = (textMesh != null) ? textMesh.text : "不明";
        Debug.Log($"ゴール！: {mojiText}");
        
        // Normalモードの時だけScoreManager（リザルト用）に送る
        if (GameManager.instance != null && GameManager.instance.currentMode == GameMode.Normal)
        {
            if (ScoreManager.instance != null) ScoreManager.instance.AddScore(mojiText, originalTag);
        }
    }
}