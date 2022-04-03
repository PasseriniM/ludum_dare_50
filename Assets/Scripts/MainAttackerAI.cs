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
    private Vector3Int lastPosition;

    private void Awake()
    {
        attackScript = GetComponent<AttackingCharacter>();
        movingScript = GetComponent<MovingCharacterScript>();
        healthScript = GetComponent<HealthScript>();
        mapManager = FindObjectOfType<MapManager>();
        lastPosition = mapManager.map.WorldToCell(transform.position);
        mapManager.logicGrid.Subscribe(lastPosition, gameObject);
    }

    public bool IsAttacking()
    {
        return state == CurrentState.Attacking;
    }

    public bool IsDead()
    {
        return state == CurrentState.Dead;
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
        if(healthScript.IsDead())
        {
            state = CurrentState.Dead;
        }
    }
    public bool IsTargetObstructed()
    {
        if (movingScript.pathManager.currentPosition == movingScript.pathManager.GetCurrentTargetCell())
        {
            return false;
        }
        List<GameObject> tempOccupants;
        if (mapManager.logicGrid.IsCellOccupied(movingScript.pathManager.GetCurrentTargetCell(), out tempOccupants))
        {
            foreach (GameObject possibleAlly in tempOccupants)
            {
                if (possibleAlly != gameObject)
                {
                    MainAttackerAI attacker = possibleAlly.GetComponent<MainAttackerAI>();
                    if (attacker != null && !attacker.IsDead())
                    {
                        return true;
                    }
                }
            }
        }
        return false;
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
        else
        {
            if(lastPosition!=movingScript.pathManager.currentPosition)
            {
                mapManager.logicGrid.Unsubscribe(lastPosition, gameObject);
                lastPosition = movingScript.pathManager.currentPosition;
                mapManager.logicGrid.Subscribe(lastPosition, gameObject);
            }
            //check if we need to stop
            if(IsTargetObstructed())
            {
                List<Vector3Int> newPath = new List<Vector3Int>();
                newPath.Add(movingScript.pathManager.currentPosition);
                movingScript.StartPath(newPath);
                state = CurrentState.Idle;
            }
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
