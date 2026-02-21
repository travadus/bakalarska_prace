using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Represents a single selectable UI item within the script list.
/// </summary>
public class ScriptListItem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameText;

    private ScriptData myData;

    /// <summary>
    /// Initializes the UI element with the provided script data and sets up interaction listeners.
    /// </summary>
    /// <param name="data">The underlying data model associated with this list item.</param>
    public void Setup(ScriptData data)
    {
        myData = data;
        nameText.text = data.scriptName;

        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    /// <summary>
    /// Handles the button click event, prompting the manager to open the associated script in the editor.
    /// </summary>
    private void OnClick()
    {
        ScriptFileManager.Instance.OpenEditorFor(myData);
    }
}