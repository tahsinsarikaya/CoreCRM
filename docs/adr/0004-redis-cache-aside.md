# ADR 0004: Redis Cache-Aside with Invalidation (Not Write-Through)

## Status
Accepted

## Context
`EntityDefinition` reads (`GET /entity-definitions/{id}`) are expected to be
frequent relative to writes — a schema, once defined, is typically read far
more often than it changes. This makes it a good caching candidate.

## Decision
A cache-aside pattern is used: `GetById` checks Redis first; on a miss, it
loads from PostgreSQL and populates the cache with a 10-minute TTL. Any
operation that changes a definition's shape (`AddField`, `Delete`)
explicitly invalidates (deletes) the cache entry rather than updating it
in place (write-through).

Because the domain entity (`EntityDefinition`) has private constructors and
a read-only `Fields` collection by design (see the entity itself), it
cannot be deserialized directly from JSON. A separate DTO
(`EntityDefinitionDto`) is used as the wire format, and a dedicated
`EntityDefinition.Restore(...)` factory reconstructs the domain object from
cached data — distinct from the normal constructor, which stamps
`CreatedAt = UtcNow` and is meant only for genuinely new entities.

## Consequences
- Simpler and less error-prone than write-through: there's no risk of the
  cached value silently drifting from what was actually persisted, since
  the cache is always repopulated from the database on the next read.
- The first read after any write is always a cache miss (a deliberate
  trade-off — correctness over always-warm reads).
- The 10-minute TTL is a safety net in case an invalidation path is ever
  missed, at the cost of allowing up to 10 minutes of staleness in that
  failure scenario.
- Introduces a second "recreate a domain object" pathway (`Restore`)
  alongside the normal constructor and EF Core's own materialization —
  three ways to end up with an `EntityDefinition` instance, each for a
  different origin (new, from DB, from cache), which adds a small amount
  of conceptual overhead future contributors need to understand.