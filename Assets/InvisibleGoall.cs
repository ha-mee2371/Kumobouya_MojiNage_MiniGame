using UnityEngine; // Unityの基本機能を使うための宣言
using System.Collections; // コルーチン（IEnumerator）を使うための宣言
using TMPro; // TextMeshProを操作するための宣言
using UnityEngine.UI; // UIを操作するための宣言

// 一定時間ごとにゴールを隠蔽して難易度を上げるクラス
public class InvisibleGoal : MonoBehaviour 
{
    [Header("隠蔽のタイミング設定")]
    public float waitTime = 15.0f;     // ゴールが見えている（平和な）時間
    public float invisibleTime = 5.0f; // ゴールが姿を消す（デバフ）時間

    [Header("UI・演出の参照")]
    public GameObject HideImage; // 隠蔽が始まったことを知らせるエフェクト画像
    public TextMeshProUGUI timerText; // 隠蔽（デバフ発動）までのカウントダウンを表示するUI
    public Image buttonImage; // ボタンの見た目を変更するための参照

    private CanvasGroup canvasGroup; // 演出用画像の透明度を制御するためのコンポーネント

    private float currentTimer; // 次の隠蔽発動までの残り時間を計算する変数
    public bool isInvisibleMode { get; private set; } = false; // 現在、隠蔽（デバフ）中かどうかを外部に教えるフラグ
    private bool isPaused = false; // フリーモードのボタン操作で、このシステム自体を止めているかどうかのフラグ

    // ゲーム開始時の初期設定
    void Start() 
    {
        // GameManagerに保存されている選んだ色をUIに適用する
        if (timerText != null && GameManager.instance != null)
        {
            timerText.color = GameManager.instance.selectedColor;
        }

        // 隠蔽演出用画像の準備
        if (HideImage != null)
        {
            canvasGroup = HideImage.GetComponent<CanvasGroup>(); 
            if (canvasGroup == null) canvasGroup = HideImage.AddComponent<CanvasGroup>(); 
            HideImage.SetActive(true); 
            canvasGroup.alpha = 0f; 
        }

        currentTimer = waitTime; 
        StartCoroutine(GoalFlashLoop()); 
    }

    // フリーモードのボタンから呼ばれる、システムのON/OFF切り替え関数
    public void SetPause(bool pause)
    {
        isPaused = pause; 

        // ボタンの色を切り替える（停止中は暗く、動作中は白に）
        if (buttonImage != null)
        {
            buttonImage.color = pause ? Color.gray : Color.white;
        }

        if (isPaused)
        {
            // 停止：デバフを解除し、カウントを15にキープする
            SetGoalVisibility(true); 
            if (canvasGroup != null) canvasGroup.alpha = 0f; 
            isInvisibleMode = false; 
            currentTimer = waitTime; 
        }
        else
        {
            // 再開：15からカウントダウンを開始する
            currentTimer = waitTime; 
        }
    }

    public void StopInvisibleLoop()
    {
        StopAllCoroutines(); 
        SetGoalVisibility(true); 
        if (timerText != null) timerText.text = ""; 
        if (canvasGroup != null) canvasGroup.alpha = 0f; 
        isInvisibleMode = false; 
    }

    public void ResetTimer()
    {
        if (!isInvisibleMode && !isPaused)
        {
            currentTimer = waitTime; 
        }
    }

    void Update() 
    {
        // システム停止中はカウントを15で固定して表示
        if (isPaused) 
        {
            currentTimer = waitTime;
            if (timerText != null) timerText.text = Mathf.CeilToInt(currentTimer).ToString("00");
            return;
        }

        if (!isInvisibleMode && currentTimer > 0) currentTimer -= Time.deltaTime;

        if (timerText != null) 
        {
            if (isInvisibleMode) timerText.text = "00";
            else timerText.text = Mathf.CeilToInt(currentTimer).ToString("00");
        }
    }

    IEnumerator GoalFlashLoop()
    {
        while (true) 
        {
            while (isPaused || currentTimer > 0.001f) yield return null;

            isInvisibleMode = true; 
            SetGoalVisibility(false); // 最新リスト取得

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f; 
                yield return new WaitForSeconds(0.6f); 
                float duration = 1.0f; 
                float currentTime = 0f;
                while (currentTime < duration)
                {
                    if (isPaused) break; 
                    currentTime += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTime / duration); 
                    yield return null;
                }
                canvasGroup.alpha = 0f; 
            }

            float elapsed = 0;
            while (elapsed < (invisibleTime - 1.6f))
            {
                if (isPaused) break; 
                elapsed += Time.deltaTime;
                yield return null;
            }

            SetGoalVisibility(true); 
            isInvisibleMode = false; 
            if (!isPaused) currentTimer = waitTime; 
        }
    }

    // 表示・非表示を切り替える際に、その都度最新の子供たちを見つける
    void SetGoalVisibility(bool isVisible)
    {
        // レンダラー（画像など）を取得して切り替え
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) if (r != null) r.enabled = isVisible;
        
        // テキストパーツを取得して切り替え
        TMPro.TMP_Text[] texts = GetComponentsInChildren<TMPro.TMP_Text>();
        foreach (TMPro.TMP_Text t in texts)
        {
            // カウントダウン用のタイマーだけは消さない
            if (t != null && t != timerText) t.enabled = isVisible;
        }
    }

    // ボタンから呼ぶための「切り替え」用関数
    public void TogglePause()
    {
        // 今の状態を反転させる（trueならfalse、falseならtrueに）
        SetPause(!isPaused);
    }
}