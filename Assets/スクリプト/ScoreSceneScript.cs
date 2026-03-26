using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class ScoreSceneScript : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button_gamestart;
    [SerializeField] private Button button_title;
    [SerializeField] private TMP_Text text_score;
    [SerializeField] private TMP_Text text_maxScore;

    [Header("PlayFab")]
    [SerializeField] private string statisticName = "Prime Bomb";

    private AudioSource audioSource;
    public AudioClip scoreBGM;

    public AudioClip button;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (ScoreScript.Instance == null)
        {
            Debug.LogError("ScoreScript.Instance が見つかりません。");
            return;
        }

        int s = ScoreScript.Instance.Score;
        int max = ScoreScript.Instance.MaxScore;

        if (text_score != null) text_score.text = s.ToString();
        if (text_maxScore != null) text_maxScore.text = max.ToString();

        if (button_gamestart != null)
            button_gamestart.onClick.AddListener(DelayChangeGameScene);

        if (button_title != null)
            button_title.onClick.AddListener(DelayChangeTitleScene);

        LoginPlayFabThenSubmit(max);

        if (audioSource != null && scoreBGM != null)
        {
            audioSource.clip = scoreBGM; 
            audioSource.loop = true;     
            audioSource.Play();
        }
    }

    private void LoginPlayFabThenSubmit(int scoreToSubmit)
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            SubmitScore(scoreToSubmit);
            return;
        }

        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(
            request,
            result =>
            {
                Debug.Log("PlayFab Login OK");
                SubmitScore(scoreToSubmit);
            },
            error =>
            {
                Debug.LogError("PlayFab Login Failed: " + error.GenerateErrorReport());
            }
        );
    }

    private void SubmitScore(int score)
    {
        var statisticUpdate = new StatisticUpdate
        {
            StatisticName = statisticName,
            Value = score
        };

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> { statisticUpdate }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(
            request,
            _ => Debug.Log("スコア送信成功"),
            error => Debug.LogError("スコア送信失敗: " + error.GenerateErrorReport())
        );
    }

    public void OnButton()
    {
        audioSource.PlayOneShot(button);
    }

    public void ChangeGameScene()
    {
        audioSource.Stop();
        ScoreScript.Instance.ResetScore();
        SceneManager.LoadScene("GameScene");
    }

    public void DelayChangeGameScene()
    {
        Invoke("ChangeGameScene",0.5f);
    }

    public void ChangeTitleScene()
    {
        audioSource.Stop();
        ScoreScript.Instance.ResetScore();
        SceneManager.LoadScene("TitleScene");
    }

    public void DelayChangeTitleScene()
    {
        Invoke("ChangeTitleScene",0.5f);
    }
}