using UnityEngine; // Unityの基本機能を使うための宣言

public class MojiSpawner : MonoBehaviour // 文字（MojiPrefab）を自動で生成し続けるのクラス
{
    public GameObject mojiPrefab; // 生成する文字の元となるプレハブ
    public float spawnInterval = 1.0f; // 文字生成の間隔（秒）

    // ゲームが始まった瞬間に呼ばれる
    void Start() 
    {
        // SpawnMojiという関数を、0秒後から、spawnInterval秒ごとに繰り返し実行
        InvokeRepeating("SpawnMoji", 0, spawnInterval);
    }

    // 文字を生成する実際の処理
    void SpawnMoji()
    {
        // 画面の左側から右側の間で、ランダムなX座標を決める
        float randomX = Random.Range(-8.0f, -1.0f);
        
        // 生成する高さはSpawnerがある場所と同じ
        Vector3 spawnPos = new Vector3(randomX, transform.position.y, 0);
        
        // 決まった場所にプレハブから新しい文字を生成する
        Instantiate(mojiPrefab, spawnPos, Quaternion.identity);
    }

    // ゲーム終了時や特定のイベントで、文字降りを止めたい時に呼ぶ関数
    public void StopSpawning()
    {
        // 繰り返し実行していたSpawnMojiをキャンセル
        CancelInvoke("SpawnMoji"); 
        Debug.Log("文字降りを強制終了");
    }
}