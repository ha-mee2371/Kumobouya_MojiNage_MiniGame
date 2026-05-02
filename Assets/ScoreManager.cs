using UnityEngine; // Unityの基本機能を使うための宣言
using TMPro; // TextMeshPro用宣言

public class ScoreManager : MonoBehaviour // ゲーム中のスコア計算と文字カウントを一手に引き受けるマネージャー
{
    // どこからでも「ScoreManager.instance」でアクセスできるようにする、シングルトン
    public static ScoreManager instance;

    [Header("UI表示設定")]
    public TextMeshProUGUI scoreTextUI; // 画面上にスコアを表示するためのテキストUIの参照

    [Header("現在のスコア")]
    public int totalScore = 0; // プレイヤーが稼いだ合計点数

    [Header("文字のカウント")]
    // 「なんやて」の各文字がいくつ集まったかを記録する変数
    public int countNa = 0;
    public int countN = 0;
    public int countYa = 0;
    public int countTe = 0;

    [Header("参照するスクリプト")]
    // 重力変化や透明化の状態をチェックするために、他のスクリプトを参照する
    public PeriodicGravity gravityScript;
    public InvisibleGoal invisibleScript;

    void Awake()
    {
        // 世界に一つだけのScoreManagerを確定させる、シングルトン
        if (instance == null) instance = this;
    }

    void Start()
    {
        // 他のシーンから持ち越した「こだわりの色」をUIに適用する演出
        if (scoreTextUI != null && GameManager.instance != null)
        {
            // GameManagerが持っている選択色を、スコアの文字色に流し込む
            scoreTextUI.color = GameManager.instance.selectedColor;
        }
    }

    void Update()
    {
        // 最新のスコアを「SCORE: 0000」の形式で表示更新
        if (scoreTextUI != null)
        {
            scoreTextUI.text = "SCORE: " + totalScore.ToString("0000");
        }
    }

    // 文字をキャッチした時に呼ばれる、スコア加算
    public void AddScore(string moji, string tag)
    {
        // 重力異常や透明化などのディザスターが発生しているか確認
        bool isDisasterActive = false;
        if ((gravityScript != null && gravityScript.isHeavyMode) || 
            (invisibleScript != null && invisibleScript.isInvisibleMode))
        {
            isDisasterActive = true;
        }

        // 正解のターゲット（な・ん・や・て）に当たった時の処理
        if (tag == "Target")
        {
            // デバフ発生中は、点数が1.5倍になる
            int points = isDisasterActive ? 15 : 10;
            totalScore += points;

            // どの文字を拾ったか、正確にカウントしていく
            if (moji == "な") countNa++;
            else if (moji == "ん") countN++;
            else if (moji == "や") countYa++;
            else if (moji == "て") countTe++;
            
            Debug.Log($"{moji} は {points}点！");
        }
        // Otherタグの文字に当たってしまった時の処理
        else if (tag == "Other")
        {
            // デバフ中は0点、通常時は-3点
            int points = isDisasterActive ? 0 : -3;
            totalScore += points;

            // スコアがマイナスにならないようにする
            if (totalScore < 0) 
            {
                totalScore = 0;
            }
            
            Debug.Log($"{moji} は {points}点！");
        }
    }

    // 「な・ん・や・て」が何セット揃ったかを計算する
    public int GetNanyateSets()
    {
        // 全ての文字のカウントの中で、一番小さい数字が完成したセット数になる
        int sets = Mathf.Min(Mathf.Min(countNa, countN), Mathf.Min(countYa, countTe));
        
        Debug.Log($"【なんやて集計】 な:{countNa} / ん:{countN} / や:{countYa} / て:{countTe} ⇒ {sets}セットでしたぁん♪");
        return sets;
    }
}