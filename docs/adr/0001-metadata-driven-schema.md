# ADR 0001: Metadata-Driven Schema Instead of Fixed Tables

## Status
Accepted

## Context
A traditional CRM hardcodes tables per business object (a `Customers` table,
a `Policies` table, etc.). Adding a new business object or a new field to an
existing one requires a schema migration and a deployment. In practice,
different sectors (insurance, health tourism, etc.) need different fields on
what is conceptually "a customer," and those needs change over time.

## Decision
Instead of fixed tables, the system defines three core concepts:
`EntityDefinition` (a runtime-defined business object, e.g. "Customer"),
`FieldDefinition` (the fields that object has, with a type and a required
flag), and `EntityRecord` (an actual record, storing its field values as
JSON rather than in dedicated columns).

Adding a new business object, or a new field to an existing one, is an API
call (`POST /entity-definitions`, `POST /entity-definitions/{id}/fields`),
not a migration.

## Consequences
- New business objects and fields can be added without a deployment.
- Field-level validation (required, type-matching) has to be enforced in
  application code (`EntityRecordValidator`) rather than relying on the
  database schema itself — the database only guarantees structural
  constraints (uniqueness, foreign keys), not field-level business rules.
- Querying by a specific field's value is less efficient than a normal
  indexed column would be (see ADR 0002) — acceptable for this project's
  scope, but a real system with heavy filtering needs would likely add
  targeted indexes or a read-model projection.
- No compile-time type safety when working with record values — they are
  read and written as `Dictionary<string, object?>`.