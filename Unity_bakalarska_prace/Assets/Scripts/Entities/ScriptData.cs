using System;

/// <summary>
/// Data model representing a user-created script.
/// </summary>
[Serializable]
public class ScriptData
{
    public string scriptName;
    public string sourceCode;
    public bool isRunning;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptData"/> class.
    /// </summary>
    /// <param name="name">The name of the script.</param>
    /// <param name="defaultCode">The initial boilerplate or existing source code.</param>
    public ScriptData(string name, string defaultCode)
    {
        this.scriptName = name;
        this.sourceCode = defaultCode;
        this.isRunning = false;
    }
}