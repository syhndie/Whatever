# AGENTS.md

## Purpose
- Defines repo-specific guidance for this Blazor project.
- Keep changes small, correct, and aligned with existing architecture.

## Core Workflow
- Start with a brief plan: goal, constraints, first executable step.
- Prefer minimal diffs over broad refactors.
- Reuse existing patterns/services/components before adding new abstractions.

## GitHub CLI (gh)
- Prefer `gh` for PR and issue workflows when available.
- If `gh` is not on PATH in this shell, use the full executable path:
  - `C:\Program Files\GitHub CLI\gh.exe`
- Verify availability and auth only if needed:
  - `gh --version`
  - `gh auth status`
- If `gh` is installed but not recognized, restart the terminal or call it via the full path above.
- Typical PR flow from the current branch:
  - `gh pr list --base master --head <branch> --state open`
  - `gh pr create --base master --head <branch> --fill`

## Blazor Standards
- Prefer strongly typed models/view-models over dynamic structures.
- Keep component logic in `@code` blocks or partial code-behind files when complexity grows.
- Use dependency injection for services; avoid static state unless clearly intentional.
- Keep rendering predictable: avoid unnecessary re-renders and side effects during render.
- Use async APIs end-to-end (`async`/`await`) and avoid sync-over-async.

## UI and Components
- Use MudBlazor as the first option for UI components, layout, forms, dialogs, and feedback patterns when appropriate.
- Before building custom UI controls, check whether MudBlazor already provides a suitable component.
- Keep styling consistent with MudBlazor theming; only add custom CSS when needed.
- Preserve accessibility basics (labels, keyboard navigation, semantic markup).

## Data and Validation
- Validate input at the UI boundary and service boundary.
- Use explicit error handling and user-facing error states (not silent failures).
- Prefer EF Core patterns already used in the repo; avoid introducing new data layers without need.

## Performance and Reliability
- Optimize only when there is a demonstrated bottleneck.
- Avoid premature abstraction and unnecessary dependencies.
- Add targeted logging for diagnosable failures; avoid noisy logs.

## Validation Checklist
- Run the smallest meaningful checks first:
  - Build the project.
  - Run affected tests (or add tests if a behavior change is introduced).
  - Verify changed pages/components manually when UI behavior changed.
- If full validation is not possible, clearly state what was not verified.

## Response Expectations
- Report: what changed, why, and remaining risks.
- Keep summaries concise and actionable.
