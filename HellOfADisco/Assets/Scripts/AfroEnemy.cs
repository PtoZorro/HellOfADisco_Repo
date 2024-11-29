using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfroEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject enemySprite;
    [SerializeField] GameObject enemyBulletPrefab;
    [SerializeField] Transform lookPointPos;
    [SerializeField] CapsuleCollider2D collider;
    [SerializeField] GameObject[] pickUps;
    SpriteRenderer spriteRenderer;
    Animator animator;
    Rigidbody2D rb;
    Vector2 playerDirection;


    [Header("Stats")]
    [SerializeField] float health;
    [SerializeField] float maxHealth;
    [SerializeField] float gunBulletDamage;
    [SerializeField] float shotgunBulletDamage;
    [SerializeField] float fireRate;
    bool canShoot;
    bool dying;
    bool death;

    Color originalColor;
    Color damagedColor = Color.red;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = enemySprite.GetComponent<SpriteRenderer>();
        animator = enemySprite.GetComponent<Animator>();
        collider = GetComponent<CapsuleCollider2D>();

        health = maxHealth;
        originalColor = spriteRenderer.color;

        dying = false;
        StartCoroutine(StartDontShoot());

        AudioManager.Instance.PlaySFX(11);
    }

    private void Update()
    {
        if (health <= 0)
        {
            health = 0;
            if (!dying) { StartCoroutine(Death()); }
        }

        if (!dying) { Shoot(); SpriteSystem(); }

        ShootPoint();
    }

    void ShootPoint()
    {
        Vector2 playerPos = GameObject.Find("Player").transform.position;
        playerDirection = playerPos - (Vector2)transform.position;

        lookPointPos.up = playerDirection;
    }

    IEnumerator StartDontShoot()
    {
        yield return new WaitForSeconds(1);
        canShoot = true;
    }

    void Shoot()
    {
        if (canShoot && !dying)
        {
            canShoot = false;

            Instantiate(enemyBulletPrefab, lookPointPos.position, lookPointPos.rotation);
            animator.SetTrigger("attack");
            Invoke(nameof(RestartShoot), fireRate);
        }
    }

    // Función para reiniciar la condición de poder disparar cuando es llamada
    void RestartShoot()
    {
        // El prefab de balas es un empty que expulsa varias balas, una vez disparadas, el empty se desactivará en la jerarquía
        GameObject[] enemyBulletsEmpty = GameObject.FindGameObjectsWithTag("Enemy3Bullet");
        foreach (GameObject enemyBulletEmpty in enemyBulletsEmpty)
        {
            Destroy(enemyBulletEmpty);
        }
        canShoot = true;
    }

    void SpriteSystem()
    {
        if (lookPointPos.up.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (lookPointPos.up.x >= 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GunBullet"))
        {
            if (!dying)
            {
                if (GameManager.Instance.damageAmplified) { health -= gunBulletDamage * 2; }
                else { health -= gunBulletDamage; }
                spriteRenderer.color = damagedColor;
                StartCoroutine(nameof(NormalColor));
            }
        }
        if (other.CompareTag("ShotgunBullet"))
        {
            if (!dying)
            {
                if (GameManager.Instance.damageAmplified) { health -= shotgunBulletDamage * 2; }
                else { health -= shotgunBulletDamage; }
                spriteRenderer.color = damagedColor;
                StartCoroutine(nameof(NormalColor));
            }
        }
    }

    IEnumerator NormalColor()
    {
        yield return new WaitForSeconds(.1f);
        spriteRenderer.color = originalColor;
    }

    IEnumerator Death()
    {
        dying = true;
        collider.enabled = false;
        animator.SetTrigger("death");
        GameManager.Instance.remainingEnemies--;
        AudioManager.Instance.PlaySFX(10);

        // Generar un índice aleatorio para elegir uno de los objetos del array
        int randomIndex = Random.Range(0, pickUps.Length);

        // Generar un número aleatorio entre 0 y 1
        float randomValue = Random.value;

        // Verificar si el valor aleatorio es menor o igual a la probabilidad
        if (randomValue <= .4)
        {
            // Instanciar el objeto aleatorio seleccionado del array
            Instantiate(pickUps[randomIndex], transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(3);
        gameObject.SetActive(false);
    }
}
