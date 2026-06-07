namespace WorkflowSddHarness.Domain.Model;

public enum CodingAgentErrorCategory
{
    None,
    CliNotFound,
    NonZeroExit,
    EmptyOutput,
    MalformedOutput,
    TimedOut
}
