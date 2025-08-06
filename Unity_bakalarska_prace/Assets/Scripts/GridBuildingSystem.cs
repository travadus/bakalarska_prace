using UnityEngine;
using UnityEngine.EventSystems;

public class GridBuildingSystem : MonoBehaviour
{

    [SerializeField] private Transform testTransform;
    private GridXZ<GridObject> grid;

    private void Awake()
    {
        int gridWidth = 10;
        int gridHeight = 10;
        float cellSize = 10f;
        grid = new GridXZ<GridObject>(gridWidth, gridHeight, cellSize, new Vector3(-50, 0, -50), (GridXZ<GridObject> g, int x, int z) => new GridObject(g, x, z));
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

            GridObject gridObject = grid.GetGridObject(x, z);

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (gridObject.CanBuild())
            {
                Transform buildTransform = Instantiate(testTransform, grid.GetWorldPosition(x, z), Quaternion.identity);
                gridObject.SetTransform(buildTransform);
            }
            else
            {
                //Pøidat pop-up okno, kde se toto bude psat!
                Debug.Log("Cannot build here!");
            }
            
        }
    }

}
