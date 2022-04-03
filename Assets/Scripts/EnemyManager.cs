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

    private void Awake()
    {
        mapManager = FindObjectOfType<MapManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach(EnemySpawningEntry enemy in enemyList)
        {
            Vector3 position = mapManager.map.CellToWorld(enemy.spawnPoint);
            position.z = 0f;
            GameObject enemyInstance = Instantiate(enemy.gameObject, position, Quaternion.identity);
            MovingCharacterScript movingScript = enemyInstance.GetComponent<MovingCharacterScript>();
            movingScript.StartPath(enemy.path);
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
                if(!movingScript.pathManager.IsMoving() && !movingScript.HasReachedBackupPathDestination())
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
