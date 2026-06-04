using System.Linq;
using System.Reflection;
using System.Text.Json;
using WorkflowSddHarness.Domain.Model;
using Xunit;

namespace WorkflowSddHarness.Domain.Tests;

public class HarnessConfigTests
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void Deserialize_GivenCamelCaseJson_PopulatesAllSections()
    {
        const string json = """
            {
                "version": 1,
                "paths": {
                    "specsRoot": ".specs/features",
                    "rules": ".harness/rules",
                    "state": ".specs/project/STATE.md",
                    "observability": ".harness/observability",
                    "src": "src"
                },
                "providers": {
                    "claude": { "enabled": true, "cmd": "claude", "headlessFlag": "-p", "modelFlag": "--model", "installHint": "npm i -g @anthropic-ai/claude-code" }
                },
                "effortMap": {
                    "claude": { "high": "claude-opus-4-8", "medium": "claude-sonnet-4-6", "low": "claude-haiku-4-5" }
                },
                "steps": {
                    "design":  { "provider": "claude", "effort": "high",   "maxTokens": 12000 },
                    "execute": { "provider": "claude", "effort": "medium", "maxTokens": 6000 }
                },
                "rules": { "apply": ["architecture", "testing"] },
                "gates": {
                    "coverage":  { "enabled": true, "policy": "WARN" },
                    "lintTasks": { "enabled": true, "policy": "HALT" }
                }
            }
            """;

        var config = JsonSerializer.Deserialize<HarnessConfig>(json, _options)!;

        Assert.Equal(1, config.Version);
        Assert.Equal(".specs/features", config.Paths.SpecsRoot);
        Assert.Equal(".harness/rules", config.Paths.Rules);
        Assert.Equal("src", config.Paths.Src);
        Assert.True(config.Providers.ContainsKey("claude"));
        Assert.True(config.Providers["claude"].Enabled);
        Assert.Equal("claude", config.Providers["claude"].Cmd);
        Assert.True(config.EffortMap.ContainsKey("claude"));
        Assert.Equal("claude-opus-4-8", config.EffortMap["claude"]["high"]);
        Assert.True(config.Steps.ContainsKey("design"));
        Assert.Equal(12000, config.Steps["design"].MaxTokens);
        Assert.Contains("architecture", config.Rules.Apply);
        Assert.Contains("testing", config.Rules.Apply);
        Assert.True(config.Gates.ContainsKey("coverage"));
        Assert.Equal("WARN", config.Gates["coverage"].Policy);
        Assert.True(config.Gates.ContainsKey("lintTasks"));
        Assert.Equal("HALT", config.Gates["lintTasks"].Policy);
    }

    [Fact]
    public void Deserialize_GivenStringWithAccents_KeepsTextIntact()
    {
        const string json = """
            {
                "version": 1,
                "paths": { "specsRoot": "", "rules": "", "state": "", "observability": "", "src": "" },
                "providers": {
                    "claude": { "enabled": true, "cmd": "claude", "headlessFlag": "-p", "modelFlag": "--model", "installHint": "Instale com: npm i -g @anthropic-ai/claude-code (atenção: requer Node.js ≥18)" }
                },
                "effortMap": {},
                "steps": {},
                "rules": { "apply": [] },
                "gates": {}
            }
            """;

        var config = JsonSerializer.Deserialize<HarnessConfig>(json, _options)!;

        Assert.Equal("Instale com: npm i -g @anthropic-ai/claude-code (atenção: requer Node.js ≥18)",
            config.Providers["claude"].InstallHint);
    }

    [Fact]
    public void Model_HasNoCostProperties()
    {
        var modelTypes = typeof(HarnessConfig).Assembly
            .GetTypes()
            .Where(t => t.Namespace == "WorkflowSddHarness.Domain.Model");

        var costProperties = modelTypes
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Where(p =>
                p.Name.Contains("cost", System.StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains("usdBrl", System.StringComparison.OrdinalIgnoreCase) ||
                p.Name.Contains("costCap", System.StringComparison.OrdinalIgnoreCase))
            .Select(p => $"{p.DeclaringType!.Name}.{p.Name}")
            .ToList();

        Assert.Empty(costProperties);
    }
}
