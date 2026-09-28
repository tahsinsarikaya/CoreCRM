using CoreCRM.Domain.Entities;
using CoreCRM.Domain.Enums;
using CoreCRM.Infrastructure.Caching;
using CoreCRM.Tests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Tests
{
    public class RedisEntityDefinitionCacheTests : IClassFixture<RedisFixture>
    {
        private readonly RedisEntityDefinitionCache _sut;

        public RedisEntityDefinitionCacheTests(RedisFixture fixture)
        {
            _sut = new RedisEntityDefinitionCache(fixture.Connection);
        }

        [Fact]
        public async Task SetAndGet_RoundTripsEntityDefinitionCorrectly()
        {
            var definition = new EntityDefinition(Guid.NewGuid(), "Product", "Ürün");
            definition.AddField(new FieldDefinition(Guid.NewGuid(), definition.Id, "Sku", "Stok Kodu", FieldType.Text, isRequired: true));

            await _sut.SetAsync(definition);
            var cached = await _sut.GetAsync(definition.Id);

            Assert.NotNull(cached);
            Assert.Equal(definition.Name, cached!.Name);
            Assert.Equal(definition.DisplayName, cached.DisplayName);
            Assert.Single(cached.Fields);
            Assert.Equal("Sku", cached.Fields.First().Name);
        }

        [Fact]
        public async Task GetAsync_ForUncachedId_ReturnsNull()
        {
            var result = await _sut.GetAsync(Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task InvalidateAsync_RemovesEntryFromCache()
        {
            var definition = new EntityDefinition(Guid.NewGuid(), "Order", "Sipariş");
            await _sut.SetAsync(definition);

            await _sut.InvalidateAsync(definition.Id);
            var result = await _sut.GetAsync(definition.Id);

            Assert.Null(result);
        }
    }
}
