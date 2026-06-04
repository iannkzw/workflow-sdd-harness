using System.Collections.Generic;

namespace WorkflowSddHarness.Domain.Model;

public sealed record HarnessConfig
{
    public int Version { get; init; }
    public PathsConfig Paths { get; init; } = new();
    public IReadOnlyDictionary<string, ProviderConfig> Providers { get; init; } = new Dictionary<string, ProviderConfig>();
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> EffortMap { get; init; } = new Dictionary<string, IReadOnlyDictionary<string, string>>();
    public IReadOnlyDictionary<string, StepConfig> Steps { get; init; } = new Dictionary<string, StepConfig>();
    public IReadOnlyDictionary<string, GatesConfig> Gates { get; init; } = new Dictionary<string, GatesConfig>();
    public RulesApplyConfig Rules { get; init; } = new();
}

public sealed record PathsConfig
{
    public string SpecsRoot { get; init; } = "";
    public string Rules { get; init; } = "";
    public string State { get; init; } = "";
    public string Observability { get; init; } = "";
    public string Src { get; init; } = "";
}

public sealed record ProviderConfig
{
    public bool Enabled { get; init; }
    public string Cmd { get; init; } = "";
    public string HeadlessFlag { get; init; } = "";
    public string ModelFlag { get; init; } = "";
    public string InstallHint { get; init; } = "";
}

public sealed record StepConfig
{
    public string Provider { get; init; } = "";
    public string Effort { get; init; } = "";
    public int MaxTokens { get; init; }
}

public sealed record GatesConfig
{
    public bool Enabled { get; init; }
    public string Policy { get; init; } = "";
}

public sealed record RulesApplyConfig
{
    public IReadOnlyList<string> Apply { get; init; } = [];
}
