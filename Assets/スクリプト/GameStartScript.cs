using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartScript : MonoBehaviour
{
    [Header("ランキングUI")]
    [SerializeField] private TMP_Text[] rankingTexts;
    [SerializeField] private TMP_Text text_myRanking;
    [SerializeField] private GameObject gameObject_ranking;

    [Header("PlayFab")]
    [SerializeField] private string statisticName = "Prime Bomb";

    [Header("UI")]
    [SerializeField] private TMP_Text text_myscore;
    [SerializeField] private Button button_gamestart;
    [SerializeField] private Button button_ranking;
    [SerializeField] private Button button_back;

    private AudioSource audioSource;
    public AudioClip titleBGM;
    public AudioClip button;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        RefreshAllUI();

        if (button_gamestart != null)
            button_gamestart.onClick.AddListener(DelayChangeScene);

        if (button_ranking != null)
            button_ranking.onClick.AddListener(Ranking);

        if (button_back != null)
            button_back.onClick.AddListener(Back);

        if (audioSource != null && titleBGM != null)
        {
            audioSource.clip = titleBGM;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void OnEnable()
    {
        // このシーンに戻ってきた時にも再更新
        RefreshAllUI();
    }

    private void RefreshAllUI()
    {
        UpdateBestScoreText();
        RefreshRankingUI();
    }

    private void RefreshRankingUI()
    {
        ClearRankingUI();
        ClearMyRankingUI();
        LoginPlayFabThenFetchRanking();
    }

    private void UpdateBestScoreText()
    {
        if (text_myscore == null) return;

        if (ScoreScript.Instance != null)
        {
            text_myscore.text = ScoreScript.Instance.MaxScore.ToString();
        }
        else
        {
            text_myscore.text = "0";
            Debug.LogWarning("ScoreScript.Instance が null です");
        }
    }

    private void LoginPlayFabThenFetchRanking()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            GetRankingTop10();
            GetMyRanking();
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
                GetRankingTop10();
                GetMyRanking();
            },
            error =>
            {
                Debug.LogError("PlayFab Login Failed: " + error.GenerateErrorReport());
            }
        );
    }

    private void GetRankingTop10()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = statisticName,
            StartPosition = 0,
            MaxResultsCount = 100
        };

        PlayFabClientAPI.GetLeaderboard(
            request,
            result =>
            {
                ClearRankingUI();

                string myId = PlayFabSettings.staticPlayer.PlayFabId;

                for (int i = 0; i < result.Leaderboard.Count && i < rankingTexts.Length; i++)
                {
                    var item = result.Leaderboard[i];
                    int rank = item.Position + 1;
                    int score = item.StatValue;

                    string name = string.IsNullOrEmpty(item.DisplayName)
                        ? item.PlayFabId
                        : item.DisplayName;

                    rankingTexts[i].text = "No." + rank + " " + name + "\nScore " + score;

                    if (item.PlayFabId == myId)
                    {
                        rankingTexts[i].color = Color.yellow;
                    }
                    else
                    {
                        rankingTexts[i].color = Color.white;
                    }
                }
            },
            error =>
            {
                Debug.LogError("ランキング取得失敗: " + error.GenerateErrorReport());
            }
        );
    }

    private void GetMyRanking()
    {
        var request = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = statisticName,
            MaxResultsCount = 1
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(
            request,
            result =>
            {
                if (text_myRanking == null)
                {
                    Debug.LogWarning("text_myRanking が未設定です");
                    return;
                }

                if (result.Leaderboard != null && result.Leaderboard.Count > 0)
                {
                    var item = result.Leaderboard[0];
                    int rank = item.Position + 1;
                    int score = item.StatValue;

                    string name = string.IsNullOrEmpty(item.DisplayName)
                        ? item.PlayFabId
                        : item.DisplayName;

                    text_myRanking.text = "My Rank : No." + rank + "\nScore " + score;
                }
                else
                {
                    text_myRanking.text = "My Rank : ランキング外";
                }
            },
            error =>
            {
                Debug.LogError("自分のランキング取得失敗: " + error.GenerateErrorReport());

                if (text_myRanking != null)
                    text_myRanking.text = "My Rank : 取得失敗";
            }
        );
    }

    private void ClearRankingUI()
    {
        if (rankingTexts == null) return;

        for (int i = 0; i < rankingTexts.Length; i++)
        {
            if (rankingTexts[i] != null)
            {
                rankingTexts[i].text = "-";
                rankingTexts[i].color = Color.white;
            }
        }
    }

    private void ClearMyRankingUI()
    {
        if (text_myRanking != null)
            text_myRanking.text = "-";
    }

    public void Ranking()
    {
        if (gameObject_ranking != null)
            gameObject_ranking.SetActive(true);

        // ランキング画面を開くたびに最新化
        RefreshAllUI();
    }

    public void Back()
    {
        if (gameObject_ranking != null)
            gameObject_ranking.SetActive(false);
    }

    public void ChangeScene()
    {
        if (audioSource != null)
            audioSource.Stop();

        if (ScoreScript.Instance != null)
        {
            ScoreScript.Instance.ResetScore();
        }
        else
        {
            Debug.LogWarning("ScoreScript.Instance が null です");
        }

        SceneManager.LoadScene("GameScene");
    }

    public void DelayChangeScene()
    {
        Invoke(nameof(ChangeScene), 0.5f);
    }

    public void OnButton()
    {
        if (audioSource != null && button != null)
        {
            audioSource.PlayOneShot(button);
        }
    }
}