using System.Linq;
using System.Reflection;
using WorkflowSddHarness.Domain.Model;

namespace WorkflowSddHarness.Domain.Tests;

public class CodingAgentResultTests
{
    [Fact]
    public void Success_SetsIsSuccessTrueAndText()
    {
        var result = CodingAgentResult.Success("response text");

        Assert.True(result.IsSuccess);
        Assert.Equal("response text", result.Text);
        Assert.Equal(CodingAgentErrorCategory.None, result.ErrorCategory);
    }

    [Fact]
    public void Success_SetsTokensWhenProvided()
    {
        var result = CodingAgentResult.Success("text", inputTokens: 100, outputTokens: 200);

        Assert.Equal(100L, result.InputTokens);
        Assert.Equal(200L, result.OutputTokens);
    }

    [Fact]
    public void Success_TokensAreNullByDefault()
    {
        var result = CodingAgentResult.Success("text");

        Assert.Null(result.InputTokens);
        Assert.Null(result.OutputTokens);
    }

    [Fact]
    public void Failure_SetsIsSuccessFalseAndErrorCategory()
    {
        var result = CodingAgentResult.Failure(CodingAgentErrorCategory.CliNotFound, "cli missing");

        Assert.False(result.IsSuccess);
        Assert.Equal(CodingAgentErrorCategory.CliNotFound, result.ErrorCategory);
        Assert.Equal("cli missing", result.ErrorMessage);
    }

    [Fact]
    public void Failure_SetsAllErrorFields()
    {
        var result = CodingAgentResult.Failure(
            CodingAgentErrorCategory.NonZeroExit,
            errorMessage: "process failed",
            rawOutput: "raw",
            stderr: "error output",
            exitCode: 1);

        Assert.Equal(CodingAgentErrorCategory.NonZeroExit, result.ErrorCategory);
        Assert.Equal("process failed", result.ErrorMessage);
        Assert.Equal("raw", result.RawOutput);
        Assert.Equal("error output", result.Stderr);
        Assert.Equal(1, result.ExitCode);
    }

    [Fact]
    public void Failure_OptionalFieldsAreNullByDefault()
    {
        var result = CodingAgentResult.Failure(CodingAgentErrorCategory.TimedOut);

        Assert.Null(result.ErrorMessage);
        Assert.Null(result.RawOutput);
        Assert.Null(result.Stderr);
        Assert.Null(result.ExitCode);
    }

    [Fact]
    public void Model_HasNoCostProperties()
    {
        var modelTypes = typeof(CodingAgentResult).Assembly
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
