using WorkflowSddHarness.Infrastructure.CodingAgents;

namespace WorkflowSddHarness.Infrastructure.Tests;

public class EffectivePromptBuilderTests
{
    private const string SubagentsBlock =
        "\n\n=== SUBAGENTS RECOMENDADOS ===\nUtilize sub agents para ler, escrever e editar arquivos e responder ao agent principal.";

    [Fact]
    public void Build_EmptySkills_NoSubagents_ReturnsPromptUnchanged()
    {
        var result = EffectivePromptBuilder.Build("do the thing", [], false);

        Assert.Equal("do the thing", result);
    }

    [Fact]
    public void Build_EmptySkills_WithSubagents_AppendsSubagentsBlock()
    {
        var result = EffectivePromptBuilder.Build("do the thing", [], true);

        Assert.Equal("do the thing" + SubagentsBlock, result);
    }

    [Fact]
    public void Build_WithSkills_NoSubagents_PrefixesSkillsAndKeepsPromptIntact()
    {
        var result = EffectivePromptBuilder.Build("do the thing", ["coding-guidelines", "best-practices"], false);

        Assert.Equal("/coding-guidelines /best-practices\ndo the thing", result);
    }

    [Fact]
    public void Build_WithSkills_WithSubagents_PrefixesSkillsAndAppendsSubagentsBlock()
    {
        var result = EffectivePromptBuilder.Build("do the thing", ["coding-guidelines", "best-practices"], true);

        Assert.Equal("/coding-guidelines /best-practices\ndo the thing" + SubagentsBlock, result);
    }
}
