using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    public float health;
    private MapManager mapManager;

    private float startHealth;
    private void Awake()
    {
        startHealth = health;
        mapManager = FindObjectOfType<MapManager>();
    }

    private void FixedUpdate()
    {
        float poisonModifier = mapManager.GetPoisonModifier(mapManager.map.WorldToCell(transform.position));
        if(poisonModifier!=0)
        {
            ApplyDamage(poisonModifier * Time.fixedDeltaTime);
        }
    }

    public void ApplyDamage(float damage)
    {
        health -= damage;
        Mathf.Clamp(health, -1f, startHealth);
    }
    public bool IsDead() { return health <= 0f; }
    public void Reset() { health = startHealth; }
}
