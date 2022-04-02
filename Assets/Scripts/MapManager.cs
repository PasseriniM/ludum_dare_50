using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class MapManager : MonoBehaviour
{
    [SerializeField]
    public Tilemap map;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private List<TileData> tileDatas;

    private Dictionary<TileBase, TileData> dataFromTiles;

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

    public void OnPoint(InputAction.CallbackContext context)
    {
        mousePosition = context.ReadValue<Vector2>();
        mouseWorldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        Vector3Int gridPosition = map.WorldToCell(mouseWorldPosition);

        TileBase clickedTile = map.GetTile(gridPosition);

        float speedModifier = dataFromTiles[clickedTile].speedModifier;
        float poisonModifier = dataFromTiles[clickedTile].poisonModifier;

        print("Position " + gridPosition + " there is a " + clickedTile + " which has speed mod " + speedModifier + " and poison mod "+ poisonModifier);
    }
}
