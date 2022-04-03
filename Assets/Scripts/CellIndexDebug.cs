using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellIndexDebug : MonoBehaviour
{
    private Camera mainCamera;
    private MapManager mapManager;

    private void Awake()
    { 
        mapManager = FindObjectOfType<MapManager>();
        mainCamera = FindObjectOfType<Camera>();
    }

    private void OnGUI()
    {
        BoundsInt bounds = mapManager.map.cellBounds;
        for(int i = bounds.xMin; i<bounds.xMax; i++)
        {
            for (int j = bounds.yMin; j < bounds.yMax; j++)
            {
                Vector3 screenPoint = mainCamera.WorldToScreenPoint(mapManager.map.CellToWorld(new Vector3Int(i,j,0)));
                GUI.Label(new Rect(screenPoint.x,mainCamera.pixelHeight-screenPoint.y,40,20),""+i+","+j);
            }
        }
    }
}
