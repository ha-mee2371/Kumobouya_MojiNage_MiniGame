using UnityEngine; // Unityの基本クラスを使用するための宣言
using UnityEngine.UI; // Slider（スライダー）やImage（画像）を操作するための宣言

// パワーゲージの溜まり具合に応じて色を変化させるクラス
public class PowerGauge : MonoBehaviour 
{
    public Slider slider; // 監視対象となるUIスライダーの参照
    public Image fillImage; // スライダーの中身の画像の参照

    // ゲームが開始された時に一度だけ呼ばれる
    void Start() 
    {
        // 1スライダーの値が変わった時に、特定の関数を呼び出すように予約する
        if (slider != null)
        {
            // 値が変わるたびにOnSliderChanged関数を実行
            slider.onValueChanged.AddListener(OnSliderChanged);
            
            // 最初の一回目も、現在の値に合わせて正しく色を塗っておく
            OnSliderChanged(slider.value); 
        }
    }

    // スライダーの値が変わった時だけ、自動的に実行される関数
    void OnSliderChanged(float val)
    {
        // もし fillImage が設定されていなければ何もしない
        if (fillImage == null) return;

        // 溜まり具合に応じて、ゲージの色を3段階で塗り替えるath
        if (val < 0.33f) // 33%未満
        {
            fillImage.color = Color.green; 
        }
        else if (val < 0.66f) // 66%未満
        {
            fillImage.color = Color.yellow; 
        }
        else //66%以上
        {
            fillImage.color = Color.red; 
        }
    }
}