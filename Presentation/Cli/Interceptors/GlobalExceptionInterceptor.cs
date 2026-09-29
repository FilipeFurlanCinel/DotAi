using Spectre.Console.Cli;

namespace Presentation.Common.Interceptors;

public sealed class GlobalExceptionInterceptor : ICommandInterceptor
{
    public void Intercept(CommandContext context, CommandSettings settings)
    {
        // Hook reserved for cross-cutting command concerns (logging, etc.).
    }
}
