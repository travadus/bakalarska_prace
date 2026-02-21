using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Core system managing grid-based object placement.
/// </summary>
public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance { get; private set; }

    [SerializeField] private List<PlacedObjectTypeSO> placedObjectTypeSOList;
    private PlacedObjectTypeSO placedObjectTypeSO;
    private GridXZ<GridObject> grid;

    /// <summary>
    /// Invoked when the currently selected building type changes or is deselected.
    /// </summary>
    public event EventHandler OnSelectedChanged;

    /// <summary>
    /// Invoked successfully upon the physical placement of a new object on the grid.
    /// </summary>
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

    /// <summary>
    /// Represents a single cell within the construction grid.
    /// </summary>
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

    /// <summary>
    /// Processes player input for deselecting objects or initiating the construction sequence.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectObjectType();
            return;
        }

        if (placedObjectTypeSO == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Vector3 mouseWorldPosition = Mouse3D.GetMouseWorldPosition();
            if (mouseWorldPosition == Vector3.zero) return;

            grid.GetXZ(mouseWorldPosition, out int x, out int z);

            if (x < 0 || z < 0 || x >= grid.GetWidth() || z >= grid.GetHeight())
            {
                return;
            }

            GridObject gridObject = grid.GetGridObject(x, z);

            if (gridObject.CanBuild())
            {
                if (EconomyManager.Instance == null)
                {
                    Debug.LogError("EconomyManager is missing in the scene!");
                    return;
                }

                float cost = placedObjectTypeSO.constructionCost;

                if (EconomyManager.Instance.CanAfford(cost))
                {
                    EconomyManager.Instance.TrySpendMoney(cost, $"Construction: {placedObjectTypeSO.nameString}");

                    Transform buildTransform = Instantiate(placedObjectTypeSO.prefab, grid.GetWorldPosition(x, z), Quaternion.identity);
                    gridObject.SetTransform(buildTransform);

                    OnObjectPlaced?.Invoke(this, EventArgs.Empty);

                    if (PlayerScriptEngine.Instance != null)
                    {
                        PlayerScriptEngine.Instance.LogSystemMessage($"Built {placedObjectTypeSO.nameString} for {cost} €.");
                    }
                }
                else
                {
                    if (PlayerScriptEngine.Instance != null)
                    {
                        PlayerScriptEngine.Instance.LogMessage($"Insufficient funds! Required: {cost} €", Color.red);
                    }
                    else
                    {
                        Debug.Log("Not enough money!");
                    }
                }
            }
            else
            {
                Debug.Log("Area is occupied!");
            }
        }
    }

    /// <summary>
    /// Sets the active building type for placement.
    /// </summary>
    /// <param name="placedObjectTypeSO">The ScriptableObject defining the building to be placed.</param>
    public void SelectObjectType(PlacedObjectTypeSO placedObjectTypeSO)
    {
        this.placedObjectTypeSO = placedObjectTypeSO;
        RefreshSelectedObjectType();
    }

    /// <summary>
    /// Clears the currently selected building type, aborting placement mode.
    /// </summary>
    public void DeselectObjectType()
    {
        placedObjectTypeSO = null;
        RefreshSelectedObjectType();
    }

    private void RefreshSelectedObjectType()
    {
        OnSelectedChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Calculates the world position snapped to the nearest grid cell based on current mouse coordinates.
    /// </summary>
    /// <returns>The snapped Vector3 position, or raw mouse position if no object is selected.</returns>
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