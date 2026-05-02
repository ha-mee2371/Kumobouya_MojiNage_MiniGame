using UnityEngine; // Unityの基本クラスを使用するための宣言
using UnityEngine.UI; // ImageなどのUIコンポーネント用
using TMPro; // TextMeshPro用
using System.Collections; // コルーチンを使うために必須

// リザルト画面の演出を管理するクラス
public class ResultManager : MonoBehaviour 
{
    [Header("UI Panels")]
    public GameObject resultCanvas; // リザルト画面全体のキャンバス
    public Image backgroundSiro; // 背景を白くフェードさせるための画像
    public GameObject arcadeRegistrationPanel;

    [Header("Ranking Systems")]
    public ArcadeScoreManager arcadeScoreManager; // スコアをサーバーに送るためのマネージャー参照
    
    [Header("Text Elements")]
    // 各種スコアや個別の文字カウントを表示するテキスト群
    public TextMeshProUGUI baseScoreText;
    public TextMeshProUGUI countNaText;
    public TextMeshProUGUI countNText;
    public TextMeshProUGUI countYaText;
    public TextMeshProUGUI countTeText;
    public TextMeshProUGUI nanyateSetsText;
    public TextMeshProUGUI setBonusText;
    public TextMeshProUGUI multiBonusText;
    public TextMeshProUGUI totalScoreText;

    [Header("New Symbols")] 
    public GameObject tasuObject;   // 演出で使う「＋」の記号
    public GameObject kakeruObject; // 演出で使う「×」の記号

    // 成績によって出し分けるくもぼうやの画像
    [Header("Kumo Images")]
    public GameObject kumoSad;
    public GameObject kumoNormal;
    public GameObject kumoHappy;

    [Header("Buttons")]
    public GameObject buttonSet; // 演出が終わった後に表示するボタン

    private GameObject myTimeUpImg; // 「TimeUp」のロゴ画像を一時的に保持

    void Awake()
    {
        // 最初は全て非表示にして、準備万端にしておく
        resultCanvas.SetActive(false);
        backgroundSiro.color = new Color(1, 1, 1, 0); // 背景は透明
        SetAllUIActive(false); 

        // シーン内から自動で見つけ出す
        if (arcadeScoreManager == null)
        {
            arcadeScoreManager = Object.FindFirstObjectByType<ArcadeScoreManager>();
        }
    }

    // ゲーム終了時に他のスクリプトから呼ばれる、演出開始の合図
    public void StartResultSequence(GameObject timeUpImg)
    {
        Debug.Log("くもぼうやは結果発表が気になるようです。");
        this.gameObject.SetActive(true); 
        myTimeUpImg = timeUpImg;
        StartCoroutine(ResultCoroutine()); // 演出を開始
    }

    // 演出の本体
    IEnumerator ResultCoroutine()
    {
        // Time Up画像を画面中央へ移動
        if (myTimeUpImg != null)
        {
            Vector3 startPos = myTimeUpImg.transform.localPosition;
            Vector3 endPos = new Vector3(startPos.x, 150, 0);
            float elapsed = 0;
            float duration = 1.0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = t * t * (3f - 2f * t); 
                myTimeUpImg.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }
        }

        // 背景を白くフェードインさせる
        resultCanvas.SetActive(true);
        float alpha = 0;
        while (alpha < 0.7f)
        {
            alpha += Time.deltaTime * 1.5f;
            backgroundSiro.color = new Color(1, 1, 1, alpha);
            yield return null; 
        }

        yield return new WaitForSeconds(0.2f);

        // スコア計算
        var sm = ScoreManager.instance;
        int sets = sm.GetNanyateSets(); // 「なんやて」が何セット揃ったか計算
        int bonusScore = sets * 10;
        float multiplier = (sets >= 6) ? 2.0f : (sets >= 3) ? 1.5f : 1.0f; // セット数に応じて倍率が変わる

        int finalScore = Mathf.FloorToInt((sm.totalScore + bonusScore) * multiplier);

        // 【重要】通信マネージャーに最終スコアをセット
        if (arcadeScoreManager != null)
        {
            arcadeScoreManager.scoreToUpload = finalScore;
            Debug.Log("くもぼうやはArcadeScoreManagerにスコアを持っていくようだ: " + finalScore);
        }
        else
        {
            Debug.LogWarning("くもぼうやはArcadeScoreManagerが見つからなくて焦っている。");
        }

