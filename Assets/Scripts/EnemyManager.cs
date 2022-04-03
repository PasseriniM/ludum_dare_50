using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<GameObject> enemyList;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private MapManager mapManager;
    private HQCharacterAI characterHQ;

    public int SpawnGridXMin;
    public int SpawnGridXMax;
    public int SpawnGridYMin;
    public int SpawnGridYMax;

    public float spawnIntervalInitial;
    public float spawnIntervalDecreaseAfterEachSpawn;

    private float currentWaitTime = 0f;
    private float currentInterval;

    private void Awake()
    {
        mapManager = FindObjectOfType<MapManager>();
        characterHQ = FindObjectOfType<HQCharacterAI>();
        currentInterval = spawnIntervalInitial;
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

    public Vector3Int GetRandomStartPosition()
    {
        Vector3Int position = new Vector3Int(0, 0, 0);
        if(Random.Range(0f,1f)>0.5f)
        {
            position.x = Mathf.RoundToInt(Random.Range(SpawnGridXMin, SpawnGridXMax));
        }
        else
        {
            position.y = Mathf.RoundToInt(Random.Range(SpawnGridYMin, SpawnGridYMax));
        }
        return position;
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
    private void SpawnRandomCharacter()
    {
        int characterToSpawnIndex = Mathf.RoundToInt(Random.Range(0, enemyList.Count-1));
        GameObject objectPrefab = enemyList[characterToSpawnIndex];
        Vector3Int startCell = GetRandomStartPosition();

        List<GameObject> characters;
        if(mapManager.logicGrid.IsCellOccupied(startCell, out characters))
        {
            //Try one more time
            startCell = GetRandomStartPosition();
            if (mapManager.logicGrid.IsCellOccupied(startCell, out characters))
            {
                return;
            }
        }

        Vector3 position = mapManager.map.CellToWorld(startCell);
        position.z = 0f;
        GameObject enemyInstance = Instantiate(objectPrefab, position, Quaternion.identity, transform);
        MovingCharacterScript movingScript = enemyInstance.GetComponent<MovingCharacterScript>();
        movingScript.pathManager.UpdateCurrentPosition(position);

        List<Vector3Int> path;
        CreatePathToHQ(startCell, out path);
        movingScript.StartPath(path);

        movingScript.MemorizeBackupPath();
        spawnedEnemies.Add(enemyInstance);
    }

    private static bool IsNullObject(GameObject targetObject)
    {
        return targetObject == null;
    }

    private void FixedUpdate()
    {
        System.Predicate<GameObject> predicate = IsNullObject;
        spawnedEnemies.RemoveAll(predicate);

        for (int i=0; i<spawnedEnemies.Count; i++)
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
                    List<Vector3Int> trimmedPath = movingScript.GetOldTrimmedPath();
                    List<GameObject> occupying;
                    bool obstructed = false;
                    if(mapManager.logicGrid.IsCellOccupied(trimmedPath[0],out occupying))
                    {
                        foreach(GameObject obstruction in occupying)
                        {
                            if (obstruction != spawnedEnemy)
                            {
                                MainAttackerAI attacker = obstruction.GetComponent<MainAttackerAI>();
                                if (attacker != null && !attacker.IsDead())
                                {
                                    obstructed = true ;
                                    break;
                                }
                            }
                        }
                    }
                    if(!obstructed)
                    {
                        movingScript.ResumeOldPath();
                    }
                }
            }
        }

        currentWaitTime += Time.fixedDeltaTime;
        if(currentWaitTime>=currentInterval)
        {
            currentWaitTime = 0f;
            SpawnRandomCharacter();
            currentInterval -= spawnIntervalDecreaseAfterEachSpawn;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
