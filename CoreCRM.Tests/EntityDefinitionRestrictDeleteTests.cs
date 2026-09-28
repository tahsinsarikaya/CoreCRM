using CoreCRM.Domain.Entities;
using CoreCRM.Domain.Enums;
using CoreCRM.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Tests
{
    public class EntityDefinitionRestrictDeleteTests : IClassFixture<PostgresFixture>
    {
        private readonly PostgresFixture _fixture;

        public EntityDefinitionRestrictDeleteTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task DeletingEntityDefinition_WithExistingRecords_ThrowsDueToForeignKeyRestrict()
        {
            var definition = new EntityDefinition(Guid.NewGuid(), "TestEntity", "Test Varlığı");
            definition.AddField(new FieldDefinition(Guid.NewGuid(), definition.Id, "Name", "İsim", FieldType.Text, isRequired: true));

            await using (var setupContext = _fixture.CreateDbContext())
            {
                setupContext.EntityDefinitions.Add(definition);
                await setupContext.SaveChangesAsync();

                var record = new EntityRecord(Guid.NewGuid(), definition.Id);
                record.SetValue("Name", "Test Kaydı");
                setupContext.EntityRecords.Add(record);
                await setupContext.SaveChangesAsync();
            }

            // Ayrı bir context ile, sanki farklı bir istek şemayı silmeye çalışıyormuş gibi
            await using var deleteContext = _fixture.CreateDbContext();
            var toDelete = await deleteContext.EntityDefinitions.FindAsync(definition.Id);
            deleteContext.EntityDefinitions.Remove(toDelete!);

            var act = async () => await deleteContext.SaveChangesAsync();

            await Assert.ThrowsAsync<DbUpdateException>(act);

            // Doğrulama: şema hâlâ DB'de duruyor, silinmemiş
            await using var verifyContext = _fixture.CreateDbContext();
            var stillExists = await verifyContext.EntityDefinitions.AnyAsync(e => e.Id == definition.Id);
            Assert.True(stillExists);
        }
    }
}