        // 数字がドゥルルと増えていくカウントアップ演出
        yield return StartCoroutine(CountUpWithPrefix(baseScoreText, "基本のスコア: ", sm.totalScore));
        yield return StartCoroutine(CountUpWithPrefix(countNaText, "な: ", sm.countNa, "個"));
        yield return StartCoroutine(CountUpWithPrefix(countNText, "ん: ", sm.countN, "個"));
        yield return StartCoroutine(CountUpWithPrefix(countYaText, "や: ", sm.countYa, "個"));
        yield return StartCoroutine(CountUpWithPrefix(countTeText, "て: ", sm.countTe, "個"));

        yield return StartCoroutine(CountUpWithPrefix(nanyateSetsText, "なんやて", sets, "セット"));

        // セット数に合わせて「くもぼうや」の表情を変える
        kumoSad.SetActive(sets <= 2);
        kumoNormal.SetActive(sets >= 3 && sets <= 5);
        kumoHappy.SetActive(sets >= 6);
        
        yield return new WaitForSeconds(0.8f); 

        // ボーナスと倍率の追加演出
        tasuObject.SetActive(true); // 「＋」を表示
        yield return StartCoroutine(CountUpWithPrefix(setBonusText, "なんやてセットボーナス: ", bonusScore));
        
        kakeruObject.SetActive(true); // 「×」を表示
        yield return StartCoroutine(CountUpFloatWithPrefix(multiBonusText, "倍率ボーナス: ", 1.0f, multiplier, "倍"));
        
        yield return new WaitForSeconds(0.5f);
        
        // 最後に合計スコアを表示する
        yield return StartCoroutine(CountUpWithPrefix(totalScoreText, "合計スコア: ", finalScore));

        buttonSet.SetActive(true); // 全ての演出が終わったらボタンを出す
    }

    // 整数をカウントアップさせる共通関数
    IEnumerator CountUpWithPrefix(TextMeshProUGUI text, string prefix, int targetValue, string suffix = "")
    {
        text.gameObject.SetActive(true);
        int current = 0;
        float time = 0;
        float duration = 0.5f;
        while (time < duration)
        {
            time += Time.deltaTime;
            current = Mathf.FloorToInt(Mathf.Lerp(0, targetValue, time / duration));
            text.text = $"{prefix}{current}{suffix}";
            yield return null;
        }
        text.text = $"{prefix}{targetValue}{suffix}";
    }

    // 小数点をカウントアップさせる関数
    IEnumerator CountUpFloatWithPrefix(TextMeshProUGUI text, string prefix, float startValue, float targetValue, string suffix = "")
    {
        text.gameObject.SetActive(true);
        float elapsed = 0;
        float duration = 0.5f; 
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float current = Mathf.Lerp(startValue, targetValue, t);
            
            text.text = $"{prefix}{current:F1}{suffix}"; // 小数点第一位まで表示
            yield return null;
        }
        text.text = $"{prefix}{targetValue:F1}{suffix}";
    }

    // ランキング登録パネルを開く関数
    public void OpenRegistrationPanel()
    {
        if (arcadeRegistrationPanel != null)
        {
            arcadeRegistrationPanel.SetActive(true); 
        }
    }

    // 全てのUI要素を一括で非表示にする初期化用関数
    void SetAllUIActive(bool active)
    {
        baseScoreText.gameObject.SetActive(active);
        countNaText.gameObject.SetActive(active);
        countNText.gameObject.SetActive(active);
        countYaText.gameObject.SetActive(active);
        countTeText.gameObject.SetActive(active);
        nanyateSetsText.gameObject.SetActive(active);
        setBonusText.gameObject.SetActive(active);
        multiBonusText.gameObject.SetActive(active);
        totalScoreText.gameObject.SetActive(active);
        
        tasuObject.SetActive(active);
        kakeruObject.SetActive(active);

        kumoSad.SetActive(active);
        kumoNormal.SetActive(active);
        kumoHappy.SetActive(active);
        buttonSet.SetActive(active);
    }
}