using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using TMPro;

public class PlayerScriptEngine : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_InputField consoleOutput;

    [Header("Settings")]
    [SerializeField] private string wrapperClassName = "UserScript";

    private object compiledInstance;
    private MethodInfo cachedMethod;
    private List<MetadataReference> cachedReferences;

    // Fronta pro bezpeèné volání Unity API z vedlejšího vlákna
    private ConcurrentQueue<Action> mainThreadQueue = new ConcurrentQueue<Action>();

    private Thread executionThread;
    private bool isScriptActive = false;

    public event Action OnScriptStopped;

    public event Action<CodeWindow> OnCodeDeployed;
    public static PlayerScriptEngine Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        LoadReferences();

        // 1. Pøihlášení k logùm (aby GameAPI.Log fungoval)
        GameAPI.OnLogMessage += (msg) => EnqueueAction(() => LogToConsole(msg, Color.white));

        // 2. Pøihlášení k dalším eventùm (Nákup, Prodej...) - ZDE SI DOPLÒ SVÉ METODY Z GAMEAPI
        // Napø: GameAPI.OnBuyEnergyRequest += (amount) => EnqueueAction(() => MarketSystem.Buy(amount));

        // 3. NAPOJENÍ NA TIME SYSTEM (Srdce simulace)
        // Pøedpokládám, že TimeSystem má event "OnTick" nebo "OnHourChanged"
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick += ExecutePlayerTick;
        }
    }

    private void Update()
    {
        // Vybírání pøíkazù z fronty (aby se provedly na hlavním vláknì)
        while (mainThreadQueue.TryDequeue(out Action action))
        {
            action.Invoke();
        }
    }

    // --- TLAÈÍTKO PLAY ---
    public void CompileAndRun(string sourceCode, CodeWindow senderWindow)
    {
        StopCurrentScript(); // Reset

        ClearConsole();
        LogToConsole("Compiling...", Color.yellow);

        string cleanCode = SanitizeCode(sourceCode);
        string finalSource = WrapCode(cleanCode);

        Assembly assembly = Compile(finalSource);

        if (assembly != null)
        {
            // Vytvoøíme instanci tøídy UserScript
            CreateScriptInstance(assembly);

            if (cachedMethod != null)
            {
                isScriptActive = true;
                LogToConsole("System ONLINE. Waiting for Game Tick...", Color.green);
                OnCodeDeployed?.Invoke(senderWindow);
            }
        }
    }

    // --- TLAÈÍTKO STOP ---
    public void StopCurrentScript()
    {
        isScriptActive = false;
        if (executionThread != null && executionThread.IsAlive) executionThread.Abort();
        LogToConsole("System OFFLINE.", Color.orange);
        OnScriptStopped?.Invoke();
    }

    // --- AUTOMATICKÉ SPUŠTÌNÍ PØI TICKU ---
    private void ExecutePlayerTick(DateTime gameTime)
    {
        // Pokud je skript vypnutý nebo není zkompilovaný, nic nedìlej
        if (!isScriptActive || compiledInstance == null || cachedMethod == null) return;

        // Spustíme Main() v novém vláknì (bezpeènost)
        executionThread = new Thread(() =>
        {
            try
            {
                cachedMethod.Invoke(compiledInstance, null);
            }
            catch (Exception ex)
            {
                EnqueueAction(() =>
                {
                    LogToConsole($"Runtime Error: {ex.InnerException?.Message ?? ex.Message}", Color.red);
                    // Když nastane chyba, musíme to vypnout i vizuálnì
                    StopCurrentScript();
                });
            }
        });

        executionThread.IsBackground = true;
        executionThread.Start();

        // BEZPEÈNOSTNÍ POJISTKA (WATCHDOG)
        // Pokud hráèùv kód trvá déle než 0.5 sekundy (zacyklil se), zabijeme ho.
        StartCoroutine(WatchdogCoroutine(executionThread));
    }

    private IEnumerator WatchdogCoroutine(Thread thread)
    {
        yield return new WaitForSeconds(0.5f);
        if (thread != null && thread.IsAlive)
        {
            // Nejdøív ho natvrdo zabijeme, a nezdržuje
            thread.Abort();

            LogToConsole("Error: Script took too long. Stopped.", Color.red);

            // A pak zavoláme oficiální "proceduru zastavení", aby se aktualizovalo UI
            StopCurrentScript();
        }
    }

    // --- KOMPILACE A PØÍPRAVA ---

    private void CreateScriptInstance(Assembly assembly)
    {
        Type type = assembly.GetType(wrapperClassName);
        if (type == null) return;

        compiledInstance = Activator.CreateInstance(type);

        // Hledáme metodu "Main" (void, bez parametrù)
        MethodInfo method = type.GetMethod("Main", BindingFlags.Public | BindingFlags.Instance);

        if (method != null)
        {
            cachedMethod = method;
        }
        else
        {
            LogToConsole("Error: Code must contain 'public void Main()'", Color.red);
            cachedMethod = null;
        }
    }

    private string WrapCode(string playerCode)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using static GameAPI;"); // Aby mohl psát Log() pøímo

        sb.AppendLine($"public class {wrapperClassName}");
        sb.AppendLine("{");

        // ZDE JE ZMÌNA: Už nevytváøíme metodu za hráèe.
        // Hráèùv kód se vloží pøímo do tìla tøídy.
        // Takže hráè musí napsat "public void Main() { ... }"

        sb.AppendLine("#line 1 \"PlayerEditor\"");
        sb.AppendLine(playerCode);
        sb.AppendLine("#line default");

        sb.AppendLine("}");

        return sb.ToString();
    }

    // --- POMOCNÉ METODY (Stejné jako døív) ---
    public void EnqueueAction(Action action) => mainThreadQueue.Enqueue(action);
    private string SanitizeCode(string input) => input?.Replace("\u200B", "").Replace("\uFEFF", "") ?? "";

    private void LogToConsole(string message, Color color)
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        consoleOutput.text += $"<color=#{hexColor}>{message}</color>\n";

        if (consoleOutput.text.Length > 5000)
        {
            // Smažeme prvních 1000 znakù, aby se uvolnila pamì
            consoleOutput.text = consoleOutput.text.Substring(1000);
        }

        StartCoroutine(AutoScrollToBottom());
    }

    private IEnumerator AutoScrollToBottom()
    {
        // 1. Poèkáme, až Unity vykreslí nový text (DÙLEŽITÉ)
        yield return new WaitForEndOfFrame();

        // 2. Øekneme InputFieldu, a si pøepoèítá velikost textu TEÏ HNED
        consoleOutput.ForceLabelUpdate();

        // 3. Pokud máš pøiøazený scrollbar, nastavíme ho natvrdo dolù
        if (consoleOutput.verticalScrollbar != null)
        {
            consoleOutput.verticalScrollbar.value = 1f;
        }

        // 4. POJISTKA: Pøesuneme "neviditelný" kurzor na konec textu.
        // Tím se pohled posune tam, kde je kurzor (tedy na konec).
        consoleOutput.caretPosition = consoleOutput.text.Length;
    }

    private void ClearConsole() => consoleOutput.text = "";

    // Tady vlož svou metodu Compile() a LoadReferences() z minula
    // (Zkracuji to, aby se to vešlo do zprávy - použij ty, co už máš funkèní)
    private Assembly Compile(string sourceCode)
    {
        // ... Tvùj kód pro Roslyn kompilaci ...
        // Je to ten dlouhý blok s CSharpCompilation.Create
        // Pokud ho nemáš uložený, øekni, pošlu ho znovu.
        return PlayerScriptCompiler.Compile(sourceCode, cachedReferences, (err) => LogToConsole(err, Color.red));
    }

    private void LoadReferences()
    {
        cachedReferences = new List<MetadataReference>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location));
        foreach (var asm in assemblies) cachedReferences.Add(MetadataReference.CreateFromFile(asm.Location));
    }

    public void LogSystemMessage(string message)
    {
        // Použijeme tøeba žlutou nebo azurovou barvu, aby to vypadalo jako "System Info"
        LogToConsole($"[SYSTEM]: {message}", Color.cyan);
    }

    public void LogMessage(string message, Color color)
    {
        // EnqueueAction zajistí, že se to provede bezpeènì na hlavním vláknì
        EnqueueAction(() => LogToConsole(message, color));
    }
}
