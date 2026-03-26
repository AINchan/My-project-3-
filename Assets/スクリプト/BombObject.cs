using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class BombObject : MonoBehaviour, IMergeNumber
{
    private const string BGM_VOLUME_KEY = "BGM_VOLUME";

    [SerializeField] private TMP_Text text;

    private int number;
    private BombScript bomb;
    public Animator animator;

    [SerializeField] private AudioClip gattaiSound;
    [SerializeField] private AudioClip deleteSound;

    void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TMP_Text>(true);

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void SetBomb(BombScript b)
    {
        bomb = b;
    }

    public void SetNumber(int value)
    {
        number = value;

        if (text != null)
            text.text = number.ToString();

        // 素数なら消える
        if (IsPrime(number))
        {
            float volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.5f);

            if (deleteSound != null)
            {
                // 音を再生
                AudioSource.PlayClipAtPoint(deleteSound, Camera.main.transform.position, volume);
            }

            ScoreScript.Instance?.AddScore(number);

            Destroy(gameObject);
            return;
        }

        UpdateScale();

        // ゲームオーバー処理
        if (number >= 100)
        {
            if (animator != null)
                animator.SetBool("爆発", true);

            if (bomb != null)
            {
                bomb.gameover = true;
                bomb.gamestart = false;
            }

            Invoke(nameof(Effect), 2.9f);
            Invoke(nameof(HideBomb), 3.0f);
            Invoke(nameof(ChangeScene), 5f);
        }
    }

    public int GetNumber()
    {
        return number;
    }

    private void UpdateScale()
    {
        float scale = 0.0060606f * number + 0.15f;
        transform.localScale = new Vector3(scale, scale, scale);

        TMP_Text childText = GetComponentInChildren<TMP_Text>(true);
        if (childText != null)
        {
            float c = 1f - number / 100f;
            childText.color = new Color(1f, c, c);
        }

        float volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.5f);

        if (gattaiSound != null)
        {
            AudioSource.PlayClipAtPoint(gattaiSound, Camera.main.transform.position, volume);
        }
    }

    private bool IsPrime(int a)
    {
        if (a <= 1) return false;
        if (a == 2) return true;
        if (a % 2 == 0) return false;

        for (int i = 3; i * i <= a; i += 2)
        {
            if (a % i == 0)
                return false;
        }

        return true;
    }

    public void Effect()
    {
        if (bomb == null) return;

        if (bomb.prefab1 != null)
            Instantiate(bomb.prefab1, transform.position, Quaternion.identity);

        if (bomb.prefab2 != null)
            Instantiate(bomb.prefab2, transform.position, Quaternion.identity);
    }

    public void HideBomb()
    {
        gameObject.SetActive(false);
    }

    public void ChangeScene()
    {
        Debug.Log("シーン切り替え");
        SceneManager.LoadScene("ScoreScene");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.gameObject.GetComponent<IMergeNumber>();
        if (other == null) return;

        if (bomb == null)
        {
            Debug.LogError($"{name}: BombScript が設定されていません");
            return;
        }

        if (!bomb.gameover)
            bomb.TryMerge(this, other);
    }
}