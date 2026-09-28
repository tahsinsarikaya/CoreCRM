using CoreCRM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Infrastructure.Caching
{
    public interface IEntityDefinitionCache
    {
        Task<EntityDefinition?> GetAsync(Guid id);
        Task SetAsync(EntityDefinition definition);
        Task InvalidateAsync(Guid id);
    }
}
