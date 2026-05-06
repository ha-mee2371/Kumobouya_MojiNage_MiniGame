using UnityEngine; // Unityの基本クラス
using TMPro; // TextMeshPro操作
using System.Collections; // コルーチン
using UnityEngine.UI; // Image操作

public class FreeModeNanyateManager : MonoBehaviour 
{
    [Header("UI設定")] 
    public TextMeshProUGUI pointText; 
    public RectTransform spawnPoint; 

    [Header("演出設定")] 
    public GameObject kumoPrefab; 
    public Canvas canvas; 
    public Sprite[] kumoSprites; 

    private float currentPoints = 0f; 

    void Start() { UpdateUI(); } 

    // ポイント獲得とリセット処理
    public void CollectPoint(string targetTag) 
    {
        // デバフ中かどうかを判定
        bool isDebuff = IsAnyDebuffActive();

        if (targetTag == "Target") 
        {
            // 通常+10、デバフ中+15
            currentPoints += isDebuff ? 15f : 10f; 
        }
        else if (targetTag == "Other") 
        {
            // その他の文字でタイマーリセット
            if (GameManager.instance != null) GameManager.instance.RequestDebuffReset();
            
            // 通常時のみ-3点、デバフ中は減点なし
            if (!isDebuff) currentPoints -= 3f; 
        }
        
        if (currentPoints < 0) currentPoints = 0; // スコアがマイナスにならないようにガード
        UpdateUI(); 
    }

    // デバフが発動中かチェックする
    bool IsAnyDebuffActive()
    {
        var g = Object.FindAnyObjectByType<PeriodicGravity>();
        var i = Object.FindAnyObjectByType<InvisibleGoal>();
        return (g != null && g.isHeavyMode) || (i != null && i.isInvisibleMode);
    }

    // 全てのデバフタイマーをリセットする
    void ResetAllDebuffTimers()
    {
        var g = Object.FindAnyObjectByType<PeriodicGravity>();
        if (g != null) g.ResetTimer();
        var i = Object.FindAnyObjectByType<InvisibleGoal>();
        if (i != null) i.ResetTimer();
    }

    public void OnNanyateButtonClick() 
    {
        // 目標スコア=100
        if (currentPoints >= 100f) 
        {
            currentPoints -= 100f; 
            UpdateUI(); 
            StartCoroutine(SpawnKumoRoutine()); 
            Debug.Log("100ポイント消費してポップコーン発動");
        }
    }

    void UpdateUI() 
    {
        if (pointText != null) 
        {
            pointText.text = Mathf.FloorToInt(currentPoints).ToString() + "なんやて"; 
        }
    }

    IEnumerator SpawnKumoRoutine() 
    {
        for (int i = 0; i < 5; i++) // ポップコーン生成数=5
        {
            GameObject kumo = Instantiate(kumoPrefab, canvas.transform); 
            RectTransform rt = kumo.GetComponent<RectTransform>(); 
            rt.position = spawnPoint.position; 
            Vector3 pos = rt.localPosition; pos.z = 0; rt.localPosition = pos; 
            rt.localScale = new Vector3(0.3f, 0.3f, 1f); 
            if (kumoSprites.Length > 0) kumo.GetComponent<Image>().sprite = kumoSprites[Random.Range(0, kumoSprites.Length)]; 
            StartCoroutine(KumoPhysics(kumo)); 
            yield return new WaitForSeconds(0.05f); 
        }
    }

    IEnumerator KumoPhysics(GameObject kumo) // 重たくならないように
    {
        RectTransform rt = kumo.GetComponent<RectTransform>(); 
        Vector2 velocity = new Vector2(Random.Range(-500f, 500f), Random.Range(500f, 1000f)); 
        float gravity = -1800f; 
        float rotateSpeed = Random.Range(-600f, 600f); 
        float timer = 0; 
        while (timer < 3f) 
        {
            velocity.y += gravity * Time.deltaTime; 
            rt.anchoredPosition += velocity * Time.deltaTime; 
            rt.Rotate(0, 0, rotateSpeed * Time.deltaTime); 
            timer += Time.deltaTime; 
            yield return null; 
        }
        Destroy(kumo); 
    }
}