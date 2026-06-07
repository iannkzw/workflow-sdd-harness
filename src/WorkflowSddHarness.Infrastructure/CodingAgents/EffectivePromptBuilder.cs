namespace WorkflowSddHarness.Infrastructure.CodingAgents;

public static class EffectivePromptBuilder
{
    private const string SubagentsBlock =
        "\n\n=== SUBAGENTS RECOMENDADOS ===\nUtilize sub agents para ler, escrever e editar arquivos e responder ao agent principal.";

    public static string Build(string prompt, IReadOnlyList<string> skills, bool allowSubagents)
    {
        var result = skills.Count > 0
            ? string.Join(" ", skills.Select(s => $"/{s}")) + "\n" + prompt
            : prompt;

        if (allowSubagents)
            result += SubagentsBlock;

        return result;
    }
}
