# Application Insights KQL Query Knowledge Base

This directory contains a collection of useful KQL (Kusto Query Language) queries for analyzing Application Insights telemetry from our Azure service.

## Purpose

These queries serve as building blocks for AI agents and human operators to:
- Quickly diagnose service issues
- Monitor critical metrics
- Analyze performance patterns
- Track user behavior and usage

## Directory Structure

The queries are organized into the following categories:

- **availability**: Queries related to service availability and uptime
- **dependencies**: Queries for analyzing external service dependencies
- **exceptions**: Queries for error detection and analysis
- **performance**: Queries related to application performance metrics
- **usage**: Queries about user behavior and feature usage

## Query File Format

Each query file follows a standardized format:

```
// Title: [Short descriptive title]
// Description: [What the query analyzes]
// Use Case: [When and how to use this query]
// Last Updated: [Date]

// Parameters:
// - [Parameter1]: [Description]
// - [Parameter2]: [Description]

// Query:
[KQL query code]

// Expected Output:
// - [field1]: [description]
// - [field2]: [description]

// Interpretation:
// [How to interpret the results]
```

## Using These Queries

These queries can be executed directly through the ApplicationInsightsMCP tool using:

```csharp
await ExecuteQuery("[paste query here]");
```

## Important Queries

### Critical Service Monitoring
- [Failed Background Work Items](./exceptions/failed-work-items.kql): Detects background work items that have failed after all retry attempts

## Contributing

When adding new queries, please follow the standardized format and place them in the appropriate category directory.