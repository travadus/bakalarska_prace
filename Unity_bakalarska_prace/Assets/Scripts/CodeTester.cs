using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

public class CodeTester : MonoBehaviour
{
    public TMPro.TMP_InputField codeInput;

    public string methodToRun;

    public void OnRunButtonClicked()
    {

        var originalOut = Console.Out;
        var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            string playerCode = codeInput.text ?? string.Empty;

            string fullCode =
@"using System;
using UnityEngine;
using static GameAPI;

public class PlayerScript {
#line 1 ""PlayerCode""
" + playerCode + @"
#line default
}";

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(fullCode);

            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();

            var thisAsm = typeof(CodeTester).Assembly;
            if (!string.IsNullOrEmpty(thisAsm.Location))
                references.Add(MetadataReference.CreateFromFile(thisAsm.Location));

            var apiAsm = typeof(GameAPI).Assembly;
            if (!string.IsNullOrEmpty(apiAsm.Location))
                references.Add(MetadataReference.CreateFromFile(apiAsm.Location));

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
                    // Compilation failed print all error diagnostics
                    string errors = string.Join("\n", result.Diagnostics
                        .Where(d => d.Severity == DiagnosticSeverity.Error)
                        .Select(d => d.ToString()));
                    Debug.LogError("Compilation failed:\n" + errors);
                    return;
                }

                // Load compiled assembly into memory
                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());

                // Find the PlayerScript type
                Type playerScriptType = assembly.GetType("PlayerScript");
                if (playerScriptType == null)
                {
                    Debug.LogError("Type PlayerScript was not found.");
                    return;
                }

                // Get all public void methods with no parameters
                var allMethods = playerScriptType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(m => m.ReturnType == typeof(void) && m.GetParameters().Length == 0)
                    .ToList();

                if (allMethods.Count == 0)
                {
                    Debug.LogError("No public methods found in PlayerScript with signature: void MethodName()");
                    return;
                }

                // Create an instance if there are any non-static methods
                object instance = null;
                bool hasInstanceMethods = allMethods.Any(m => !m.IsStatic);
                if (hasInstanceMethods)
                {
                    try { instance = Activator.CreateInstance(playerScriptType); }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to create PlayerScript instance: " + e);
                        return;
                    }
                }

                // Pick the method to run
                MethodInfo methodToInvoke = null;

                if (!string.IsNullOrWhiteSpace(methodToRun))
                {
                    // Use the provided method name
                    methodToInvoke = allMethods.FirstOrDefault(m => m.Name == methodToRun);
                    if (methodToInvoke == null)
                    {
                        Debug.LogError($"Method '{methodToRun}' not found. Available methods:\n" +
                                       string.Join(", ", allMethods.Select(m => m.Name)));
                        return;
                    }
                }
                else
                {
                    // No method specified: if only one exists, run it; else list them
                    if (allMethods.Count == 1)
                    {
                        methodToInvoke = allMethods[0];
                    }
                    else
                    {
                        Debug.Log("Multiple methods found. Set 'methodToRun' to one of these:\n" +
                                  string.Join(", ", allMethods.Select(m => m.Name)));
                        return;
                    }
                }

                // Invoke the selected method
                try
                {
                    if (methodToInvoke.IsStatic)
                    {
                        Debug.Log($"Running static method {methodToInvoke.Name}()");
                        methodToInvoke.Invoke(null, null);
                    }
                    else
                    {
                        Debug.Log($"Running instance method {methodToInvoke.Name}()");
                        methodToInvoke.Invoke(instance, null);
                    }

                    // Output any Console.WriteLine calls
                    string consoleOutput = writer.ToString();
                    if (!string.IsNullOrWhiteSpace(consoleOutput))
                        Debug.Log("Console output:\n" + consoleOutput);
                }
                catch (TargetInvocationException tex)
                {
                    Debug.LogError("Error inside player's method: " + tex.InnerException);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error while invoking method: " + ex);
                }
            }
        }
        finally
        {
            // Restore original Console.Out
            Console.SetOut(originalOut);
        }
    }
}
