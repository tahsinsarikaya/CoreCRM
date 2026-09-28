# ADR 0002: EntityRecord Values Stored as PostgreSQL JSONB

## Status
Accepted

## Context
`EntityRecord` needs to store an arbitrary, schema-defined set of field
values per record. A classic EAV (Entity-Attribute-Value) design would use
one row per field per record in a separate table — simple to reason about,
but requires a join (or many rows) to reconstruct a single record and
tends to perform poorly at scale.

## Decision
Field values are stored as a single `jsonb` column (`entity_records.values`)
containing all of a record's field values as a JSON object, mapped via EF
Core's `HasConversion` on the domain's `Dictionary<string, object?>`
property. PostgreSQL's `jsonb` (as opposed to plain `json`) stores data in
a decomposed binary format that supports indexing and querying into the
document (e.g. `->>'FieldName'`), unlike a plain text/json column.

## Consequences
- Reading or writing a full record is a single-row operation — no joins
  needed to reconstruct it.
- Values round-trip through `System.Text.Json`, which represents deserialized
  values as `JsonElement` rather than plain CLR types — code that reads
  record values (like `EntityRecordValidator`) must convert `JsonElement`
  to a real CLR type before doing type checks or comparisons
  (`JsonValueConverter.ToClrValue`), otherwise checks like `value is string`
  silently fail. This is easy to miss and cost real debugging time in this
  project.
- Querying "all customers where Age > 30" would require PostgreSQL JSON
  operators and wouldn't use a standard B-tree index by default — a
  production system with this requirement would likely add a
  [GIN index](https://www.postgresql.org/docs/current/datatype-json.html#JSON-INDEXING)
  on the jsonb column or maintain expression indexes on specific hot fields.