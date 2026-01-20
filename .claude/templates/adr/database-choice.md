# ADR-{{NUMBER}}: Database Technology Selection

## Status
Proposed | Accepted | Deprecated | Superseded

## Date
{{DATE}}

## Story
{{STORY_ID}}

## Context

We need to select a database technology for {{PROJECT_NAME}}. The application requires:

- **Data model**: [Relational | Document | Key-Value | Graph | Time-series]
- **Expected data volume**: [Small < 10GB | Medium 10-100GB | Large > 100GB]
- **Read/Write ratio**: [Read-heavy | Write-heavy | Balanced]
- **Consistency requirements**: [Strong | Eventual]
- **Query patterns**: [Simple CRUD | Complex joins | Full-text search | Aggregations]

### Current State
<!-- Describe current database situation if any -->

### Constraints
- Budget: {{BUDGET}}
- Team expertise: {{EXPERTISE}}
- Infrastructure: [Cloud | On-premise | Hybrid]
- Compliance: [GDPR | HIPAA | SOC2 | None]

## Options Considered

### Option 1: PostgreSQL
**Type**: Relational (SQL)

**Pros**:
- ACID compliant with strong consistency
- Rich feature set (JSON, full-text search, extensions)
- Excellent .NET support (Npgsql, EF Core)
- Open source, no licensing costs
- Strong community and ecosystem

**Cons**:
- Horizontal scaling requires additional tooling (Citus)
- More complex setup than managed solutions

**Cost**: Free (OSS) + Infrastructure

### Option 2: SQL Server
**Type**: Relational (SQL)

**Pros**:
- First-class .NET integration
- Enterprise features (Always On, columnstore)
- Excellent tooling (SSMS, Azure Data Studio)
- Azure SQL for managed service

**Cons**:
- Licensing costs for Enterprise features
- Vendor lock-in

**Cost**: $$ - $$$$

### Option 3: MongoDB
**Type**: Document (NoSQL)

**Pros**:
- Flexible schema for evolving data models
- Horizontal scaling built-in
- Good for hierarchical data
- Atlas for managed service

**Cons**:
- No ACID transactions across documents (improved in v4+)
- Less suitable for complex relationships
- Different query paradigm from SQL

**Cost**: Free (OSS) + Infrastructure or Atlas pricing

### Option 4: Azure Cosmos DB
**Type**: Multi-model (NoSQL)

**Pros**:
- Global distribution
- Multiple consistency levels
- Multi-model API (SQL, MongoDB, Cassandra)
- Fully managed

**Cons**:
- Higher cost at scale
- Azure lock-in
- Complex pricing model

**Cost**: $$$ - $$$$$

## Decision

We will use **{{SELECTED_OPTION}}** because:

1. {{REASON_1}}
2. {{REASON_2}}
3. {{REASON_3}}

## Consequences

### Positive
- {{POSITIVE_1}}
- {{POSITIVE_2}}

### Negative
- {{NEGATIVE_1}}
- {{NEGATIVE_2}}

### Risks
- {{RISK_1}}: Mitigation - {{MITIGATION_1}}

## Implementation Notes

### Connection String Pattern
```
<!-- Add connection string pattern -->
```

### EF Core Configuration
```csharp
// Add DbContext configuration
```

### Migration Strategy
<!-- Describe how to handle schema migrations -->

## References
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/)
- [MongoDB Documentation](https://docs.mongodb.com/)
- [Cosmos DB Documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/)
