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

- **diagnostics**: Queries for operation tracing and detailed debugging
- **exceptions**: Queries for error detection and analysis
- **usage**: Queries about user behavior and feature usage

## Common Workflows

- **Subscription Activity Analysis**: Use `diagnostics/subscription-activity.kql` to analyze all activities related to a specific subscription ID, including associated exceptions and operational status

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