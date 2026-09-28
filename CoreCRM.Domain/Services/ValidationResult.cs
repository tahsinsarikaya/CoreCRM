using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Domain.Services
{
    public record ValidationError(string FieldName, string Message);

    public class ValidationResult
    {
        private readonly List<ValidationError> _errors = new();

        public IReadOnlyCollection<ValidationError> Errors => _errors.AsReadOnly();
        public bool IsValid => _errors.Count == 0;

        public void AddError(string fieldName, string message)
        {
            _errors.Add(new ValidationError(fieldName, message));
        }
    }
}
