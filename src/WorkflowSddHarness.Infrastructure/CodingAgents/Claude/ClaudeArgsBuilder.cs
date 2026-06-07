namespace WorkflowSddHarness.Infrastructure.CodingAgents.Claude;

internal static class ClaudeArgsBuilder
{
    private const string BaseTools = "Read,Grep,Glob,Edit,Write,Bash(dotnet build *),Bash(dotnet test *)";

    public static IReadOnlyList<string> Build(
        string headlessFlag,
        string modelFlag,
        string model,
        bool allowSubagents,
        IReadOnlyList<string> extraTools)
    {
        var tools = BaseTools;

        if (allowSubagents)
            tools += ",Agent";

        if (extraTools.Count > 0)
            tools += "," + string.Join(",", extraTools);

        return new[]
        {
            headlessFlag,
            modelFlag,
            model,
            "--output-format", "json",
            "--permission-mode", "acceptEdits",
            "--allowedTools", tools
        };
    }
}
