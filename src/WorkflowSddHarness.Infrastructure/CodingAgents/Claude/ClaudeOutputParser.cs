using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkflowSddHarness.Infrastructure.CodingAgents.Claude;

internal sealed record ClaudeParseOutcome
{
    public bool Success { get; init; }
    public string? Text { get; init; }
    public long? InputTokens { get; init; }
    public long? OutputTokens { get; init; }
}

internal sealed class ClaudeJsonResponse
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("usage")]
    public ClaudeUsage? Usage { get; set; }
}

internal sealed class ClaudeUsage
{
    [JsonPropertyName("input_tokens")]
    public long? InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public long? OutputTokens { get; set; }
}

internal static class ClaudeOutputParser
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ClaudeParseOutcome Parse(string stdout)
    {
        if (string.IsNullOrWhiteSpace(stdout))
            return new ClaudeParseOutcome { Success = false };

        var trimmed = stdout.Trim();

        ClaudeJsonResponse? response;
        try
        {
            response = JsonSerializer.Deserialize<ClaudeJsonResponse>(trimmed, _options);
        }
        catch (JsonException)
        {
            return new ClaudeParseOutcome { Success = false };
        }

        if (response?.Result is null)
            return new ClaudeParseOutcome { Success = false };

        return new ClaudeParseOutcome
        {
            Success = true,
            Text = response.Result,
            InputTokens = response.Usage?.InputTokens,
            OutputTokens = response.Usage?.OutputTokens
        };
    }
}
