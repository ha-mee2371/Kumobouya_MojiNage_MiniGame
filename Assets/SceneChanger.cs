using UnityEngine; // Unityの基本クラスを使用するための宣言
using UnityEngine.SceneManagement; // シーン移動

public class SceneChanger : MonoBehaviour
{
    public void GoToNormalMode()
    {
        SceneManager.LoadScene("GameScene"); 
    }

    public void GoToFreeMode()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("FreeModeScene");
    }

    public void GoToTitle()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("TitleScene");
    }
}