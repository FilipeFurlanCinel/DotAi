using Usecases.Common;
using ConfigurationModel = Domain.ConfigurationAggregate.Models.Configuration;

namespace Usecases.Configuration.Specifications;

public sealed class EnabledConfigurationSpecification
    : BaseSpecification<ConfigurationModel>
{
    public EnabledConfigurationSpecification()
        : base(c => c.IsEnabled)
    {
        AddInclude(c => c.LlmModel!);
        AddInclude(c => c.PromptConfig);
    }
}
