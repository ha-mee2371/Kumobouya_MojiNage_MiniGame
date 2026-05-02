using UnityEngine; // Unityの基本クラスを使用するための宣言

public partial class BarGenerator : MonoBehaviour // 障害物をランダムに生成するクラスの定義
{
    // インスペクター上での操作を分かりやすくグループ化する工夫
    [Header("生成するプレハブのリスト")]
    public GameObject barPrefab1; // ー
    public GameObject barPrefab2; // 六角形

    public int minBars = 10; // 1回に生成する最小の個数
    public int maxBars = 15; // 1回に生成する最大の個数
    
    // 生成する範囲を指定
    public Vector2 xRange = new Vector2(-8f, 0f);
    public Vector2 yRange = new Vector2(-2f, 5f);

    void Start() // ゲーム開始時に一度だけ呼ばれるイベント関数
    {
        GenerateBars(); // 障害物生成処理を実行
    }

    void GenerateBars() // 実際に障害物を配置するメインの関数
    {
        int barCount = Random.Range(minBars, maxBars + 1); // 指定範囲内でランダムな個数を決定する
        Debug.Log($"[BarGenerator] Count determined: {barCount} bars.");

        for (int i = 0; i < barCount; i++) // 決まった個数分、ループで生成処理を回す
        {
            GameObject selectedPrefab; // プレハブを格納する変数
            
            // どちらのプレハブを使うか抽選
            if (Random.value > 0.2f) { // 0.2より大きい（＝80%）ならprefab1を選ぶ確率計算
                selectedPrefab = barPrefab1; // ー
            } 
            else {
                selectedPrefab = barPrefab2; // 六角形
            }

            Vector3 spawnPos = new Vector3 // 生成場所を決定するための座標データを作成
            (
                Random.Range(xRange.x, xRange.y),
                Random.Range(yRange.x, yRange.y),
                0 // 2Dゲームなので奥行き（Z）は0固定
            );

            // 選ばれた方の棒を生成
            GameObject newBar = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
            Debug.Log($"[BarGenerator] Spawned {selectedPrefab.name} at {spawnPos}");

            // 長さ（Scale）の設定
            if (selectedPrefab == barPrefab2) // 六角形が選ばれたかどうかの判定
            {
                // 六角形にはなにもしない
            }
            else 
            {
                float randomWidth = Random.Range(1.0f, 2.0f); // 横幅をランダムに変える
                newBar.transform.localScale = new Vector3(randomWidth, 0.1f, 1.0f); // スケールを適用して形を変形させる
            }

            // 回転速度の設定
            float randomSpeed = Random.Range(100f, 200f) * (Random.value > 0.5f ? 1f : -1f); // 速度と左右どちらに回るかをランダム決定
            var script = newBar.GetComponent<YokobouRotateObstacle>(); // 生成した物に回転用スクリプトがついているか確認
            if(script != null) { // スクリプトが見つかった場合のみ処理し、クラッシュを防ぐ
                script.rotateSpeed = randomSpeed; // ランダムに決まった回転速度をスクリプトに渡す
                Debug.Log($"[BarGenerator] Assigned Speed: {randomSpeed} to {newBar.name}");
            }
        }
    }
}