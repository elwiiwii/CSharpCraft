---
name: proactive-design
description: |-
  Proactive consulting and alternative generation when unexpected problems arise.
  Use proactively when a bug is discovered, test fails, build breaks, design conflict,
  architecture decision needed, unexpected behavior, compilation error, or runtime error.

  Examples:
  - user encounters a build error → STOP, diagnose root cause, present 2-3 fix alternatives with tradeoffs
  - user reports a bug → pause current work, identify impact, propose root fix vs workaround options
  - architectural conflict detected → generate alternative designs, explain tech debt implications of each
  - test regression → diagnose, propose fix approaches, suggest documentation/workflow updates to prevent recurrence
---

## Escalation Protocol

When something unexpected arises, follow this sequence:

### 1. PAUSE
Stop all active changes. Do not continue in the current direction.

### 2. DIAGNOSE
Identify the root cause. Ask:
- What exactly went wrong?
- Why did it happen? (not just the symptom)
- Is this a new problem or a recurrence?
- Does this reveal a flaw in the existing design/assumptions?

### 3. GENERATE ALTERNATIVES
Produce at least 2 distinct approaches. For each, document:
- **Approach**: What it involves
- **Impact**: What changes are needed
- **Risk**: Likelihood of further issues
- **Effort**: Estimated work
- **Tech debt**: Does this fix the root cause or just patch the symptom?
- **Design quality**: Does this improve or compromise the architecture?

### 4. PRESENT TO USER
Present the options clearly:

> **Problem:** {one-line summary}
>
> **Root cause:** {diagnosis}
>
> **Options:**
> 1. {Option A} — {brief} — Impact: X | Risk: Y | Effort: Z
> 2. {Option B} — {brief} — Impact: X | Risk: Y | Effort: Z
> 3. {Option C} — {brief} — Impact: X | Risk: Y | Effort: Z
>
> **Recommendation:** {your assessment}
>
> *Waiting for your decision before proceeding.*

### 5. WAIT
Do not proceed until the user responds.

## Technical Debt Detection

Flag any proposed fix that works around a deeper problem. Warning signs:
- "We'll fix it properly later"
- Duplicating existing flawed logic
- Adding special cases instead of fixing the general case
- Copy-paste with minor modifications

**Rule:** Never compromise architectural design to work around past mistakes. If a design flaw is discovered during debugging, the fix must include addressing the design flaw.

## Learn & Prevent Cycle

After every resolution, identify preventive measures:

1. Could a unit test catch this? → **Add a test**
2. Could documentation clarify this? → **Update docs** (CLAUDE.md, README, code comments)
3. Could a code review catch this? → **Add a review checklist item**
4. Could a linting/analysis rule catch this? → **Propose a rule**
5. Could a CI check catch this? → **Propose a CI step**
6. Could a workflow change prevent this? → **Propose a workflow update**

Propose at least one preventive action with every resolution.
