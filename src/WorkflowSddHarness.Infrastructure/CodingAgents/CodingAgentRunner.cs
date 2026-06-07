using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace WorkflowSddHarness.Infrastructure.CodingAgents;

public sealed class CodingAgentRunner
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public async Task<ProcessRunResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string stdinText,
        TimeSpan? timeout,
        CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Utf8NoBom,
            StandardErrorEncoding = Utf8NoBom,
        };
        foreach (var arg in arguments)
            psi.ArgumentList.Add(arg);

        using var proc = new Process { StartInfo = psi };
        try
        {
            if (!proc.Start())
                return new ProcessRunResult { Started = false };
        }
        catch (Win32Exception)
        {
            return new ProcessRunResult { Started = false };
        }
        catch (FileNotFoundException)
        {
            return new ProcessRunResult { Started = false };
        }

        // Start reading stdout/stderr concurrently before writing stdin to prevent deadlock
        var stdoutTask = proc.StandardOutput.ReadToEndAsync();
        var stderrTask = proc.StandardError.ReadToEndAsync();

        // Write stdin as UTF-8 and close to signal EOF to the child process
        await using (var stdinWriter = new StreamWriter(proc.StandardInput.BaseStream, Utf8NoBom))
            await stdinWriter.WriteAsync(stdinText);

        using var linkedCts = timeout.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                new CancellationTokenSource(timeout.Value).Token)
            : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        bool timedOut = false;
        try
        {
            await proc.WaitForExitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException)
        {
            try { proc.Kill(entireProcessTree: true); } catch { }

            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            timedOut = true;
        }

        await Task.WhenAll(stdoutTask, stderrTask);

        int exitCode = 0;
        try { exitCode = proc.ExitCode; } catch { }

        return new ProcessRunResult
        {
            Started = true,
            StdOut = stdoutTask.Result,
            StdErr = stderrTask.Result,
            ExitCode = exitCode,
            TimedOut = timedOut,
        };
    }
}
