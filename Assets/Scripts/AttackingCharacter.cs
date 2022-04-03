using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingCharacter : MonoBehaviour
{
    public float damagePerSecond = 1f;
    
    private GameObject currentTarget = null;

    private MapManager mapManager;
    private MovingCharacterScript movingCharacter;
    private HealthScript healthScript;
    private AttackTypeEntityScript attackTypeScript;
    private AttackerTypeRepository attackTypeRepository;

    private void Awake()
    {
        mapManager = FindObjectOfType<MapManager>();
        attackTypeRepository = FindObjectOfType<AttackerTypeRepository>();
        movingCharacter = GetComponent<MovingCharacterScript>();
        attackTypeScript = GetComponent<AttackTypeEntityScript>();
        healthScript = GetComponent<HealthScript>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public bool IsAttacking()
    {
        return currentTarget != null;
    }

    private GameObject GetOpposingTarget(List<GameObject> priorityTargets)
    {
        GameObject result = priorityTargets[0];
        if (priorityTargets.Count > 1)
        {
            //find and kill the messenger/non attacking unit
            foreach (GameObject possibleTarget in priorityTargets)
            {
                AttackingCharacter isAttacker = possibleTarget.GetComponent<AttackingCharacter>();
                if (isAttacker == null)
                {
                    result = possibleTarget;
                }
            }
        }
        MovingCharacterScript script = result.GetComponent<MovingCharacterScript>();
        if(script.faction == movingCharacter.faction)
        {
            return null;
        }
        return result;
    }

    private GameObject GetNextTarget()
    {
        List<GameObject> priorityTargets;
        Vector3Int newFacingDirection;
        mapManager.logicGrid.GetPrioritySurroundingCharacters(movingCharacter.pathManager.currentPosition,
            movingCharacter.pathManager.facingDirection, out newFacingDirection,
            out priorityTargets);

        GameObject result = null;
        if (priorityTargets != null && priorityTargets.Count > 0)
        {
            result = GetOpposingTarget(priorityTargets);
            if(result!=null)
            {
                movingCharacter.pathManager.facingDirection = newFacingDirection;
            }
        }

        return result;
    }

    private void FixedUpdate()
    {
        if (currentTarget != null)
        {
            MovingCharacterScript targetMovingScript = currentTarget.GetComponent<MovingCharacterScript>();
            Vector3Int dir = targetMovingScript.pathManager.currentPosition - movingCharacter.pathManager.currentPosition;
            if(!LogicGrid.IsValidDirection(movingCharacter.pathManager.currentPosition,dir))
            {
                currentTarget = null;
                return;
            }
            HealthScript targetHealthScript = currentTarget.GetComponent<HealthScript>();
            AttackerType targetAttackerType = currentTarget.GetComponent<AttackTypeEntityScript>().attackerType;
            float modifier = attackTypeRepository.GetModifier(attackTypeScript.attackerType, targetAttackerType);
            targetHealthScript.ApplyDamage(damagePerSecond * modifier * Time.fixedDeltaTime);
            if (targetHealthScript.IsDead())
            {
                currentTarget = null;
            }
        }
        else
        {
            currentTarget = GetNextTarget();
        }
    }
}
