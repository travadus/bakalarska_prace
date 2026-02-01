[System.Serializable]
public class ScriptData
{
    public string scriptName;
    public string sourceCode;
    public bool isRunning;

    public ScriptData(string name, string defaultCode)
    {
        this.scriptName = name;
        this.sourceCode = defaultCode;
        this.isRunning = false;
    }
}