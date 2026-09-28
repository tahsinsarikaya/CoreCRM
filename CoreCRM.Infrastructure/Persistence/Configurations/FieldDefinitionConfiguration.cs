using CoreCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Infrastructure.Persistence.Configurations
{
    public class FieldDefinitionConfiguration : IEntityTypeConfiguration<FieldDefinition>
    {
        public void Configure(EntityTypeBuilder<FieldDefinition> builder)
        {
            builder.ToTable("field_definitions");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.Label)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(f => f.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Aynı entity içinde aynı isimde iki alan olmasın — domain'de zaten
            // AddField() içinde kontrol ediyoruz, burada DB seviyesinde de garanti altına alıyoruz.
            builder.HasIndex(f => new { f.EntityDefinitionId, f.Name })
                .IsUnique();
        }
    }
}
