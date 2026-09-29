using System.Diagnostics;
using System.Text;
using CrossCutting;
using Infrastructure.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Presentation;
using Spectre.Console.Cli;
using Usecases.Commit;
using Usecases.Commit.Commands;
using Usecases.Common;

var skipDatabaseBootstrap = args.Any(a => a is "-h" or "--help" or "-?");

var workspace = Environment.GetEnvironmentVariable("DOTAI_WORKSPACE")
                ?? Directory.GetCurrentDirectory();

var repositoryRoot = TryResolveGitRoot(workspace) ?? workspace;
DotAiPaths.EnsureDotAiDirectory(repositoryRoot);
var databasePath = DotAiPaths.GetDatabasePath(repositoryRoot);
var connectionString = $"Data Source={databasePath}";

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Warning);
});

services.AddMediatRServices();
services.AddInfrastructure(configuration, connectionString);
services.AddScoped<CommitWorkflowService>();

if (!skipDatabaseBootstrap)
{
    try
    {
        await using var bootstrapProvider = services.BuildServiceProvider();
        await bootstrapProvider.EnsureDatabaseAsync();
    }
    catch (Exception ex) when (IsLikelyPermissionError(ex))
    {
        Spectre.Console.AnsiConsole.MarkupLine(
            $"[red]Cannot write to[/] {databasePath}");
        Spectre.Console.AnsiConsole.MarkupLine(
            "[yellow]If this folder was created by Docker as root, fix ownership:[/]");
        Spectre.Console.AnsiConsole.MarkupLine(
            $"  sudo chown -R \"$USER\" \"{DotAiPaths.GetDotAiDirectory(repositoryRoot)}\"");
        return 1;
    }
}

var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);
app.SetDefaultCommand<CommitCommand>();
app.Configure(cli =>
{
    cli.PropagateExceptions();
    cli.ConfigureCommands();
});

try
{
    return app.Run(args);
}
catch (Exception ex)
{
    Spectre.Console.AnsiConsole.MarkupLine("[bold red]An unexpected error occurred:[/]");
    if (ex is InvalidOperationException or ArgumentException)
        Spectre.Console.AnsiConsole.MarkupLine($"[yellow]{ex.Message}[/]");
    else
        Spectre.Console.AnsiConsole.WriteException(ex, Spectre.Console.ExceptionFormats.ShortenPaths);

    return 1;
}

static bool IsLikelyPermissionError(Exception ex)
{
    var message = ex.ToString();
    return message.Contains("readonly", StringComparison.OrdinalIgnoreCase)
           || message.Contains("permission", StringComparison.OrdinalIgnoreCase)
           || message.Contains("UnauthorizedAccess", StringComparison.OrdinalIgnoreCase);
}

static string? TryResolveGitRoot(string startDirectory)
{
    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = startDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8
        };
        psi.ArgumentList.Add("rev-parse");
        psi.ArgumentList.Add("--show-toplevel");

        using var process = Process.Start(psi);
        if (process is null)
            return null;

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return process.ExitCode == 0 ? output.Trim() : null;
    }
    catch
    {
        return null;
    }
}
