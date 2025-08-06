using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingTypeSO", menuName = "ScriptableObjects/BuildingTypeSO")]
public class PlacedObjectTypeSO
{
    public string nameString;
    public Transform prefab;
    public Transform visual;
    public int width;
    public int length;

    public List<Vector2Int> GetGridPositionList(Vector2Int offset)
    {
        List<Vector2Int> gridPositionList = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                gridPositionList.Add(offset + new Vector2Int(x, y));
            }
        }

        return gridPositionList;
    }
}
