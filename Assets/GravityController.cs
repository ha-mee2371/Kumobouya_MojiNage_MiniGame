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

    private float originalGravity; // 元々の世界の重力値を保存しておく変数
    private PlayerController playerScript; // プレイヤーの移動速度を直接書き換えるための参照
    private CanvasGroup canvasGroup; // UIフェード演出のためのコンポーネント

    private float currentTimer; // 現在の経過時間を計測する内部変数
    
    public bool isHeavyMode { get; private set; } = false; // 現在重力モードかどうか

    private float basePlayerSpeed; // プレイヤーの本来の移動速度を保存
    
    // ゲーム開始時の初期化
    void Start() 
    {
        if (timerText != null && GameManager.instance != null) // GameManagerからテーマカラーを引き継いでUIの色を統一する
        {
            timerText.color = GameManager.instance.selectedColor;
        }

        originalGravity = Physics2D.gravity.y; // 初期状態の重力を記憶しておく重要な処理
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); // プレイヤーをタグで検索
        if (playerObj != null)
        {
            playerScript = playerObj.GetComponent<PlayerController>();
            if (playerScript != null) basePlayerSpeed = playerScript.moveSpeed; // 元の速度をメモ
        }

        if (GravityImage != null) // 演出用UIのセットアップ
        {
            canvasGroup = GravityImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = GravityImage.AddComponent<CanvasGroup>(); // コンポーネントがなければ自動で足す
            }
            
            GravityImage.SetActive(true); 
            canvasGroup.alpha = 0f; // 最初は隠しておく
        }

        currentTimer = waitTime;
        StartCoroutine(GravityLoop()); // 永久ループする重力サイクルを開始
    }

    // ゲーム終了時などに外部（GameManager等）から呼ばれる停止関数
    public void StopGravityLoop() 
    {
        StopAllCoroutines(); // 全てのループを停止
        
        Physics2D.gravity = new Vector2(0, originalGravity); // 重力を元に戻す
        
        if (playerScript != null)
        {
            playerScript.moveSpeed = basePlayerSpeed; // プレイヤーの速度も元通り
        }

        if (timerText != null) timerText.text = "";
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        isHeavyMode = false;
    }

    // 何らかのアクションで重力発生を遅らせる関数
    public void ResetTimer() 
    {
        if (!isHeavyMode) // 重力発生中でなければタイマーをリセット可能
        {
            currentTimer = waitTime;
            Debug.Log("重力までの時間を延長ぼうや");
        }
    }

    // 毎フレームのタイマー更新
    void Update() 
    {
        if (!isHeavyMode && currentTimer > 0) // 平和タイム中だけカウントダウン
        {
            currentTimer -= Time.deltaTime;
        }

        if (timerText != null) // UIに秒数を表示。
        {
            timerText.text = Mathf.CeilToInt(currentTimer).ToString("00");
        }
    }

    // 重力のON/OFFを管理するメインコルーチン
    IEnumerator GravityLoop() 
    {
        while (true) 
        {
            // 平和タイムの待機
            isHeavyMode = false;
            while (currentTimer > 0.001f) 
            {
                yield return null; 
            }
            currentTimer = 0;

            // 重力開始の物理演算操作
            isHeavyMode = true; 
            Debug.Log("にんにんぼうやが重力デバフを発動");
            Physics2D.gravity = new Vector2(0, originalGravity * gravityMultiplier);
            
            // プレイヤーの移動能力を制限する処理
            if (playerScript != null) 
            {
                playerScript.moveSpeed = basePlayerSpeed * slowSpeedMultiplier;
            }

            // 視覚的な演出
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                yield return new WaitForSeconds(0.6f);

                float duration = 1.0f; 
                float fadeTime = 0f;
                while (fadeTime < duration) 
                {
                    fadeTime += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTime / duration);
                    yield return null;
                }
                canvasGroup.alpha = 0f;
            }

            // 重力持続時間の待機
            float remainingHeavyTime = heavyTime - 1.6f;
            if (remainingHeavyTime > 0)
            {
                yield return new WaitForSeconds(remainingHeavyTime);
            }

            // 世界を平和に戻すリセット処理
            Debug.Log("にんにんぼうやは去ったようだ");
            Physics2D.gravity = new Vector2(0, originalGravity);
            
            // プレイヤーの速度を元に戻す
            if (playerScript != null) 
            {
                playerScript.moveSpeed = basePlayerSpeed;
            }

            isHeavyMode = false; 
            currentTimer = waitTime;
        }
    }
}