using CoreCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CoreCRM.Infrastructure.Persistence.Configurations
{
    public class EntityRecordConfiguration : IEntityTypeConfiguration<EntityRecord>
    {
        public void Configure(EntityTypeBuilder<EntityRecord> builder)
        {
            builder.ToTable("entity_records");

            builder.HasKey(r => r.Id);

            builder.HasOne<EntityDefinition>()
                .WithMany()
                .HasForeignKey(r => r.EntityDefinitionId)
                .OnDelete(DeleteBehavior.Restrict); // kayıt varken şema silinemesin

            // "Values" private bir Dictionary üzerinden expose ediliyor,
            // EntityDefinition.Fields'ta yaptığımız gibi field access mode gerekiyor.
            builder.Property(r => r.Values)
                .HasColumnName("values")
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, (JsonSerializerOptions?)null) ?? new())
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyDictionary<string, object?>>(
                    (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
                    v => JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null)!));
        }
    }
}
