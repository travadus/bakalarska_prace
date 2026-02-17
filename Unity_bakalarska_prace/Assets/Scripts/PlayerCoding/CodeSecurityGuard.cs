using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;

public class CodeSecurityGuard : CSharpSyntaxWalker
{
    public List<string> FoundErrors { get; private set; } = new List<string>();

    // Cache reference to manager to avoid calling Instance repeatedly
    private ResearchManager _researchManager;

    public CodeSecurityGuard()
    {
        _researchManager = ResearchManager.Instance;
    }

    // --- 1. VARIABLES ---
    public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        // Check if "tech_variables" is unlocked
        if (!IsUnlocked("tech_variables"))
        {
            AddError(node, "Variables are locked! Research 'Variable Storage'.");
        }
        base.VisitVariableDeclaration(node);
    }

    // --- 2. OPERATORS (+, -, *, /, %) ---
    public override void VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        // We only care about arithmetic operators for now
        if (node.IsKind(SyntaxKind.AddExpression) ||
            node.IsKind(SyntaxKind.SubtractExpression) ||
            node.IsKind(SyntaxKind.MultiplyExpression) ||
            node.IsKind(SyntaxKind.DivideExpression) ||
            node.IsKind(SyntaxKind.ModuloExpression))
        {
            if (!IsUnlocked("tech_operators"))
            {
                AddError(node, $"Operator '{node.OperatorToken}' is locked! Research 'Basic Math Operators'.");
            }
        }
        base.VisitBinaryExpression(node);
    }

    // --- 3. IF / ELSE ---
    public override void VisitIfStatement(IfStatementSyntax node)
    {
        if (!IsUnlocked("tech_conditions"))
        {
            AddError(node, "Conditional logic (if/else) is locked! Research 'Logic Gates'.");
        }
        base.VisitIfStatement(node);
    }

    // --- 4. SWITCH ---
    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    {
        if (!IsUnlocked("tech_switch"))
        {
            AddError(node, "Switch statements are locked! Research 'Advanced Branching'.");
        }
        base.VisitSwitchStatement(node);
    }

    // --- 5. WHILE LOOPS ---
    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        if (!IsUnlocked("tech_loops"))
        {
            AddError(node, "While loops are locked! Research 'Looping Structures'.");
        }
        base.VisitWhileStatement(node);
    }

    // Also block For/Do loops if they fall under the same category
    public override void VisitForStatement(ForStatementSyntax node)
    {
        if (!IsUnlocked("tech_loops")) AddError(node, "For loops are locked! Research 'Looping Structures'.");
        base.VisitForStatement(node);
    }

    // --- 6. RANDOM ---
    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        string calledMethod = node.Expression.ToString();

        // Check for Random.Range, System.Random, etc.
        if (calledMethod.Contains("Random") || calledMethod.Contains("UnityEngine.Random") || calledMethod.Contains("System.Random"))
        {
            if (!IsUnlocked("tech_random"))
            {
                AddError(node, "RNG (Random) is locked! Research 'Random generator'.");
            }
        }

        base.VisitInvocationExpression(node);
    }

    // --- 7. ARRAYS ---
    public override void VisitArrayType(ArrayTypeSyntax node)
    {
        if (!IsUnlocked("tech_arrays"))
        {
            AddError(node, "Arrays [] are locked! Research 'Data Structures I'.");
        }
        base.VisitArrayType(node);
    }

    // --- 8. LISTS ---
    public override void VisitGenericName(GenericNameSyntax node)
    {
        // Checks for List<T>
        if (node.Identifier.Text == "List")
        {
            if (!IsUnlocked("tech_lists"))
            {
                AddError(node, "Lists (List<T>) are locked! Research 'Data Structures II'.");
            }
        }
        base.VisitGenericName(node);
    }

    // --- 9. USER-DEFINED METHODS ---
    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        string methodName = node.Identifier.Text;

        // The entry point 'Main' is always allowed, as it is required for script execution.
        // We must still traverse its body to validate the code inside.
        if (methodName == "Main")
        {
            base.VisitMethodDeclaration(node);
            return;
        }

        // Any method other than 'Main' is considered a custom user-defined function.
        // This capability is restricted until the specific technology is researched.
        if (!IsUnlocked("tech_methods"))
        {
            AddError(node, $"Defining custom method '{methodName}' is restricted. Research 'Modular Programming' to unlock function definitions.");
            return;
        }

        // If the technology is unlocked, proceed with standard validation of the method body.
        base.VisitMethodDeclaration(node);
    }

    // --- Helper Methods ---

    private bool IsUnlocked(string techID)
    {
        if (_researchManager == null) return false; // Default to locked if manager missing
        return _researchManager.IsTechUnlocked(techID);
    }

    private void AddError(SyntaxNode node, string message)
    {
        // Calculate line number (0-based -> 1-based)
        int line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        FoundErrors.Add($"Line {line}: {message}");
    }
}