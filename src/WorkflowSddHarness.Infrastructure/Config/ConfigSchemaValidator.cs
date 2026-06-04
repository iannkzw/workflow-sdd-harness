using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace WorkflowSddHarness.Infrastructure.Config;

public sealed record ValidationError(string Field, string Message);

public sealed record ValidationOutcome
{
    public bool IsValid { get; init; }
    public IReadOnlyList<ValidationError> Errors { get; init; } = [];

    public static ValidationOutcome Ok() => new() { IsValid = true };

    public static ValidationOutcome Fail(IReadOnlyList<ValidationError> errors) =>
        new() { IsValid = false, Errors = errors };
}

public sealed class ConfigSchemaValidator
{
    private readonly JsonSchema _schema;

    public ConfigSchemaValidator()
    {
        var assembly = typeof(ConfigSchemaValidator).Assembly;
        const string resourceName =
            "WorkflowSddHarness.Infrastructure.Config.Schemas.config.schema.v1.json";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded schema resource not found: {resourceName}");

        using var reader = new StreamReader(stream);
        _schema = JsonSchema.FromText(reader.ReadToEnd());
    }

    public ValidationOutcome Validate(JsonNode rawConfig)
    {
        var element = rawConfig.Deserialize<JsonElement>();
        var results = _schema.Evaluate(element, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List
        });

        if (results.IsValid)
            return ValidationOutcome.Ok();

        var errors = (results.Details ?? [])
            .Where(d => !d.IsValid && d.Errors is { Count: > 0 })
            .SelectMany(d => d.Errors!.Select(kvp => new ValidationError(
                Field: d.InstanceLocation.ToString(),
                Message: kvp.Value)))
            .ToList();

        return ValidationOutcome.Fail(errors);
    }
}
