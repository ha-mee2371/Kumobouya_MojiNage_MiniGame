using UnityEngine; // Unityの基本クラスを使用するための宣言
using TMPro; // TextMeshPro（InputFieldなど）を操作するための宣言
using System.Text.RegularExpressions; // 正規表現（Regex）という強力な文字列操作機能を使うための宣言

public class HiraganaValidator : MonoBehaviour // 入力された文字を「ひらがな」だけに強制する
{
    public TMP_InputField nameInputField; // 監視対象となる入力欄の参照

    // オブジェクトが生成された時に一度だけ呼ばれる
    void Start() 
    {
        // 入力欄の中身が書き換わるたびに「OnValueChanged」という関数を呼ぶ
        nameInputField.onValueChanged.AddListener(OnValueChanged);
    }

    // 文字が入力・変更されるたびに実行されるメインロジック
    public void OnValueChanged(string input)
    {
        string validated = Regex.Replace(input, @"[^ぁ-んー]", ""); // ひらがな以外を受け付けない

        // もし、元の入力と「ひらがなのみ」にした後の文字列が違うなら（＝ひらがな以外が含まれていたなら）
        if (validated != input)
        {
            // 入力欄の文字を強制的に「ひらがなのみ」の文字列に書き換える
            nameInputField.text = validated;
        }
    }
}