# GitHub Copilot Instructions - Azure Application Insights Analyzer

This repository contains tools for analyzing Azure Application Insights data with specialized KQL queries to diagnose service issues.

## Key Capabilities

1. Execute KQL queries from the knowledge base
2. Explain what different queries do and how to interpret the results
3. Recommend appropriate queries based on user problems
4. Analyze query results and provide actionable insights
5. Help troubleshoot service issues using Application Insights data
6. Track complete operation timelines using operation IDs for diagnostics

## Knowledge Base Structure

When suggesting KQL queries, focus on the structured query knowledge base in `/knowledge-base/kql-queries/` which contains:

- **exceptions**: For error detection and analysis (failed work items, exceptions)
- **diagnostics**: For operation tracing and detailed debugging

## Query Selection Guidelines

- For failed operations: Recommend `exceptions/failed-work-items.kql` 
- For tracing specific operations: Use `diagnostics/operation-traces-by-id.kql`
- For repository impact analysis: Suggest `exceptions/failed-work-items-repositories.kql`
- For exception pattern analysis: Use `exceptions/failed-work-items-exceptions.kql`

## Result Interpretation Assistance

- Help analyze query results by explaining key columns and metrics
- Try to output the results in a user-friendly format (e.g., tables, charts), mostly summarize the data
- Suggest next troubleshooting steps based on findings
- Provide context about normal vs. abnormal patterns
- Connect individual traces to overall service health
