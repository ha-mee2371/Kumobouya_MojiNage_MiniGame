using UnityEngine; // Unityの基本クラスを使用するための宣言
using UnityEngine.UI; // スライダーなどのUIコンポーネントを操作するための宣言
using TMPro; // TextMeshProを操作するための宣言

//くもぼうやのすべて
public class PlayerController : MonoBehaviour 
{
    public float moveSpeed = 5.0f; // ぼうやの移動スピード

    [Header("移動制限の設定")]
    public float limitLeftX = 8.0f; // 左の壁
    public float limitRightX = 2.0f; // 右の壁

    public Transform leftHand; // 左手の位置の参照
    public Transform rightHand; // 右手の位置の参照

    [Header("UIの設定")]
    public Slider powerSlider; // チャージ具合を表示するゲージの参照

    private GameObject leftMoji = null; // 現在左手に持っている文字
    private GameObject rightMoji = null; // 現在右手に持っている文字h
    private int nextHandToShoot = 0; // 次にどちらの手から投げるかを決めるフラグ（0:左、1:右）

    private float shotPower = 0f; // 現在どれくらいパワーを溜めているか
    private bool isCharging = false; // パワーを溜めている最中か

    private bool isMobileLeft = false; // モバイルで左ボタンが押されているか
    private bool isMobileRight = false; // モバイルで右ボタンが押されているか
    private bool isMobileCharging = false; // モバイルで投げボタンが押されているか

    // ゲーム開始時の初期設定
    void Start() 
    {
        if (powerSlider != null) powerSlider.value = 0f; // スライダーを空に
    }

    // 毎フレームの入力監視と移動処理
    void Update() 
    {
        // 移動処理
        float h = Input.GetAxis("Horizontal"); // PCのキー入力（左右）を受け取る
        if (isMobileLeft) h = -1f; // モバイルボタンが優先される
        if (isMobileRight) h = 1f;

        // 計算した方向へ移動させる
        transform.Translate(Vector3.right * h * moveSpeed * Time.deltaTime);

        // ぼうやが画面外に冒険しないように制限
        float clampedX = Mathf.Clamp(transform.position.x, -limitLeftX, limitRightX);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

        // ポーズ中はチャージを強制リセットしてズルを防止
        if (Time.timeScale == 0) 
        {
            if (isCharging || isMobileCharging)
            {
                isCharging = false;
                isMobileCharging = false;
                shotPower = 0f;
                if (powerSlider != null) powerSlider.value = 0f;
            }
            return; 
        }

        // チャージ開始
        if (Input.GetKeyDown(KeyCode.Space) || (isMobileCharging && !isCharging))
        {
            // 何か文字を持っていればチャージを開始
            if (leftMoji != null || rightMoji != null)
            {
                isCharging = true;
                shotPower = 0f;
                Debug.Log("ぐぬぬぬぬぬ！");
            }
        }

        // チャーーーーーーーーージ！
        if (isCharging)
        {
            shotPower += Time.deltaTime * 0.8f; // 時間とともにパワーが溜まる
            if (shotPower > 1.0f) shotPower = 1.0f; // 100%が限界

            if (powerSlider != null) powerSlider.value = shotPower; // ゲージに反映

            // ボタンが離されたか判定して、投げ飛ばす
            bool spaceUp = Input.GetKeyUp(KeyCode.Space);
            bool mobileUp = (isMobileCharging == false && !Input.GetKey(KeyCode.Space));

            if (spaceUp || mobileUp)
            {
                isCharging = false;
                Shoot(shotPower); // とんでけ～～～
                shotPower = 0f;

                if (powerSlider != null) powerSlider.value = 0f;
            }
        }
    }

    // モバイルUIから入力を受け取る
    public void SetMobileLeftInput(bool isDown) { isMobileLeft = isDown; }
    public void SetMobileRightInput(bool isDown) { isMobileRight = isDown; }
    public void SetMobileThrowInput(bool isDown) { isMobileCharging = isDown; }

    // 文字を拾う処理
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 降ってきた文字にぶつかったら……
        if (collision.gameObject.name.Contains("Moji"))
        {
            if (leftMoji == null) // 左手が空いていれば左手に
            {
                HoldMoji(collision.gameObject, leftHand, out leftMoji);
                if (rightMoji == null) nextHandToShoot = 0; // 両方空なら次は左から投げる
                return;
            }
            else if (rightMoji == null) // 右手が空いていれば右手に
            {
                if (collision.gameObject != leftMoji) // 同じ文字を両手で持たないようにする
                {
                    HoldMoji(collision.gameObject, rightHand, out rightMoji);
                }
            }
        }
    }

    // 文字を手に固定する
    void HoldMoji(GameObject moji, Transform hand, out GameObject handVar)
    {
        handVar = moji;
        moji.layer = LayerMask.NameToLayer("HoldMoji"); // レイヤーを変えて衝突を制御

        Rigidbody2D rb = moji.GetComponent<Rigidbody2D>();
        rb.simulated = false; // 一旦物理を止めて……

        moji.transform.SetParent(hand, true); 
        moji.transform.localPosition = Vector3.zero; // 手にピタッと吸着
        moji.transform.localRotation = Quaternion.identity;

        // 物理的に動かないようにキネマティックに設定
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        rb.simulated = true;
        Debug.Log(moji.GetComponentInChildren<TextMeshPro>().text + " を手にいれたくもぼうや");
    }

    // 5. 投げ飛ばす処理
    void Shoot(float power)
    {
        GameObject target = null;

        // 左右の手から交互に、あるいは持っている方の手から選ぶ
        if (nextHandToShoot == 0)
        {
            if (leftMoji != null) { target = leftMoji; leftMoji = null; nextHandToShoot = 1; }
            else if (rightMoji != null) { target = rightMoji; rightMoji = null; }
        }
        else
        {
            if (rightMoji != null) { target = rightMoji; rightMoji = null; nextHandToShoot = 0; }
            else if (leftMoji != null) { target = leftMoji; leftMoji = null; }
        }

        if (target != null)
        {
            target.transform.SetParent(null); // 親子関係を解除
            target.transform.localScale = Vector3.one;
            target.transform.position = transform.position + new Vector3(1.2f, 1.5f, 0); // 少し斜め上から発射

            Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic; // 物理挙動をダイナミックに戻す
            rb.constraints = RigidbodyConstraints2D.None;
            rb.freezeRotation = false;
            target.GetComponent<Collider2D>().enabled = true;

            rb.gravityScale = 1.0f; // 重力を戻す
            Vector2 shootDirection = new Vector2(1.0f, 1.3f); // 右斜め上に向かって
            float finalForce = 100f + (power * 350f); // 溜めたパワーで勢いを変える

            rb.AddForce(shootDirection * finalForce); // 物理的な力を加えてぶん投げ
            Debug.Log("ぶん投げぼうやは" + (int)(power * 100) + "%の力を発揮した");
        }
    }
}