using UnityEngine; // Unityの基本クラスを使用するための宣言
using UnityEngine.UI; // ボタンや画像などUI操作のための宣言
using System.Collections; // コルーチン（IEnumerator）を使用するための宣言
using System.Collections.Generic; // リスト（List）などのコレクションを使用するための宣言
using TMPro; // TextMeshProを操作するための宣言
using UnityEngine.SceneManagement; // シーンのロードやイベント取得のための宣言

// ゲーム全体の状態（開始・終了・タイマー）を統括するクラス
public class GameManager : MonoBehaviour 
{
    public static GameManager instance; // どこからでもアクセスできるようにするシングルトン

    [Header("文字色データ")]
    public Color selectedColor = Color.white; // プレイヤーが選んだテーマカラーを保持する変数

    [Header("UIの設定")]
    public GameObject startImage;  
    public GameObject timeUpImage; 
    public TextMeshProUGUI timerText; 

    [Header("ゲーム設定")]
    public float timeLimit = 99f; // ゲームの制限時間
    public PlayerController player; 

    private float currentTimer; // 現在の残り時間を計測する内部変数
    private bool isGameRunning = false; // ゲームが進行中かどうかのフラグ

    void Awake() // 生成時の初期化。Startより先に呼ばれる
    {
        if (instance == null) // シングルトンの確立。
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // シーンをまたいでも消えないようにする
            
            // シーンロード時のイベントを登録する
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject); // 2つ目があれば即座に破棄して重複を防ぐ
        }
    }

    void OnDestroy() // オブジェクト破棄時の後処理
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // イベントの登録を解除するメモリ漏れ防止
    }

    // 新しいシーンが読み込まれた時に自動で呼ばれる関数
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        if (scene.name == "GameScene") // ゲーム本編のシーンだった場合のみ初期化
        {
            StopAllCoroutines(); // 前のシーンの残りカスコルーチンを止める
            InitGame();
        }
    }

    // ゲーム本編開始時のセットアップ。
    void InitGame() 
    {
        startImage = GameObject.Find("StartImage");
        timeUpImage = GameObject.Find("TimeUpImage");
        player = FindFirstObjectByType<PlayerController>();

        // これがないと「ゲーム中にタイトルに戻るボタン」が1回しか使えない。2回目のゲームを始めるとタイトルに戻るボタンが反応しない。
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in allButtons)
        {
            if (btn == null || btn.gameObject == null) continue;
            string n = btn.gameObject.name;

            if (n.Contains("Title")) // ボタンの名前に「Title」が含まれていれば自動接続
            {
                btn.onClick.RemoveAllListeners(); // 二重登録を防ぐ
                btn.onClick.AddListener(BackToTitle); // タイトルへ戻る処理を登録
            }
        }

        GameObject tTextObj = GameObject.Find("TimerText");
        if (tTextObj != null) timerText = tTextObj.GetComponent<TextMeshProUGUI>();

        if (startImage != null) startImage.SetActive(false);
        if (timeUpImage != null) timeUpImage.SetActive(false);

        currentTimer = timeLimit; // タイマーを最大値にリセット
        isGameRunning = false;
        UpdateTimerText();

        if (player != null) player.enabled = false; // 開始演出中は動けないようにする

        StartCoroutine(GameLoopCoroutine()); // メインループの開始
    }

    IEnumerator GameLoopCoroutine() // ゲームの開始から終了までの流れを1つのコルーチンで管理する
    {
        yield return new WaitForSeconds(0.5f);
        if (player != null) player.enabled = true; 

        if (startImage != null) // 「START」の演出
        {
            startImage.SetActive(true);
            yield return StartCoroutine(PoyonAnimation(startImage.transform)); // 弾むアニメ演出
            yield return new WaitForSeconds(0.8f);
            startImage.SetActive(false);
        }

        isGameRunning = true; // タイマーが動き出す

        // タイマーが0になるまで、このコルーチンをここで待機させる
        while (currentTimer > 0)
        {
            yield return null;
        }

        isGameRunning = false; // タイムアップ
        if (player != null) player.enabled = false;

        // 他のマネージャー達に止まれと命令を送る司令塔としての役割
        MojiSpawner spawner = FindFirstObjectByType<MojiSpawner>();
        if (spawner != null) spawner.StopSpawning();

        PeriodicGravity gravity = FindFirstObjectByType<PeriodicGravity>();
        if (gravity != null) gravity.StopGravityLoop();

        InvisibleGoal goal = FindFirstObjectByType<InvisibleGoal>();
        if (goal != null) goal.StopInvisibleLoop();

        // 画面上の全オブジェクトを一斉に静止させる
        GameObject[] TargetMojis = GameObject.FindGameObjectsWithTag("Target");
        foreach (GameObject l in TargetMojis)
        {
            Rigidbody2D rb = l.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false; // 物理演算をオフ
        }

        GameObject[] OtherMojis = GameObject.FindGameObjectsWithTag("Other");
        foreach (GameObject l in OtherMojis)
        {
            Rigidbody2D rb = l.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = false;
        }

        if (timeUpImage != null) // 「TIME UP」の演出
        {
            timeUpImage.SetActive(true);
            yield return StartCoroutine(PoyonAnimation(timeUpImage.transform));
            yield return new WaitForSeconds(0.5f); 
            
            // 非表示のCanvasも見つけられるように探し方を変更
            ResultManager rm = Resources.FindObjectsOfTypeAll<ResultManager>()[0];
            if (rm != null)
            {
                rm.gameObject.SetActive(true); // まずオブジェクト自体を起こす
                rm.StartResultSequence(timeUpImage);
            }
        }
    }

    void Update() // 毎フレームのタイマー更新
    {
        if (isGameRunning)
        {
            currentTimer -= Time.deltaTime; 
            if (currentTimer <= 0) currentTimer = 0;
            UpdateTimerText();
        }
    }

    IEnumerator PoyonAnimation(Transform Target) // UIを弾ませる共通のアニメーション関数
    {
        Target.localScale = Vector3.zero;
        float duration = 0.5f;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float scale = Mathf.Sin(t * Mathf.PI * 2.5f) * (1 - t) + 1;
            Target.localScale = new Vector3(scale, scale, 1);
            yield return null;
        }
        Target.localScale = Vector3.one;
    }

    void UpdateTimerText() // タイマー表示を整形するヘルパー関数
    {
        if (timerText != null)
        {
            timerText.color = selectedColor; // テーマカラーを反映する
            timerText.text = "TIME: " + Mathf.CeilToInt(currentTimer).ToString("D2");
        }
    }

    public void BackToTitle() // タイトルに戻る
    {
        StopAllCoroutines(); 
        Time.timeScale = 1f; 
        SceneManager.LoadScene("TitleScene");
    }
}