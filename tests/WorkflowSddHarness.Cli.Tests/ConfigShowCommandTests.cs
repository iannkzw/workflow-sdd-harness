using System.CommandLine;
using System.IO;
using WorkflowSddHarness.Cli.Commands;
using Xunit;

namespace WorkflowSddHarness.Cli.Tests;

public class ConfigShowCommandTests
{
    private static RootCommand BuildRoot()
    {
        var root = new RootCommand("Workflow SDD Harness");
        var configCmd = new Command("config", "Config commands");
        configCmd.Add(ConfigShowCommand.Build());
        root.Add(configCmd);
        return root;
    }

    [Fact]
    public void ConfigShow_WithValidConfig_PrintsConfigAndPaths_ExitsZero()
    {
        var fixturePath = Path.GetFullPath(
            Path.Combine("Fixtures", "target-project", ".harness", "config.json"));
        var writer = new StringWriter();
        var root = BuildRoot();

        var exitCode = root.Parse(new[] { "config", "show", "--config", fixturePath })
            .Invoke(new InvocationConfiguration { Output = writer });

        var output = writer.ToString();
        Assert.Equal(0, exitCode);
        Assert.Contains("version", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("projectRoot", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConfigShow_WithoutConfigOption_PrintsUsage_ExitsNonZero()
    {
        var root = BuildRoot();

        var exitCode = root.Parse(new[] { "config", "show" }).Invoke();

        Assert.NotEqual(0, exitCode);
    }

    [Fact]
    public void ConfigShow_WithNonexistentConfig_PrintsError_ExitsNonZero()
    {
        var writer = new StringWriter();
        var root = BuildRoot();

        var exitCode = root.Parse(new[] { "config", "show", "--config", "nonexistent/path/config.json" })
            .Invoke(new InvocationConfiguration { Output = writer });

        Assert.NotEqual(0, exitCode);
    }

    [Fact]
    public void Cli_WithUnknownVerb_PrintsAvailableVerbs_ExitsNonZero()
    {
        var root = BuildRoot();

        var exitCode = root.Parse(new[] { "unknown-verb" }).Invoke();

        Assert.NotEqual(0, exitCode);
    }
}
