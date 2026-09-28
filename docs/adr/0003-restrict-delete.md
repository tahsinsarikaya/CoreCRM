# ADR 0003: Two-Layer Protection Against Deleting a Schema in Use

## Status
Accepted

## Context
Deleting an `EntityDefinition` that still has `EntityRecord`s pointing to it
would either orphan those records or (with a cascading delete) silently
destroy real data — unacceptable for a CRM where records represent actual
business data (e.g. hundreds of customer entries).

## Decision
Two layers of protection, deliberately redundant:

1. **Application layer:** `DELETE /entity-definitions/{id}` first counts
   existing records for that definition. If any exist, it throws
   `EntityDefinitionHasRecordsException`, mapped to a `409 Conflict` with a
   message stating exactly how many records are blocking the deletion.
2. **Database layer:** the foreign key from `entity_records` to
   `entity_definitions` is configured with `OnDelete(DeleteBehavior.Restrict)`,
   so even if the application-level check were bypassed or raced, PostgreSQL
   itself refuses the delete (SQLSTATE `23503`), which `GlobalExceptionHandler`
   also maps to a `409 Conflict`.

## Consequences
- Users get a clear, actionable error in the common case (the application
  check).
- The database constraint is the actual source of truth/safety net — it
  cannot be bypassed by a bug in the application-level check, a race
  condition between the count check and the delete, or a future code path
  that skips the controller entirely (e.g. a bulk admin script).
- Deleting a schema that has records requires deleting the records first;
  there is no cascading "delete everything" shortcut — a deliberate choice
  favoring data safety over convenience.