using UnityEngine; // Unityの基本クラスを使用するための宣言
using TMPro; // UIテキスト（TextMeshPro）を操作するための宣言
using System.Collections; // コルーチン（IEnumerator）を使用するための宣言
using UnityEngine.UI; // Imageコンポーネントなどを操作するための宣言

// 「なんやてポイント」の集計と演出を管理するクラス
public class FreeModeNanyateManager : MonoBehaviour 
{
    // インスペクター上での操作を分かりやすくグループ化する工夫
    [Header("UI設定")] 
    public TextMeshProUGUI pointText; // ポイントを表示するテキストUIの参照
    public RectTransform spawnPoint; // 演出用オブジェクトが出現する基準位置

    [Header("演出設定")] 
    public GameObject kumoPrefab; // 噴き出す演出用オブジェクト（プレハブ）の参照
    public Canvas canvas; // 演出を表示するためのキャンバスの参照
    public Sprite[] kumoSprites; // 演出時にランダムで切り替える画像リスト

    private float currentPoints = 0f; // 現在蓄積されているポイントの内部数値

    // 開始時にUIを初期化
    void Start() { UpdateUI(); } 

    // ポイントを獲得する際の処理。タグで判定する。
    public void CollectPoint(string targetTag) 
    {
        if (targetTag == "Target") currentPoints += 0.3f; // 特定のターゲットなら0.3加算
        else if (targetTag == "Other") currentPoints += 0.1f; // それ以外なら0.1加算
        UpdateUI(); // 数値が変わったら表示を更新
    }

    // 演出発動ボタンが押された時の処理
    public void OnNanyateButtonClick() 
    {
        if (currentPoints >= 10f) // 10ポイント以上あるかチェックする
        {
            currentPoints -= 10f; // ポイントを消費
            UpdateUI(); // 消費後の数値を反映
            StartCoroutine(SpawnKumoRoutine()); // 噴き出し演出を非同期で開始
            Debug.Log("なんやてポイントを消費してくもぼうやポップコーンを発動！");
        }
    }

    // UIの表示を一括更新するヘルパー関数
    void UpdateUI() 
    {
        if (pointText != null) // 参照エラーを防ぐ
        {
            pointText.text = currentPoints.ToString("F1") + "なんやて"; // 小数点第1位まで表示
        }
    }

    // 複数の演出用オブジェクトを時間差で生成するループ
    IEnumerator SpawnKumoRoutine() 
    {
        for (int i = 0; i < 4; i++) // 4つのオブジェクトを生成
        {
            GameObject kumo = Instantiate(kumoPrefab, canvas.transform); // 指定したキャンバスの子として生成
            RectTransform rt = kumo.GetComponent<RectTransform>(); // UI位置操作のためにRectTransformを取得

            rt.position = spawnPoint.position; // 出現位置をボタンの位置などに合わせる

            Vector3 pos = rt.localPosition; // Z座標のズレを防ぐ
            pos.z = 0; // 2D演出なのでZは0
            rt.localPosition = pos; // 座標を再設定

            rt.localScale = new Vector3(0.3f, 0.3f, 1f); // 演出サイズを調整

            if (kumoSprites.Length > 0) // 画像リストが空でないか確認
            {
                kumo.GetComponent<Image>().sprite = kumoSprites[Random.Range(0, kumoSprites.Length)]; // リストからランダムに画像を選ぶ
            }

            StartCoroutine(KumoPhysics(kumo)); // 生成した個体に物理挙動を与えるコルーチンを個別に開始
            yield return new WaitForSeconds(0.1f); // 少しずつずらして噴き出させる演出
        }
    }

    // 1つの演出個体の物理挙動をシミュレーションする関数
    IEnumerator KumoPhysics(GameObject kumo) 
    {
        RectTransform rt = kumo.GetComponent<RectTransform>(); // 制御対象の座標取得
        
        // 初速度をランダムに決定し、バラバラに飛び散るようにする
        Vector2 velocity = new Vector2(Random.Range(-400f, 400f), Random.Range(400f, 800f)); 
        float gravity = -1500f; // UI座標系に合わせた独自の重力値
        float rotateSpeed = Random.Range(-500f, 500f); // 回転速度もランダムにして躍動感を出す

        float timer = 0; // 生存時間の計測開始
        while (timer < 3f) // 3秒間だけ物理演算を継続する
        {
            velocity.y += gravity * Time.deltaTime; // 重力を速度に加算
            rt.anchoredPosition += velocity * Time.deltaTime; // 速度を座標に加算
            rt.Rotate(0, 0, rotateSpeed * Time.deltaTime); // 回転を適用
            timer += Time.deltaTime; // 時間経過を加算
            yield return null; // 1フレーム待機して滑らかに動かす
        }
        Destroy(kumo); // 演出終了後にメモリを解放
    }
}