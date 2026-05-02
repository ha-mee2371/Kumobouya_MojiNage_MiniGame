using UnityEngine; // Unityの基本機能を使うための宣言
using UnityEngine.UI; // ImageコンポーネントなどUI操作に必要な宣言

public class ColorSelector : MonoBehaviour // キャラクターの色選択UIを制御するクラス
{    
    public GameObject Kusa; // 選択状態を示すUI（草のアイコン）の参照
    private Vector3 offsetPos; // 上の草の相対的な位置を保持する

    void Start() // ゲーム開始時に呼ばれる初期化処理
    {
        // エディタ上で調整した相対位置を記憶
        offsetPos = Kusa.transform.localPosition; // 初期状態のズレ（オフセット）を保存し、再配置時に再現する

        // ゲーム開始時は緑（Mido）に移動して、色も覚えさせる
        GameObject defaultKnob = GameObject.Find("Mido");
        if (defaultKnob != null)
        {
            // 位置を合わせるだけでなく、色を覚えさせる処理を呼ぶ
            SetMePosition(defaultKnob);
        }
        else
        {
            Debug.LogError("くもぼうやが初期色を認識できませんでした。このまま任意色が選択されなかった場合、デバッグ色で開始します。");
        }
    }
   
   // ボタンがクリックされた時にインスペクターから呼ばれるイベント関数
    public void OnColorSelected(GameObject clickedKnob)
    {
        SetMePosition(clickedKnob); // クリックされた画像の位置と色を反映させる
    }

    // 指定されたknobにUIを移動させ、データを同期する核心の関数
    void SetMePosition(GameObject knob)
    {
        Kusa.transform.SetParent(knob.transform); // 選択UI（草）をknobの子要素にする
        Kusa.transform.localPosition = offsetPos; // 保存しておいた相対座標を代入して、常に同じ位置関係で表示する

        Image knobImage = knob.GetComponent<Image>(); // knobが持っている色情報を取得する
        if (knobImage != null) // Imageが存在するかチェックする
        {
            if (GameManager.instance != null) // シングルトンのGameManagerが存在するか確認
            {
                GameManager.instance.selectedColor = knobImage.color; // モード選択画面で選んだ色をSceneを跨いで保持できるようGameManagerへ渡す
                Debug.Log($"くもぼうやが{knobImage.color}を持ちました。");
            }
            else 
            {
                // もしいなくても、エラーを出さずに教えてくれる
                Debug.LogWarning("くもぼうやが色を持てなくて泣いています。");
            }
        }
    }
}