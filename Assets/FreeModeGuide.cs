using UnityEngine;
using System.Collections;
using TMPro; // テキストを使う場合

// フリーモード開始時に操作説明をフェードアウトで表示するクラス
public class FreeModeGuide : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // 透過度を操るためのコンポーネントを取得
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 最初は隠しておく
        canvasGroup.alpha = 0f;
    }

    // 他のスクリプトからこの関数を呼んで表示を開始する
    public void ShowGuide(float displayTime = 5.0f, float fadeDuration = 1.0f)
    {
        StopAllCoroutines(); // 二重発動防止
        StartCoroutine(FadeRoutine(displayTime, fadeDuration));
    }

    private IEnumerator FadeRoutine(float displayTime, float fadeDuration)
    {
        // 1. パッと表示
        canvasGroup.alpha = 1f;

        // 2. 5秒待機
        yield return new WaitForSeconds(displayTime);

        // 3. 徐々に消えていく（フェードアウト）
        float currentTime = 0f;
        while (currentTime < fadeDuration)
        {
            currentTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, currentTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}