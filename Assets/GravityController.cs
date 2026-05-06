using UnityEngine; // Unityの基本クラスを使用するための宣言
using System.Collections; // コルーチン（IEnumerator）を使用するための宣言
using UnityEngine.UI; // UIを操作するための宣言
using TMPro; // TextMeshProを操作するための宣言

// 一定周期で世界の重力を変動させる環境制御クラス
public class PeriodicGravity : MonoBehaviour 
{
    public float gravityMultiplier = 5.0f; // 重力中に何倍の負荷をかけるか
    public float slowSpeedMultiplier = 0.3f; // 重力中のプレイヤーの鈍足化率
    public float waitTime = 15.0f; // 重力が発生していない時間
    public float heavyTime = 5.0f; // 重力が発生している時間

    public GameObject GravityImage; // 重力発生を視覚的に伝えるエフェクト
    public TextMeshProUGUI timerText; // 次の重力発生までのカウントダウン表示
    public Image buttonImage; // ボタンの見た目を変更するための参照

    private float originalGravity; // 元々の世界の重力値を保存しておく変数
    private PlayerController playerScript; // プレイヤーの移動速度を直接書き換えるための参照
    private CanvasGroup canvasGroup; // UIフェード演出のためのコンポ―ネント

    private float currentTimer; // 現在の経過時間を計測する内部変数
    public bool isHeavyMode { get; private set; } = false; // 現在重力モードかどうか
    private float basePlayerSpeed; // プレイヤーの本来の移動速度を保存

    private bool isPaused = false; // フリーモード用のポーズフラグ

    void Start() 
    {
        // ホバーの色を活かすため、Start時のみ適用
        if (timerText != null && GameManager.instance != null)
        {
            timerText.color = GameManager.instance.selectedColor;
        }

        originalGravity = Physics2D.gravity.y;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerScript = playerObj.GetComponent<PlayerController>();
            if (playerScript != null) basePlayerSpeed = playerScript.moveSpeed;
        }

        if (GravityImage != null)
        {
            canvasGroup = GravityImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = GravityImage.AddComponent<CanvasGroup>();
            GravityImage.SetActive(true); 
            canvasGroup.alpha = 0f;
        }

        currentTimer = waitTime;
        StartCoroutine(GravityLoop());
    }

    // フリーモードのON/OFF切り替え用に関数を追加
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
            // 停止：重力と速度を戻し、カウントを15に固定
            Physics2D.gravity = new Vector2(0, originalGravity);
            if (playerScript != null) playerScript.moveSpeed = basePlayerSpeed;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            isHeavyMode = false;
            currentTimer = waitTime; 
        }
        else
        {
            // 再開：15からカウントダウン
            currentTimer = waitTime;
        }
    }

    public void StopGravityLoop() 
    {
        StopAllCoroutines();
        Physics2D.gravity = new Vector2(0, originalGravity);
        if (playerScript != null) playerScript.moveSpeed = basePlayerSpeed;
        if (timerText != null) timerText.text = "";
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        isHeavyMode = false;
    }

    // 重力までの時間をリセットする機能（フリーモードでも呼ばれる）
    public void ResetTimer() 
    {
        if (!isHeavyMode && !isPaused) // 発生中や停止中でなければリセット可能
        {
            currentTimer = waitTime;
            Debug.Log("重力までの時間を延長ぼうや");
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

        if (!isHeavyMode && currentTimer > 0) 
        {
            currentTimer -= Time.deltaTime;
        }

        if (timerText != null) 
        {
            // デバフ発動中のタイマーは00
            if (isHeavyMode) timerText.text = "00";
            else timerText.text = Mathf.CeilToInt(currentTimer).ToString("00");
        }
    }

    IEnumerator GravityLoop() 
    {
        while (true) 
        {
            // ポーズ中かタイマーがある間は待機
            while (isPaused || currentTimer > 0.001f) yield return null; 

            isHeavyMode = true; 
            Physics2D.gravity = new Vector2(0, originalGravity * gravityMultiplier);
            if (playerScript != null) playerScript.moveSpeed = basePlayerSpeed * slowSpeedMultiplier;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                yield return new WaitForSeconds(0.6f);
                float duration = 1.0f; 
                float fadeTime = 0f;
                while (fadeTime < duration) 
                {
                    if (isPaused) break; // 途中でオフにされたら中断
                    fadeTime += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTime / duration);
                    yield return null;
                }
                canvasGroup.alpha = 0f;
            }

            float elapsed = 0;
            while (elapsed < (heavyTime - 1.6f))
            {
                if (isPaused) break;
                elapsed += Time.deltaTime;
                yield return null;
            }

            Physics2D.gravity = new Vector2(0, originalGravity);
            if (playerScript != null) playerScript.moveSpeed = basePlayerSpeed;
            isHeavyMode = false; 
            if (!isPaused) currentTimer = waitTime;
        }
    }

    // ボタンから呼ぶための「切り替え」用関数
    public void TogglePause()
    {
        // 今の状態を反転させる（trueならfalse、falseならtrueに）
        SetPause(!isPaused);
    }
}