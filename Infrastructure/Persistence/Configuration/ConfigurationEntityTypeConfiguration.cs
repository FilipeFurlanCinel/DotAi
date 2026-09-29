using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ConfigurationModel = Domain.ConfigurationAggregate.Models.Configuration;

namespace Infrastructure.Persistence.Configuration;

public class ConfigurationEntityTypeConfiguration : IEntityTypeConfiguration<ConfigurationModel>
{
    public void Configure(EntityTypeBuilder<ConfigurationModel> builder)
    {
        builder.ToTable("Configurations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.Property(x => x.CommitPrefix)
            .HasMaxLength(100);

        builder.Property(x => x.LlmModelId)
            .IsRequired();

        builder.Property(x => x.PromptConfigurationId)
            .IsRequired();

        builder.HasOne(x => x.LlmModel)
            .WithMany()
            .HasForeignKey(x => x.LlmModelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PromptConfig)
            .WithOne()
            .HasForeignKey<ConfigurationModel>(x => x.PromptConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.PromptConfig).AutoInclude();
        builder.Navigation(x => x.LlmModel).AutoInclude();
    }
}
