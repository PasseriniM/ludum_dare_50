using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AllySpawningEntry
{
    public GameObject gameObject;
    public Vector3Int spawnPoint;
}

public class AllySpawner : MonoBehaviour
{
    public List<AllySpawningEntry> allyList;

    private MapManager mapManager;

    private void Awake()
    {
        mapManager = FindObjectOfType<MapManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (AllySpawningEntry ally in allyList)
        {
            Vector3 position = mapManager.map.CellToWorld(ally.spawnPoint);
            position.z = 0f;
            GameObject allyInstance = Instantiate(ally.gameObject, position, Quaternion.identity, transform);
            MovingCharacterScript movingScript = allyInstance.GetComponent<MovingCharacterScript>();
            movingScript.pathManager.startPosition = ally.spawnPoint;
            movingScript.pathManager.UpdateCurrentPosition(position);
        }
    }

}
