using CoreCRM.Domain.Entities;
using CoreCRM.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Domain.Services
{
    public class EntityRecordValidator
    {
        public ValidationResult Validate(EntityDefinition definition, EntityRecord record)
        {
            var result = new ValidationResult();

            foreach (var field in definition.Fields)
            {
                var value = record.GetValue(field.Name);

                if (field.IsRequired && IsEmpty(value))
                {
                    result.AddError(field.Name, $"'{field.Label}' is required.");
                    continue;
                }

                if (value is null)
                    continue; // zorunlu değilse ve boşsa, tip kontrolüne gerek yok

                if (!MatchesType(field.Type, value))
                {
                    result.AddError(field.Name, $"'{field.Label}' must be of type {field.Type}.");
                }
            }

            return result;
        }

        private static bool IsEmpty(object? value) =>
            value is null || (value is string s && string.IsNullOrWhiteSpace(s));

        private static bool MatchesType(FieldType type, object value) => type switch
        {
            FieldType.Text => value is string,
            FieldType.Number => value is int or long or decimal or double,
            FieldType.Date => value is DateTimeOffset or DateTime,
            FieldType.Boolean => value is bool,
            FieldType.Choice => value is string, // seçim listesi değerleri şimdilik string olarak tutuluyor
            _ => false
        };
    }
}
