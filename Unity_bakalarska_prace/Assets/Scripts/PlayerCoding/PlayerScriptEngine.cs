using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
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
        // Pøihlášení k logùm z GameAPI (bezpeèné logování)
        GameAPI.OnLogMessage += HandlePlayerLog;
    }

    private void OnDestroy()
    {
        GameAPI.OnLogMessage -= HandlePlayerLog;
    }

    private void HandlePlayerLog(string message)
    {
        LogToConsole(message, Color.white);
    }

    // --- HLAVNÍ METODA PRO SPUŠTÌNÍ ---
    public void CompileAndRun(string sourceCode, CodeWindow senderWindow)
    {
        ClearConsole();
        LogToConsole("Compiling...", Color.yellow);

        string cleanCode = SanitizeCode(sourceCode);
        string finalSource = WrapCode(cleanCode);

        Assembly assembly = Compile(finalSource);

        if (assembly != null)
        {
            LogToConsole("Compilation Successful! Switching to new code.", Color.green);
            RunScript(assembly);
            OnCodeDeployed?.Invoke(senderWindow);
        }
    }

    // --- POMOCNÉ METODY (Compile, Wrap, Sanitize) ---

    private void RunScript(Assembly assembly)
    {
        try
        {
            Type type = assembly.GetType(wrapperClassName);
            if (type == null) return;

            compiledInstance = Activator.CreateInstance(type);

            // Najdeme metodu Main nebo jinou void bez parametrù
            MethodInfo method = type.GetMethod("Main") ??
                                type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                .FirstOrDefault(m => m.GetParameters().Length == 0 && m.ReturnType == typeof(void));

            if (method != null)
            {
                cachedMethod = method;

                // BEZPEÈNÉ SPUŠTÌNÍ - Žádné CaptureConsoleOutput!
                method.Invoke(compiledInstance, null);
            }
            else
            {
                LogToConsole("Warning: No executable method found (e.g. 'public void Main()').", Color.orange);
            }
        }
        catch (Exception ex)
        {
            LogToConsole($"Runtime Error: {ex.InnerException?.Message ?? ex.Message}", Color.red);
        }
    }

    private Assembly Compile(string sourceCode)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        string assemblyName = $"{wrapperClassName}_{Guid.NewGuid().ToString().Substring(0, 8)}";

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            cachedReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using (var ms = new MemoryStream())
        {
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                foreach (Diagnostic diagnostic in result.Diagnostics)
                {
                    if (diagnostic.Severity == DiagnosticSeverity.Error)
                    {
                        LogToConsole($"Error ({diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1}): {diagnostic.GetMessage()}", Color.red);
                    }
                }
                return null;
            }

            ms.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }
    }

    private string WrapCode(string playerCode)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using UnityEngine;");
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

    private string SanitizeCode(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        return input.Replace("\u200B", "").Replace("\uFEFF", "");
    }

    private void LoadReferences()
    {
        cachedReferences = new List<MetadataReference>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location));

        foreach (var asm in assemblies)
        {
            cachedReferences.Add(MetadataReference.CreateFromFile(asm.Location));
        }
    }

    // --- UI HELPERY ---

    private void LogToConsole(string message, Color color)
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        consoleOutput.text += $"<color=#{hexColor}>{message}</color>\n";
    }

    private void ClearConsole()
    {
        consoleOutput.text = "";
    }
}