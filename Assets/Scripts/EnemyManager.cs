using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawningEntry
{
    public GameObject gameObject;
    public Vector3Int spawnPoint;
    public List<Vector3Int> path;
}

public class EnemyManager : MonoBehaviour
{
    public List<EnemySpawningEntry> enemyList;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private MapManager mapManager;
    private HQCharacterAI characterHQ;

    private void Awake()
    {
        mapManager = FindObjectOfType<MapManager>();
        characterHQ = FindObjectOfType<HQCharacterAI>();
    }

    public Vector3Int GetCloserDirection(Vector3Int startPos, Vector3Int endPos, List<Vector3Int> directions)
    {
        if(directions.Count>0)
        {
            Vector3Int result = directions[0];
            float minDist = (startPos + result - endPos).magnitude;
            for (int i= 1; i<directions.Count; i++)
            {
                float dist = (startPos + directions[i] - endPos).magnitude;
                if (dist < minDist)
                {
                    minDist = dist;
                    result = directions[i];
                }
            }
            return result;
        }
        return startPos;
    }

    private void CreatePathToHQ(Vector3Int startPos, out List<Vector3Int> path)
    {
        path = new List<Vector3Int>();
        Vector3Int endPos = characterHQ.GetClosestPosition(startPos);
        Vector3Int facing = endPos - startPos;
        facing = facing.x > 0 ? new Vector3Int(1, 0, 0) : new Vector3Int(-1, 0, 0);

        Vector3Int lastPosition = startPos;
        int pathCounter = 0;
        while(pathCounter<10000)
        {
            List<Vector3Int> adjacencyList;
            mapManager.logicGrid.FillAdjacencyList(lastPosition, facing, out adjacencyList);
            Vector3Int nextDir = GetCloserDirection(lastPosition, endPos, adjacencyList);
            lastPosition = lastPosition + nextDir;
            path.Add(lastPosition);

            Vector3Int nextDistance = endPos - lastPosition;
            if (LogicGrid.IsValidAdjacent(lastPosition,nextDistance))
            {
                break;
            }
            pathCounter++;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach(EnemySpawningEntry enemy in enemyList)
        {
            Vector3 position = mapManager.map.CellToWorld(enemy.spawnPoint);
            position.z = 0f;
            GameObject enemyInstance = Instantiate(enemy.gameObject, position, Quaternion.identity,transform);
            MovingCharacterScript movingScript = enemyInstance.GetComponent<MovingCharacterScript>();
            movingScript.pathManager.UpdateCurrentPosition(position);
            if (enemy.path!=null && enemy.path.Count>0)
            {
                movingScript.StartPath(enemy.path);
            }
            else
            {
                List<Vector3Int> path;
                CreatePathToHQ(enemy.spawnPoint, out path);
                movingScript.StartPath(path);
            }
            movingScript.MemorizeBackupPath();
            spawnedEnemies.Add(enemyInstance);
        }
    }

    private void FixedUpdate()
    {
        for(int i=0; i<spawnedEnemies.Count; i++)
        {
            GameObject spawnedEnemy = spawnedEnemies[i];
            if(spawnedEnemy !=null)
            {
                MovingCharacterScript movingScript = spawnedEnemy.GetComponent<MovingCharacterScript>();
                AttackingCharacter attackingCharacter = spawnedEnemy.GetComponent<AttackingCharacter>();
                if (!attackingCharacter.IsAttacking() && 
                    !movingScript.pathManager.IsMoving() && 
                    !movingScript.HasReachedBackupPathDestination())
                {
                    movingScript.ResumeOldPath();
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
