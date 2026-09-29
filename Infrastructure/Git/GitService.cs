using System.Diagnostics;
using System.Text;
using Usecases.Git;

namespace Infrastructure.Git;

public sealed class GitService : IGitService
{
    private readonly string _workingDirectory;

    public GitService()
    {
        _workingDirectory = Environment.GetEnvironmentVariable("DOTAI_WORKSPACE")
                            ?? Directory.GetCurrentDirectory();
    }

    public string? TryGetRepositoryRoot(string? startDirectory = null)
    {
        try
        {
            return GetRepositoryRoot(startDirectory);
        }
        catch
        {
            return null;
        }
    }

    public string GetRepositoryRoot(string? startDirectory = null)
    {
        var start = startDirectory ?? _workingDirectory;
        var output = RunGit(start, "rev-parse", "--show-toplevel");
        return output.Trim();
    }

    public async Task<string> GetStagedDiffAsync(CancellationToken cancellationToken = default)
    {
        return await RunGitAsync(GetRepositoryRoot(), cancellationToken, "diff", "--cached");
    }

    public async Task<string> GetDiffForFilesAsync(
        IEnumerable<string> files,
        CancellationToken cancellationToken = default)
    {
        var fileList = files.ToList();
        if (fileList.Count == 0)
            return await GetStagedDiffAsync(cancellationToken);

        var args = new List<string> { "diff", "--cached", "--" };
        args.AddRange(fileList);
        return await RunGitAsync(GetRepositoryRoot(), cancellationToken, args.ToArray());
    }

    public async Task<bool> HasStagedChangesAsync(CancellationToken cancellationToken = default)
    {
        var output = await RunGitAsync(
            GetRepositoryRoot(),
            cancellationToken,
            "diff",
            "--cached",
            "--name-only");
        return !string.IsNullOrWhiteSpace(output);
    }

    public async Task CommitAsync(string message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Commit message must be provided.", nameof(message));

        await RunGitAsync(GetRepositoryRoot(), cancellationToken, "commit", "-m", message);
    }

    public async Task<IReadOnlyList<string>> ListTrackedFilesAsync(CancellationToken cancellationToken = default)
    {
        var output = await RunGitAsync(GetRepositoryRoot(), cancellationToken, "ls-files");
        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static string RunGit(string workingDirectory, params string[] args)
    {
        var psi = CreateStartInfo(workingDirectory, args);
        using var process = Process.Start(psi)
                            ?? throw new InvalidOperationException("Failed to start git process.");

        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"git {string.Join(' ', args)} failed: {stderr}".Trim());

        return stdout;
    }

    private static async Task<string> RunGitAsync(
        string workingDirectory,
        CancellationToken cancellationToken,
        params string[] args)
    {
        var psi = CreateStartInfo(workingDirectory, args);
        using var process = Process.Start(psi)
                            ?? throw new InvalidOperationException("Failed to start git process.");

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"git {string.Join(' ', args)} failed: {stderr}".Trim());

        return stdout;
    }

    private static ProcessStartInfo CreateStartInfo(string workingDirectory, string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        foreach (var arg in args)
            psi.ArgumentList.Add(arg);

        return psi;
    }
}
