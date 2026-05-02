using UnityEngine; // Unityの基本機能を使うための宣言

// 横棒の障害物を回転させるための専用クラス
public class YokobouRotateObstacle : MonoBehaviour 
{
    // 回転するスピードの設定
    public float rotateSpeed = 100f; 

    void Update()
    {
        // 毎フレーム、Z軸を中心に回転させる
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
}