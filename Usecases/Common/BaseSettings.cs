using System.ComponentModel;
using Spectre.Console.Cli;

namespace Usecases.Common;

public class BaseSettings : CommandSettings
{
    [CommandOption("-v|--verbose")]
    [Description("Enable verbose output.")]
    public bool Verbose { get; set; }
}

