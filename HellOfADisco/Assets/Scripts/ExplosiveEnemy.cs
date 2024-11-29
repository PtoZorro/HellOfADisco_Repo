using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.AI;

public class ExplosiveEnemy : MonoBehaviour
{
    [Header("References")]
    Transform target;
    NavMeshAgent agent;
    [SerializeField] GameObject enemySprite;
    [SerializeField] GameObject damageCollider;
    [SerializeField] GameObject explosionLight;
    [SerializeField] GameObject[] pickUps;
    SpriteRenderer spriteRenderer;
    Animator animator;

    [Header("Stats")]
    [SerializeField] float health;
    [SerializeField] float maxHealth;
    [SerializeField] float gunBulletDamage;
    [SerializeField] float shotgunBulletDamage;
    public bool detonation;
    bool exploding;
    Color originalColor;
    Color damagedColor = Color.red;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = enemySprite.GetComponent<SpriteRenderer>();
        animator = enemySprite.GetComponent<Animator>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        health = maxHealth;
        originalColor = spriteRenderer.color;

        damageCollider.SetActive(false);
        explosionLight.SetActive(false);

        detonation = false;
        exploding = false;

        AudioManager.Instance.PlaySFX(11);
    }

    private void Update()
    {
        if (health <= 0)
        {
            health = 0;
            if (!exploding) { StartCoroutine(Death()); }
        }

        if (!detonation) { Move(); }
        SpriteSystem();

        if (detonation && !exploding) { StartCoroutine(nameof(Explosion)); }

        if (GameManager.Instance.damageAmplified) { gunBulletDamage = gunBulletDamage * 2; shotgunBulletDamage = shotgunBulletDamage * 2; }
    }

    void Move()
    {
        target = GameObject.Find("Player").transform;
        agent.SetDestination(target.position);
    }

    void SpriteSystem()
    {
        if (agent.velocity.x >= 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (agent.velocity.x < 0)
        {
            spriteRenderer.flipX = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GunBullet"))
        {
            if (!exploding)
            {
                if (GameManager.Instance.damageAmplified) { health -= gunBulletDamage * 2; }
                else { health -= gunBulletDamage; }
                spriteRenderer.color = damagedColor;
                StartCoroutine(nameof(NormalColor));
            }
        }
        if (other.CompareTag("ShotgunBullet"))
        {
            if (!exploding)
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

    IEnumerator Explosion()
    {
        exploding = true;
        animator.SetTrigger("explosion");
        GameManager.Instance.remainingEnemies--;

        yield return new WaitForSeconds(.6f);
        damageCollider.SetActive(true);
        explosionLight.SetActive(true);
        AudioManager.Instance.PlaySFX(1);

        yield return new WaitForSeconds(.35f);
        damageCollider.SetActive(false);
        explosionLight.SetActive(false);

        yield return new WaitForSeconds(.2f);
        gameObject.SetActive(false);

        yield return null;
    }

    IEnumerator Death()
    {
        exploding = true;
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
