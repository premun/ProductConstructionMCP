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
- Suggest next troubleshooting steps based on findings
- Provide context about normal vs. abnormal patterns
- Connect individual traces to overall service health

## Formatting and Output
- Output the results in a user-friendly format (e.g., tables, charts) when appropriate
- Prefer being concise and focus on listing the relevant information rather than explaining everything in detail
- When referencing source code files and lines (e.g. `ProductConstructionService.DependencyFlow.PullRequestUpdater.ProcessCodeFlowUpdateAsync (lines 997, 999)`), include a link to the relevant file in the https://github.com/dotnet/arcade-services repository (you can append the `#L997-L999` notation to link to the exact lines of code).
- Use markdown formatting for code snippets and links
- Do not suggest recommendations for fixing the problems when not explicitly asked for them
