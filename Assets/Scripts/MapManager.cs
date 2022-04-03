using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class LogicGrid
{
    private Dictionary<Vector3Int, List<GameObject>> characterPerCell = new Dictionary<Vector3Int, List<GameObject>>();

    private Vector3Int left = new Vector3Int(-1,0,0);
    private Vector3Int right = new Vector3Int(1, 0, 0);

    private Vector3Int upLeftEven = new Vector3Int(-1, 1, 0);
    private Vector3Int upRightEven = new Vector3Int(0, 1, 0);
    private Vector3Int downLeftEven = new Vector3Int(-1, -1, 0);
    private Vector3Int downRightEven = new Vector3Int(0, -1, 0);

    private Vector3Int upLeftOdd = new Vector3Int(0, 1, 0);
    private Vector3Int upRightOdd = new Vector3Int(1, 1, 0);
    private Vector3Int downLeftOdd = new Vector3Int(0, -1, 0);
    private Vector3Int downRightOdd = new Vector3Int(1, -1, 0);

    public static bool IsValidDirection(Vector3Int position, Vector3Int direction)
    {
        if(position.y % 2 == 0)
        {
            return !((direction.x == 0 && direction.y == 0) ||
                (direction.x > 0 && direction.y != 0));
        }
        else
        {
            return !((direction.x == 0 && direction.y == 0) ||
               (direction.x < 0  && direction.y != 0));
        }
    }

    public void FillAdjacencyList(Vector3Int curPosition,Vector3Int startDirection, out List<Vector3Int> adjacencyList)
    {
        if (!IsValidDirection(curPosition,startDirection))
        {
            adjacencyList = null;
            return;
        }

        startDirection.z = 0;

        startDirection.x= Mathf.Clamp(startDirection.x, -1, 1);
        startDirection.y= Mathf.Clamp(startDirection.y, -1, 1);

        adjacencyList = new List<Vector3Int>();
        adjacencyList.Add(startDirection);
        if (curPosition.y % 2 != 0)
        {
            if (startDirection == upRightOdd)
            {
                adjacencyList.Add(upLeftOdd);
                adjacencyList.Add(right);
                adjacencyList.Add(left);
                adjacencyList.Add(downRightOdd);
                adjacencyList.Add(downLeftOdd);
            }
            else if (startDirection == right)
            {
                adjacencyList.Add(upRightOdd);
                adjacencyList.Add(downRightOdd);
                adjacencyList.Add(upLeftOdd);
                adjacencyList.Add(downLeftOdd);
                adjacencyList.Add(left);
            }
            else if (startDirection == downRightOdd)
            {
                adjacencyList.Add(right);
                adjacencyList.Add(downLeftOdd);
                adjacencyList.Add(upRightOdd);
                adjacencyList.Add(left);
                adjacencyList.Add(upLeftOdd);
            }
            else if (startDirection == downLeftOdd)
            {
                adjacencyList.Add(downRightOdd);
                adjacencyList.Add(left);
                adjacencyList.Add(right);
                adjacencyList.Add(upLeftOdd);
                adjacencyList.Add(upRightOdd);
            }
            else if (startDirection == left)
            {
                adjacencyList.Add(downLeftOdd);
                adjacencyList.Add(upLeftOdd);
                adjacencyList.Add(downRightOdd);
                adjacencyList.Add(upRightOdd);
                adjacencyList.Add(right);
            }
            else if (startDirection == upLeftOdd)
            {
                adjacencyList.Add(left);
                adjacencyList.Add(upRightOdd);
                adjacencyList.Add(downLeftOdd);
                adjacencyList.Add(right);
                adjacencyList.Add(downRightOdd);
            }
        }
        else
        {
            if (startDirection == upRightEven)
            {
                adjacencyList.Add(upLeftEven);
                adjacencyList.Add(right);
                adjacencyList.Add(left);
                adjacencyList.Add(downRightEven);
                adjacencyList.Add(downLeftEven);
            }
            else if (startDirection == right)
            {
                adjacencyList.Add(upRightEven);
                adjacencyList.Add(downRightEven);
                adjacencyList.Add(upLeftEven);
                adjacencyList.Add(downLeftEven);
                adjacencyList.Add(left);
            }
            else if (startDirection == downRightEven)
            {
                adjacencyList.Add(right);
                adjacencyList.Add(downLeftEven);
                adjacencyList.Add(upRightEven);
                adjacencyList.Add(left);
                adjacencyList.Add(upLeftEven);
            }
            else if (startDirection == downLeftEven)
            {
                adjacencyList.Add(downRightEven);
                adjacencyList.Add(left);
                adjacencyList.Add(right);
                adjacencyList.Add(upLeftEven);
                adjacencyList.Add(upRightEven);
            }
            else if (startDirection == left)
            {
                adjacencyList.Add(downLeftEven);
                adjacencyList.Add(upLeftEven);
                adjacencyList.Add(downRightEven);
                adjacencyList.Add(upRightEven);
                adjacencyList.Add(right);
            }
            else if (startDirection == upLeftEven)
            {
                adjacencyList.Add(left);
                adjacencyList.Add(upRightEven);
                adjacencyList.Add(downLeftEven);
                adjacencyList.Add(right);
                adjacencyList.Add(downRightEven);
            }
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
        FillAdjacencyList(position,startDirection, out adjacencyRules);
        foreach(Vector3Int dir in adjacencyRules)
        {
            List<GameObject> partialCharacters;
            if (characterPerCell.TryGetValue(position+dir, out partialCharacters))
            {
                if(partialCharacters.Count>0)
                {
                    priorityDirection = dir;
                    characters.AddRange(partialCharacters);
                    return;
                }
            }
        }
    }

    public void GetAllSurroundingCharacters(Vector3Int position, Vector3Int startDirection, out List<GameObject> characters)
    {
        characters = new List<GameObject>();
        List<Vector3Int> adjacencyRules;
        FillAdjacencyList(position, startDirection, out adjacencyRules);
        foreach (Vector3Int dir in adjacencyRules)
        {
            List<GameObject> partialCharacters;
            if (characterPerCell.TryGetValue(position+dir, out partialCharacters))
            {
                if (partialCharacters.Count > 0)
                {
                    characters.AddRange(partialCharacters);
                }
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

        if (tile != null && dataFromTiles.ContainsKey(tile))
        {
            poisonModifier = dataFromTiles[tile].poisonModifier;
        }
        return poisonModifier;
    }
}
