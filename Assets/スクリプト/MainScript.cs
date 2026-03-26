using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MainScript : MonoBehaviour
{
    [Header("数字ごとのPrefab")]
    [SerializeField] private GameObject[] playerPrefabs;

    [SerializeField] private float fallSpeed = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private BombScript bomb;

    [Header("Next表示")]
    [SerializeField] private Transform nextAnchor;

    private GameObject nextPreviewObj;
    private int nextNumber;

    public TMP_Text text_maxScore;

    private PlayerScript player;
    private bool isPlayerStopped;
    private bool spawnedAfterStop;

    public Button button_gamestart;
    public GameObject gameObject_gamestart;

    public Button button_right;
    public Button button_left;
    public Button button_fall;

    private bool pressRight;
    private bool pressLeft;
    private bool pressFall;

    private AudioSource audioSource;
    public AudioClip gameBGM;

    public AudioClip button;
    public AudioClip bombSound;

    public bool check;
 

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (bomb == null)
            bomb = FindObjectOfType<BombScript>();

        check = false;
    }

    void Start()
    {
        if (audioSource != null && gameBGM != null)
        {
            audioSource.clip = gameBGM; 
            audioSource.loop = true;     
            audioSource.Play();          
        }

        GenerateNext();
        SpawnPlayer();

        // ⭐ ここ追加（最初は操作不可）
        SetControlButtons(false);

        if (bomb != null && !bomb.gamestart)
        {
            if (gameObject_gamestart != null)
                gameObject_gamestart.SetActive(true);
        }

        if (ScoreScript.Instance != null && text_maxScore != null)
        {
            int max = ScoreScript.Instance.MaxScore;
            text_maxScore.text = max.ToString();
        }

        if (button_gamestart != null)
            button_gamestart.onClick.AddListener(GameStart);
    }

    void Update()
    {
        if (bomb != null && bomb.ok)
        {
            bomb.ok = false;

            if (player != null) return;

            isPlayerStopped = true;
            spawnedAfterStop = true;

            SpawnPlayer();
            return;
        }

        if (player == null) return;

        float move = 0f;

        if (Input.GetKey(KeyCode.A)) move = -1f;
        if (Input.GetKey(KeyCode.D)) move = 1f;

        if (pressLeft) move = -1f;
        if (pressRight) move = 1f;

        player.SetMoveInput(move);

        if (isPlayerStopped)
        {
            if (!spawnedAfterStop)
            {
                spawnedAfterStop = true;
                SpawnPlayer();
            }
        }
        else
        {
            spawnedAfterStop = false;
        }

        if (bomb.gameover)
        {
            if (check) return;
            audioSource.Stop();
            audioSource.clip = null;
            Invoke("Bomb",2.8f);
            check = true;
        }
    }

    void SetControlButtons(bool enable)
    {
        if (button_right != null) button_right.interactable = enable;
        if (button_left != null) button_left.interactable = enable;
        if (button_fall != null) button_fall.interactable = enable;
    }


    void FixedUpdate()
    {
        if (player == null) return;
        if (bomb == null) return;

        bool canMove = !bomb.gameover && bomb.gamestart;
        bool fastFall = Input.GetKey(KeyCode.S) || pressFall;

        if (canMove)
        {
            player.Move(moveSpeed, fallSpeed, isPlayerStopped, fastFall);
        }
        else
        {
            player.Stop();
        }
    }

    public void Bomb()
    {
        audioSource.Play();
        audioSource.PlayOneShot(bombSound);
    }

    public void NotifyPlayerStoppedByStage()
    {
        isPlayerStopped = true;
    }

    private void GenerateNext()
    {
        nextNumber = Random.Range(1, 51);
        UpdateNextPreview();
    }

    private void UpdateNextPreview()
    {
        if (nextAnchor == null) return;

        GameObject prefab = GetPrefabByNumber(nextNumber);
        if (prefab == null) return;

        if (nextPreviewObj != null)
            Destroy(nextPreviewObj);

        nextPreviewObj = Instantiate(prefab, nextAnchor.position, Quaternion.identity);
        nextPreviewObj.transform.SetParent(nextAnchor, false);
        nextPreviewObj.transform.localPosition = Vector3.zero;
        nextPreviewObj.transform.localRotation = Quaternion.identity;
        nextPreviewObj.transform.localScale = Vector3.one;

        var previewPlayer = nextPreviewObj.GetComponent<PlayerScript>();
        if (previewPlayer != null)
        {
            previewPlayer.SetNumber(nextNumber);
            previewPlayer.enabled = false;
        }

        var rb2d = nextPreviewObj.GetComponent<Rigidbody2D>();
        if (rb2d != null)
            rb2d.simulated = false;

        var col2d = nextPreviewObj.GetComponent<Collider2D>();
        if (col2d != null)
            col2d.enabled = false;
    }

    private void SpawnPlayer()
    {
        GameObject prefab = GetPrefabByNumber(nextNumber);

        if (prefab == null)
        {
            Debug.LogError("生成用Prefabが見つかりません");
            return;
        }

        GameObject obj = Instantiate(prefab, new Vector3(0, 4.5f, 0), Quaternion.identity);

        player = obj.GetComponent<PlayerScript>();

        if (player == null)
        {
            Debug.LogError("PlayerScriptがありません");
            return;
        }

        if (bomb != null)
            player.SetBomb(bomb);

        player.SetNumber(nextNumber);

        GenerateNext();

        isPlayerStopped = false;
    }

    private GameObject GetPrefabByNumber(int number)
    {
        int index = GetPrefabIndexByNumber(number);

        if (playerPrefabs == null || playerPrefabs.Length <= index)
        {
            Debug.LogError("playerPrefabs の設定数が足りません。6個設定してください。");
            return null;
        }

        if (playerPrefabs[index] == null)
        {
            Debug.LogError($"playerPrefabs[{index}] が未設定です。");
            return null;
        }

        return playerPrefabs[index];
    }

    private int GetPrefabIndexByNumber(int number)
    {
        if (number >= 1 && number <= 9) return 0;
        if (number >= 10 && number <= 19) return 1;
        if (number >= 20 && number <= 29) return 2;
        if (number >= 30 && number <= 39) return 3;
        if (number >= 40 && number <= 49) return 4;
        return 5;
    }

    public void OnButton()
    {
        audioSource.PlayOneShot(button);
    }



    public void GameStart()
    {
        if (gameObject_gamestart != null)
            gameObject_gamestart.SetActive(false);

        if (bomb != null)
            bomb.gamestart = true;

        SetControlButtons(true);
    }

    public void DownRight() => pressRight = true;
    public void UpRight() => pressRight = false;
    public void DownLeft() => pressLeft = true;
    public void UpLeft() => pressLeft = false;
    public void DownFall() => pressFall = true;
    public void UpFall() => pressFall = false;
}