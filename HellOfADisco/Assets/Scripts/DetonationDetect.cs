using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetonationDetect : MonoBehaviour
{
    [SerializeField] ExplosiveEnemy explosiveEnemy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            explosiveEnemy.detonation = true;
        }
    }
}
