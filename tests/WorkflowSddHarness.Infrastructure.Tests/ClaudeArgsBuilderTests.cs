using WorkflowSddHarness.Infrastructure.CodingAgents.Claude;

namespace WorkflowSddHarness.Infrastructure.Tests;

public class ClaudeArgsBuilderTests
{
    private const string BaseTools = "Read,Grep,Glob,Edit,Write,Bash(dotnet build *),Bash(dotnet test *)";

    [Fact]
    public void Build_BasicArgs_NoSubagents_NoExtraTools()
    {
        var result = ClaudeArgsBuilder.Build("-p", "--model", "claude-opus-4-8", false, []);

        Assert.Equal(["-p", "--model", "claude-opus-4-8", "--output-format", "json", "--permission-mode", "acceptEdits", "--allowedTools", BaseTools], result);
    }

    [Fact]
    public void Build_WithAllowSubagents_AppendsAgentToTools()
    {
        var result = ClaudeArgsBuilder.Build("-p", "--model", "claude-opus-4-8", true, []);

        var toolsArg = result[result.Count - 1];
        Assert.Contains(",Agent", toolsArg);
        Assert.Equal(BaseTools + ",Agent", toolsArg);
    }

    [Fact]
    public void Build_WithExtraTools_AppendsThemToToolsString()
    {
        var result = ClaudeArgsBuilder.Build("-p", "--model", "claude-opus-4-8", false, ["mcp__ide", "mcp__fs"]);

        var toolsArg = result[result.Count - 1];
        Assert.Equal(BaseTools + ",mcp__ide,mcp__fs", toolsArg);
    }

    [Fact]
    public void Build_WithSubagentsAndExtraTools_AgentBeforeExtraTools()
    {
        var result = ClaudeArgsBuilder.Build("-p", "--model", "claude-opus-4-8", true, ["mcp__ide"]);

        Assert.Equal(["-p", "--model", "claude-opus-4-8", "--output-format", "json", "--permission-mode", "acceptEdits", "--allowedTools", BaseTools + ",Agent,mcp__ide"], result);
    }

    [Fact]
    public void Build_DoesNotEmitForbiddenFlags()
    {
        var result = ClaudeArgsBuilder.Build("-p", "--model", "claude-opus-4-8", true, ["mcp__ide"]);

        Assert.DoesNotContain("--max-tokens", result);
        Assert.DoesNotContain("--timeout", result);
        Assert.DoesNotContain("--append-system-prompt", result);
    }
}
