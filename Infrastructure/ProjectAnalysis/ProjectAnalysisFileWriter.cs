using Usecases.Common;
using Usecases.ProjectAnalysis;

namespace Infrastructure.ProjectAnalysis;

public sealed class ProjectAnalysisFileWriter : IProjectAnalysisWriter
{
    public string GetAnalysisFilePath(string repositoryRoot) =>
        DotAiPaths.GetAnalysisPath(repositoryRoot);

    public async Task WriteAsync(
        string repositoryRoot,
        string content,
        CancellationToken cancellationToken = default)
    {
        DotAiPaths.EnsureDotAiDirectory(repositoryRoot);
        var path = DotAiPaths.GetAnalysisPath(repositoryRoot);
        await File.WriteAllTextAsync(path, content, cancellationToken);
    }
}
