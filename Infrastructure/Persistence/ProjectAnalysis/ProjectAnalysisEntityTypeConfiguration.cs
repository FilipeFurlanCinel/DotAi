using Domain.ProjectAnalysisAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.ProjectAnalysis;

public class ProjectAnalysisEntityTypeConfiguration : IEntityTypeConfiguration<Domain.ProjectAnalysisAggregate.Models.ProjectAnalysis>
{
    public void Configure(EntityTypeBuilder<Domain.ProjectAnalysisAggregate.Models.ProjectAnalysis> builder)
    {
        builder.ToTable("ProjectAnalyses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SourceRoot)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasIndex(x => x.SourceRoot)
            .IsUnique();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.Summary)
            .HasMaxLength(2000);
    }
}
