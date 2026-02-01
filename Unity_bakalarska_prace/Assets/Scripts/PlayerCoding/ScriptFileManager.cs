using UnityEngine;
using System.Collections.Generic;

public class ScriptFileManager : MonoBehaviour
{
    public static ScriptFileManager Instance;

    // Seznam všech skriptù (tohle se bude v budoucnu ukládat do JSONu)
    public List<ScriptData> allScripts = new List<ScriptData>();

    private Dictionary<ScriptData, CodeWindow> openWindows = new Dictionary<ScriptData, CodeWindow>();

    [Header("Prefabs")]
    public GameObject codeWindowPrefab;
    public Transform canvasParent;

    [Header("List UI Settings")]
    public Transform listContentContainer; // ZDE pøetáhni ten objekt "Content" ze ScrollView
    public GameObject listButtonPrefab;    // ZDE pøetáhni ten tvùj nový prefab tlaèítka
    public GameObject scriptsPanel;        // ZDE pøetáhni celý ten Panel se seznamem (pro zavírání/otvírání)

    private void Awake()
    {
        Instance = this;

        if (scriptsPanel != null) scriptsPanel.SetActive(false);
    }

    // Už nepotøebujeme parametr 'name', vygenerujeme si ho sami
    public void CreateNewScript()
    {
        // Vymyslíme unikátní název (Script 1, Script 2...)
        int count = allScripts.Count + 1;
        string autoName = $"Script {count}";

        // Kontrola, jestli název už náhodou neexistuje (pro jistotu)
        while (DoesScriptExist(autoName))
        {
            count++;
            autoName = $"Script {count}";
        }

        string defaultContent =
            "public void Main()\n" +
            "{\n" +
            "    \n" +
            "}";

        ScriptData newScript = new ScriptData(autoName, defaultContent);
        allScripts.Add(newScript);

        RefreshFileListUI(); // Aktualizujeme seznam tlaèítek

        // VOLITELNÉ: Rovnou otevøeme editor pro ten nový skript
        OpenEditorFor(newScript); 
    }

    public void OpenEditorFor(ScriptData data)
    {
        // 1. KONTROLA: Je už okno pro tento skript otevøené?
        if (openWindows.ContainsKey(data) && openWindows[data] != null)
        {
            // ANO, je otevøené -> Pøeneseme ho do popøedí (aby bylo vidìt)
            openWindows[data].transform.SetAsLastSibling();

            // Mùžeme ho i trochu zvýraznit (volitelné)
            // openWindows[data].HighlightWindow();

            return; // Konec, nové okno nevytváøíme
        }

        // 2. Pokud není otevøené -> Vytvoøíme nové
        GameObject windowObj = Instantiate(codeWindowPrefab, canvasParent);
        CodeWindow window = windowObj.GetComponent<CodeWindow>();

        // Naèteme data
        window.LoadScript(data);

        // 3. ZAREGISTRUJEME HO DO SLOVNÍKU
        openWindows.Add(data, window);

        if (scriptsPanel != null) scriptsPanel.SetActive(false);
    }

    public void UnregisterWindow(ScriptData data)
    {
        if (data != null && openWindows.ContainsKey(data))
        {
            openWindows.Remove(data);
        }
    }

    public void RefreshFileListUI()
    {
        // 1. Smažeme všechna stará tlaèítka
        foreach (Transform child in listContentContainer)
        {
            Destroy(child.gameObject);
        }

        // 2. Vytvoøíme nová tlaèítka podle aktuálních dat
        foreach (ScriptData script in allScripts)
        {
            GameObject btn = Instantiate(listButtonPrefab, listContentContainer);

            // Nastavíme data tlaèítku
            ScriptListItem itemScript = btn.GetComponent<ScriptListItem>();
            if (itemScript != null)
            {
                itemScript.Setup(script);
            }
        }
    }

    public void ToggleScriptsPanel()
    {
        bool isActive = !scriptsPanel.activeSelf;
        scriptsPanel.SetActive(isActive);

        // Když panel otevíráme, rovnou ho aktualizujeme, aby byl seznam èerstvý
        if (isActive)
        {
            RefreshFileListUI();
        }
    }

    // Pøidal jsem ti pomocnou metodu pro mazání (budeš potøebovat)
    public void DeleteScript(ScriptData data)
    {
        if (allScripts.Contains(data))
        {
            allScripts.Remove(data);
            // Zde v budoucnu zavoláš: RefreshFileListUI();
        }
    }

    // Pomocná metoda pro kontrolu, zda jméno už existuje (pro UI)
    public bool DoesScriptExist(string name)
    {
        return allScripts.Exists(s => s.scriptName == name);
    }
}