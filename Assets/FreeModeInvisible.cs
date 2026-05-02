using UnityEngine; // Unityの基本クラスを使用するための宣言
using System.Collections; // コルーチン（IEnumerator）を使用するための宣言
using TMPro; // TextMeshProを操作するための宣言

// 特定の周期でオブジェクトを透明化させるクラスの定義
public class FreeModeInvisible : MonoBehaviour
{
    // インスペクター上での操作を分かりやすくグループ化する工夫
    [Header("隠蔽の設定")]
    public float waitTime = 15.0f; // 透明化が始まるまでの待機時間
    public float invisibleTime = 5.0f; // 透明化が持続する時間

    // UI関連の参照をまとめる
    [Header("UI・演出参照")]
    public GameObject HideImage; // 隠蔽発生中を示すエフェクト画像の参照
    public TextMeshProUGUI timerText; // カウントダウンを表示するUIの参照

    private Renderer[] allRenderers; // 自分と子供たちが持つ全ての描画コンポーネントを保持
    private TMPro.TMP_Text[] allTexts; // 自分と子供たちが持つ全てのテキストコンポーネントを保持する
    private CanvasGroup canvasGroup; // UIエフェクトの透明度を制御するためのコンポーネント

    private float currentTimer; // 現在の残り時間を計測する
    private bool isInvisibleMode = false; // 現在透明化中かどうかの状態フラグ
    private bool isPaused = false; // 一時停止中かどうかの判定フラグ

// ゲーム開始時の初期化処理
    void Start()
    {
        if (timerText != null && GameManager.instance != null) // UIとデータ管理クラスの両方が存在するか確認
        {
            timerText.color = GameManager.instance.selectedColor; // プレイヤーが選んだ色をUIに反映させる「統一感」の演出
        }

        allRenderers = GetComponentsInChildren<Renderer>(); // 子供を含む全描画コンポーネントを一括取得する
        allTexts = GetComponentsInChildren<TMPro.TMP_Text>(); // 子供を含む全テキストを一括取得する
        if (HideImage != null)
        {
            canvasGroup = HideImage.GetComponent<CanvasGroup>(); // 透明度操作用コンポーネントを取得
            if (canvasGroup == null) canvasGroup = HideImage.AddComponent<CanvasGroup>(); //なければ自動で追加する。セットアップ漏れ防止。
            canvasGroup.alpha = 0f; // 初期状態ではエフェクトを見えなくしておく
        }

        currentTimer = waitTime; // タイマーを初期値にセット
        StartCoroutine(GoalFlashLoop()); // 透明化のループ処理を非同期で開始
    }

    // 外部からポーズ状態を切り替えるための関数
    public void SetPause(bool pause)
    {
        isPaused = pause; // 状態を更新
        
        if (isPaused) // 停止時のリセット処理
        {
            SetGoalVisibility(true); // 強制的に元に戻す
            if (canvasGroup != null) canvasGroup.alpha = 0f; // エフェクトも消去

            if (timerText != null) timerText.text = waitTime.ToString("00"); // フラグもリセット
            
            isInvisibleMode = false; // フラグもリセット
        }
        else
        {
            currentTimer = waitTime; // タイマーを最初からやり直させる
            if (timerText != null) timerText.text = waitTime.ToString("00"); // UIを更新
        }
    }

    // 毎フレームの更新処理
    void Update()
    {
        if (isPaused) return; // 一時停止中なら何もしない

        if (isInvisibleMode) // 透明化中の場合
        {
            if (timerText != null) timerText.text = "00"; // 透明期間中は「00」表示
            return; // 透明化中のカウントダウン処理はスキップ
        }

        if (currentTimer > 0) // カウントダウンが可能な場合
        {
            currentTimer -= Time.deltaTime; // 正確な時間経過を差し引く
        }

        if (timerText != null) // カウントダウンをUIに反映
        {
             timerText.text = Mathf.CeilToInt(currentTimer).ToString("00"); // 切り上げ表示で「00」秒まで綺麗に見せる
        }
    }

    // 透明化のサイクルを管理するコルーチン
    IEnumerator GoalFlashLoop()
    {
        while (true) // ゲームが動いている間ずっと繰り返すループ
        {
            while (isPaused || currentTimer > 0.001f) yield return null; // ポーズ中かタイマーがある間は待機する
            
            
            isInvisibleMode = true; // 隠蔽モード開始
            SetGoalVisibility(false); // オブジェクトを消す

            if (canvasGroup != null) // 隠蔽開始の合図を出す演出処理
            {
                canvasGroup.alpha = 1f; // パッと表示
                yield return new WaitForSeconds(0.6f); // 少し待機
                float fade = 0;
                while (fade < 1.0f) // フェードアウト
                {
                    if (isPaused) break; // フェード中にポーズされたら中断
                    fade += Time.deltaTime; // 時間経過
                    canvasGroup.alpha = 1.0f - fade; // アルファ値を減らす
                    yield return null; // 1フレーム待機
                }
            }

            float elapsed = 0;
            while (elapsed < (invisibleTime - 1.6f)) // 隠蔽状態を一定時間維持するループ（演出時間を引いた分）
            {
                if (isPaused) break; // ポーズされたらループを抜ける安全策
                elapsed += Time.deltaTime; // 経過時間を計測
                yield return null;
            }

            SetGoalVisibility(true); // ゴールを表示
            isInvisibleMode = false; // 隠蔽終了

            if (!isPaused) currentTimer = waitTime; // ポーズ中でなければ次のサイクルへ向けてタイマーを再セット
        }
    }

    void SetGoalVisibility(bool isVisible) // 描画の一括切り替えを行うヘルパー関数
    {
        foreach (Renderer r in allRenderers) if (r != null) r.enabled = isVisible; // 全てのレンダラーの表示・非表示を切り替え
        
        foreach (TMPro.TMP_Text t in allTexts) // 全てのテキストについても同様に処理
        {
            if (t != null && t != timerText) // ただし、カウントダウン用のタイマー自身は消さない
            {
                t.enabled = isVisible; // テキストの表示・非表示を切り替え
            }
        }
    }
}