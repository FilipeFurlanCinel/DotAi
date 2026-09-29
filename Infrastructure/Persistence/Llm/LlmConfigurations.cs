using Domain.LlmAggregate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Llm;

public class LlmConfigurations : IEntityTypeConfiguration<LlmModel>
{
    public void Configure(EntityTypeBuilder<LlmModel> builder)
    {
        builder.ToTable("LlmModels");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ProviderType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.ApiKey)
            .HasMaxLength(255);

        builder.Property(x => x.Endpoint)
            .HasMaxLength(500);
    }
}