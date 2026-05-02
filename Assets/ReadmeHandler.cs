using UnityEngine; // Unityの基本クラスを使用するための宣言
using UnityEngine.SceneManagement; // タイトル画面に戻るために必要

// GitHubのReadmeを開いたり、シーンを移動したりする管理クラス
public class ReadmeHandler : MonoBehaviour 
{
    // 開くボタンが押された時に実行される関数
    public void OpenGithubLink()
    {
        // ビルド済みの方のREADME。開発の方のURLは左記READMEの中に記載。
        string url = "https://github.com/ha-mee2371/Portfolio/tree/main/Kumobouya_Mojinage_Minigame#readme"; 
        
        // ブラウザを立ち上げて、指定したURLを自動で開く
        Application.OpenURL(url);
        
        Debug.Log("くもぼうやはGithubを覗きに行きました。");
    }

    // 戻るボタンなどでタイトル画面に帰還するための関数
    public void GoToTitle()
    {
        // ロード画面を挟まず、一瞬でタイトルシーンへ切り替える
        SceneManager.LoadScene("TitleScene");
    }
}