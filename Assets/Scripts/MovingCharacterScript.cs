using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathAndPositionManager
{
    public MapManager mapManager;

    public void Start(List<Vector3Int> newPath)
    {
        bool oldArrived = false;
        Vector3Int oldTarget = currentPosition;
        Vector3Int oldPrevious = currentPosition;
        if (cellList!=null && cellList.Count > 0)
        {
            oldArrived = HasArrived();
            oldTarget = GetCurrentTargetCell();
            oldPrevious = GetPreviousTargetCell();
        }
        startPosition = currentPosition;
        currentIndex = 0;
        cellList = newPath;
        if (cellList.Count > 0)
        {
            if (!oldArrived && oldTarget != GetCurrentTargetCell())
            {
                //we need to change direction
                if (currentPosition != GetCurrentTargetCell())
                {
                    cellList.Insert(0, currentPosition);
                    cellList.Insert(0, oldTarget);
                    currentIndex = 1;
                }
            }

            cellList.Insert(0, oldPrevious);
            currentIndex = 1;
            UpdateFacingDirection();
        }
    }
    public Vector3Int GetCurrentTargetCell() { return !IsMoving()? currentPosition : cellList[currentIndex]; }
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
        result.z = zOffset;
        return result;
    }
    public Vector3 GetPreviousTarget()
    {
        Vector3 result = mapManager.map.CellToWorld(GetPreviousTargetCell());
        result.y += yOffset;
        result.z = zOffset;
        return result;
    }
    public bool IsMoving() { return currentIndex >= 0 && !HasArrived(); }
    public bool HasArrived() { return currentIndex >= cellList.Count; }
    public void UpdateCurrentPosition(Vector3 updatedCurrentPosition)
    { 
        currentWorldPosition = updatedCurrentPosition;
        currentPosition = mapManager.map.WorldToCell(updatedCurrentPosition); 
    }
    public bool TryAdvancePath()
    {
        if (IsMoving() && currentPosition == GetCurrentTargetCell())
        {
            //we need to check if we're close enough to the center of the cell
             Vector3 distanceVec = GetCurrentTarget()-currentWorldPosition;
             if(distanceVec.sqrMagnitude<0.00001f)
             {
                currentIndex++;
                UpdateFacingDirection();
                return true;
             }
            
        }
        return false;
    }   
    
    private void UpdateFacingDirection() 
    {
        Vector3Int newFacing =  GetCurrentTargetCell() - currentPosition;
        if(LogicGrid.IsValidDirection(currentPosition,newFacing))
        {
            facingDirection = newFacing;
        }
    }

    public Vector3 currentWorldPosition = new Vector3(0, 0, 0);

    private int currentIndex = -1;
    public List<Vector3Int> cellList;
    public Vector3Int startPosition = new Vector3Int(0, 0, 0);
    public Vector3Int currentPosition = new Vector3Int(0, 0, 0);
    public Vector3Int facingDirection = new Vector3Int(1, 0, 0 );

    public float yOffset = 0f;
    public float zOffset = 0f;
}

public class MovingCharacterScript : MonoBehaviour
{
    private MapManager mapManager;

    //seconds necessary to move from one cell to the next
    public float timeBetweenCells;

    public PathAndPositionManager pathManager = new PathAndPositionManager();
    private List<Vector3Int> lastPath;
    private float amountToMove = 0f;

    void Awake()
    {
        mapManager = FindObjectOfType<MapManager>();
        pathManager.mapManager = mapManager;
    }

    private void Start()
    {
        pathManager.zOffset = transform.position.z;
        pathManager.UpdateCurrentPosition(transform.position);
    }

    public void StartPath(List<Vector3Int> newPath)
    {
        pathManager.Start(newPath);
        amountToMove = Mathf.Clamp(InverseLerp(pathManager.GetPreviousTarget(), pathManager.GetCurrentTarget(), pathManager.currentWorldPosition), 0f, 1f);
    }

    public void MemorizeBackupPath()
    {
        lastPath = pathManager.cellList;
    }

    public List<Vector3Int> GetOldTrimmedPath()
    {
        List<Vector3Int> trimmedPath = new List<Vector3Int>(); 
        bool addNextCell = false;
        foreach (Vector3Int cell in lastPath)
        {
            if (addNextCell)
            {
                trimmedPath.Add(cell);
            }
            if (cell == pathManager.currentPosition)
            {
                addNextCell = true;
            }
        }
        return trimmedPath;
    }

    public void ResumeOldPath()
    {
        List<Vector3Int> trimmedPath = GetOldTrimmedPath();
        
        if(trimmedPath.Count>0)
        {
            StartPath(trimmedPath);
        }
        else
        {
            StartPath(lastPath);
        }
        MemorizeBackupPath();
    }

    public bool HasReachedBackupPathDestination()
    {
        return pathManager.currentPosition == lastPath[lastPath.Count - 1];
    }

    private void FixedUpdate()
    {
        pathManager.UpdateCurrentPosition(transform.position);
        if (pathManager.TryAdvancePath())
        {
            amountToMove = 0f;
            if (pathManager.HasArrived())
            {
                print("I have arrived");
                return;
            }
        }
    }
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        if(a==b || value==b)
        {
            return 1f;
        }
        if(value == a)
        {
            return 0f;
        }
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
    }

    // Update is called once per frame
    void Update()
    {
        //Get current speed expressed in grid cells per second
        float baseSpeed = 1f / timeBetweenCells;
        float currentSpeed = baseSpeed * mapManager.GetSpeedModifier(pathManager.currentPosition);
        float amountToMoveTowardsTarget = currentSpeed * Time.deltaTime;
        amountToMove += amountToMoveTowardsTarget;
        amountToMove = Mathf.Clamp(amountToMove, 0f, 1f);
        transform.position = Vector3.Lerp(pathManager.GetPreviousTarget(), pathManager.GetCurrentTarget(), amountToMove);
    }
}
