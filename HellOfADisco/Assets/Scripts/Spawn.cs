using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    [SerializeField] GameObject enemy1Prefab;
    [SerializeField] GameObject enemy2Prefab;
    [SerializeField] GameObject enemy3Prefab;
    GameObject tempPrefab;
    [SerializeField] bool enemy1;
    [SerializeField] bool enemy2;
    [SerializeField] bool enemy3;
    [SerializeField] float spawnRate1;
    [SerializeField] float spawnRate2;
    [SerializeField] float spawnRate3;
    float tempSpawnRate;
    bool spawning;
    public bool canSpawn;


    void Start()
    {
        if (enemy1) { tempPrefab = enemy1Prefab; tempSpawnRate = spawnRate1; }
        else if (enemy2) { tempPrefab = enemy2Prefab; tempSpawnRate = spawnRate2; }
        else if (enemy3) { tempPrefab = enemy3Prefab; tempSpawnRate = spawnRate3; }
        spawning = false;
        canSpawn = true;

        StartCoroutine(ReadEnemies());
    }

    void Update()
    {
        if (!spawning)
        {
            spawning = true;
            Invoke(nameof(SpawnEnemy), tempSpawnRate);
        }
    }

    void SpawnEnemy()
    {
        if (GameManager.Instance.enemiesToSpawn > 0 && canSpawn)
        {
            GameManager.Instance.enemiesToSpawn--;
            Instantiate(tempPrefab, transform.position, Quaternion.identity);
        }
        spawning = false;
    }

    IEnumerator ReadEnemies()
    {
        yield return new WaitForSeconds(.1f);
        yield return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy3"))
        {
            canSpawn = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy3"))
        {
            canSpawn = true;
        }
    }
}
