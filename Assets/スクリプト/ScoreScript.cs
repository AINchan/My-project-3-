using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreScript : MonoBehaviour
{
    public static ScoreScript Instance { get; private set; }

    private const string KEY_MAX_SCORE = "MAX_SCORE";

    public int Score { get; private set; }
    public int MaxScore { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;
    

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ハイスコア読み込み
        MaxScore = PlayerPrefs.GetInt(KEY_MAX_SCORE, 0);

        // シーン切り替え時イベント
        SceneManager.sceneLoaded += OnSceneLoaded;

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // シーンが読み込まれたらScoreTextタグを探して自動アタッチ
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var go = GameObject.FindWithTag("ScoreText");

        if (go != null)
        {
            var tmp = go.GetComponent<TextMeshProUGUI>();

            if (tmp != null)
            {
                scoreText = tmp;
                UpdateUI();
                Debug.Log("ScoreText 自動アタッチ");
            }
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveMaxScore();
    }

    private void OnApplicationQuit()
    {
        SaveMaxScore();
    }

    // スコア加算
    public void AddScore(int amount)
    {
        Score += amount;

        if (Score > MaxScore)
        {
            MaxScore = Score;
            SaveMaxScore();
        }

        UpdateUI();
    }

    // スコアリセット
    public void ResetScore()
    {
        Score = 0;
        UpdateUI();
    }

    // 手動バインド（必要なら）
    public void BindText(TextMeshProUGUI newText)
    {
        scoreText = newText;
        UpdateUI();
    }

    // ハイスコア保存
    public void SaveMaxScore()
    {
        PlayerPrefs.SetInt(KEY_MAX_SCORE, MaxScore);
        PlayerPrefs.Save();
    }

    // デバッグ用
    public void ClearMaxScoreForDebug()
    {
        PlayerPrefs.DeleteKey(KEY_MAX_SCORE);
        PlayerPrefs.Save();
        MaxScore = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = Score.ToString();
        }

        Debug.Log("スコア書き換え → " + Score);
    }
}