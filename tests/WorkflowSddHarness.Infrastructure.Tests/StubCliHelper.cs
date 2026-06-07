namespace WorkflowSddHarness.Infrastructure.Tests;

internal static class StubCliHelper
{
    public static string GetStubPath()
    {
        var baseDir = AppContext.BaseDirectory;
        var exeName = "WorkflowSddHarness.StubCli.exe";
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows))
        {
            exeName = "WorkflowSddHarness.StubCli";
        }
        return System.IO.Path.Combine(baseDir, exeName);
    }
}
