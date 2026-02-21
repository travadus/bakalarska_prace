using UnityEngine;
using UnityEngine.UI;

public class BuildingSelectButton : MonoBehaviour
{
    [SerializeField] private PlacedObjectTypeSO buildingType;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => {
            GridBuildingSystem.Instance.SelectObjectType(buildingType);
        });
    }
}