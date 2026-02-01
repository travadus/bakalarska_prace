using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

/// <summary>
/// Pomocná statická tøída, která øeší pouze "špinavou práci" s Roslyn kompilátorem.
/// Oddìluje logiku kompilace od herní logiky.
/// </summary>
public static class PlayerScriptCompiler
{
    public static Assembly Compile(string source, List<MetadataReference> refs, Action<string> onError)
    {
        // 1. Parsování textu na strom syntaxe
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        // 2. Náhodné jméno pro assembly (aby se nehádaly, když jich bude víc)
        string assemblyName = $"UserScript_{Guid.NewGuid().ToString().Substring(0, 8)}";

        // 3. Nastavení kompilace
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // 4. Samotná kompilace do pamìti (MemoryStream)
        using (var ms = new MemoryStream())
        {
            EmitResult result = compilation.Emit(ms);

            // 5. Kontrola chyb
            if (!result.Success)
            {
                foreach (Diagnostic d in result.Diagnostics)
                {
                    if (d.Severity == DiagnosticSeverity.Error)
                    {
                        // Pošleme chybu zpìt do PlayerScriptEngine pøes Action callback
                        onError($"Error ({d.Location.GetLineSpan().StartLinePosition.Line + 1}): {d.GetMessage()}");
                    }
                }
                return null; // Kompilace se nepovedla
            }

            // 6. Úspìch -> Naèteme assembly z pamìti a vrátíme ji
            ms.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }
    }
}