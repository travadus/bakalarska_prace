using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

/// <summary>
/// Utility class responsible for the runtime compilation of C# source code.
/// </summary>
public static class PlayerScriptCompiler
{
    /// <summary>
    /// Compiles raw source code into an in-memory assembly.
    /// </summary>
    /// <param name="source">The raw string content of the C# script.</param>
    /// <param name="refs">The list of metadata references required for the compilation context.</param>
    /// <param name="onError">A callback action to report diagnostic errors back to the caller.</param>
    /// <returns>A loaded <see cref="Assembly"/> if successful; otherwise, null.</returns>
    public static Assembly Compile(string source, List<MetadataReference> refs, Action<string> onError)
    {
        // 1. Parse the source text into an Abstract Syntax Tree (AST)
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        // 2. Generate a unique assembly identity to prevent collisions during repeated compilation cycles
        string assemblyName = $"UserScript_{Guid.NewGuid().ToString().Substring(0, 8)}";

        // 3. Configure the compilation object
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // 4. Emit the compilation result into a memory-resident stream
        using (var ms = new MemoryStream())
        {
            EmitResult result = compilation.Emit(ms);

            // 5. Diagnostic analysis and error reporting
            if (!result.Success)
            {
                foreach (Diagnostic d in result.Diagnostics)
                {
                    if (d.Severity == DiagnosticSeverity.Error)
                    {
                        // Propagate the error with line-number context back to the UI handler
                        onError($"Error ({d.Location.GetLineSpan().StartLinePosition.Line + 1}): {d.GetMessage()}");
                    }
                }
                return null;
            }

            // 6. Finalization: Load the assembly into the current AppDomain from the raw memory buffer
            ms.Seek(0, SeekOrigin.Begin);
            return Assembly.Load(ms.ToArray());
        }
    }
}