using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

public class CodeTester : MonoBehaviour
{
    // Propojíš ho s TMP_InputField v Inspectoru
    public TMPro.TMP_InputField codeInput;

    public void OnRunButtonClicked()
    {
        var writer = new StringWriter();
        Console.SetOut(TextWriter.Null);
        Console.SetOut(writer);

        string playerCode = codeInput.text;

        string fullCode = @"
            using System;
            using UnityEngine;
            using static GameAPI;

            public class PlayerScript {
            " + playerCode + @"
            }";

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fullCode);

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location));

        var compilation = CSharpCompilation.Create(
            "PlayerScriptAssembly",
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using (var ms = new MemoryStream())
        {
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                string errors = string.Join("\n", result.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.ToString()));
                Debug.LogError("Compilation failed:\n" + errors);
                return;
            }

            Debug.Log("Compilation successful!");

            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());

            Type playerScriptType = assembly.GetType("PlayerScript");
            MethodInfo method = playerScriptType?.GetMethod("Trade", BindingFlags.Public | BindingFlags.Static);

            if (method != null)
            {
                try
                {
                    Debug.Log("Spoustim metodu Trade()");
                    method.Invoke(null, null);

                    string consoleOutput = writer.ToString();
                    if (!string.IsNullOrWhiteSpace(consoleOutput))
                    {
                        Debug.Log("Console output:\n" + consoleOutput);
                    }

                    Debug.Log("Metoda Trade() byla dokoncena.");
                }
                catch (Exception ex)
                {
                    Debug.LogError("Chyba pri volani Trade(): " + ex.Message);
                }
            }
            else
            {
                Debug.Log("Metoda Trade() nebyla nalezena.");
            }

        }
    }
}