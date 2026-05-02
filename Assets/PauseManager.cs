using UnityEngine; // Unityの基本クラスを使用するための宣言

// ゲームの一時停止（ポーズ）と再開を管理する司令塔クラス
public class PauseManager : MonoBehaviour 
{
    private bool isPaused = false; // 現在ゲームが止まっているかどうかを記憶する
    public GameObject pauseUIPanel; // 一時停止中に表示するメニュー画面の参照

    // ゲームが始まった瞬間に呼ばれる初期化処理
    void Start()
    {
        // ゲーム開始時は必ず動いている状態からスタートさせる
        isPaused = false;
        Time.timeScale = 1f; // ゲーム内の時間の進み方を通常に設定

        // ポーズメニューが表示されたままなら隠しておく
        if (pauseUIPanel != null)
        {
            pauseUIPanel.SetActive(false);
        }
    }

    // ボタンが押されるたびに停止と再開を切り替える
    public void TogglePause()
    {
        // 現在の状態を反転させる（trueならfalseに、falseならtrueに！）
        isPaused = !isPaused;

        if (isPaused) // 停止状態になった時の処理
        {
            Time.timeScale = 0f; // ゲーム内の時間を完全停止にする
            if (pauseUIPanel != null) pauseUIPanel.SetActive(true); // メニュー画面を表示する
            Debug.Log("くもぼうや休憩中"); 
        }
        else 
        {
            Time.timeScale = 1f; // 時間の進みを元に戻す
            if (pauseUIPanel != null) pauseUIPanel.SetActive(false); // メニュー画面を消す
            Debug.Log("くもぼうや大忙し"); 
        }
    }
}