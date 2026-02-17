using UnityEngine;
using System;
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

    // Queue for safe execution of Unity API on the main thread
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

        // 1. Subscribe to logs
        GameAPI.OnLogMessage += (msg) => EnqueueAction(() => LogToConsole(msg, Color.white));

        // 2. Subscribe to Time System ticks
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick += ExecutePlayerTick;
        }
    }

    private void Update()
    {
        // Execute queued actions on the main thread
        while (mainThreadQueue.TryDequeue(out Action action))
        {
            action.Invoke();
        }
    }

    // --- MAIN EXECUTION METHOD (Called by Play Button) ---
    public void CompileAndRun(string sourceCode, CodeWindow senderWindow)
    {
        // 1. Always stop previous script first
        StopCurrentScript();
        ClearConsole();

        // --- STEP 2: SECURITY & RESEARCH CHECK (Roslyn Guard) ---
        // Before we even try to compile, we check if the code uses locked features.

        SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode);
        CodeSecurityGuard guard = new CodeSecurityGuard(); // Guard checks ResearchManager internally
        guard.Visit(tree.GetRoot());

        if (guard.FoundErrors.Count > 0)
        {
            LogToConsole("--- SECURITY CHECK FAILED: LOCKED TECHNOLOGY DETECTED ---", Color.red);
            foreach (string error in guard.FoundErrors)
            {
                LogToConsole(error, Color.red);
            }
            // STOP HERE! Do not compile.
            return;
        }

        LogToConsole("Security check passed. Compiling...", Color.yellow);
        // ---------------------------------------------------------

        // --- STEP 3: COMPILATION ---
        string cleanCode = SanitizeCode(sourceCode);
        string finalSource = WrapCode(cleanCode);

        Assembly assembly = Compile(finalSource);

        if (assembly != null)
        {
            // Create instance of the UserScript class
            CreateScriptInstance(assembly);

            if (cachedMethod != null)
            {
                isScriptActive = true;
                LogToConsole("System ONLINE. Waiting for Game Tick...", Color.green);

                // Notify the window that code was successfully deployed
                OnCodeDeployed?.Invoke(senderWindow);
            }
        }
    }

    // --- BUTTON: STOP ---
    public void StopCurrentScript()
    {
        isScriptActive = false;
        if (executionThread != null && executionThread.IsAlive) executionThread.Abort();
        LogToConsole("System OFFLINE.", Color.orange);
        OnScriptStopped?.Invoke();
    }

    // --- AUTOMATIC EXECUTION ON TICK ---
    private void ExecutePlayerTick(DateTime gameTime)
    {
        // If script is inactive or invalid, do nothing
        if (!isScriptActive || compiledInstance == null || cachedMethod == null) return;

        // Run Main() in a separate thread for safety
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
                    // On runtime error, stop the script
                    StopCurrentScript();
                });
            }
        });

        executionThread.IsBackground = true;
        executionThread.Start();

        // SAFETY WATCHDOG
        // If the script runs longer than 0.5s (infinite loop), kill it.
        StartCoroutine(WatchdogCoroutine(executionThread));
    }

    private IEnumerator WatchdogCoroutine(Thread thread)
    {
        yield return new WaitForSeconds(0.5f);
        if (thread != null && thread.IsAlive)
        {
            thread.Abort();
            LogToConsole("Error: Script took too long (Infinite Loop?). Stopped.", Color.red);
            StopCurrentScript();
        }
    }

    // --- COMPILATION HELPERS ---

    private void CreateScriptInstance(Assembly assembly)
    {
        Type type = assembly.GetType(wrapperClassName);
        if (type == null) return;

        compiledInstance = Activator.CreateInstance(type);

        // Look for "Main" method
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
        sb.AppendLine("using static GameAPI;"); // Enables direct access to API methods

        sb.AppendLine($"public class {wrapperClassName}");
        sb.AppendLine("{");

        // Map lines for correct error reporting
        sb.AppendLine("#line 1 \"PlayerEditor\"");
        sb.AppendLine(playerCode);
        sb.AppendLine("#line default");

        sb.AppendLine("}");

        return sb.ToString();
    }

    private Assembly Compile(string sourceCode)
    {
        // Use your existing Compiler helper
        return PlayerScriptCompiler.Compile(sourceCode, cachedReferences, (err) => LogToConsole(err, Color.red));
    }

    private void LoadReferences()
    {
        cachedReferences = new List<MetadataReference>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location));
        foreach (var asm in assemblies) cachedReferences.Add(MetadataReference.CreateFromFile(asm.Location));
    }

    // --- UTILS ---
    public void EnqueueAction(Action action) => mainThreadQueue.Enqueue(action);
    private string SanitizeCode(string input) => input?.Replace("\u200B", "").Replace("\uFEFF", "") ?? "";

    public void LogSystemMessage(string message)
    {
        LogToConsole($"[SYSTEM]: {message}", Color.cyan);
    }

    public void LogMessage(string message, Color color)
    {
        EnqueueAction(() => LogToConsole(message, color));
    }

    private void LogToConsole(string message, Color color)
    {
        if (consoleOutput == null) return;

        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        consoleOutput.text += $"<color=#{hexColor}>{message}</color>\n";

        if (consoleOutput.text.Length > 5000)
        {
            consoleOutput.text = consoleOutput.text.Substring(1000);
        }

        StartCoroutine(AutoScrollToBottom());
    }

    private IEnumerator AutoScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        if (consoleOutput != null)
        {
            consoleOutput.ForceLabelUpdate();
            if (consoleOutput.verticalScrollbar != null)
            {
                consoleOutput.verticalScrollbar.value = 0f; // 0 usually means bottom in Unity UI
            }
            consoleOutput.caretPosition = consoleOutput.text.Length;
        }
    }

    private void ClearConsole()
    {
        if (consoleOutput != null) consoleOutput.text = "";
    }
}