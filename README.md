# CoreCRM

A metadata-driven CRM core: business objects and their fields are defined
at runtime through the API, not hardcoded as database tables. Built as a
portfolio project drawing on real CRM/multi-sector platform experience
(insurance and health tourism), demonstrating a schema-flexible design
rather than another fixed-schema CRUD app.

## What this project demonstrates

- **Metadata-driven design** — define a new business object ("Customer",
  "Policy", anything) and its fields through the API, with no migration or
  deployment required (see [ADR 0001](docs/adr/0001-metadata-driven-schema.md))
- **PostgreSQL JSONB storage** for flexible record data, with EF Core value
  conversion between a domain `Dictionary` and a `jsonb` column
  (see [ADR 0002](docs/adr/0002-jsonb-storage.md))
- **Two-layer data-safety guarantee** — a schema in use cannot be deleted,
  enforced at both the application level (a clear, informative error) and
  the database level (a `Restrict` foreign key as the real source of truth)
  (see [ADR 0003](docs/adr/0003-restrict-delete.md))
- **Redis cache-aside** for frequently-read schema definitions, with
  explicit invalidation on write rather than write-through
  (see [ADR 0004](docs/adr/0004-redis-cache-aside.md))
- A domain model that stays true to rich-domain-model principles even
  under the pressure of serialization: private constructors, a read-only
  `Fields` collection, and a dedicated `Restore()` factory to reconstruct
  entities from cache without abusing the "create new" constructor
- A test suite spanning four levels: pure domain unit tests, JSON
  conversion unit tests (one of which caught a real numeric-widening bug),
  and Testcontainers-based integration tests against real PostgreSQL and
  Redis instances

## Architecture

    Client
      |
      |  POST /entity-definitions        (define a schema)
      |  POST /entity-definitions/{id}/fields   (evolve a schema)
      |  POST /entity-definitions/{id}/records  (create data)
      v
    CoreCRM.Api
      |                     |
      |  cache-aside        |  writes
      v                     v
    Redis              PostgreSQL
    (schema cache)     entity_definitions
                        field_definitions
                        entity_records (values as jsonb)

### Projects

| Project | Responsibility |
|---|---|
| `CoreCRM.Domain` | Core entities (`EntityDefinition`, `FieldDefinition`, `EntityRecord`), `EntityRecordValidator` — no infrastructure dependencies |
| `CoreCRM.Infrastructure` | EF Core + PostgreSQL persistence (JSONB mapping), Redis cache (`IEntityDefinitionCache`), JSON/CLR value conversion |
| `CoreCRM.Api` | HTTP API for defining schemas, evolving them, and creating/reading records; global exception handling; Swagger UI |
| `CoreCRM.Tests` | Unit tests for domain logic and JSON conversion + Testcontainers-based integration tests for the restrict-delete constraint and Redis caching |

### Architecture Decision Records

- [0001 — Metadata-Driven Schema Instead of Fixed Tables](docs/adr/0001-metadata-driven-schema.md)
- [0002 — EntityRecord Values Stored as PostgreSQL JSONB](docs/adr/0002-jsonb-storage.md)
- [0003 — Two-Layer Protection Against Deleting a Schema in Use](docs/adr/0003-restrict-delete.md)
- [0004 — Redis Cache-Aside with Invalidation (Not Write-Through)](docs/adr/0004-redis-cache-aside.md)

## Running locally

**Prerequisites:** .NET 10 SDK, Docker Desktop

1. Start PostgreSQL and Redis:

   docker compose up -d

2. Apply database migrations (from `CoreCRM.Infrastructure`, or via Package
   Manager Console with `CoreCRM.Api` as the startup project):

   dotnet ef database update --startup-project ../CoreCRM.Api

3. Run the API:

   cd CoreCRM.Api
   dotnet run

4. Swagger UI: `https://localhost:<port>/swagger`

### Trying it out

Define a schema:

    POST https://localhost:<port>/entity-definitions
    Content-Type: application/json

    {
      "name": "Customer",
      "displayName": "Müşteri",
      "fields": [
        { "name": "FirstName", "label": "Ad", "type": "Text", "isRequired": true },
        { "name": "Age", "label": "Yaş", "type": "Number", "isRequired": false }
      ]
    }

Add a field to it later, without a migration:

    POST https://localhost:<port>/entity-definitions/{id}/fields
    Content-Type: application/json

    { "name": "LastName", "label": "Soyad", "type": "Text", "isRequired": false }

Create a record against the schema:

    POST https://localhost:<port>/entity-definitions/{id}/records
    Content-Type: application/json

    { "values": { "FirstName": "Ahmet", "Age": 30 } }

Try deleting the schema now — it will be rejected with a `409 Conflict`
telling you exactly how many records are blocking it.

## Running tests

    dotnet test

Requires Docker to be running — the integration tests spin up real
PostgreSQL and Redis containers via Testcontainers to verify:
- **Restrict delete:** the database-level foreign key constraint rejects
  deleting an `EntityDefinition` that still has records, independent of
  any application-level check
- **Redis cache:** set/get round-tripping of a full `EntityDefinition`
  (including its fields), a miss on an uncached id, and correct removal on
  invalidation

## Known simplifications

This is a portfolio project, not a production system:

- No authentication/authorization on the API
- Field-level validation covers required/type-matching only — no
  cross-field rules, no `Choice` field value-list enforcement
- No pagination on any list-style endpoint (none exist yet — only
  single-record lookups)
- Redis cache only covers `EntityDefinition` reads; `EntityRecord` reads
  always hit the database
- No event/messaging layer yet (a natural extension point — e.g.
  publishing a "SchemaChanged" event on `AddField`)
