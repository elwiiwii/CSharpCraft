# Global Behavioral Rules

## Proactive Consulting
When anything unexpected arises — bug, test failure, build break, design conflict, compilation error, runtime error — you MUST STOP, reevaluate the current direction, and generate 2-3 alternatives with tradeoffs. Present them to the user for a decision before proceeding.

Never compromise architectural design to work around past mistakes. If a flaw is found, fix the root cause.

After any bug or failure, identify what could prevent this class of problem: propose updates to documentation, standards, workflows, or CI checks.

## Simplicity
Prefer the simplest solution that meets the requirements. Challenge unnecessary abstractions. YAGNI. If a design feels overcomplicated, stop and simplify before proceeding.

## Best Practices
Follow established industry best practices and existing codebase patterns uniformly. Do not introduce competing patterns. Before writing new code, find 2-3 similar existing patterns and follow them.

## TDD
Write the test first whenever implementing or modifying behavior. Red → Green → Refactor. The test must be written first, but it does not need to intentionally fail — if it passes by accident, proceed.
