using System.Collections.Generic;
using System.IO;
using System.Linq;
using WorkflowSddHarness.Domain.Model;

namespace WorkflowSddHarness.Infrastructure.Config;

public sealed class TargetProjectPaths
{
    public string ProjectRoot { get; }
    public string HarnessDir { get; }
    public string SpecsRoot { get; }
    public string RulesDir { get; }
    public string Src { get; }
    public string ObservabilityDir { get; }

    private TargetProjectPaths(string configPath, HarnessConfig config)
    {
        HarnessDir = Path.GetDirectoryName(Path.GetFullPath(configPath))!;
        ProjectRoot = Path.GetDirectoryName(HarnessDir)!;
        SpecsRoot = Path.GetFullPath(Path.Combine(ProjectRoot, config.Paths.SpecsRoot));
        RulesDir = Path.GetFullPath(Path.Combine(ProjectRoot, config.Paths.Rules));
        Src = Path.GetFullPath(Path.Combine(ProjectRoot, config.Paths.Src));
        ObservabilityDir = Path.GetFullPath(Path.Combine(ProjectRoot, config.Paths.Observability));
    }

    public static TargetProjectPaths Resolve(string configPath, HarnessConfig config) =>
        new(configPath, config);

    public IReadOnlyList<string> MissingRequiredPaths()
    {
        var required = new[] { SpecsRoot, RulesDir, Src };
        return required
            .Where(p => !Directory.Exists(p) && !File.Exists(p))
            .ToList();
    }
}
