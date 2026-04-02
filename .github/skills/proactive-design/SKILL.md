---
name: proactive-design
description: '**WORKFLOW SKILL** — Be proactive and vigilant in identifying and removing convoluted, fragile, or poorly designed architecture. Use for design reviews, refactor plans, and code health audits. DO NOT USE for trivial lint/style changes.'
---

# Proactive Design Review (ProactiveDesign)

Purpose
- Help the agent identify and remediate convoluted or poorly designed architecture.
- Encourage safe, incremental improvements that increase maintainability and clarity.

When to Use
- During code review or PR triage when design or code-quality issues are suspected.
- When working with legacy modules with high complexity or technical debt.
- When a proposed change increases coupling, hidden state, or long-term maintenance cost.

Behavior / Responsibilities
- Explain the specific design smells found (tight coupling, god objects, hidden global state, unclear invariants).
- Propose concrete, minimal refactor steps that preserve behavior and add tests.
- Provide a staged plan: small PRs, test updates, verification steps, and rollback strategy.
- Estimate impact and effort; call out migration steps for breaking changes.
- If a breaking API change is required, escalate and require explicit approval before implementing.

Guardrails
- Never justify or preserve bad design merely because "it's how it was done before." Challenge historical decisions with data and rationale.
- Avoid large one-shot refactors without tests, benchmarks, and explicit approval.
- Do not change unrelated code in the same PR; keep changes scoped and reviewable.
- Do not remove functionality without tests and a documented migration path.

Checklist (for suggested refactors)
- Add or update unit/integration tests covering behavior before and after change.
- Verify full test suite passes locally and in CI.
- Add a short design rationale in the PR description and link to this skill if appropriate.
- For public API changes: include a migration plan, versioning notes, and deprecation timeline.

Example agent prompts
- "Perform a design review of `PSharp8/Graphics/TextureCache.cs` and propose safe refactors with tests."
- "This module has high cyclomatic complexity — suggest incremental improvements and list required tests."

Non-goals
- This skill is not for formatting, trivial lint fixes, or demand-driven small style changes. Use normal linters or formatting tools for those.

Acceptance Criteria
- Suggestions include concrete steps and minimal, test-backed code changes.
- Large or breaking changes are accompanied by migration plans, tests, and an explicit approval step.

Resources
- Design refactor checklist: [.github/DESIGN-REFACTOR-CHECKLIST.md](../../DESIGN-REFACTOR-CHECKLIST.md)
- Workspace guidance: [.github/copilot-instructions.md](../../copilot-instructions.md)

