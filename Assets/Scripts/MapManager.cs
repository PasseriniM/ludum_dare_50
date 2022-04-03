using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class LogicGrid
{
    private Dictionary<Vector3Int, List<GameObject>> characterPerCell;

    public bool IsValidDirection(Vector3Int direction)
    {
        return !((direction.x == 0 && direction.y == 0) ||
            (direction.x == 0 && direction.y == 1) ||
            (direction.x == 1 && direction.y == -1) ||
            (direction.x == 1 && direction.y == -1));
    }

    public void FillAdjacencyList(Vector3Int startDirection, out List<Vector3Int> adjacencyList)
    {
        if (!IsValidDirection(startDirection))
        {
            adjacencyList = null;
            return;
        }

        startDirection.z = startDirection.z != 0 ? 0 : startDirection.z;

        Mathf.Clamp(startDirection.x, -1, 1);
        Mathf.Clamp(startDirection.y, -1, 1);

        adjacencyList = new List<Vector3Int>();
        adjacencyList.Add(startDirection);
        if (startDirection.x == 1 && startDirection.y == 1)
        {
            adjacencyList.Add(new Vector3Int(-1, 1, 0));
            adjacencyList.Add(new Vector3Int(1, 0, 0));
            adjacencyList.Add(new Vector3Int(-1, 0, 0));
            adjacencyList.Add(new Vector3Int(0, -1, 0));
            adjacencyList.Add(new Vector3Int(-1, -1, 0));
        }
        else if( startDirection.x == 1 && startDirection.y == 0)
        {
            adjacencyList.Add(new Vector3Int(1, 1, 0));
            adjacencyList.Add(new Vector3Int(0, -1, 0));
            adjacencyList.Add(new Vector3Int(-1, 1, 0));
            adjacencyList.Add(new Vector3Int(-1, -1, 0));
            adjacencyList.Add(new Vector3Int(-1, 0, 0));
        }
        else if (startDirection.x == 0 && startDirection.y == -1)
        {
            adjacencyList.Add(new Vector3Int(1, 0, 0));
            adjacencyList.Add(new Vector3Int(-1, -1, 0));
            adjacencyList.Add(new Vector3Int(1, 1, 0));
            adjacencyList.Add(new Vector3Int(-1, 0, 0));
            adjacencyList.Add(new Vector3Int(-1, 1, 0));
        }
        else if (startDirection.x == -1 && startDirection.y == -1)
        {
            adjacencyList.Add(new Vector3Int(0, -1, 0));
            adjacencyList.Add(new Vector3Int(-1, 0, 0));
            adjacencyList.Add(new Vector3Int(1, 0, 0));
            adjacencyList.Add(new Vector3Int(-1, 1, 0));
            adjacencyList.Add(new Vector3Int(1, 1, 0));
        }
        else if (startDirection.x == -1 && startDirection.y == 0)
        {
            adjacencyList.Add(new Vector3Int(-1, -1, 0));
            adjacencyList.Add(new Vector3Int(-1, 1, 0));
            adjacencyList.Add(new Vector3Int(0, -1, 0));
            adjacencyList.Add(new Vector3Int(1, 1, 0));
            adjacencyList.Add(new Vector3Int(1, 0, 0));
        }
        else if (startDirection.x == -1 && startDirection.y == 1)
        {
            adjacencyList.Add(new Vector3Int(-1, 0, 0));
            adjacencyList.Add(new Vector3Int(1, 1, 0));
            adjacencyList.Add(new Vector3Int(-1, -1, 0));
            adjacencyList.Add(new Vector3Int(1, 0, 0));
            adjacencyList.Add(new Vector3Int(0, -1, 0));
        }

    }
    public void Subscribe(Vector3Int position, GameObject gameObject)
    {
        List<GameObject> characters;
        if(characterPerCell.TryGetValue(position, out characters))
        {
            characters.Add(gameObject);
        }
        else
        {
            characters = new List<GameObject>();
            characters.Add(gameObject);
            characterPerCell.Add(position, characters);
        }
    }

    public bool IsCellOccupied(Vector3Int position, out List<GameObject> characters)
    {
        characters = null;
        if (characterPerCell.TryGetValue(position, out characters))
        {
            return true;
        }
        return false;
    }

    public void Unsubscribe(Vector3Int position, GameObject gameObject)
    {
        List<GameObject> characters;
        if (characterPerCell.TryGetValue(position, out characters))
        {
            characters.Remove(gameObject);
        }
    }

    public void GetPrioritySurroundingCharacters(Vector3Int position, Vector3Int startDirection, out Vector3Int priorityDirection, out List<GameObject> characters)
    {
        characters = new List<GameObject>();
        priorityDirection = startDirection;
        List<Vector3Int> adjacencyRules;
        FillAdjacencyList(startDirection, out adjacencyRules);
        foreach(Vector3Int dir in adjacencyRules)
        {
            List<GameObject> partialCharacters;
            if (characterPerCell.TryGetValue(position, out partialCharacters))
            {
                priorityDirection = dir;
                characters.AddRange(partialCharacters);
                return;
            }
        }
    }

    public void GetAllSurroundingCharacters(Vector3Int position, Vector3Int startDirection, out List<GameObject> characters)
    {
        characters = new List<GameObject>();
        List<Vector3Int> adjacencyRules;
        FillAdjacencyList(startDirection, out adjacencyRules);
        foreach (Vector3Int dir in adjacencyRules)
        {
            List<GameObject> partialCharacters;
            if (characterPerCell.TryGetValue(position, out partialCharacters))
            {
                characters.AddRange(partialCharacters);
            }
        }
    }

}

public class MapManager : MonoBehaviour
{
    [SerializeField]
    public Tilemap map;

    [SerializeField]
    private List<TileData> tileDatas;

    private Dictionary<TileBase, TileData> dataFromTiles;

    public LogicGrid logicGrid = new LogicGrid();

    Vector2 mousePosition;
    Vector3 mouseWorldPosition;

    private void Awake()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();
        foreach(var tileData in tileDatas)
        {
            foreach( var tile in tileData.tiles)
            {
                dataFromTiles.Add(tile, tileData);
            }
        }
    }

    public float GetSpeedModifier(Vector3Int cellPosition)
    {
        float speedModifier = 1f;
        TileBase tile = map.GetTile(cellPosition);
        
        if(tile != null && dataFromTiles.ContainsKey(tile))
        {
            speedModifier = dataFromTiles[tile].speedModifier;
        }

        return speedModifier;
    }
    public float GetPoisonModifier(Vector3Int cellPosition)
    {
        float poisonModifier = 0f;
        TileBase tile = map.GetTile(cellPosition);

        if (tile != null && dataFromTiles[tile] != null)
        {
            poisonModifier = dataFromTiles[tile].poisonModifier;
        }

        return poisonModifier;
    }
}
