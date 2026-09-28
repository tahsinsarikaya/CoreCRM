using System;
using System.Collections.Generic;
using System.Text;

namespace CoreCRM.Domain.Exceptions
{
    public class EntityDefinitionHasRecordsException : Exception
    {
        public Guid EntityDefinitionId { get; }
        public int RecordCount { get; }

        public EntityDefinitionHasRecordsException(Guid entityDefinitionId, int recordCount)
            : base($"Cannot delete entity definition '{entityDefinitionId}' because it has {recordCount} existing record(s). Delete the records first.")
        {
            EntityDefinitionId = entityDefinitionId;
            RecordCount = recordCount;
        }
    }
}
