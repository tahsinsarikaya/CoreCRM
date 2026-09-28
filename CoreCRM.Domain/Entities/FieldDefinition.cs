using CoreCRM.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Domain.Entities
{
    public class FieldDefinition
    {
        public Guid Id { get; private set; }
        public Guid EntityDefinitionId { get; private set; }
        public string Name { get; private set; } = default!;      // örn: "FirstName" — kayıtlardaki JSONB key'i bu olacak
        public string Label { get; private set; } = default!;     // örn: "Ad" — UI'da gösterilecek isim
        public FieldType Type { get; private set; }
        public bool IsRequired { get; private set; }

        private FieldDefinition() { } // EF Core için

        public FieldDefinition(Guid id, Guid entityDefinitionId, string name, string label, FieldType type, bool isRequired = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Field name cannot be empty.", nameof(name));

            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentException("Field label cannot be empty.", nameof(label));

            Id = id;
            EntityDefinitionId = entityDefinitionId;
            Name = name;
            Label = label;
            Type = type;
            IsRequired = isRequired;
        }
    }
}
