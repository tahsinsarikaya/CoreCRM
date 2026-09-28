using CoreCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Infrastructure.Persistence.Configurations
{
    public class EntityDefinitionConfiguration : IEntityTypeConfiguration<EntityDefinition>
    {
        public void Configure(EntityTypeBuilder<EntityDefinition> builder)
        {
            builder.ToTable("entity_definitions");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            // Aynı isimde iki "Customer" tanımı olmasın — DB seviyesinde garanti.
            builder.HasIndex(e => e.Name)
                .IsUnique();

            builder.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(200);

            // "Fields" private bir List<FieldDefinition> üzerinden expose ediliyor (Entities.md'de gördüğün gibi).
            // EF Core'a bu backing field'ı kullanmasını açıkça söylüyoruz.
            builder.Metadata
                .FindNavigation(nameof(EntityDefinition.Fields))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(e => e.Fields)
                .WithOne()
                .HasForeignKey(f => f.EntityDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
