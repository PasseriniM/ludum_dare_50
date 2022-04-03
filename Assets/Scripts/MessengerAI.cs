using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessengerAI : MonoBehaviour
{
    private MovingCharacterScript movingScript;
    private HealthScript healthScript;
    private MapManager mapManager;

    private Dictionary<Vector3Int, List<Vector3Int>> messages;

    private enum CurrentState { Idle, Moving, Dead };
    private CurrentState state = CurrentState.Idle;
    private Vector3Int lastPosition;

    private void Awake()
    {
        movingScript = GetComponent<MovingCharacterScript>();
        healthScript = GetComponent<HealthScript>();
        mapManager = FindObjectOfType<MapManager>();
        lastPosition = mapManager.map.WorldToCell(transform.position);
        mapManager.logicGrid.Subscribe(lastPosition, gameObject);
    }

    public void AddMessage(Vector3Int position, List<Vector3Int> message)
    {
        messages.Add(position, message);
    }

    public void GetAlliedOccupants(out List<GameObject> occupants)
    {
        occupants = new List<GameObject>();
        List<GameObject> tempOccupants;
        if (mapManager.logicGrid.IsCellOccupied(movingScript.pathManager.currentPosition, out tempOccupants))
        {
            foreach(GameObject possibleAlly in tempOccupants)
            {
                if(possibleAlly!=gameObject)
                {
                    MovingCharacterScript script = possibleAlly.GetComponent<MovingCharacterScript>();
                    MainAttackerAI attacker = possibleAlly.GetComponent<MainAttackerAI>();
                    if (script.faction == movingScript.faction && attacker!=null)
                    {
                        occupants.Add(possibleAlly);
                    }
                }
            }
        }
    }

    public void TryDeliverMessage()
    {
        List<Vector3Int> message;
        if(messages.TryGetValue(movingScript.pathManager.currentPosition, out message))
        {
            bool messageDelivered = false;
            List<GameObject> occupants;
            GetAlliedOccupants(out occupants);
            foreach(GameObject occupant in occupants)
            {
                MovingCharacterScript script = occupant.GetComponent<MovingCharacterScript>();
                MainAttackerAI attacker = occupant.GetComponent<MainAttackerAI>();
                if(!attacker.IsAttacking() && !attacker.IsDead())
                {
                    messageDelivered = true;
                    script.StartPath(message);
                }
            }

            if(messageDelivered)
            {
                messages.Remove(movingScript.pathManager.currentPosition);
            }
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
        if (healthScript.IsDead())
        {
            state = CurrentState.Dead;
        }
    }

    private void IdleUpdate()
    {
        if (movingScript.pathManager.IsMoving())
        {
            state = CurrentState.Moving;
        }
    }

    private void MovingUpdate()
    {
        if (movingScript.pathManager.HasArrived())
        {
            state = CurrentState.Idle;
        }
        else
        {
            if (lastPosition != movingScript.pathManager.currentPosition)
            {
                mapManager.logicGrid.Unsubscribe(lastPosition, gameObject);
                lastPosition = movingScript.pathManager.currentPosition;
                mapManager.logicGrid.Subscribe(lastPosition, gameObject);
            }
            TryDeliverMessage();
        }
    }
    private void DeadUpdate()
    {
        //nothing. Play animation? Destroy?
        Destroy(gameObject);
    }
}