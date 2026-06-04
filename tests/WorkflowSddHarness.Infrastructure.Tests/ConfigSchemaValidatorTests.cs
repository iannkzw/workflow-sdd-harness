using System.IO;
using System.Text.Json.Nodes;
using WorkflowSddHarness.Infrastructure.Config;
using Xunit;

namespace WorkflowSddHarness.Infrastructure.Tests;

public class ConfigSchemaValidatorTests
{
    private readonly ConfigSchemaValidator _validator = new();

    [Fact]
    public void Validate_WhenConfigIsValid_ReturnsOk()
    {
        var json = File.ReadAllText(Path.Combine("Fixtures", "config.valid.json"));
        var node = JsonNode.Parse(json)!;

        var outcome = _validator.Validate(node);

        Assert.True(outcome.IsValid);
        Assert.Empty(outcome.Errors);
    }

    [Fact]
    public void Validate_WhenRequiredFieldMissing_ReturnsErrorNamingField()
    {
        var json = """
            {
                "version": 1,
                "paths": {
                    "specsRoot": ".specs/features",
                    "rules": ".harness/rules",
                    "state": ".specs/project/STATE.md",
                    "observability": ".harness/observability",
                    "src": "src"
                }
            }
            """;
        var node = JsonNode.Parse(json)!;

        var outcome = _validator.Validate(node);

        Assert.False(outcome.IsValid);
        var allText = string.Join(" ", outcome.Errors.Select(e => $"{e.Field} {e.Message}"));
        Assert.Contains("providers", allText, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_WhenFieldHasWrongType_ReturnsError()
    {
        var json = """
            {
                "version": "one",
                "paths": {
                    "specsRoot": ".specs/features",
                    "rules": ".harness/rules",
                    "state": ".specs/project/STATE.md",
                    "observability": ".harness/observability",
                    "src": "src"
                },
                "providers": {}
            }
            """;
        var node = JsonNode.Parse(json)!;

        var outcome = _validator.Validate(node);

        Assert.False(outcome.IsValid);
    }

    [Fact]
    public void Validate_WhenExtraUnknownField_DoesNotFail()
    {
        var json = """
            {
                "version": 1,
                "paths": {
                    "specsRoot": ".specs/features",
                    "rules": ".harness/rules",
                    "state": ".specs/project/STATE.md",
                    "observability": ".harness/observability",
                    "src": "src"
                },
                "providers": {},
                "unknownFieldThatDoesNotExistInSchema": "some value"
            }
            """;
        var node = JsonNode.Parse(json)!;

        var outcome = _validator.Validate(node);

        Assert.True(outcome.IsValid);
    }
}
