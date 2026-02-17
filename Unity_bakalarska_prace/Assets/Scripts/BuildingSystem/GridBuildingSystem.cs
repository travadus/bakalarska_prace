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
        // 1. Deselect with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectObjectType();
            return;
        }

        // 2. If nothing selected, do nothing
        if (placedObjectTypeSO == null)
        {
            return;
        }

        // 3. Building Logic
        if (Input.GetMouseButtonDown(0))
        {
            // Check UI click
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 mouseWorldPosition = Mouse3D.GetMouseWorldPosition();
            if (mouseWorldPosition == Vector3.zero) return;

            grid.GetXZ(mouseWorldPosition, out int x, out int z);

            // Check grid bounds
            if (x < 0 || z < 0 || x >= grid.GetWidth() || z >= grid.GetHeight())
            {
                return;
            }

            GridObject gridObject = grid.GetGridObject(x, z);

            if (gridObject.CanBuild())
            {
                // --- NEW: ECONOMY CHECK START ---

                // 1. Check if we have an EconomyManager instance
                if (EconomyManager.Instance == null)
                {
                    Debug.LogError("EconomyManager is missing in the scene!");
                    return;
                }

                // 2. Check if player can afford the building cost defined in ScriptableObject
                // (Make sure you added 'public float constructionCost;' to PlacedObjectTypeSO)
                float cost = placedObjectTypeSO.constructionCost;

                if (EconomyManager.Instance.CanAfford(cost))
                {
                    // 3. DEDUCT MONEY
                    // Using "Construction" as the category/reason for the log
                    EconomyManager.Instance.TrySpendMoney(cost, $"Construction: {placedObjectTypeSO.nameString}");

                    // 4. BUILD (Original logic)
                    Transform buildTransform = Instantiate(placedObjectTypeSO.prefab, grid.GetWorldPosition(x, z), Quaternion.identity);
                    gridObject.SetTransform(buildTransform);

                    // 5. Fire Events
                    OnObjectPlaced?.Invoke(this, EventArgs.Empty);

                    // Optional: Visual/Audio feedback for spending money
                    if (PlayerScriptEngine.Instance != null)
                    {
                        PlayerScriptEngine.Instance.LogSystemMessage($"Built {placedObjectTypeSO.nameString} for {cost} €.");
                    }
                }
                else
                {
                    // PLAYER IS BROKE
                    if (PlayerScriptEngine.Instance != null)
                    {
                        PlayerScriptEngine.Instance.LogMessage($"Insufficient funds! Required: {cost} €", Color.red);
                    }
                    else
                    {
                        Debug.Log("Not enough money!");
                    }
                }
                // --- ECONOMY CHECK END ---
            }
            else
            {
                Debug.Log("Area is occupied!");
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
