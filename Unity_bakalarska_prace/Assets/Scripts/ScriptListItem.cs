using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScriptListItem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText; // Text na tlaèítku

    private ScriptData myData;       // Data, která toto tlaèítko drží

    public void Setup(ScriptData data)
    {
        myData = data;
        nameText.text = data.scriptName;

        // Nastavíme, co se stane po kliknutí
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        // Øekneme manažerovi: "Otevøi editor pro moje data"
        ScriptFileManager.Instance.OpenEditorFor(myData);

        // Volitelnì: Zavøeme panel se seznamem (pokud to tak chceš)
        // ScriptFileManager.Instance.CloseScriptsPanel();
    }
}