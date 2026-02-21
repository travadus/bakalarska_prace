using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the lifecycle of script files within the environment. 
/// </summary>
public class ScriptFileManager : MonoBehaviour
{
    public static ScriptFileManager Instance;

    /// <summary>
    /// Collection of all existing scripts.
    /// </summary>
    public List<ScriptData> allScripts = new List<ScriptData>();

    /// <summary>
    /// Tracks active editor windows to prevent duplicate instances for the same script data.
    /// </summary>
    private Dictionary<ScriptData, CodeWindow> openWindows = new Dictionary<ScriptData, CodeWindow>();

    [Header("Prefabs")]
    public GameObject codeWindowPrefab;
    public Transform canvasParent;

    [Header("List UI Settings")]
    public Transform listContentContainer;
    public GameObject listButtonPrefab;
    public GameObject scriptsPanel;

    private void Awake()
    {
        Instance = this;

        if (scriptsPanel != null) scriptsPanel.SetActive(false);
    }

    /// <summary>
    /// Generates a new script with a default name and adds it to the global collection.
    /// </summary>
    public void CreateNewScript()
    {
        int count = allScripts.Count + 1;
        string autoName = $"Script {count}";

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

        RefreshFileListUI();

        OpenEditorFor(newScript);
    }

    /// <summary>
    /// Opens a code editor window for the specified script. 
    /// </summary>
    /// <param name="data">The script data model to be edited.</param>
    public void OpenEditorFor(ScriptData data)
    {
        if (openWindows.ContainsKey(data) && openWindows[data] != null)
        {
            openWindows[data].transform.SetAsLastSibling();
            return;
        }

        GameObject windowObj = Instantiate(codeWindowPrefab, canvasParent);
        CodeWindow window = windowObj.GetComponent<CodeWindow>();

        window.LoadScript(data);

        openWindows.Add(data, window);

        if (scriptsPanel != null) scriptsPanel.SetActive(false);
    }

    /// <summary>
    /// Removes a closed window instance from the active tracking dictionary.
    /// </summary>
    public void UnregisterWindow(ScriptData data)
    {
        if (data != null && openWindows.ContainsKey(data))
        {
            openWindows.Remove(data);
        }
    }

    /// <summary>
    /// Reconstructs the file list UI by clearing existing elements and instantiating 
    /// new list items for each script in the collection.
    /// </summary>
    public void RefreshFileListUI()
    {
        foreach (Transform child in listContentContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (ScriptData script in allScripts)
        {
            GameObject btn = Instantiate(listButtonPrefab, listContentContainer);

            ScriptListItem itemScript = btn.GetComponent<ScriptListItem>();
            if (itemScript != null)
            {
                itemScript.Setup(script);
            }
        }
    }

    /// <summary>
    /// Toggles the visibility of the scripts selection panel and refreshes the file list content.
    /// </summary>
    public void ToggleScriptsPanel()
    {
        bool isActive = !scriptsPanel.activeSelf;
        scriptsPanel.SetActive(isActive);

        if (isActive)
        {
            RefreshFileListUI();
        }
    }

    /// <summary>
    /// Removes a script.
    /// </summary>
    /// <param name="data">The script data to be deleted.</param>
    public void DeleteScript(ScriptData data)
    {
        if (allScripts.Contains(data))
        {
            allScripts.Remove(data);
        }
    }

    /// <summary>
    /// Checks if a script with the specified name already exists.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if the name is found otherwise, false.</returns>
    public bool DoesScriptExist(string name)
    {
        return allScripts.Exists(s => s.scriptName == name);
    }
}