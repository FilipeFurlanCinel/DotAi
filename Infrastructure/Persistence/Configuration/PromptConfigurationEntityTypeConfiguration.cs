using Domain.ConfigurationAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration;

public class PromptConfigurationEntityTypeConfiguration : IEntityTypeConfiguration<PromptConfiguration>
{
    public void Configure(EntityTypeBuilder<PromptConfiguration> builder)
    {
        builder.ToTable("PromptConfigurations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ComplexityLevel)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.MaxTokens)
            .IsRequired();

        builder.Property(x => x.PromptTemplateId)
            .IsRequired();

        builder.HasOne(x => x.PromptTemplate)
            .WithMany()
            .HasForeignKey(x => x.PromptTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.PromptTemplate).AutoInclude();
    }
}
