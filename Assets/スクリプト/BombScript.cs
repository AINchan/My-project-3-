using UnityEngine;
using System;

public class BombScript : MonoBehaviour
{
    [Header("合体後に生成する Bomb プレハブ")]
    [SerializeField] private GameObject bombPrefab;

    public bool ok = false;

    private bool mergingThisFrame;

    public bool gameover = false;
    public bool gamestart = false;

    public GameObject prefab1;
    public GameObject prefab2;

    private void LateUpdate()
    {
        mergingThisFrame = false;
    }

    public void TryMerge(IMergeNumber a, IMergeNumber b)
    {
        if (mergingThisFrame) return;
        if (a == null || b == null) return;

        if (bombPrefab == null)
        {
            Debug.LogError("BombScript: bombPrefab が未設定です");
            return;
        }

        // 二重発火防止（決定ルール）
        if (a.gameObject.GetInstanceID() > b.gameObject.GetInstanceID())
        {
            var tmp = a; a = b; b = tmp;
        }

        mergingThisFrame = true;

        int sum = a.GetNumber() + b.GetNumber();
        Vector3 middle = (a.transform.position + b.transform.position) / 2f;

        GameObject newObj = Instantiate(bombPrefab, middle, Quaternion.identity);

        // ★爆弾は生成直後に Dynamic 化（Rigidbody2D）
        var rb2d = newObj.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.bodyType = RigidbodyType2D.Dynamic;
            rb2d.simulated = true;
        }

        var bombObj = newObj.GetComponent<BombObject>();
        if (bombObj == null)
        {
            Debug.LogError("BombScript: 生成したBombPrefabに BombObject が付いていません");
            Destroy(newObj);
            return;
        }

        // 連鎖のため注入
        bombObj.SetBomb(this);
        bombObj.SetNumber(sum);

        Destroy(a.gameObject);
        Destroy(b.gameObject);

        ok = true;
    }
}