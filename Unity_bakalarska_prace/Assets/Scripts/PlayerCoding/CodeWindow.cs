using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CodeWindow : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField myInputField;      // Editor kódu
    public Button runButton;                 // Tlaèítko Play/Stop
    public TextMeshProUGUI runButtonText;    // Text uvnitø tlaèítka
    public Image statusIndicator;            // Barevná kontrolka

    public Button closeButton;

    // ZDE JE ZMÌNA: Nadpis je teï InputField, aby šel pøepsat
    public TMP_InputField windowTitleInput;

    private ScriptData currentData;          // Data, která toto okno edituje
    private bool isRunningThisWindow = false;

    private void Start()
    {
        // 1. Nastavení tlaèítka RUN/STOP
        if (runButton != null)
        {
            runButton.onClick.AddListener(OnRunClicked);
        }

        // 2. Nastavení editoru kódu
        if (myInputField != null)
        {
            // Když hráè píše kód, hned ho ukládáme do dat
            myInputField.onValueChanged.AddListener(OnCodeChanged);
        }

        // 3. Nastavení pøejmenování (Nadpis okna)
        if (windowTitleInput != null)
        {
            // Když hráè dopíše jméno (Enter nebo klikne jinam), uložíme to
            windowTitleInput.onEndEdit.AddListener(OnTitleRenamed);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        // 4. Pøihlášení k Enginu (posloucháme Play/Stop eventy)
        if (PlayerScriptEngine.Instance != null)
        {
            PlayerScriptEngine.Instance.OnCodeDeployed += OnCodeDeployed;
            PlayerScriptEngine.Instance.OnScriptStopped += OnScriptStopped;
        }

        // 5. Prvotní naètení dat (pokud jsme je dostali døív než Start)
        if (currentData != null)
        {
            RefreshUI();
        }

        UpdateStatusUI(false); // Výchozí stav (èervená)
    }

    private void OnDestroy()
    {
        // Úklid pøi zavøení okna
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
        // Prostì znièíme tento objekt.
        // Zbytek zaøídí metoda OnDestroy automaticky.
        Destroy(gameObject);
    }

    // --- HLAVNÍ METODA PRO NAÈTENÍ DAT ---
    public void LoadScript(ScriptData data)
    {
        currentData = data;
        RefreshUI(); // Zavoláme pomocnou metodu pro vyplnìní textù
    }

    private void RefreshUI()
    {
        if (currentData == null) return;

        // Nastavíme kód
        if (myInputField != null)
            myInputField.text = currentData.sourceCode;

        // Nastavíme název
        if (windowTitleInput != null)
            windowTitleInput.text = currentData.scriptName;
    }

    // --- LOGIKA TLAÈÍTKA RUN / STOP ---
    private void OnRunClicked()
    {
        if (isRunningThisWindow)
        {
            // Pokud bìžíme, tlaèítko funguje jako STOP
            PlayerScriptEngine.Instance.StopCurrentScript();
        }
        else
        {
            // Pokud nebìžíme, tlaèítko funguje jako RUN

            // Pojistka: Uložíme aktuální kód do dat
            if (currentData != null && myInputField != null)
            {
                currentData.sourceCode = myInputField.text;
            }

            // Spustíme kód
            if (myInputField != null)
            {
                PlayerScriptEngine.Instance.CompileAndRun(myInputField.text, this);
            }
        }
    }

    // --- LOGIKA PØEJMENOVÁNÍ ---
    private void OnTitleRenamed(string newName)
    {
        if (currentData != null)
        {
            // 1. Zmìníme jméno v datech
            currentData.scriptName = newName;

            // 2. Øekneme Manažerovi: "Hej, zmìnilo se jméno, pøepis seznam tlaèítek!"
            if (ScriptFileManager.Instance != null)
            {
                ScriptFileManager.Instance.RefreshFileListUI();
            }
        }
    }

    // --- AUTOMATICKÉ UKLÁDÁNÍ KÓDU ---
    private void OnCodeChanged(string newCode)
    {
        if (currentData != null)
        {
            currentData.sourceCode = newCode;
        }
    }

    // --- REAKCE NA ENGINE (Barvièky a text tlaèítka) ---

    // Nìkdo spustil skript (já nebo jiné okno)
    private void OnCodeDeployed(CodeWindow activeWindow)
    {
        if (activeWindow == this)
            UpdateStatusUI(true);  // Jsem to já -> ZELENÁ
        else
            UpdateStatusUI(false); // Je to nìkdo jiný -> ÈERVENÁ
    }

    // Motor se zastavil
    private void OnScriptStopped()
    {
        UpdateStatusUI(false);
    }

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