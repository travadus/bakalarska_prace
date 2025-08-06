using System.Collections.Generic;
using UnityEngine;

public class PlacedObject_Done : MonoBehaviour
{
    public static PlacedObject_Done Create(Vector3 worldPosition, Vector2Int origin, PlacedObjectTypeSO placedObjectTypeSO)
    {
        Transform placedObjectTransform = Instantiate(placedObjectTypeSO.prefab, worldPosition, Quaternion.identity);

        PlacedObject_Done placedObject = placedObjectTransform.GetComponent<PlacedObject_Done>();
        placedObject.Setup(placedObjectTypeSO, origin);

        return placedObject;
    }

    private PlacedObjectTypeSO placedObjectTypeSO;
    private Vector2Int origin;

    private void Setup(PlacedObjectTypeSO placedObjectTypeSO, Vector2Int origin)
    {
        this.placedObjectTypeSO = placedObjectTypeSO;
        this.origin = origin;
    }

    public List<Vector2Int> GetGridPositionList()
    {
        return placedObjectTypeSO.GetGridPositionList(origin);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public override string ToString()
    {
        return placedObjectTypeSO.nameString;
    }

}
