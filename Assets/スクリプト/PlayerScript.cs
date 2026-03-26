using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class PlayerScript : MonoBehaviour, IMergeNumber
{
    private const string BGM_VOLUME_KEY = "BGM_VOLUME";

    private Rigidbody2D rb;
    private float moveInput;

    private bool isTouchingRightWall;
    private bool isTouchingLeftWall;

    private TMP_Text text;
    private int number;

    private BombScript bomb;

    private AudioSource audioSource;
    [SerializeField] private AudioClip stopSound;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // 保存済み音量を反映
        float savedVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.5f);
        audioSource.volume = savedVolume;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        text = GetComponentInChildren<TMP_Text>(true);
    }

    public void SetBomb(BombScript b) => bomb = b;

    public void SetNumber(int value)
    {
        number = value;

        if (text == null) text = GetComponentInChildren<TMP_Text>(true);
        if (text != null) text.text = number.ToString();

        UpdateScaleByNumber();
    }

    private void UpdateScaleByNumber()
    {
        float scale = 0.0060606f * number + 0.15f;
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    public int GetNumber() => number;

    public void SetMoveInput(float input)
        => moveInput = Mathf.Clamp(input, -1f, 1f);

    public void Move(float moveSpeed, float fallSpeed, bool isStopped, bool fastFall)
    {
        if (isStopped) return;

        float horizontal = moveInput;
        if (isTouchingRightWall && horizontal > 0) horizontal = 0;
        if (isTouchingLeftWall && horizontal < 0) horizontal = 0;

        float currentFallSpeed = fallSpeed;
        if (fastFall) currentFallSpeed *= 15f;

        rb.linearVelocity = new Vector2(horizontal * moveSpeed, -currentFallSpeed);
    }

    public void Stop()
    {
        rb.linearVelocity = Vector2.zero;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Stage"))
        {
            if (audioSource != null && stopSound != null)
            {
                audioSource.volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 0.5f);
                audioSource.PlayOneShot(stopSound);
            }

            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;

            MainScript main = GameObject.FindObjectOfType<MainScript>();
            if (main != null)
                main.NotifyPlayerStoppedByStage();

            return;
        }

        if (collision.gameObject.CompareTag("Wall r")) isTouchingRightWall = true;
        if (collision.gameObject.CompareTag("Wall l")) isTouchingLeftWall = true;

        var other = collision.gameObject.GetComponent<IMergeNumber>();
        if (other == null) return;

        if (bomb == null)
        {
            Debug.LogError($"{name}: BombScript が設定されていません");
            return;
        }

        if (!bomb.gameover)
        {
            bomb.TryMerge(this, other);   
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall r")) isTouchingRightWall = false;
        if (collision.gameObject.CompareTag("Wall l")) isTouchingLeftWall = false;
    }
}