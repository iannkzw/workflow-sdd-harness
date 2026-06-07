using WorkflowSddHarness.Domain.Model;
using WorkflowSddHarness.Infrastructure.CodingAgents;
using WorkflowSddHarness.Infrastructure.CodingAgents.Claude;

namespace WorkflowSddHarness.Infrastructure.Tests;

public class ClaudeCliAdapterTests
{
    private static readonly string StubPath = StubCliHelper.GetStubPath();
    private static readonly CodingAgentRunner Runner = new();

    private static ClaudeCliAdapter MakeAdapter(string headlessFlag, string modelFlag = "--model") =>
        new(new ProviderConfig { Cmd = StubPath, HeadlessFlag = headlessFlag, ModelFlag = modelFlag }, Runner);

    private static CodingAgentRequest MakeRequest(string prompt = "hello", string model = "test-model") =>
        new() { Prompt = prompt, Model = model };

    [Fact]
    public async Task RunAsync_FixedStub_ReturnsSuccessWithText()
    {
        var adapter = MakeAdapter("fixed");

        var result = await adapter.RunAsync(MakeRequest());

        Assert.True(result.IsSuccess);
        Assert.Equal("Olá, ação! ç ã é.", result.Text);
        Assert.Equal(5L, result.InputTokens);
        Assert.Equal(5L, result.OutputTokens);
        Assert.Equal(CodingAgentErrorCategory.None, result.ErrorCategory);
    }

    [Fact]
    public async Task RunAsync_EchoStub_RoundTripUtf8()
    {
        var adapter = MakeAdapter("echo");
        var prompt = "— ç ã é \"aspas\"";

        var result = await adapter.RunAsync(MakeRequest(prompt));

        Assert.True(result.IsSuccess);
        Assert.Equal(prompt, result.Text);
    }

    [Fact]
    public async Task RunAsync_Exit1Stub_ReturnsNonZeroExit()
    {
        var adapter = MakeAdapter("exit1");

        var result = await adapter.RunAsync(MakeRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal(CodingAgentErrorCategory.NonZeroExit, result.ErrorCategory);
        Assert.Equal(1, result.ExitCode);
        Assert.Contains("error on stderr", result.Stderr);
    }

    [Fact]
    public async Task RunAsync_EmptyStub_ReturnsEmptyOutput()
    {
        var adapter = MakeAdapter("empty");

        var result = await adapter.RunAsync(MakeRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal(CodingAgentErrorCategory.EmptyOutput, result.ErrorCategory);
    }

    [Fact]
    public async Task RunAsync_GarbageStub_ReturnsMalformedOutputWithRawOutput()
    {
        var adapter = MakeAdapter("garbage");

        var result = await adapter.RunAsync(MakeRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal(CodingAgentErrorCategory.MalformedOutput, result.ErrorCategory);
        Assert.NotNull(result.RawOutput);
        Assert.Contains("not-json", result.RawOutput);
    }

    [Fact]
    public async Task RunAsync_SleepStub_ExceedsTimeout_ReturnsTimedOut()
    {
        var adapter = MakeAdapter("sleep", "5000");

        var result = await adapter.RunAsync(
            MakeRequest() with { Timeout = TimeSpan.FromMilliseconds(300) });

        Assert.False(result.IsSuccess);
        Assert.Equal(CodingAgentErrorCategory.TimedOut, result.ErrorCategory);
    }

    [Fact]
    public async Task RunAsync_MissingBinary_ReturnsCliNotFound()
    {
        var adapter = new ClaudeCliAdapter(
            new ProviderConfig { Cmd = "nonexistent-cli-xyz", HeadlessFlag = "-p", ModelFlag = "--model" },
            Runner);

        var result = await adapter.RunAsync(MakeRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal(CodingAgentErrorCategory.CliNotFound, result.ErrorCategory);
    }

    [Fact]
    public async Task RunAsync_ExpectedFailures_NeverThrow()
    {
        string[] modes = ["exit1", "empty", "garbage"];
        foreach (var mode in modes)
        {
            var adapter = MakeAdapter(mode);
            var ex = await Record.ExceptionAsync(() => adapter.RunAsync(MakeRequest()));
            Assert.Null(ex);
        }
    }
}
