namespace Usecases.Common;

public static class DotAiPaths
{
    public const string DirectoryName = ".dotai";
    public const string DatabaseFileName = "DotAi.db";
    public const string AnalysisFileName = "analysis.md";

    public static string GetDotAiDirectory(string repositoryRoot) =>
        Path.Combine(repositoryRoot, DirectoryName);

    public static string GetDatabasePath(string repositoryRoot) =>
        Path.Combine(GetDotAiDirectory(repositoryRoot), DatabaseFileName);

    public static string GetAnalysisPath(string repositoryRoot) =>
        Path.Combine(GetDotAiDirectory(repositoryRoot), AnalysisFileName);

    public static void EnsureDotAiDirectory(string repositoryRoot)
    {
        Directory.CreateDirectory(GetDotAiDirectory(repositoryRoot));
    }
}
