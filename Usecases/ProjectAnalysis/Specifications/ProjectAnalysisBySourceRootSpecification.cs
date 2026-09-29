using Usecases.Common;
using ProjectAnalysisModel = Domain.ProjectAnalysisAggregate.Models.ProjectAnalysis;

namespace Usecases.ProjectAnalysis.Specifications;

public sealed class ProjectAnalysisBySourceRootSpecification : BaseSpecification<ProjectAnalysisModel>
{
    public ProjectAnalysisBySourceRootSpecification(string sourceRoot)
        : base(x => x.SourceRoot == sourceRoot)
    {
    }
}
