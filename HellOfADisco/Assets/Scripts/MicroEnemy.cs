using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class MicroEnemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject enemySprite;
    [SerializeField] GameObject enemyBulletPrefab;
    [SerializeField] Transform shootPointPos;
    [SerializeField] GameObject[] pickUps;
    SpriteRenderer spriteRenderer;
    Animator animator;
    Rigidbody2D rb;
    Vector2 rotationCenter;
    Vector2 playerDirection;

    [Header("Stats")]
    [SerializeField] float health;
    [SerializeField] float maxHealth;
    [SerializeField] float normalSpeed;
    [SerializeField] float spinSpeed;
    [SerializeField] float gunBulletDamage;
    [SerializeField] float shotgunBulletDamage;
    [SerializeField] float distance;
    [SerializeField] float jumpForce;
    [SerializeField] float fireRate;
    bool canShoot;
    bool dying;
    bool isJumping;

    [SerializeField] bool spin;
    Color originalColor;
    Color damagedColor = Color.red;
    Vector2 collisionDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>(); 
        spriteRenderer = enemySprite.GetComponent<SpriteRenderer>();
        animator = enemySprite.GetComponent<Animator>();

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

        if (!dying) 
        {
            if (!isJumping) { Move(); } 
            Shoot(); 
        }
        SpriteSystem();
        ShootPoint();

        if (GameManager.Instance.damageAmplified) { gunBulletDamage = gunBulletDamage * 2; shotgunBulletDamage = shotgunBulletDamage * 2; }
    }

    void Move()
    {
        Vector2 playerPos = GameObject.Find("Player").transform.position;
        playerDirection = playerPos - (Vector2)transform.position;

        float currentDistance = playerDirection.magnitude;

        // Si la distancia es menor que la distancia mínima, comienza a girar alrededor del jugador
        if (currentDistance < distance)
        {
            spin = true;
            rotationCenter = playerPos;
        }
        else
        {
            spin = false;
        }

        // Si no estamos girando, seguimos al jugador
        if (!spin)
        {
            rb.velocity = playerDirection.normalized * normalSpeed;
        }
        else // Si estamos girando
        {
            // Calcula la dirección perpendicular para moverse alrededor del jugador
            Vector2 perpendicularDirection = new Vector2(-playerDirection.y, playerDirection.x).normalized;

            // Calcula la posición alrededor del jugador manteniendo la misma distancia
            Vector2 objectivePosition = rotationCenter + perpendicularDirection * distance;

            // Calcula la dirección hacia la posición objetivo
            Vector2 objectiveDirection = objectivePosition - (Vector2)transform.position;

            // Ajusta la velocidad para mantener la misma distancia
            float minDistance = currentDistance - distance;
            Vector2 minSpeed = objectiveDirection.normalized * normalSpeed;
            rb.velocity = minSpeed + playerDirection.normalized * minDistance;
        }
    }

    void ShootPoint()
    {
        shootPointPos.up = playerDirection;
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

            Instantiate(enemyBulletPrefab, shootPointPos.position, shootPointPos.rotation);
            animator.SetTrigger("attack");
            Invoke(nameof(RestartShoot), fireRate);
        }
    }

    // Función para reiniciar la condición de poder disparar cuando es llamada
    void RestartShoot()
    {
        canShoot = true;
    }

    void SpriteSystem()
    {
        if (rb.velocity.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (rb.velocity.x >= 0)
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
        if (other.CompareTag("Enemy") || other.CompareTag("Wall"))
        {
            collisionDirection = (transform.position - other.transform.position).normalized;
            StartCoroutine(nameof(JumpCollision));
        }
    }
    
    IEnumerator JumpCollision()
    {
        isJumping = true;
        rb.AddForce(collisionDirection * jumpForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(.5f);
        isJumping = false;
    }

    IEnumerator NormalColor()
    {
        yield return new WaitForSeconds(.1f);
        spriteRenderer.color = originalColor;
    }

    IEnumerator Death()
    {
        dying = true;
        animator.SetTrigger("death");
        GameManager.Instance.remainingEnemies--;
        AudioManager.Instance.PlaySFX(10);

        yield return new WaitForSeconds(1f);

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

        gameObject.SetActive(false);
    }
}
