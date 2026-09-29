namespace Domain.ConfigurationAggregate.Enums;

/// <summary>
/// Defines the complexity level for generating a git commit message.
/// </summary>
public enum CommitComplexityLevel
{
    /// <summary>
    /// A simple, one-line commit message.
    /// </summary>
    Simple = 1,

    /// <summary>
    /// A message following the Conventional Commits specification.
    /// </summary>
    Conventional = 2,

    /// <summary>
    /// A detailed message including a body and footer.
    /// </summary>
    Detailed = 3
}
