using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    public float health;
    private MapManager mapManager;
    private MovingCharacterScript movingScript;

    private float startHealth;
    private void Awake()
    {
        startHealth = health;
        mapManager = FindObjectOfType<MapManager>();
        movingScript = GetComponent<MovingCharacterScript>();
    }

    private void FixedUpdate()
    {
        if(movingScript!=null)
        {
            float poisonModifier = mapManager.GetPoisonModifier(movingScript.pathManager.currentPosition);
            if (poisonModifier != 0)
            {
                ApplyDamage(poisonModifier * Time.fixedDeltaTime);
            }
        }
    }

    public void ApplyDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, -1f, startHealth);
    }
    public bool IsDead() { return health <= 0f; }
    public void Reset() { health = startHealth; }

    public float HealthPercent()
	{
        return health / startHealth;
	}
}
