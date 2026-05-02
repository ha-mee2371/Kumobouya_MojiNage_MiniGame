using UnityEngine; // Unityの基本クラスを使用するための宣言

// ゴールを上下にゆらゆらと動かして、難易度を演出するクラス
public class MovingGoal : MonoBehaviour 
{
    public float moveRange = 1.5f; // 中心位置から上下にどれくらいの距離動くか
    public float moveSpeed = 0.5f; // 動くはやさの設定

    private Vector3 startPos; // 動きの基準となる、ゲーム開始時の初期位置を保存

    // オブジェクトが生成された時に一度だけ呼ばれる初期設定
    void Start() 
    {
        startPos = transform.position; // 最初の位置をメモ
    }

    void Update() // 毎フレーム、位置を計算して更新する処理
    {
        // Mathf.Sin（サイン関数）を使って、-1から1の間で変化する波を作る
        float newY = startPos.y + Mathf.Sin(Time.time * moveSpeed) * moveRange;
        
        // 計算した新しいY座標を使って、ゴールの位置を更新する
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}