using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathAndPositionManager
{
    public MapManager mapManager;

    public void Start()
    {
        if (cellList.Count > 0)
        {
            startPosition = currentPosition;
            currentIndex = 0;
            UpdateFacingDirection();
        }
    }
    public Vector3Int GetCurrentTargetCell() { return HasArrived()? currentPosition : cellList[currentIndex]; }
    public Vector3Int GetPreviousTargetCell()
    {
        if (currentIndex <= 0)
        {
            return startPosition;
        }
        if(currentIndex>=cellList.Count)
        {
            return currentPosition;
        }
        return cellList[currentIndex - 1];
    }
    public Vector3 GetCurrentTarget() 
    {
        Vector3 result= mapManager.map.CellToWorld(GetCurrentTargetCell());
        result.y += yOffset;
        return result;
    }
    public Vector3 GetPreviousTarget()
    {
        Vector3 result = mapManager.map.CellToWorld(GetPreviousTargetCell());
        result.y += yOffset;
        return result;
    }
    public Vector3Int GetCurrentLastFacingDirection() { return facingDirection; }
    public bool HasArrived() { return currentIndex >= cellList.Count; }
    public void UpdateCurrentPosition(Vector3 updatedCurrentPosition)
    { 
        currentWorldPosition = updatedCurrentPosition;
        currentPosition = mapManager.map.WorldToCell(updatedCurrentPosition); 
    }
    public bool TryAdvancePath()
    {
        if (currentIndex >= 0 && !HasArrived() && currentPosition == GetCurrentTargetCell())
        {
            //we need to check if we're close enough to the center of the cell
             Vector3 distanceVec = GetCurrentTarget()-currentWorldPosition;
             if(distanceVec.sqrMagnitude<float.Epsilon)
             {
                currentIndex++;
                UpdateFacingDirection();
                return true;
             }
            
        }
        return false;
    }   
    
    private void UpdateFacingDirection() { facingDirection = GetCurrentTargetCell() - currentPosition; }

    public Vector3 currentWorldPosition = new Vector3(0, 0, 0);

    private int currentIndex = -1;
    public List<Vector3Int> cellList;
    public Vector3Int startPosition = new Vector3Int(0, 0, 0);
    public Vector3Int currentPosition = new Vector3Int(0, 0, 0);
    public Vector3Int facingDirection = new Vector3Int(0, 0, 0 );

    private float yOffset = 0.1f;
}

public class MovingCharacterScript : MonoBehaviour
{
    private MapManager mapManager;

    //base speed in grid cells per second
    public float baseSpeed;

    public PathAndPositionManager pathManager = new PathAndPositionManager();

    float amountToMoveTowardsTarget = 0f;

void Awake()
    {
        mapManager = FindObjectOfType<MapManager>();
        pathManager.mapManager = mapManager;
    }

    private void Start()
    {
        pathManager.UpdateCurrentPosition(transform.position);
        List<Vector3Int> newList = new List<Vector3Int>();
        Vector3Int position1 = new Vector3Int(6, 2, 0);
        Vector3Int position2 = new Vector3Int(6, 3, 0);
        newList.Add(position1);
        newList.Add(position2);
        StartPath(newList);
    }

    public void StartPath(List<Vector3Int> newPath)
    {
        pathManager.cellList = newPath;
        pathManager.Start();
    }

    private void FixedUpdate()
    {
        pathManager.UpdateCurrentPosition(transform.position);
        if (pathManager.TryAdvancePath())
        {
            amountToMoveTowardsTarget = 0f;
            if (pathManager.HasArrived())
            {
                print("I have arrived");
                return;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Get current speed expressed in grid cells per second
        float currentSpeed = baseSpeed * mapManager.GetSpeedModifier(pathManager.currentPosition);
        amountToMoveTowardsTarget += currentSpeed * Time.deltaTime;
        Mathf.Clamp(amountToMoveTowardsTarget, 0f, 1f);

        transform.position = Vector3.Lerp(pathManager.GetPreviousTarget(), pathManager.GetCurrentTarget(), amountToMoveTowardsTarget);
    }
}