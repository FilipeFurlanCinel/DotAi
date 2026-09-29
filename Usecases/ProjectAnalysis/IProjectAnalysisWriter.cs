namespace Usecases.ProjectAnalysis;

public interface IProjectAnalysisWriter
{
    Task WriteAsync(string repositoryRoot, string content, CancellationToken cancellationToken = default);
    string GetAnalysisFilePath(string repositoryRoot);
}
