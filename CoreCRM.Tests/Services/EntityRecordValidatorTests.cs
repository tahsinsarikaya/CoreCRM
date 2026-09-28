using CoreCRM.Domain.Entities;
using CoreCRM.Domain.Enums;
using CoreCRM.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Tests.Services
{
    public class EntityRecordValidatorTests
    {
        private readonly EntityRecordValidator _sut = new();

        private static EntityDefinition BuildCustomerDefinition()
        {
            var definition = new EntityDefinition(Guid.NewGuid(), "Customer", "Müşteri");
            definition.AddField(new FieldDefinition(Guid.NewGuid(), definition.Id, "FirstName", "Ad", FieldType.Text, isRequired: true));
            definition.AddField(new FieldDefinition(Guid.NewGuid(), definition.Id, "Age", "Yaş", FieldType.Number, isRequired: false));
            return definition;
        }

        [Fact]
        public void Validate_WithValidRecord_ReturnsNoErrors()
        {
            var definition = BuildCustomerDefinition();
            var record = new EntityRecord(Guid.NewGuid(), definition.Id);
            record.SetValue("FirstName", "Ahmet");
            record.SetValue("Age", 30);

            var result = _sut.Validate(definition, record);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithMissingRequiredField_ReturnsError()
        {
            var definition = BuildCustomerDefinition();
            var record = new EntityRecord(Guid.NewGuid(), definition.Id);
            record.SetValue("Age", 30); // FirstName hiç set edilmedi

            var result = _sut.Validate(definition, record);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.FieldName == "FirstName");
        }

        [Fact]
        public void Validate_WithWrongType_ReturnsError()
        {
            var definition = BuildCustomerDefinition();
            var record = new EntityRecord(Guid.NewGuid(), definition.Id);
            record.SetValue("FirstName", "Ahmet");
            record.SetValue("Age", "thirty"); // sayı olmalıydı, string verildi

            var result = _sut.Validate(definition, record);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.FieldName == "Age");
        }
    }
}
