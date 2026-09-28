using CoreCRM.Domain.Enums;

namespace CoreCRM.Infrastructure.Caching;

public record FieldDefinitionDto(Guid Id, string Name, string Label, FieldType Type, bool IsRequired);

public record EntityDefinitionDto(Guid Id, string Name, string DisplayName, DateTimeOffset CreatedAt, List<FieldDefinitionDto> Fields);
