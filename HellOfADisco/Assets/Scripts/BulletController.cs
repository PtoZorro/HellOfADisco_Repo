using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Bullet Stats")]
    [SerializeField] private float bulletSpeed = 5f;

    private void Start()
    {
        transform.SetParent(null);
    }

    void Update()
    {
        transform.Translate(Vector2.up * bulletSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            gameObject.SetActive(false);
        }
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Enemy3"))
        {
            if (!GameManager.Instance.pierce) { gameObject.SetActive(false); }
        }
    }
}
