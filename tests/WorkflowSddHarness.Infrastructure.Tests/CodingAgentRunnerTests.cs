using System.Text.Json;
using WorkflowSddHarness.Infrastructure.CodingAgents;

namespace WorkflowSddHarness.Infrastructure.Tests;

public class CodingAgentRunnerTests
{
    private static readonly CodingAgentRunner Runner = new();
    private static readonly string StubPath = StubCliHelper.GetStubPath();

    [Fact]
    public async Task RunAsync_EchoMode_RoundTripUtf8WithAccents()
    {
        var input = "— ç ã é \"x\"";

        var result = await Runner.RunAsync(StubPath, ["echo"], input, timeout: null);

        Assert.True(result.Started);
        Assert.Equal(0, result.ExitCode);
        var doc = JsonDocument.Parse(result.StdOut.Trim());
        var text = doc.RootElement.GetProperty("result").GetString();
        Assert.Equal(input, text);
    }

    [Fact]
    public async Task RunAsync_Exit1Mode_CapturesSeparateStreamsAndExitCode()
    {
        var result = await Runner.RunAsync(StubPath, ["exit1"], "", timeout: null);

        Assert.True(result.Started);
        Assert.Equal(1, result.ExitCode);
        Assert.Contains("error on stderr", result.StdErr);
    }

    [Fact]
    public async Task RunAsync_SleepMode_TimesOut_ProcessKilled()
    {
        var result = await Runner.RunAsync(StubPath, ["sleep", "5000"], "", timeout: TimeSpan.FromMilliseconds(300));

        Assert.True(result.Started);
        Assert.True(result.TimedOut);
    }

    [Fact]
    public async Task RunAsync_ExternalCancel_PropagatesOperationCanceledException()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => Runner.RunAsync(StubPath, ["sleep", "5000"], "", timeout: null, cancellationToken: cts.Token));
    }

    [Fact]
    public async Task RunAsync_MissingBinary_ReturnsStartedFalse()
    {
        var result = await Runner.RunAsync("nonexistent-binary-xyz-abc", [], "", timeout: null);

        Assert.False(result.Started);
    }

    [Fact]
    public async Task RunAsync_EmptyMode_ReturnsEmptyStdout()
    {
        var result = await Runner.RunAsync(StubPath, ["empty"], "", timeout: null);

        Assert.True(result.Started);
        Assert.Equal(0, result.ExitCode);
        Assert.True(string.IsNullOrWhiteSpace(result.StdOut));
    }

    [Fact]
    public async Task RunAsync_GarbageMode_CapturesNonJsonOutput()
    {
        var result = await Runner.RunAsync(StubPath, ["garbage"], "", timeout: null);

        Assert.True(result.Started);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("not-json", result.StdOut);
    }
}
