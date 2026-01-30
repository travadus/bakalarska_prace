using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CodeWindow : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField myInputField;
    public Button runButton;
    public Image statusIndicator; // Èervená = nebìží, Zelená = bìží

    private void Start()
    {
        runButton.onClick.AddListener(OnRunClicked);

        // Pøihlásíme se k události zmìny aktivního skriptu (viz níže)
        PlayerScriptEngine.Instance.OnCodeDeployed += OnCodeChanged;
    }

    private void OnDestroy()
    {
        if (PlayerScriptEngine.Instance != null)
            PlayerScriptEngine.Instance.OnCodeDeployed -= OnCodeChanged;
    }

    private void OnRunClicked()
    {
        // Pošleme náš kód do hlavního motoru
        PlayerScriptEngine.Instance.CompileAndRun(myInputField.text, this);
    }

    // Tato metoda se zavolá, když se nìkde ve høe spustí nový kód
    private void OnCodeChanged(CodeWindow activeWindow)
    {
        if (activeWindow == this)
        {
            statusIndicator.color = Color.green; // Já jsem ten vyvolený!
        }
        else
        {
            statusIndicator.color = Color.red; // Nìkdo jiný teï øídí hru
        }
    }
}