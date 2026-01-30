using UnityEngine;
using UnityEngine.UI;

public class BuildingSelectButton : MonoBehaviour
{
    [SerializeField] private PlacedObjectTypeSO buildingType; // Sem pøetáhni ScriptableObject konkrétní budovy

    private void Awake()
    {
        // Najdeme tlaèítko na stejném objektu a pøidáme mu funkci
        GetComponent<Button>().onClick.AddListener(() => {
            GridBuildingSystem.Instance.SelectObjectType(buildingType);
        });
    }
}