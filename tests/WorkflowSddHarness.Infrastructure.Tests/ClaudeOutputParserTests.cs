using WorkflowSddHarness.Infrastructure.CodingAgents.Claude;

namespace WorkflowSddHarness.Infrastructure.Tests;

public class ClaudeOutputParserTests
{
    [Fact]
    public void Parse_ValidJsonWithUsage_ReturnsSuccessWithTokens()
    {
        const string json = """
            {
              "result": "The answer is 42",
              "usage": {
                "input_tokens": 100,
                "output_tokens": 50
              },
              "total_cost_usd": 0.001
            }
            """;

        var outcome = ClaudeOutputParser.Parse(json);

        Assert.True(outcome.Success);
        Assert.Equal("The answer is 42", outcome.Text);
        Assert.Equal(100L, outcome.InputTokens);
        Assert.Equal(50L, outcome.OutputTokens);
    }

    [Fact]
    public void Parse_ValidJsonWithoutUsage_ReturnsSuccessWithNullTokens()
    {
        const string json = """
            {
              "result": "Hello world"
            }
            """;

        var outcome = ClaudeOutputParser.Parse(json);

        Assert.True(outcome.Success);
        Assert.Equal("Hello world", outcome.Text);
        Assert.Null(outcome.InputTokens);
        Assert.Null(outcome.OutputTokens);
    }

    [Fact]
    public void Parse_InvalidJson_ReturnsFailure()
    {
        const string garbage = "this is not json at all }{";

        var outcome = ClaudeOutputParser.Parse(garbage);

        Assert.False(outcome.Success);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_NullOrEmptyInput_ReturnsFailure(string? input)
    {
        var outcome = ClaudeOutputParser.Parse(input!);

        Assert.False(outcome.Success);
    }

    [Fact]
    public void Parse_JsonWithTotalCostUsd_IgnoresFieldNoException()
    {
        const string json = """
            {
              "result": "some text",
              "usage": { "input_tokens": 10, "output_tokens": 5 },
              "total_cost_usd": 9999.99
            }
            """;

        var outcome = ClaudeOutputParser.Parse(json);

        Assert.True(outcome.Success);
        Assert.Equal("some text", outcome.Text);
    }

    [Fact]
    public void Parse_ValidJsonWithEmptyResult_ReturnsSuccessWithEmptyText()
    {
        const string json = """{ "result": "" }""";

        var outcome = ClaudeOutputParser.Parse(json);

        Assert.True(outcome.Success);
        Assert.Equal("", outcome.Text);
    }

    [Fact]
    public void Parse_JsonMissingResultField_ReturnsFailure()
    {
        const string json = """{ "usage": { "input_tokens": 10, "output_tokens": 5 } }""";

        var outcome = ClaudeOutputParser.Parse(json);

        Assert.False(outcome.Success);
    }

    [Fact]
    public void ParseOutcome_HasNoCostProperty()
    {
        var outcomeType = typeof(ClaudeParseOutcome);
        var properties = outcomeType.GetProperties();

        Assert.DoesNotContain(properties, p =>
            p.Name.Contains("Cost", StringComparison.OrdinalIgnoreCase));
    }
}
