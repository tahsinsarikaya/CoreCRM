using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Domain.Entities
{
    public class EntityRecord
    {
        private readonly Dictionary<string, object?> _values = new();

        public Guid Id { get; private set; }
        public Guid EntityDefinitionId { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        public IReadOnlyDictionary<string, object?> Values => _values;

        private EntityRecord() { } // EF Core için

        public EntityRecord(Guid id, Guid entityDefinitionId)
        {
            Id = id;
            EntityDefinitionId = entityDefinitionId;
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public void SetValue(string fieldName, object? value)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentException("Field name cannot be empty.", nameof(fieldName));

            _values[fieldName] = value;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public object? GetValue(string fieldName)
        {
            return _values.TryGetValue(fieldName, out var value) ? value : null;
        }
    }
}
