using UnityEngine; // Unityの基本機能を使うための宣言
using System.Collections; // コルーチン（IEnumerator）を使うための宣言
using System.Collections.Generic; // リストなどのコレクションを使うための宣言
using UnityEngine.UI; // UIコンポーネントを操作するための宣言
using TMPro; // TextMeshProを操作するための宣言

// 一定時間ごとにゴールを隠蔽して難易度を上げるクラス
public class InvisibleGoal : MonoBehaviour 
{
    public float waitTime = 15.0f;     // ゴールが見えている時間
    public float invisibleTime = 5.0f; // ゴールが姿を消す時間

    public GameObject HideImage; // 隠蔽が始まったことを知らせるエフェクト
    public TextMeshProUGUI timerText; // 隠蔽までのカウントダウンを表示する

    private Renderer[] allRenderers; // ゴールに含まれる全ての描画コンポーネントを保持する
    private TMPro.TMP_Text[] allTexts; // ゴールに含まれる全ての文字コンポーネントを保持する
    private CanvasGroup canvasGroup; // 演出用パネルの透明度を滑らかに操るためのコンポーネント

    private float currentTimer; // 次のイベントまでの残り時間を計測する変数

    public bool isInvisibleMode { get; private set; } = false; // 現在隠蔽中かどうか

    // ゲーム開始時の初期設定
    void Start() 
    {
        // GameManagerから選択された色をタイマーに反映する
        if (timerText != null && GameManager.instance != null)
        {
            timerText.color = GameManager.instance.selectedColor;
        }

        // 子供のオブジェクトも含めて、消すべき描画コンポーネントを全てリストアップ
        allRenderers = GetComponentsInChildren<Renderer>();
        allTexts = GetComponentsInChildren<TMPro.TMP_Text>();

        // 演出用画像の準備
        if (HideImage != null)
        {
            canvasGroup = HideImage.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = HideImage.AddComponent<CanvasGroup>();
            }
            HideImage.SetActive(true);
            canvasGroup.alpha = 0f; // 最初は透明
        }

        currentTimer = waitTime; // タイマーを初期化
        StartCoroutine(GoalFlashLoop()); // 隠蔽ループを開始
    }

    // ゲーム終了時などにループを止めて、ゴールを強制的に再表示させる関数
    public void StopInvisibleLoop()
    {
        StopAllCoroutines(); // 進行中のコルーチンを全て止める
        
        SetGoalVisibility(true); // ゴールを見える状態に戻す
        
        if (timerText != null) timerText.text = "";
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        isInvisibleMode = false;
        Debug.Log("タイムアップ");
    }

    // 特定のアクション（アイテム獲得など）で隠蔽を先延ばしにするための関数
    public void ResetTimer()
    {
        if (!isInvisibleMode) // すでに隠れている時はリセットできない
        {
            currentTimer = waitTime;
            Debug.Log("隠蔽までの時間を延長ぼうや");
        }
    }

    void Update() // 毎フレームのタイマー更新処理death
    {
        if (!isInvisibleMode && currentTimer > 0) // 見えている間だけカウントダウン
        {
            currentTimer -= Time.deltaTime;
        }

        if (timerText != null) // UIに残り秒数を「00」形式で表示
        {
            timerText.text = Mathf.CeilToInt(currentTimer).ToString("00");
        }
    }

    // 隠蔽と表示を繰り返すメインループ
    IEnumerator GoalFlashLoop()
    {
        while (true)
        {
            // 平和タイムの待機処理
            isInvisibleMode = false;
            while (currentTimer > 0.001f) { yield return null; }
            currentTimer = 0;

            // 隠蔽発動
            isInvisibleMode = true; 
            Debug.Log("にんにんぼうやが隠蔽デバフを発動");
            
            SetGoalVisibility(false); // 描画をOFF

            // 隠蔽開始の合図を滑らかにフェードアウトさせる
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f; 
                yield return new WaitForSeconds(0.6f);

                float duration = 1.0f; 
                float currentTime = 0f;
                while (currentTime < duration)
                {
                    currentTime += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTime / duration);
                    yield return null;
                }
                canvasGroup.alpha = 0f;
            }

            // 指定された時間だけ隠蔽状態をキープ
            float remainingInvisibleTime = invisibleTime - 1.6f;
            if (remainingInvisibleTime > 0)
            {
                yield return new WaitForSeconds(remainingInvisibleTime);
            }

            // 隠蔽終了、再び姿を現す
            Debug.Log("にんにんぼうやは去ったようだ");
            SetGoalVisibility(true); 
            isInvisibleMode = false;

            currentTimer = waitTime; // タイマーを戻して次のサイクルへ
        }
    }

    // 全ての描画コンポーネントを一括でON/OFFする
    void SetGoalVisibility(bool isVisible)
    {
        foreach (Renderer r in allRenderers)
        {
            if (r != null) r.enabled = isVisible; // Rendererを操作
        }

        foreach (TMPro.TMP_Text t in allTexts)
        {
            if (t != null) t.enabled = isVisible; // TextMeshProを操作
        }
    }
}