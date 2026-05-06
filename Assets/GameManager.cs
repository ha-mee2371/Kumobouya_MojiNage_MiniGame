using UnityEngine; // Unityの基本クラスを使用するための宣言
using UnityEngine.UI; // ボタンや画像などUI操作のための宣言
using System.Collections; // コルーチン（IEnumerator）を使うための宣言
using System.Collections.Generic; // リスト（List）などのコレクションを使用するための宣言
using TMPro; // TextMeshProを操作するための宣言
using UnityEngine.SceneManagement; // シーンのロードやイベント取得のための宣言

// ゲームのモードを定義する列挙型
public enum GameMode { Normal, Free }

// ゲーム全体の状態（開始・終了・タイマー）を統括するクラス
public class GameManager : MonoBehaviour 
{
    public static GameManager instance; // どこからでもアクセスできるようにするシングルトン

    [Header("モード設定")]
    public GameMode currentMode; // インスペクターでNormalかFreeか選べる

    [Header("文字色データ")]
    public Color selectedColor = Color.white; // プレイヤーが選んだテーマカラーを保持する変数

    [Header("UIの設定")]
    public GameObject startImage;  
    public GameObject timeUpImage; 
    public TextMeshProUGUI timerText; 
    public FreeModeGuide freeModeGuide; // フリーモードの操作案内スクリプトの参照

    [Header("ゲーム設定")]
    public float timeLimit = 99f; // ゲームの制限時間
    public PlayerController player; 

    private float currentTimer; // 現在の残り時間を計測する内部変数
    private bool isGameRunning = false; // ゲームが進行中かどうかのフラグ

    // リセットの順番を管理
    private bool resetGravityNext = true; 

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
        // シーン名でモードを自動判別する
        if (scene.name == "GameScene") currentMode = GameMode.Normal;
        else if (scene.name == "FreeModeScene") currentMode = GameMode.Free;

        if (scene.name == "GameScene" || scene.name == "FreeModeScene") 
        {
            StopAllCoroutines(); // 前のシーンの残りカスコルーチンを止める
            InitGame();
        }
    }

    // ゲーム本編開始時のセットアップ。
    void InitGame() 
    {
        // 開始時に重力をノーマルに戻す処理を追加
        Physics2D.gravity = new Vector2(0, -9.81f);
        resetGravityNext = true; // リセット順も初期化

        startImage = GameObject.Find("StartImage");
        timeUpImage = GameObject.Find("TimeUpImage");
        player = FindFirstObjectByType<PlayerController>();

        // 操作ガイドオブジェクトを探して自動接続
        GameObject guideObj = GameObject.Find("FreeModeGuideUI"); 
        if (guideObj != null) freeModeGuide = guideObj.GetComponent<FreeModeGuide>();

        // ボタンの自動接続処理
        Button[] allButtons = Resources.FindObjectsOfTypeAll<Button>();
        foreach (Button btn in allButtons)
        {
            if (btn == null || btn.gameObject == null) continue;
            string n = btn.gameObject.name;

            if (n.Contains("Title")) // ボタンの名前に「Title」が含まれていれば自動接続
            {
                btn.onClick.RemoveAllListeners(); // 二重登録を防ぐ
                btn.onClick.AddListener(GoToTitle); // タイトルへ戻る処理を登録
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

    // 交互にリセットする仕組み
    public void RequestDebuffReset()
    {
        if (resetGravityNext)
        {
            PeriodicGravity g = FindFirstObjectByType<PeriodicGravity>();
            if (g != null) g.ResetTimer();
        }
        else
        {
            InvisibleGoal i = FindFirstObjectByType<InvisibleGoal>();
            if (i != null) i.ResetTimer();
        }
        resetGravityNext = !resetGravityNext; // 順番を入れ替える
    }

    IEnumerator GameLoopCoroutine() // ゲームの開始から終了までの流れを1つのコルーチンで管理する
    {
        yield return new WaitForSeconds(0.5f);
        if (player != null) player.enabled = true; 

        if (startImage != null) // 「START」の演出
        {
            startImage.SetActive(true);

            // ★修正点：STARTアニメが始まるのと同時にガイド表示を開始する
            if (currentMode == GameMode.Free && freeModeGuide != null)
            {
                freeModeGuide.ShowGuide(5.0f); // 5秒表示して消える
            }

            yield return StartCoroutine(PoyonAnimation(startImage.transform)); // 弾むアニメ演出
            yield return new WaitForSeconds(0.8f);
            startImage.SetActive(false);
        }
        else
        {
            // startImageがない場合でもフリーモードならガイドを出す
            if (currentMode == GameMode.Free && freeModeGuide != null)
            {
                freeModeGuide.ShowGuide(5.0f);
            }
        }

        isGameRunning = true; // タイマーが動き出す

        // Normalモードの時だけ制限時間で終わる
        if (currentMode == GameMode.Normal)
        {
            while (currentTimer > 0)
            {
                yield return null;
            }

            isGameRunning = false; // タイムアップ
            if (player != null) player.enabled = false;

            // 他のマネージャー達に停止命令を送る
            MojiSpawner spawner = FindFirstObjectByType<MojiSpawner>();
            if (spawner != null) spawner.StopSpawning();

            PeriodicGravity gravity = FindFirstObjectByType<PeriodicGravity>();
            if (gravity != null) gravity.StopGravityLoop();

            InvisibleGoal goal = FindFirstObjectByType<InvisibleGoal>();
            if (goal != null) goal.StopInvisibleLoop();

            // 全オブジェクト静止
            GameObject[] TargetMojis = GameObject.FindGameObjectsWithTag("Target");
            foreach (GameObject l in TargetMojis)
            {
                Rigidbody2D rb = l.GetComponent<Rigidbody2D>();
                if (rb != null) rb.simulated = false;
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
                
                ResultManager rm = Resources.FindObjectsOfTypeAll<ResultManager>()[0];
                if (rm != null)
                {
                    rm.gameObject.SetActive(true);
                    rm.StartResultSequence(timeUpImage);
                }
            }
        }
    }

    void Update() // 毎フレームのタイマー更新
    {
        // Normalモードの時だけタイマーを減らす
        if (isGameRunning && currentMode == GameMode.Normal)
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
            // 選んだ色を反映
            timerText.color = selectedColor; 

            if (currentMode == GameMode.Normal)
            {
                // 通常モードの時だけ「TIME: 00」と表示する
                timerText.text = "TIME: " + Mathf.CeilToInt(currentTimer).ToString("D2");
            }
            else
            {
                // フリーモードの時は文字を消す
                timerText.text = ""; 
            }
        }
    }

    public void GoToTitle() // タイトルに戻る
    {
        Physics2D.gravity = new Vector2(0, -9.81f);
        StopAllCoroutines(); 
        Time.timeScale = 1f; 
        SceneManager.LoadScene("TitleScene");
    }
}