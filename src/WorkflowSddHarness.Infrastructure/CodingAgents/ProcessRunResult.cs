namespace WorkflowSddHarness.Infrastructure.CodingAgents;

internal sealed record ProcessRunResult
{
    public bool Started { get; init; }
    public string StdOut { get; init; } = "";
    public string StdErr { get; init; } = "";
    public int ExitCode { get; init; }
    public bool TimedOut { get; init; }
}
