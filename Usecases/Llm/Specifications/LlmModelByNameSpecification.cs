using System.Linq.Expressions;
using Domain.LlmAggregate.Models;
using Usecases.Common;

namespace Usecases.Llm.Specifications;

public sealed class LlmModelByNameSpecification : BaseSpecification<LlmModel>
{
    public LlmModelByNameSpecification(string name) 
        : base(x => x.Name == name)
    {
    }
}
