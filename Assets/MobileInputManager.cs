using UnityEngine; // Unityの基本機能を使うための宣言
using UnityEngine.UI; // ImageやCanvasGroupを操作するための宣言
using System.Collections; // コルーチンを使うための宣言
using UnityEngine.EventSystems; // ボタンなどのUIイベントを管理するための宣言

// モバイル操作UIの表示・非表示と入力を管理するクラス
public class MobileInputManager : MonoBehaviour 
{
    // インスペクター上での操作を分かりやすくグループ化する工夫
    [Header("フェードさせるモバイルボタンの親グループ")]
    [SerializeField] private CanvasGroup mobileCanvasGroup; // 透明度を一括管理

    [Header("START画像")]
    [SerializeField] private GameObject startImage; // 冒頭のSTART演出

    [Header("くもぼうやの移動スクリプト")]
    [SerializeField] private PlayerController playerController; // プレイヤーに移動指示を送る

    [Header("デバッグ設定")]
    [SerializeField] private bool showMobileControlsInEditor = true; // PC上でのテスト時にボタンを出すかどうか

    private float targetAlpha = 1f; // 目指す透明度の値

    // 初期設定
    private void Start() 
    {
        if (mobileCanvasGroup == null) return; // 参照がなければ何もしない

        bool isMobile = Application.isMobilePlatform; // 実行環境がモバイルかどうかを判定

        #if UNITY_EDITOR // Unityエディタ上での動作設定
            mobileCanvasGroup.gameObject.SetActive(showMobileControlsInEditor);
        #else // 実際にビルドして遊ぶ時の動作設定
            if (isMobile)
            {
                mobileCanvasGroup.gameObject.SetActive(true); // スマホならON
            }
            else
            {
                mobileCanvasGroup.gameObject.SetActive(false); // PCならOFF
                return; 
            }
        #endif

        mobileCanvasGroup.alpha = 1f; // 最初はハッキリ見せる
        targetAlpha = 1f;

        StartCoroutine(WatchStartImage()); // START画面が消えるのを待つ
    }

    // START画像が消えたら操作UIを半透明に
    IEnumerator WatchStartImage()
    {
        targetAlpha = 1f; 
        mobileCanvasGroup.alpha = 1f;

        // START画像が表示されている間は待機
        while (startImage != null && startImage.activeInHierarchy)
        {
            yield return null;
        }

        // STARTが消えてから1秒待って半透明化
        yield return new WaitForSeconds(1.0f);
        targetAlpha = 0.3f; // プレイの邪魔にならないよう薄くする
    }

    // 毎フレームの更新処理
    private void Update() 
    {
        if (mobileCanvasGroup != null)
        {
            // フェード
            mobileCanvasGroup.alpha = Mathf.MoveTowards(
                mobileCanvasGroup.alpha, 
                targetAlpha, 
                Time.deltaTime * 1.5f 
            );
        }
    }

    // Mobile版のボタンのトリガー
    public void MobileMoveLeft(bool isDown)
    {
        if (playerController != null) playerController.SetMobileLeftInput(isDown);
    }

    public void MobileMoveRight(bool isDown)
    {
        if (playerController != null) playerController.SetMobileRightInput(isDown);
    }
    
    // チャージ開始
    public void MobileChargeStart() 
    { 
        if (playerController != null) playerController.SetMobileThrowInput(true);
    }

    // チャージ完了・投げる
    public void MobileChargeEnd() 
    { 
        if (playerController != null) playerController.SetMobileThrowInput(false);
    }

    // ボタンが押されている間、少し色を暗くする
    private void UpdateKeyColor(GameObject buttonObj, bool isPressed)
    {
        if (buttonObj == null) return;
        
        Image img = buttonObj.GetComponent<Image>();
        if (img != null)
        {
            // 押されたら灰色、離したら白に戻す
            img.color = isPressed ? new Color(0.7f, 0.7f, 0.7f, 1.0f) : Color.white;
        }
    }
}