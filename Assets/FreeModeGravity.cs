using UnityEngine; // Unityの基本クラスを使用するための宣言
using System.Collections; // コルーチン（IEnumerator）を使用するための宣言
using TMPro; // テキスト表示にTextMeshProを使用するための宣言

// 定期的に重力ギミックを発動させるクラス
public class FreeModeGravity : MonoBehaviour
{
    // インスペクター上での操作を分かりやすくグループ化する工夫
    [Header("重力の設定")]
    public float gravityMultiplier = 5.0f; // 重力何倍にするか
    public float slowSpeedMultiplier = 0.3f; // 重力中にプレイヤーの移動速度を何倍に下げるか
    public float waitTime = 15.0f; // 次の重力発動までの待機秒数
    public float heavyTime = 5.0f; // 重力が強くなっている継続時間

    // UI関連の参照をまとめる
    [Header("UI・演出参照")]
    public GameObject GravityImage; // 重力発生中を示すエフェクト画像の参照
    public TextMeshProUGUI timerText; // カウントダウンを表示するUIの参照

    private float originalGravity; // 初期状態の重力値を保存しておくための変数
    private float currentTimer; // 現在のカウントダウン秒数を管理する変数
    private bool isHeavyMode = false; // 現在重力モード中かどうかのフラグ
    private bool isPaused = false; // ゲームが一時停止中かどうかを判定するフラグ

    private PlayerController playerScript; // プレイヤーの移動速度を直接操作するためのスクリプト参照
    private float basePlayerSpeed; // プレイヤーの元の移動速度を記憶しておく変数
    private CanvasGroup canvasGroup; // エフェクトの透過度を滑らかに操作するためのコンポーネント

    // ゲーム開始時の初期設定
    void Start()
    {
        if (timerText != null && GameManager.instance != null) // UIとデータ管理クラスの両方が存在するか確認
        {
            timerText.color = GameManager.instance.selectedColor; // プレイヤーが選んだ色をUIに反映させる「統一感」の演出
        }

        originalGravity = Physics2D.gravity.y; // Unityの物理設定から現在の重力を取得・保存する
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); // タグを使ってプレイヤーを特定
        if (playerObj != null) // プレイヤーが見つかった場合のみ処理
        {
            playerScript = playerObj.GetComponent<PlayerController>(); // 速度制御のためにスクリプトを取得
            if (playerScript != null) basePlayerSpeed = playerScript.moveSpeed; // 元の速度を退避させておく「復元用データ」の保持
        }

        if (GravityImage != null) // エフェクト画像がある場合
        {
            canvasGroup = GravityImage.GetComponent<CanvasGroup>(); // 透過度制御用のコンポーネントを取得
            if (canvasGroup == null) canvasGroup = GravityImage.AddComponent<CanvasGroup>(); // なければ自動で追加する。セットアップ漏れ防止。
            canvasGroup.alpha = 0f; // 初期状態ではエフェクトを見えなくしておく
        }

        currentTimer = waitTime; // タイマーを初期値にセット
        StartCoroutine(GravityLoop()); // 重力変化のループをコルーチンで開始する非同期処理
        Debug.Log("[GravitySystem] Initialized and Loop Started.");
    }

    // 外部（一時停止メニュー等）から呼ばれるポーズ切り替え関数
    public void SetPause(bool pause)
    {
        isPaused = pause; // 状態を更新
        if (isPaused) // 停止させる時の処理
        {
            Physics2D.gravity = new Vector2(0, originalGravity); // 重力を元に戻す安全設計
            if (playerScript != null) playerScript.moveSpeed = basePlayerSpeed; // 速度も元に戻す
            if (canvasGroup != null) canvasGroup.alpha = 0f; // エフェクトも消す

            // 停止中は「15」を出す
            if (timerText != null) timerText.text = waitTime.ToString("00");
            isHeavyMode = false;
        }
        else
        {
            currentTimer = waitTime; // 再開時にタイマーをリセットする
            if (timerText != null) timerText.text = waitTime.ToString("00");
        }
    }

    // 毎フレームの更新処理
    void Update()
    {
        if (isPaused) return; // 一時停止中なら何もしない

        if (!isHeavyMode && currentTimer > 0) // 重力モード中でなく、タイマーが残っている場合
        {
            currentTimer -= Time.deltaTime; // 実時間に合わせてタイマーを減らすカウント
        }

        if (timerText != null) // UIが存在する場合
        {
            timerText.text = Mathf.CeilToInt(currentTimer).ToString("00"); // 切り上げ表示で「00」秒まで綺麗に見せる
        }
    }

    // 重力ギミックのメインサイクルコルーチン
    IEnumerator GravityLoop()
    {
        while (true) // ゲームが動いている間ずっと繰り返すループ
        {
            while (isPaused || currentTimer > 0.001f) yield return null; // ポーズ中かタイマーがある間は待機する処理
            
            isHeavyMode = true; // 重力モード開始
            Physics2D.gravity = new Vector2(0, originalGravity * gravityMultiplier); // 物理演算の重力を一気に強める
            if (playerScript != null) playerScript.moveSpeed = basePlayerSpeed * slowSpeedMultiplier; // プレイヤーの移動を制限し重さを表現する

            if (canvasGroup != null) // 視覚演出の処理
            {
                canvasGroup.alpha = 1f; // 重力エフェクトをパッと表示
                yield return new WaitForSeconds(0.6f); // 少しの間だけ表示を維持
                float fade = 0;
                while (fade < 1.0f) // フェードアウト
                {
                    if (isPaused) break; // フェード中にポーズされたら中断する
                    fade += Time.deltaTime; // 時間経過で値を増やす
                    canvasGroup.alpha = 1.0f - fade; // アルファ値を減らしていく
                    yield return null; // 1フレーム待機
                }
            }

            float elapsed = 0;
            while (elapsed < (heavyTime - 1.6f)) // 重力増加状態を一定時間維持するループ（演出時間を引いた分）
            {
                if (isPaused) break; // ポーズされたらループを抜ける安全策
                elapsed += Time.deltaTime; // 経過時間を計測
                yield return null;
            }

            Physics2D.gravity = new Vector2(0, originalGravity); // 重力を元の状態に復元
            if (playerScript != null) playerScript.moveSpeed = basePlayerSpeed; // プレイヤーの速度も復元
            isHeavyMode = false; // 重力デバフ終了フラグ

            if (!isPaused) currentTimer = waitTime; // ポーズ中でなければ次のサイクルへ向けてタイマーを再セット
        }
    }
}