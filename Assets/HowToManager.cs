using UnityEngine; // Unityの基本クラスを使用するための宣言

// 遊び方画面の表示切り替えを管理するクラス
public class HowToManager : MonoBehaviour 
{
    public GameObject page1;
    public GameObject page2;
    public GameObject HowToCanvas; 

    // 遊び方を開く
    public void OpenHowTo() {
        HowToCanvas.SetActive(true);
        ShowPage1();
    }

    // 1枚目を表示
    public void ShowPage1() {
        page1.SetActive(true);
        page2.SetActive(false);
    }

    // 2枚目を表示
    public void ShowPage2() {
        page1.SetActive(false);
        page2.SetActive(true);
    }

    // 遊び方を閉じる
    public void CloseHowTo() {
        HowToCanvas.SetActive(false);
    }
}