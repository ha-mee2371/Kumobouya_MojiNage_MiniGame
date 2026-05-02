using UnityEngine; // Unityの基本機能を使うための宣言
using UnityEngine.SceneManagement; // Sceneを切り替える宣言

public class TitleManager : MonoBehaviour
{
    public void GoToModeSelect()
    {
        SceneManager.LoadScene("ModeSelect");
    }

    public void GoToReadme()
    {
        SceneManager.LoadScene("ReadmeWarning");
    }

    public void GoToInformation()
    {
        SceneManager.LoadScene("Information");
    }
}