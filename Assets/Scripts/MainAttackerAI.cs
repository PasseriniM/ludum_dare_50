using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainAttackerAI : MonoBehaviour
{
    private AttackingCharacter attackScript;
    private MovingCharacterScript movingScript;
    private HealthScript healthScript;
    private MapManager mapManager;

    private enum CurrentState { Idle, Attacking, Moving, Dead };
    private CurrentState state = CurrentState.Idle;

    private void Awake()
    {
        attackScript = GetComponent<AttackingCharacter>();
        movingScript = GetComponent<MovingCharacterScript>();
        healthScript = GetComponent<HealthScript>();
        mapManager = FindObjectOfType<MapManager>();
    }

    private void FixedUpdate()
    {
        switch(state)
        {
            case CurrentState.Idle:
                {
                    IdleUpdate();
                    break;
                }
            case CurrentState.Attacking:
                {
                    AttackingUpdate();
                    break;
                }
            case CurrentState.Moving:
                {
                    MovingUpdate();
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
    }

    private void IdleUpdate()
    {
        if(movingScript.pathManager.IsMoving())
        {
            state = CurrentState.Moving;
        }
        else if(attackScript.IsAttacking())
        {
            state = CurrentState.Attacking;
        }
    }
    private void AttackingUpdate()
    {
        if(!attackScript.IsAttacking())
        {
            state = CurrentState.Idle;
        }
    }
    private void MovingUpdate()
    {
        if(movingScript.pathManager.HasArrived())
        {
            state = CurrentState.Idle;
        }
        else if (attackScript.IsAttacking())
        {
            List<Vector3Int> newPath = new List<Vector3Int>();
            newPath.Add(movingScript.pathManager.currentPosition);
            movingScript.StartPath(newPath);
            state = CurrentState.Attacking;
        }
    }
    private void DeadUpdate()
    {
        //nothing. Play animation? Destroy?
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
