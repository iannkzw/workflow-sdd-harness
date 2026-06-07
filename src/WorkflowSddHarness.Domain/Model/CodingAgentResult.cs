namespace WorkflowSddHarness.Domain.Model;

public sealed record CodingAgentResult
{
    public bool IsSuccess { get; init; }
    public string Text { get; init; } = "";
    public long? InputTokens { get; init; }
    public long? OutputTokens { get; init; }
    public CodingAgentErrorCategory ErrorCategory { get; init; }
    public string? ErrorMessage { get; init; }
    public string? RawOutput { get; init; }
    public string? Stderr { get; init; }
    public int? ExitCode { get; init; }

    public static CodingAgentResult Success(
        string text,
        long? inputTokens = null,
        long? outputTokens = null) =>
        new()
        {
            IsSuccess = true,
            Text = text,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            ErrorCategory = CodingAgentErrorCategory.None
        };

    public static CodingAgentResult Failure(
        CodingAgentErrorCategory errorCategory,
        string? errorMessage = null,
        string? rawOutput = null,
        string? stderr = null,
        int? exitCode = null) =>
        new()
        {
            IsSuccess = false,
            ErrorCategory = errorCategory,
            ErrorMessage = errorMessage,
            RawOutput = rawOutput,
            Stderr = stderr,
            ExitCode = exitCode
        };
}
