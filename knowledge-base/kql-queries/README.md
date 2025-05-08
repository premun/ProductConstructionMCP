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

```kql
// Title: [Short descriptive title]
// Description: [What the query analyzes]

// Parameters:
// - [Parameter1]: [Description]
// - [Parameter2]: [Description]

// Query:
[KQL query code]

// Expected Output:
// - [field1]: [description]
// - [field2]: [description]
```

## Contributing

When adding new queries, please follow the standardized format and place them in the appropriate category directory.