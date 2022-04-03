using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HQCharacterAI : MonoBehaviour
{
    private MovingCharacterScript movingScript;
    private HealthScript healthScript;
    private MapManager mapManager;

    private enum CurrentState { Idle, Dead };
    private CurrentState state = CurrentState.Idle;

    public List<Vector3Int> cellsOccupied;

    private void Awake()
    {
        movingScript = GetComponent<MovingCharacterScript>();
        healthScript = GetComponent<HealthScript>();
        mapManager = FindObjectOfType<MapManager>();
    }

    private void Start()
    {
        foreach (Vector3Int cell in cellsOccupied)
        {
            mapManager.logicGrid.Subscribe(cell, gameObject);
        }
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case CurrentState.Idle:
                {
                    IdleUpdate();
                    break;
                }
            case CurrentState.Dead:
                {
                    DeadUpdate();
                    break;
                }
            default:
                {
                    //unreachable
                    break;
                }
        }
        if (healthScript.IsDead())
        {
            state = CurrentState.Dead;
        }
    }

    private void IdleUpdate()
    {
    }
    private void DeadUpdate()
    {
        //nothing. Play animation? Destroy?
        //Destroy(gameObject);
    }
}
