using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    public float health;

    private float startHealth;
    private void Awake()
    {
        startHealth = health;
    }

    public void ApplyDamage(float damage) { health -= damage; }
    public bool IsDead() { return health <= 0f; }
    public void Reset() { health = startHealth; }
}
