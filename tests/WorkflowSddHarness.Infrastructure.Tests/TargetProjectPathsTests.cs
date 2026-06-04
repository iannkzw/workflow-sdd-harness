using System.IO;
using WorkflowSddHarness.Domain.Model;
using WorkflowSddHarness.Infrastructure.Config;
using Xunit;

namespace WorkflowSddHarness.Infrastructure.Tests;

public class TargetProjectPathsTests
{
    private static readonly string FixtureConfigPath =
        Path.GetFullPath(Path.Combine("Fixtures", "target-project", ".harness", "config.json"));

    private static readonly string FixtureProjectRoot =
        Path.GetFullPath(Path.Combine("Fixtures", "target-project"));

    private static HarnessConfig MakeConfig(
        string specsRoot = ".specs/features",
        string rules = "rules",
        string src = "src",
        string observability = ".harness/observability") =>
        new()
        {
            Paths = new PathsConfig
            {
                SpecsRoot = specsRoot,
                Rules = rules,
                State = ".specs/project/STATE.md",
                Observability = observability,
                Src = src
            }
        };

    [Fact]
    public void Resolve_GivenConfigInsideHarness_ProjectRootIsHarnessParent()
    {
        var paths = TargetProjectPaths.Resolve(FixtureConfigPath, MakeConfig());

        Assert.Equal(FixtureProjectRoot, paths.ProjectRoot);
        Assert.Equal(Path.Combine(FixtureProjectRoot, ".harness"), paths.HarnessDir);
    }

    [Fact]
    public void Resolve_GivenRelativePaths_ProducesAbsolutePaths()
    {
        var paths = TargetProjectPaths.Resolve(FixtureConfigPath, MakeConfig());

        Assert.True(Path.IsPathRooted(paths.SpecsRoot));
        Assert.True(Path.IsPathRooted(paths.RulesDir));
        Assert.True(Path.IsPathRooted(paths.Src));

        Assert.Equal(
            Path.GetFullPath(Path.Combine(FixtureProjectRoot, ".specs", "features")),
            paths.SpecsRoot);
        Assert.Equal(
            Path.GetFullPath(Path.Combine(FixtureProjectRoot, "rules")),
            paths.RulesDir);
        Assert.Equal(
            Path.GetFullPath(Path.Combine(FixtureProjectRoot, "src")),
            paths.Src);
    }

    [Fact]
    public void MissingRequiredPaths_WhenAllExist_ReturnsEmpty()
    {
        var paths = TargetProjectPaths.Resolve(FixtureConfigPath, MakeConfig());

        var missing = paths.MissingRequiredPaths();

        Assert.Empty(missing);
    }

    [Fact]
    public void MissingRequiredPaths_WhenSrcAbsent_ListsIt()
    {
        var config = MakeConfig(src: "nonexistent-src-dir-that-does-not-exist");
        var paths = TargetProjectPaths.Resolve(FixtureConfigPath, config);

        var missing = paths.MissingRequiredPaths();

        Assert.Single(missing);
        Assert.Contains(paths.Src, missing);
    }
}
