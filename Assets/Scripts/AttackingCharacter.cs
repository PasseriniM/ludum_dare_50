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

    void Awake()
    {
        mapManager = FindObjectOfType<MapManager>();
        movingCharacter = GetComponent<MovingCharacterScript>();
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
            HealthScript targetScript = currentTarget.GetComponent<HealthScript>();
            targetScript.ApplyDamage(damagePerSecond * Time.fixedDeltaTime);
            if (targetScript.IsDead())
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
