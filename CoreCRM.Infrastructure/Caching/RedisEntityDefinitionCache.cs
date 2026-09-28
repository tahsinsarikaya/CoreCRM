using CoreCRM.Domain.Entities;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace CoreCRM.Infrastructure.Caching
{
    public class RedisEntityDefinitionCache : IEntityDefinitionCache
    {
        private readonly IConnectionMultiplexer _redis;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public RedisEntityDefinitionCache(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        private static string Key(Guid id) => $"entity-definition:{id}";

        public async Task<EntityDefinition?> GetAsync(Guid id)
        {
            var db = _redis.GetDatabase();
            var json = await db.StringGetAsync(Key(id));

            if (json.IsNullOrEmpty)
                return null;

            var dto = JsonSerializer.Deserialize<EntityDefinitionDto>((string)json!);
            if (dto is null) return null;

            var fields = dto.Fields.Select(f => new FieldDefinition(f.Id, dto.Id, f.Name, f.Label, f.Type, f.IsRequired));
            return EntityDefinition.Restore(dto.Id, dto.Name, dto.DisplayName, dto.CreatedAt, fields);
        }

        public async Task SetAsync(EntityDefinition definition)
        {
            var dto = new EntityDefinitionDto(
                definition.Id,
                definition.Name,
                definition.DisplayName,
                definition.CreatedAt,
                definition.Fields.Select(f => new FieldDefinitionDto(f.Id, f.Name, f.Label, f.Type, f.IsRequired)).ToList());

            var db = _redis.GetDatabase();
            await db.StringSetAsync(Key(definition.Id), JsonSerializer.Serialize(dto), CacheDuration);
        }

        public async Task InvalidateAsync(Guid id)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(Key(id));
        }
    }
}
