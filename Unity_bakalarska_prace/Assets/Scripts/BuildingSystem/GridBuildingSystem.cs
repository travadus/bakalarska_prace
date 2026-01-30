using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance { get; private set; }

    [SerializeField] private List<PlacedObjectTypeSO> placedObjectTypeSOList;
    private PlacedObjectTypeSO placedObjectTypeSO;
    private GridXZ<GridObject> grid;

    public event EventHandler OnSelectedChanged;

    // Event po položení
    public event EventHandler OnObjectPlaced;

    private void Awake()
    {
        Instance = this;

        int gridWidth = 10;
        int gridHeight = 10;
        float cellSize = 10f;
        grid = new GridXZ<GridObject>(gridWidth, gridHeight, cellSize, new Vector3(-50, 0, -50), (GridXZ<GridObject> g, int x, int z) => new GridObject(g, x, z));

        placedObjectTypeSO = null;
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
            grid.TriggerGridObjectChanged(x, z);
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
        // 1. Logika pro zrušení výbìru klávesou ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectObjectType();
            return;
        }

        // 2. Pokud nemáme vybranou budovu, nic nestavíme a konèíme Update
        if (placedObjectTypeSO == null)
        {
            return;
        }

        // 3. Stavìní budovy
        if (Input.GetMouseButtonDown(0))
        {
            // Kontrola, zda neklikáme do UI (tlaèítek)
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            grid.GetXZ(Mouse3D.GetMouseWorldPosition(), out int x, out int z);

            Vector3 wp = Mouse3D.GetMouseWorldPosition();
            if (wp == Vector3.zero) return; // Kliknutí mimo validní plochu (Mouse3D vrací zero)

            // Kontrola hranic gridu
            if (x < 0 || z < 0 || x >= grid.GetWidth() || z >= grid.GetHeight())
            {
                // Debug.Log("Mimo hranice gridu!");
                return;
            }

            GridObject gridObject = grid.GetGridObject(x, z);

            if (gridObject.CanBuild())
            {
                // Instanciace objektu
                Transform buildTransform = Instantiate(placedObjectTypeSO.prefab, grid.GetWorldPosition(x, z), Quaternion.identity);
                gridObject.SetTransform(buildTransform);

                // Vyvolání eventu po položení (pro zvuk, èástice, atd.)
                OnObjectPlaced?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // Zde mùžeš pozdìji napojit ten Pop-up
                Debug.Log("Zde už nìco stojí!");
                // UtilsClass.CreateWorldTextPopup("Cannot Build Here!", Mouse3D.GetMouseWorldPosition());
            }
        }
    }

    public void SelectObjectType(PlacedObjectTypeSO placedObjectTypeSO)
    {
        this.placedObjectTypeSO = placedObjectTypeSO;
        RefreshSelectedObjectType();
    }

    public void DeselectObjectType()
    {
        placedObjectTypeSO = null;
        RefreshSelectedObjectType();
    }

    private void RefreshSelectedObjectType()
    {
        OnSelectedChanged?.Invoke(this, EventArgs.Empty);
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
