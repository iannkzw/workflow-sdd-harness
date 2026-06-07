using System.Collections.Generic;

namespace WorkflowSddHarness.Domain.Model;

public sealed record CodingAgentRequest
{
    public required string Prompt { get; init; }
    public required string Model { get; init; }
    public IReadOnlyList<string> Skills { get; init; } = [];
    public IReadOnlyList<string> ExtraTools { get; init; } = [];
    public bool AllowSubagents { get; init; } = false;
    public System.TimeSpan? Timeout { get; init; } = null;
}
