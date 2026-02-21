using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the front-end interface for the script editor window. 
/// Handles real-time code editing, script execution control, and synchronized UI state management.
/// </summary>
public class CodeWindow : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField myInputField;
    public Button runButton;
    public TextMeshProUGUI runButtonText;
    public Image statusIndicator;
    public Button closeButton;
    public TMP_InputField windowTitleInput;

    private ScriptData currentData;
    private bool isRunningThisWindow = false;

    private void Start()
    {
        if (runButton != null)
        {
            runButton.onClick.AddListener(OnRunClicked);
        }

        if (myInputField != null)
        {
            myInputField.onValueChanged.AddListener(OnCodeChanged);
        }

        if (windowTitleInput != null)
        {
            windowTitleInput.onEndEdit.AddListener(OnTitleRenamed);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        if (PlayerScriptEngine.Instance != null)
        {
            PlayerScriptEngine.Instance.OnCodeDeployed += OnCodeDeployed;
            PlayerScriptEngine.Instance.OnScriptStopped += OnScriptStopped;
        }

        if (currentData != null)
        {
            RefreshUI();
        }

        UpdateStatusUI(false);
    }

    private void OnDestroy()
    {
        if (PlayerScriptEngine.Instance != null)
        {
            PlayerScriptEngine.Instance.OnCodeDeployed -= OnCodeDeployed;
            PlayerScriptEngine.Instance.OnScriptStopped -= OnScriptStopped;
        }

        if (ScriptFileManager.Instance != null && currentData != null)
        {
            ScriptFileManager.Instance.UnregisterWindow(currentData);
        }
    }

    private void OnCloseClicked()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Binds a specific script data model to the editor window and refreshes the display.
    /// </summary>
    /// <param name="data">The script data to be loaded.</param>
    public void LoadScript(ScriptData data)
    {
        currentData = data;
        RefreshUI();
    }

    /// <summary>
    /// Synchronizes the UI input fields with the values stored in the current data model.
    /// </summary>
    private void RefreshUI()
    {
        if (currentData == null) return;

        if (myInputField != null)
            myInputField.text = currentData.sourceCode;

        if (windowTitleInput != null)
            windowTitleInput.text = currentData.scriptName;
    }

    /// <summary>
    /// Toggles the execution state of the current script. 
    /// </summary>
    private void OnRunClicked()
    {
        if (isRunningThisWindow)
        {
            PlayerScriptEngine.Instance.StopCurrentScript();
        }
        else
        {
            if (currentData != null && myInputField != null)
            {
                currentData.sourceCode = myInputField.text;
            }

            if (myInputField != null)
            {
                PlayerScriptEngine.Instance.CompileAndRun(myInputField.text, this);
            }
        }
    }

    /// <summary>
    /// Updates the script's name.
    /// </summary>
    /// <param name="newName">The new name entered by the player.</param>
    private void OnTitleRenamed(string newName)
    {
        if (currentData != null)
        {
            currentData.scriptName = newName;

            if (ScriptFileManager.Instance != null)
            {
                ScriptFileManager.Instance.RefreshFileListUI();
            }
        }
    }

    /// <summary>
    /// Listener that updates the underlying source code data as the player types.
    /// </summary>
    private void OnCodeChanged(string newCode)
    {
        if (currentData != null)
        {
            currentData.sourceCode = newCode;
        }
    }

    // --- ENGINE STATE SYNCHRONIZATION ---

    private void OnCodeDeployed(CodeWindow activeWindow)
    {
        if (activeWindow == this)
            UpdateStatusUI(true);
        else
            UpdateStatusUI(false);
    }

    private void OnScriptStopped()
    {
        UpdateStatusUI(false);
    }

    /// <summary>
    /// Updates the visual state of the window, including the status indicator and action button text.
    /// </summary>
    /// <param name="running">The current execution state.</param>
    private void UpdateStatusUI(bool running)
    {
        isRunningThisWindow = running;

        if (running)
        {
            if (statusIndicator != null) statusIndicator.color = Color.green;
            if (runButtonText != null) runButtonText.text = "STOP";
        }
        else
        {
            if (statusIndicator != null) statusIndicator.color = Color.red;
            if (runButtonText != null) runButtonText.text = "RUN";
        }
    }
}