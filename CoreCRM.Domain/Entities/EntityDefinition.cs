using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Domain.Entities
{
    public class EntityDefinition
    {
        private readonly List<FieldDefinition> _fields = new();

        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;         // örn: "Customer" — teknik anahtar
        public string DisplayName { get; private set; } = default!;  // örn: "Müşteri" — UI'da gösterilecek isim
        public DateTimeOffset CreatedAt { get; private set; }

        public IReadOnlyCollection<FieldDefinition> Fields => _fields.AsReadOnly();

        private EntityDefinition() { } // EF Core için

        public EntityDefinition(Guid id, string name, string displayName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Entity name cannot be empty.", nameof(name));

            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Entity display name cannot be empty.", nameof(displayName));

            Id = id;
            Name = name;
            DisplayName = displayName;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        private EntityDefinition(Guid id, string name, string displayName, DateTimeOffset createdAt)
        {
            Id = id;
            Name = name;
            DisplayName = displayName;
            CreatedAt = createdAt;
        }

        public static EntityDefinition Restore(Guid id, string name, string displayName, DateTimeOffset createdAt, IEnumerable<FieldDefinition> fields)
        {
            var definition = new EntityDefinition(id, name, displayName, createdAt);
            foreach (var field in fields)
            {
                definition.AddField(field);
            }
            return definition;
        }

        public void AddField(FieldDefinition field)
        {
            if (_fields.Any(f => f.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"A field named '{field.Name}' already exists on entity '{Name}'.");

            _fields.Add(field);
        }
    }
}
