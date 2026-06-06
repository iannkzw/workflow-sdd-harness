using System.IO;
using WorkflowSddHarness.Infrastructure.Config;
using Xunit;

namespace WorkflowSddHarness.Infrastructure.Tests;

public class ConfigLoaderTests
{
    private readonly ConfigLoader _loader = new();

    private static string FixturePath(string name) =>
        Path.GetFullPath(Path.Combine("Fixtures", name));

    [Fact]
    public void Load_WhenPathDoesNotExist_ReturnsFailureWithoutThrowing()
    {
        var result = _loader.Load(FixturePath("does-not-exist.json"));

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Load_WhenPathIsDirectory_ReturnsFailure()
    {
        var result = _loader.Load(FixturePath("target-project"));

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Load_WhenJsonIsMalformed_ReturnsParseFailure()
    {
        var result = _loader.Load(FixturePath("config.invalid-json.json"));

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("JSON", string.Join(" ", result.Errors), System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_WhenJsonIsEmpty_ReturnsParseFailure()
    {
        var emptyPath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(emptyPath, "");
            var result = _loader.Load(emptyPath);

            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
        }
        finally
        {
            File.Delete(emptyPath);
        }
    }

    [Fact]
    public void Load_WhenSchemaInvalid_ReturnsValidationFailure()
    {
        var result = _loader.Load(FixturePath("config.schema-invalid.json"));

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Load_WhenConfigValid_ReturnsSuccessWithPopulatedConfig()
    {
        var result = _loader.Load(FixturePath("config.valid.json"));

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Config);
        Assert.Equal(1, result.Config.Version);
        Assert.NotEmpty(result.Config.Providers);
        Assert.NotEmpty(result.Config.Steps);
    }

    [Fact]
    public void Load_WhenConfigHasAccentedStrings_KeepsThemIntact()
    {
        var result = _loader.Load(FixturePath("config.valid.json"));

        Assert.True(result.IsSuccess);
        var installHint = result.Config!.Providers["claude"].InstallHint;
        Assert.Contains("atenção", installHint, StringComparison.Ordinal);
        Assert.Contains("≥", installHint, StringComparison.Ordinal);
    }
}
