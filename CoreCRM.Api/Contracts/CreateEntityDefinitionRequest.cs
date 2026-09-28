using CoreCRM.Domain.Enums;

namespace CoreCRM.Api.Contracts;

public record CreateFieldRequest(string Name, string Label, FieldType Type, bool IsRequired);

public record CreateEntityDefinitionRequest(string Name, string DisplayName, List<CreateFieldRequest> Fields);
