namespace Usecases.Git;

public interface IGitService
{
    string? TryGetRepositoryRoot(string? startDirectory = null);
    string GetRepositoryRoot(string? startDirectory = null);
    Task<string> GetStagedDiffAsync(CancellationToken cancellationToken = default);
    Task<string> GetDiffForFilesAsync(IEnumerable<string> files, CancellationToken cancellationToken = default);
    Task<bool> HasStagedChangesAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(string message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListTrackedFilesAsync(CancellationToken cancellationToken = default);
}
