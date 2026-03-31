using UnityEngine;
using TMPro;
using System.Reflection;
using System.Linq;

public class HelpManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject methodPrefab;
    public Transform contentParent;

    private void Start()
    {
        FillHelpWindow();
    }

    public void FillHelpWindow()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        var methods = typeof(GameAPI).GetMethods(BindingFlags.Public | BindingFlags.Static);

        foreach (var method in methods)
        {
            // KROK 1: Zkusíme najít náš "štítek" (atribut) nad metodou
            var docAttribute = method.GetCustomAttribute<APIDocAttribute>();

            // Pokud metoda nemá [APIDoc], pøeskoèíme ji
            if (docAttribute == null) continue;

            GameObject entry = Instantiate(methodPrefab, contentParent);

            // KROK 2: Formátování názvu a parametrù
            string paramsInfo = string.Join(", ", method.GetParameters()
                .Select(p => $"{SimplifyTypeName(p.ParameterType.Name)} {p.Name}"));

            string fullSignature = $"{method.Name}({paramsInfo})";

            if (entry.transform.Find("MethodName Text").TryGetComponent<TextMeshProUGUI>(out var nameText))
            {
                nameText.text = fullSignature;
            }

            // KROK 3: Výpis popisu pøímo z atributu
            if (entry.transform.Find("MethodDescription Text").TryGetComponent<TextMeshProUGUI>(out var descText))
            {
                descText.text = docAttribute.Description;
            }
        }
    }

    // Pomocná funkce, aby v UI nebylo "Single", ale "float"
    private string SimplifyTypeName(string typeName)
    {
        switch (typeName)
        {
            case "Single": return "float";
            case "Int32": return "int";
            case "Boolean": return "bool";
            case "String": return "string";
            default: return typeName;
        }
    }
}