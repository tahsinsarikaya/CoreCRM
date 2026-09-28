using CoreCRM.Api.Contracts;
using CoreCRM.Domain.Entities;
using CoreCRM.Domain.Exceptions;
using CoreCRM.Infrastructure.Caching;
using CoreCRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoreCRM.Api.Controllers
{
    [ApiController]
    [Route("entity-definitions")]
    public class EntityDefinitionsController : ControllerBase
    {
        private readonly CoreCrmDbContext _dbContext;
        private readonly IEntityDefinitionCache _cache;
        private readonly ILogger<EntityDefinitionsController> _logger;

        public EntityDefinitionsController(CoreCrmDbContext dbContext, IEntityDefinitionCache cache, ILogger<EntityDefinitionsController> logger)
        {
            _dbContext = dbContext;
            _cache = cache;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEntityDefinitionRequest request, CancellationToken cancellationToken)
        {
            var definition = new EntityDefinition(Guid.NewGuid(), request.Name, request.DisplayName);

            foreach (var fieldRequest in request.Fields)
            {
                definition.AddField(new FieldDefinition(
                    Guid.NewGuid(), definition.Id, fieldRequest.Name, fieldRequest.Label, fieldRequest.Type, fieldRequest.IsRequired));
            }

            _dbContext.EntityDefinitions.Add(definition);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = definition.Id }, new { id = definition.Id });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var cached = await _cache.GetAsync(id);
            if (cached is not null)
            {
                _logger.LogInformation("Cache HIT for entity definition {Id}", id);
                return Ok(cached);
            }

            _logger.LogInformation("Cache MISS for entity definition {Id}, querying database", id);

            var definition = await _dbContext.EntityDefinitions
                .Include(e => e.Fields)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (definition is null)
                return NotFound();

            await _cache.SetAsync(definition);
            return Ok(definition);
        }

        [HttpPost("{id:guid}/fields")]
        public async Task<IActionResult> AddField(Guid id, [FromBody] CreateFieldRequest request, CancellationToken cancellationToken)
        {
            var definition = await _dbContext.EntityDefinitions
                .Include(e => e.Fields)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (definition is null)
                return NotFound();

            var field = new FieldDefinition(Guid.NewGuid(), definition.Id, request.Name, request.Label, request.Type, request.IsRequired);
            definition.AddField(field);

            _dbContext.FieldDefinitions.Add(field);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _cache.InvalidateAsync(id); // şema değişti, eski cache artık geçersiz

            return CreatedAtAction(nameof(GetById), new { id = definition.Id }, new { fieldId = field.Id });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var definition = await _dbContext.EntityDefinitions.FindAsync([id], cancellationToken);

            if (definition is null)
                return NotFound();

            var recordCount = await _dbContext.EntityRecords.CountAsync(r => r.EntityDefinitionId == id, cancellationToken);
            if (recordCount > 0)
                throw new EntityDefinitionHasRecordsException(id, recordCount);

            _dbContext.EntityDefinitions.Remove(definition);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _cache.InvalidateAsync(id); // silindi, cache'te kalmasın

            return NoContent();
        }
    }
}
