using WorkflowSddHarness.Domain.Model;
using WorkflowSddHarness.Domain.Ports;

namespace WorkflowSddHarness.Infrastructure.CodingAgents.Claude;

public sealed class ClaudeCliAdapter : ICodingAgentPort
{
    private readonly ProviderConfig _config;
    private readonly CodingAgentRunner _runner;

    public ClaudeCliAdapter(ProviderConfig providerConfig, CodingAgentRunner runner)
    {
        _config = providerConfig;
        _runner = runner;
    }

    public async Task<CodingAgentResult> RunAsync(CodingAgentRequest request, CancellationToken cancellationToken = default)
    {
        var effectivePrompt = EffectivePromptBuilder.Build(request.Prompt, request.Skills, request.AllowSubagents);
        var args = ClaudeArgsBuilder.Build(_config.HeadlessFlag, _config.ModelFlag, request.Model, request.AllowSubagents, request.ExtraTools);

        var proc = await _runner.RunAsync(_config.Cmd, args, effectivePrompt, request.Timeout, cancellationToken);

        if (!proc.Started)
            return CodingAgentResult.Failure(CodingAgentErrorCategory.CliNotFound);

        if (proc.TimedOut)
            return CodingAgentResult.Failure(CodingAgentErrorCategory.TimedOut);

        if (proc.ExitCode != 0)
            return CodingAgentResult.Failure(
                CodingAgentErrorCategory.NonZeroExit,
                stderr: proc.StdErr,
                exitCode: proc.ExitCode);

        if (string.IsNullOrWhiteSpace(proc.StdOut))
            return CodingAgentResult.Failure(CodingAgentErrorCategory.EmptyOutput);

        var parsed = ClaudeOutputParser.Parse(proc.StdOut);

        if (!parsed.Success)
            return CodingAgentResult.Failure(CodingAgentErrorCategory.MalformedOutput, rawOutput: proc.StdOut);

        if (string.IsNullOrEmpty(parsed.Text))
            return CodingAgentResult.Failure(CodingAgentErrorCategory.EmptyOutput);

        return CodingAgentResult.Success(parsed.Text, parsed.InputTokens, parsed.OutputTokens);
    }
}
