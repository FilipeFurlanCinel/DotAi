using Domain.LlmAggregate.Models;
using Usecases.Common;

namespace Usecases.Llm.Specifications;

public sealed class LlmModelByIdSpecification : BaseSpecification<LlmModel>
{
    public LlmModelByIdSpecification(Guid id)
        : base(x => x.Id == id)
    {
    }
}
