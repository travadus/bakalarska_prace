using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEngine;

/// <summary>
/// A syntax analyzer that performs code validation.
/// </summary>
public class CodeSecurityGuard : CSharpSyntaxWalker
{
    public List<string> FoundErrors { get; private set; } = new List<string>();

    private ResearchManager _researchManager;

    public CodeSecurityGuard()
    {
        _researchManager = ResearchManager.Instance;
    }

    // --- SYNTAX NODE VISITORS ---

    /// <summary>
    /// Validates variable declarations against the research requirement.
    /// </summary>
    public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
    {
        if (!IsUnlocked("tech_variables"))
        {
            AddError(node, "Variables are locked! Research 'Variable Storage'.");
        }
        base.VisitVariableDeclaration(node);
    }

    /// <summary>
    /// Checks arithmetic operations against the research requirement.
    /// </summary>
    public override void VisitBinaryExpression(BinaryExpressionSyntax node)
    {
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

    /// <summary>
    /// Validates conditional branching against the research requirement.
    /// </summary>
    public override void VisitIfStatement(IfStatementSyntax node)
    {
        if (!IsUnlocked("tech_conditions"))
        {
            AddError(node, "Conditional logic (if/else) is locked! Research 'Logic Gates'.");
        }
        base.VisitIfStatement(node);
    }

    /// <summary>
    /// Validates switch statements against the research requirement.
    /// </summary>
    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    {
        if (!IsUnlocked("tech_switch"))
        {
            AddError(node, "Switch statements are locked! Research 'Advanced Branching'.");
        }
        base.VisitSwitchStatement(node);
    }

    /// <summary>
    /// Validates while loops against the research requirement.
    /// </summary>
    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        if (!IsUnlocked("tech_loops"))
        {
            AddError(node, "While loops are locked! Research 'Looping Structures'.");
        }
        base.VisitWhileStatement(node);
    }

    /// <summary>
    /// Validates for loops against the research requirement.
    /// </summary>
    public override void VisitForStatement(ForStatementSyntax node)
    {
        if (!IsUnlocked("tech_loops"))
        {
            AddError(node, "For loops are locked! Research 'Looping Structures'.");
        }
        base.VisitForStatement(node);
    }

    /// <summary>
    /// Monitors method searching for Random-related calls to enforce research restrictions.
    /// </summary>
    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        string calledMethod = node.Expression.ToString();

        if (calledMethod.Contains("Random") || calledMethod.Contains("UnityEngine.Random") || calledMethod.Contains("System.Random"))
        {
            if (!IsUnlocked("tech_random"))
            {
                AddError(node, "RNG (Random) is locked! Research 'Random generator'.");
            }
        }

        base.VisitInvocationExpression(node);
    }

    /// <summary>
    /// Validates array declarations against the research requirement.
    /// </summary>
    public override void VisitArrayType(ArrayTypeSyntax node)
    {
        if (!IsUnlocked("tech_arrays"))
        {
            AddError(node, "Arrays [] are locked! Research 'Data Structures I'.");
        }
        base.VisitArrayType(node);
    }

    /// <summary>
    /// Checks for List usage against the research requirement.
    /// </summary>
    public override void VisitGenericName(GenericNameSyntax node)
    {
        if (node.Identifier.Text == "List")
        {
            if (!IsUnlocked("tech_lists"))
            {
                AddError(node, "Lists (List<T>) are locked! Research 'Data Structures II'.");
            }
        }
        base.VisitGenericName(node);
    }

    /// <summary>
    /// Validates custom method definitions.
    /// </summary>
    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        string methodName = node.Identifier.Text;

        if (methodName == "Main")
        {
            base.VisitMethodDeclaration(node);
            return;
        }

        if (!IsUnlocked("tech_methods"))
        {
            AddError(node, $"Defining custom method '{methodName}' is restricted. Research 'Modular Programming' to unlock function definitions.");
            return;
        }

        base.VisitMethodDeclaration(node);
    }

    // --- HELPER METHODS ---

    /// <summary>
    /// Checks if a specific technology is unlocked via the Research Manager.
    /// </summary>
    private bool IsUnlocked(string techID)
    {
        if (_researchManager == null) return false;
        return _researchManager.IsTechUnlocked(techID);
    }

    /// <summary>
    /// Appends an error message including the line number to the error list.
    /// </summary>
    private void AddError(SyntaxNode node, string message)
    {
        int line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        FoundErrors.Add($"Line {line}: {message}");
    }
}