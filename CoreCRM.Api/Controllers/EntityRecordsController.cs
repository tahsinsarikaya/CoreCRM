using CoreCRM.Api.Contracts;
using CoreCRM.Domain.Entities;
using CoreCRM.Domain.Services;
using CoreCRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoreCRM.Infrastructure.Serialization;

namespace CoreCRM.Api.Controllers
{
    [ApiController]
    [Route("entity-definitions/{entityDefinitionId:guid}/records")]
    public class EntityRecordsController : ControllerBase
    {
        private readonly CoreCrmDbContext _dbContext;
        private readonly EntityRecordValidator _validator;

        public EntityRecordsController(CoreCrmDbContext dbContext, EntityRecordValidator validator)
        {
            _dbContext = dbContext;
            _validator = validator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid entityDefinitionId, [FromBody] CreateEntityRecordRequest request, CancellationToken cancellationToken)
        {
            var definition = await _dbContext.EntityDefinitions
                .Include(e => e.Fields)
                .FirstOrDefaultAsync(e => e.Id == entityDefinitionId, cancellationToken);

            if (definition is null)
                return NotFound();

            var record = new EntityRecord(Guid.NewGuid(), entityDefinitionId);
            foreach (var (key, value) in request.Values)
            {
                record.SetValue(key, JsonValueConverter.ToClrValue(value));
            }

            var validationResult = _validator.Validate(definition, record);
            if (!validationResult.IsValid)
            {
                return ValidationProblem(new ValidationProblemDetails(
                    validationResult.Errors.ToDictionary(e => e.FieldName, e => new[] { e.Message })));
            }

            _dbContext.EntityRecords.Add(record);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return CreatedAtAction(nameof(GetById), new { entityDefinitionId, id = record.Id }, new { id = record.Id });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid entityDefinitionId, Guid id, CancellationToken cancellationToken)
        {
            var record = await _dbContext.EntityRecords
                .FirstOrDefaultAsync(r => r.Id == id && r.EntityDefinitionId == entityDefinitionId, cancellationToken);

            return record is null ? NotFound() : Ok(record);
        }
    }
}
