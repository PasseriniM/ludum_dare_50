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
            GameObject enemyInstance = Instantiate(enemy.gameObject, mapManager.map.CellToWorld(enemy.spawnPoint), Quaternion.identity);
            MovingCharacterScript movingScript = enemyInstance.GetComponent<MovingCharacterScript>();
            movingScript.StartPath(enemy.path);
        }
    }

    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
