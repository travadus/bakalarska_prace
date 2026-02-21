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

/// <summary>
/// The central execution engine responsible for compiling and running player-written scripts.
/// </summary>
public class PlayerScriptEngine : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_InputField consoleOutput;

    [Header("Settings")]
    [SerializeField] private string wrapperClassName = "UserScript";

    private object compiledInstance;
    private MethodInfo cachedMethod;
    private List<MetadataReference> cachedReferences;

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

        GameAPI.OnLogMessage += (msg) => EnqueueAction(() => LogToConsole(msg, Color.white));

        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnTick += ExecutePlayerTick;
        }
    }

    /// <summary>
    /// Processes the main thread queue to execute calls from background threads.
    /// </summary>
    private void Update()
    {
        while (mainThreadQueue.TryDequeue(out Action action))
        {
            action.Invoke();
        }
    }

    /// <summary>
    /// Security validation, sanitization, compilation,
    /// and runtime instantiation of the provided source code.
    /// </summary>
    /// <param name="sourceCode">The raw script content.</param>
    /// <param name="senderWindow">The UI window instance initiating the call.</param>
    public void CompileAndRun(string sourceCode, CodeWindow senderWindow)
    {
        StopCurrentScript();
        ClearConsole();

        SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceCode);
        CodeSecurityGuard guard = new CodeSecurityGuard();
        guard.Visit(tree.GetRoot());

        if (guard.FoundErrors.Count > 0)
        {
            LogToConsole("--- SECURITY CHECK FAILED: LOCKED TECHNOLOGY DETECTED ---", Color.red);
            foreach (string error in guard.FoundErrors)
            {
                LogToConsole(error, Color.red);
            }
            return;
        }

        LogToConsole("Security check passed. Compiling...", Color.yellow);

        string cleanCode = SanitizeCode(sourceCode);
        string finalSource = WrapCode(cleanCode);

        Assembly assembly = Compile(finalSource);

        if (assembly != null)
        {
            CreateScriptInstance(assembly);

            if (cachedMethod != null)
            {
                isScriptActive = true;
                LogToConsole("System ONLINE. Waiting for Game Tick...", Color.green);

                OnCodeDeployed?.Invoke(senderWindow);
            }
        }
    }

    /// <summary>
    /// Stops active script and aborts the execution thread if necessary.
    /// </summary>
    public void StopCurrentScript()
    {
        isScriptActive = false;
        if (executionThread != null && executionThread.IsAlive) executionThread.Abort();
        LogToConsole("System OFFLINE.", Color.orange);
        OnScriptStopped?.Invoke();
    }

    /// <summary>
    /// Triggered by the global time system. Spawns a background thread to execute the script's Main method 
    /// and initiates a watchdog coroutine to prevent execution timeouts.
    /// </summary>
    /// <param name="gameTime">The current timestamp.</param>
    private void ExecutePlayerTick(DateTime gameTime)
    {
        if (!isScriptActive || compiledInstance == null || cachedMethod == null) return;

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
                    StopCurrentScript();
                });
            }
        });

        executionThread.IsBackground = true;
        executionThread.Start();

        StartCoroutine(WatchdogCoroutine(executionThread));
    }

    /// <summary>
    /// Monitors script execution time. If the thread remains active beyond the threshold, 
    /// it is terminated to prevent main thread starvation or infinite loops.
    /// </summary>
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

    private void CreateScriptInstance(Assembly assembly)
    {
        Type type = assembly.GetType(wrapperClassName);
        if (type == null) return;

        compiledInstance = Activator.CreateInstance(type);

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

    /// <summary>
    /// Wraps the player's code into a class structure with static API access.
    /// </summary>
    private string WrapCode(string playerCode)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using static GameAPI;");

        sb.AppendLine($"public class {wrapperClassName}");
        sb.AppendLine("{");

        sb.AppendLine("#line 1 \"PlayerEditor\"");
        sb.AppendLine(playerCode);
        sb.AppendLine("#line default");

        sb.AppendLine("}");

        return sb.ToString();
    }

    private Assembly Compile(string sourceCode)
    {
        return PlayerScriptCompiler.Compile(sourceCode, cachedReferences, (err) => LogToConsole(err, Color.red));
    }

    private void LoadReferences()
    {
        cachedReferences = new List<MetadataReference>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location));
        foreach (var asm in assemblies) cachedReferences.Add(MetadataReference.CreateFromFile(asm.Location));
    }

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
                consoleOutput.verticalScrollbar.value = 1f;
            }
            consoleOutput.caretPosition = consoleOutput.text.Length;
        }
    }

    private void ClearConsole()
    {
        if (consoleOutput != null) consoleOutput.text = "";
    }
}