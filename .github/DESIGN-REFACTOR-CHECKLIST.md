# Design Refactor PR Checklist

Use this checklist for PRs that propose architecture or design changes. This is part of the **Proactive Design Review skill** workflow.

## Pre-Implementation

- [ ] Identify the specific design smell(s): tight coupling, god object, hidden state, violations of SOLID, etc.
- [ ] Propose a staged refactor plan (multiple PRs vs. one large change)
- [ ] Estimate effort and risk
- [ ] For breaking API changes: obtain explicit approval from tech lead before implementation

## Implementation

- [ ] Add or update unit/integration tests covering behavior before and after the change.
- [ ] Keep the PR scope small and focused; do not change unrelated files.
- [ ] Run the full test suite locally; ensure all tests pass and zero warnings remain.
- [ ] Update inline code comments explaining the "why" (not just the "what").

## Documentation & Communication

- [ ] Include an Architecture Decision Record (ADR) if public API or cross-cutting architecture changed.
  - Template: See [DOCUMENTATION-MAINTENANCE.md](./DOCUMENTATION-MAINTENANCE.md#template-architecture-decision-record-adr)
  - Store ADR as a comment in the relevant code file or in the PR description
- [ ] Add a short design rationale in the PR description; link to `.github/skills/proactive-design/SKILL.md`
- [ ] Flag any docs that may need updating (see [DOCUMENTATION-MAINTENANCE.md](./DOCUMENTATION-MAINTENANCE.md))
- [ ] Provide a migration plan and versioning notes for any public API changes

## Verification & Rollback

- [ ] Include rollback steps and manual verification instructions for maintainers
- [ ] Note any required follow-up tasks (deprecations, performance benchmarks, cascading docs updates)
- [ ] If breaking changes: provide migration guide with before/after examples

## Reference

- **Proactive Design Review skill:** [.github/skills/proactive-design/SKILL.md](.github/skills/proactive-design/SKILL.md)
- **Documentation maintenance guide:** [DOCUMENTATION-MAINTENANCE.md](./DOCUMENTATION-MAINTENANCE.md)
- **Code conventions:** [copilot-instructions.md](./copilot-instructions.md#code-conventions)
