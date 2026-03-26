using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

// <summary>
// PlayFabで一意なcustomIDのログイン処理を行うクラス
// </summary>

public class PlayFabLogin : MonoBehaviour
{
    // アカウントを新規作成するかどうか
    bool shouldCreateAccount;

    // customIDを代入しておく変数
    string customID;

    // ==============================================================
    // ログイン処理
    // ==============================================================

    void Start()
    {
        // ログイン処理の実行
        Login();
    }

    // ログインメソッド
    void Login()
    {
        // customIDを読み込む
        customID = LoadCustomID();

        // ログイン情報の代入
        var request = new LoginWithCustomIDRequest { CustomId = customID, CreateAccount = shouldCreateAccount };

        // ログイン処理
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    // ログイン成功時の処理
    void OnLoginSuccess(LoginResult result)
    {
        // 新規でアカウントを作成しようとしたが、既にcustomIDが使われていた場合
        if (shouldCreateAccount && !result.NewlyCreated)
        {
            // 再度ログインし直す
            Login();

            return;
        }

        // 新規でアカウントを作成した場合
        if (result.NewlyCreated)
        {
            // デバイスにcustomIDを保存する
            SaveCustomID();
        }

        Debug.Log("ログインに成功しました");
        Debug.Log($"CustomID：{customID}");
    }

    // ログイン失敗時の処理
    void OnLoginFailure(PlayFabError error)
    {
        Debug.Log("ログインに失敗しました");
    }

    // ==============================================================
    // customIDの取得
    // ==============================================================

    // デバイスにcustomIDを保存する場合に使うキーの設定
    // PlayerPrefsを使って保存をcustomIDの保存を行うので、そのキーの設定です
    // 詳しくは「オフラインランキングを実装する」の「PlayerPrefsって何？」をご覧ください
    static readonly string CUSTOM_ID_SAVE_KEY = "TEST_RANKING_SAVE_KEY";

    // 自分のIDを取得するメソッド
    string LoadCustomID()
    {
        // PlayerPrefsを使って、customIDを取得する
        // もし保存されていない場合は空文字を返す
        string id = PlayerPrefs.GetString(CUSTOM_ID_SAVE_KEY);

        // もしidが空文字だったらshouldCreateAccountにtrueを代入、そうでないならfalseを代入する
        shouldCreateAccount = string.IsNullOrEmpty(id);

        // shouldCreateAccountがtrueならcustomIDを新規作成、falseなら保存されていたcustomIDを返す
        return shouldCreateAccount ? GenerateCustomID() : id;
    }

    // customIDをデバイスに保存するメソッド
    void SaveCustomID()
    {
        // PlayerPrefsを使って、customIDを保存する
        PlayerPrefs.SetString(CUSTOM_ID_SAVE_KEY, customID);
    }

    // ==============================================================
    // customIDの生成
    // ==============================================================

    // customIDに使用する文字一覧（好きに設定してOKです）
    static readonly string ID_CHARACTERS = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    
    // customIDを生成するメソッド
    string GenerateCustomID()
    {
        // customIDの長さ
        int idLength = 32;

        // 生成したcustomIDを代入する変数の初期化
        StringBuilder stringBuilder = new StringBuilder(idLength);

        // customIDをランダム出力するために乱数を使う
        var random = new System.Random();

        // customIDの生成
        for (int i = 0; i < idLength; i++)
        {
            // 乱数を使ってランダムに文字列を代入する
            // 代入された文字列をcustomIDにする
            stringBuilder.Append(ID_CHARACTERS[random.Next(ID_CHARACTERS.Length)]);
        }

        // 生成したcustomIDを返す
        return stringBuilder.ToString();
    }

    
}
