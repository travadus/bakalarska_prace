using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance { get; private set; }

    [SerializeField] private PlacedObjectTypeSO placedObjectTypeSO;
    private GridXZ<GridObject> grid;

    public event EventHandler OnSelectedChanged;
    public event EventHandler OnObjectPlaced;

    private void Awake()
    {
        Instance = this;

        int gridWidth = 10;
        int gridHeight = 10;
        float cellSize = 10f;
        grid = new GridXZ<GridObject>(gridWidth, gridHeight, cellSize, new Vector3(-50, 0, -50), (GridXZ<GridObject> g, int x, int z) => new GridObject(g, x, z));

        //placedObjectTypeSO = null;
    }

    public class GridObject
    {
        private GridXZ<GridObject> grid;
        private int x;
        private int z;
        private Transform transform;

        public GridObject(GridXZ<GridObject> grid, int x, int z)
        {
            this.grid = grid;
            this.x = x; 
            this.z = z;
        }

        public void SetTransform(Transform transform)
        {
            this.transform = transform;
            grid.TriggerGridObjectChanged(x, z);
        }

        public void ClearTransform()
        {
            transform = null;
        }

        public bool CanBuild()
        {
            return transform == null;
        }

        public override string ToString()
        {
            return x + ", " + z + "\n" + transform;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            grid.GetXZ(Mouse3D.GetMouseWorldPosition(), out int x, out int z);

            Vector3 wp = Mouse3D.GetMouseWorldPosition();
            if (wp == Vector3.zero) { Debug.Log("Cannot build here!"); return; }

            if (x < 0 || z < 0 || x >= grid.GetWidth() || z >= grid.GetHeight())
            {
                Debug.Log("Cannot build here!");
                return;
            }

            GridObject gridObject = grid.GetGridObject(x, z);

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (gridObject.CanBuild())
            {
                Transform buildTransform = Instantiate(placedObjectTypeSO.prefab, grid.GetWorldPosition(x, z), Quaternion.identity);
                gridObject.SetTransform(buildTransform);
            }
            else
            {
                //Pøidat pop-up okno, kde se toto bude psat!
                Debug.Log("Cannot build here!");
            }
            
        }
    }

    public Vector3 GetMouseWorldSnappedPosition()
    {
        Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
        grid.GetXZ(mousePosition, out int x, out int z);

        if (placedObjectTypeSO != null)
        {
            return grid.GetWorldPosition(x, z);
        }
        else
        {
            return mousePosition;
        }
    }

    public PlacedObjectTypeSO GetPlacedObjectTypeSO()
    {
        return placedObjectTypeSO;
    }

}
