using UnityEngine; // Unityの基本クラスを使用するための宣言

public class GyurururuSpin : MonoBehaviour // 対象を高速回転させる演出用クラス
{
    public GameObject targetBouya; // 回転させる対象となるオブジェクトの参照

    public float spinSpeed = 600f; // 1秒間にどれだけ回転させるかの速度設定

    private bool isSpinning = false; // 現在回転中かどうかを管理するフラグ

    // 物理演算と同じタイミングで実行される更新関数。
    void FixedUpdate()
    {
        // 回転フラグがONで、かつ回転対象が存在する場合に処理を行う
        if (isSpinning && targetBouya != null)
        {
            // Z軸を中心に、時間経過に合わせて回転
            targetBouya.transform.Rotate(0, 0, spinSpeed * Time.fixedDeltaTime);
        }
    }

    // 外部から回転をスタートさせるための関数
    public void StartSpinning()
    {
        isSpinning = true;
    }

    // 外部から回転をストップさせるための関数
    public void StopSpinning()
    {
        isSpinning = false; 
        if (targetBouya != null)
        {
            // 回転角度を0にリセットして、ぼうやを正しい向きに戻す
            targetBouya.transform.rotation = Quaternion.identity;
        }
    }
}