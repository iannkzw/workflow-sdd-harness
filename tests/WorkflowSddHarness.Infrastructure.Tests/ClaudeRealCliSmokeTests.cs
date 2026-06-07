using WorkflowSddHarness.Domain.Model;
using WorkflowSddHarness.Infrastructure.CodingAgents;
using WorkflowSddHarness.Infrastructure.CodingAgents.Claude;

namespace WorkflowSddHarness.Infrastructure.Tests;

[Trait("Category", "RealCli")]
public class ClaudeRealCliSmokeTests
{
    private static readonly ProviderConfig RealConfig = new()
    {
        Cmd = "claude",
        HeadlessFlag = "-p",
        ModelFlag = "--model"
    };

    private static readonly CodingAgentRunner Runner = new();

    private static bool IsClaudeInPath() =>
        (Environment.GetEnvironmentVariable("PATH") ?? "")
        .Split(Path.PathSeparator)
        .Any(dir =>
            File.Exists(Path.Combine(dir, "claude")) ||
            File.Exists(Path.Combine(dir, "claude.exe")) ||
            File.Exists(Path.Combine(dir, "claude.cmd")));

    private static bool ShouldRun() =>
        IsClaudeInPath() && Environment.GetEnvironmentVariable("HARNESS_E2E") == "1";

    [SkippableFact]
    public async Task RunAsync_RealClaude_AccentedPrompt_ReturnsSuccessWithUsage()
    {
        Skip.IfNot(ShouldRun(), "Skipped: claude not in PATH or HARNESS_E2E != 1");

        var adapter = new ClaudeCliAdapter(RealConfig, Runner);
        var request = new CodingAgentRequest
        {
            Prompt = "Responda apenas com a palavra: ação",
            Model = "claude-haiku-4-5-20251001",
            Timeout = TimeSpan.FromSeconds(60)
        };

        var result = await adapter.RunAsync(request);

        Assert.True(result.IsSuccess, $"Expected success but got: {result.ErrorCategory} — {result.ErrorMessage}");
        Assert.False(string.IsNullOrWhiteSpace(result.Text), "Expected non-empty result text");
        Assert.True(result.InputTokens.HasValue, "Expected usage.input_tokens in response");
        Assert.True(result.OutputTokens.HasValue, "Expected usage.output_tokens in response");
    }
}
