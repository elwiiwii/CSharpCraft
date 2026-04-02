# Documentation Maintenance Guidelines

This document defines how to keep CSharpCraft & PSharp8 documentation accurate and useful as the codebase evolves.

---

## Priority System

Documentation is categorized into three tiers based on impact and frequency of change:

### 🔴 Critical — Must Not Fall Out of Sync

These are the **ground truth** docs for developers and AI agents. Breaking these leads to failed builds, broken workflows, or incorrect architecture understanding.

**Files:**
- `copilot-instructions.md` — Workspace guide (commands, architecture, conventions)
- `.runsettings` — Test environment configuration (FNA3D backend)
- `README.md` — Quick start, project overview
- Comments in `Main.cs`, `GameOrchestrator.cs`, `Pico8.cs` — Entry points

**Check before**:
- [ ] Changing architecture (managers, orchestrator, static API)
- [ ] Changing build/test commands or environment setup
- [ ] Adding/removing projects or namespaces
- [ ] Modifying FNA backend configuration

**Update when**:
- New manager added to GameOrchestrator
- Build command changes
- Test framework or runner changes
- Major architectural refactor

---

### 🟡 Important — Review Quarterly

These docs support development velocity but aren't on the critical path. Stale info slows down new work.

**Files:**
- `TDD-WORKFLOW.md` — Testing patterns and structure
- `DOCUMENTATION-MAINTENANCE.md` (this file) — Doc update guidelines
- Inline code comments — Complex "why" logic deserves explanation
- Code examples in issue descriptions
- Architecture diagrams or decision records (if created)

**Check before**:
- [ ] Adding a new test pattern not covered in TDD-WORKFLOW.md
- [ ] Changing code organization significantly
- [ ] Introducing a new testing library or mock pattern

**Update when**:
- Test organization structure changes
- New fixture or test infrastructure patterns emerge
- Code patterns shift (e.g., new mocking approach)
- Documentation review cycle (quarterly)

---

### 🟢 Nice-to-Have — Update as Discovered

These are implementation details and optimization notes. Helpful for future developers but not critical to shipping.

**Files:**
- Implementation comments in manager classes
- Optimization notes ("Why we cache sprites this way")
- Git commit messages with context
- CHANGELOG or version notes
- Performance benchmarks or decision notes

**When to update**:
- During code review, if a future dev would benefit from "why"
- When discovering a non-obvious performance insight
- Post-incident learning (e.g., "This LRU cache was added to fix…")

---

## When to Update Docs

Don't update docs reflexively on every commit — the cost (time, tokens, review burden) should match the benefit. Use this as a guide:

### Update Immediately (docs are actively wrong)
- Build or test commands have changed
- Architecture has changed (new/removed manager, different orchestrator pattern)
- FNA/SDL backend configuration changed
- A pitfall or fix is not documented and will trip up the next developer

### Flag and Batch (docs are likely stale)
- Added a new file to a subsystem (Key Files table may be outdated)
- Renamed or moved a class referenced in docs
- Changed a public API signature
- Discovered a better test pattern

**For AI Agents:** Don't stop to rewrite documentation mid-task. Flag it in your response ("Note: Key Files table in copilot-instructions.md may need updating") and continue. Let the user trigger a deliberate doc-update task.

### Skip (not worth documenting)
- Internal refactors with no behavioral change
- Renaming private variables
- Formatting or whitespace changes
- Optimization that doesn't change the API

---

## Documentation Sync Process

### Monthly Review (AI Agent + Human)
1. Pull latest code
2. Spot-check: Are examples in docs still valid?
3. Run tests: Do they match the documented commands?
4. Review: Did any architecture change go undocumented?

### When Architecture Changes
1. **Create a decision record** (even a comment in code is better than nothing):
   ```csharp
   // DECISION (2026-03-12): Changed from static Pico8 API to instance-based orchestrator
   // because AsyncLocal<T> wasn't thread-safe under heavy async workloads.
   // Kept static facade for backward compatibility.
   ```

2. **Update copilot-instructions.md**:
   - Diagram if complex
   - Before/after if migration path matters

3. **Update TDD-WORKFLOW.md** if tests need new setup

4. **Update inline comments** in affected files

### When Adding a New Manager
1. Add entry to [Key Files & Patterns](#key-files--patterns) table in `copilot-instructions.md`
2. Register in `GameOrchestrator.CreateOrchestrator()` with comment explaining responsibility
3. If public-facing, add static wrapper to `Pico8.cs` with XML doc comments
4. Add test class in `PSharp8.Tests/` following `ManagerNameTests.cs` pattern
5. Document fixture dependencies if needed

---

## Document Ownership

| File | Owner | Review Frequency |
|------|-------|------------------|
| copilot-instructions.md | AI + Primary Dev | Before architecture changes |
| TDD-WORKFLOW.md | QA/Testing Lead + AI | Quarterly or when test patterns change |
| DOCUMENTATION-MAINTENANCE.md | Tech Lead | Annually |
| README.md | Primary Dev | Before release, when setup changes |
| .runsettings | FNA/Platform Engineer | When FNA or SDL3 backend changes |
| Inline code comments | Code Author | During code review |

---

## Handling Documentation Drift

### How Drift Happens
- Code changes, but docs weren't updated (forgot or didn't know)
- Docs describe ideal state, but code has temporary workarounds
- Multiple docs say conflicting things
- Example code in docs is outdated

### Detection
1. **Code Review Question**: "Is this still documented?"
2. **Test Failure**: Does error message match docs?
3. **Onboarding Friction**: New dev can't follow the README as-is
4. **AI Agent Confusion**: Model asks clarifying questions about process

### Resolution
1. **Immediate** (blocker): Fix docs in the same PR before merge
2. **Short-term** (1 day): Create a docs-only PR if docs lag behind
3. **Long-term** (quarterly): Schedule review of all 🔴 critical docs

---

## Tips for Clear Documentation

### Use Real Examples from the Codebase
❌ **Bad:**
```
"Managers are composable components that handle subsystems."
```

✅ **Good:**
```
"Managers are composable components (see GraphicsManager, AudioManager, InputManager).
Add a new manager by:
1. Create PSharp8/PSharp8/MySubsystem/MyNewManager.cs
2. Register in GameOrchestrator.CreateOrchestrator()
3. Optionally expose static API in Pico8.cs"
```

### Separate "How" from "Why"
❌ **Mixed (confusing):**
```csharp
// We use AsyncLocal<T> because threading issues
_orchestrator = new AsyncLocal<GameOrchestrator>();
```

✅ **Clear (separate concerns):**
```csharp
// Store orchestrator in AsyncLocal<T> to ensure thread-safe access
// in async contexts (where Task.Run() creates new threads that
// would lose normal thread-local storage).
_orchestrator = new AsyncLocal<GameOrchestrator>();

// WHY: AsyncLocal<T> is needed (not just ThreadLocal<T>)
// because EntityContainer.Initialize() is called from async methods
// and we need each Task to get its own orchestrator instance.
```

### Use Checklists for Procedures
Instead of prose, use actionable checklists:

```markdown
### Adding a New Graphics Manager

1. [ ] Create `MyGraphicsManager.cs` in `PSharp8/Graphics/`
2. [ ] Implement `IGraphicsSystem` interface
3. [ ] Add constructor guard for dependencies:
       `_device = device ?? throw new ArgumentNullException(nameof(device))`
4. [ ] Register in `GameOrchestrator.CreateOrchestrator()`:
       `services.AddSingleton<IMyGraphicsSystem>(...)` 
5. [ ] Create `MyGraphicsManagerTests.cs` in `PSharp8.Tests/`
6. [ ] Add entry to [Key Files](#key-files--patterns) in `copilot-instructions.md`
7. [ ] Update `DOCUMENTATION-MAINTENANCE.md` if new patterns emerge
```

### Version Your Documentation
Mark when docs were last verified:

```
*Last updated: 2 April 2026*
*Last verified against: .NET 10.0, FNA 26.00, CSharpCraft v1.2*
```

---

## Documentation for AI Agents

When AI agents (Copilot, subagents, etc.) work in this workspace, they should:

1. **Read copilot-instructions.md first** before making changes
2. **Check inline comments** before refactoring complex methods
3. **Follow TDD-WORKFLOW.md** when adding tests
4. **Flag stale docs** if architecture changes — don't rewrite docs mid-task
5. **Ask human** before making decisions not covered in guidance
6. **Use the Proactive Design Review skill** (.github/skills/proactive-design/SKILL.md) when encountering design smells or architecture drift. Produce a small staged plan, include the design-refactor checklist in the PR, and obtain explicit approval for breaking API changes.

**Example: AI agent working on a new feature**
```
1. Read copilot-instructions.md → understands architecture
2. Check TDD-WORKFLOW.md → writes test first (Red)
3. Implement feature (Green)
4. Refactor for clarity (Refactor)
5. If architecture changed: flag it ("Key Files table may need updating") — don't auto-update
6. Let user trigger a deliberate doc update as a separate task
```

---

## Tools & Automation

### CI/CD Checks (Future Enhancement)
- [ ] Link checker: verify all docs links are valid
- [ ] Code example validator: ensure code snippets compile
- [ ] Staleness detector: warn if docs older than 90 days
- [ ] Coverage reporter: which features are tested vs. documented

### Local Checks
```bash
# Check markdown syntax
markdownlint .github/*.md

# Find dead links (install linkchecker)
linkchecker .github/*.md
```

---

## Escalation Path

| Issue | Action | Owner |
|-------|--------|-------|
| Documentation blocks a task | Update docs immediately, notify team | Current developer |
| Docs conflict with working code | Docs fail code review review round | Code reviewer + author |
| Architectural decision undocumented | Create decision record in PR | Tech lead |
| Docs older than 6 months | Schedule review, allocate time | Project manager |
| AI agent confused by docs | Clarify docs, re-run agent | Human + tech lead |

---

## Template: Architecture Decision Record (ADR)

Use this template when documenting significant architectural changes:

```markdown
## ADR: [Brief Title]

**Date**: 2026-03-12  
**Status**: Accepted | Proposed | Superseded  
**Relates to**: Issue #123, Epic "Async Refactor"

### Context
Why did we need to make a change? What problem were we solving?

### Decision
What architecture did we choose?

### Consequences
- ✅ Pros
- ❌ Cons or trade-offs

### Implementation Notes
- Update copilot-instructions.md
- Add test infrastructure in TDD-WORKFLOW.md
- Deploy on branch X-feature-async

### Related ADRs
- ADR-001: Original async decision
```

---

## Checklist: Quarterly Documentation Review

Every 3 months, verify critical docs are still accurate:

```
□ copilot-instructions.md
  □ Build commands still work?
  □ Architecture diagram (if exists) still accurate?
  □ File paths all exist?
  □ All code examples compile?

□ TDD-WORKFLOW.md
  □ Test patterns still recommended?
  □ Fixture setup still valid?
  □ Example tests still pass?

□ README.md
  □ Setup instructions still accurate?
  □ Example runs?
  □ Feature list up-to-date?

□ Inline comments
  □ No stale TODOs?
  □ "Why" comments still explain current code?

□ Links
  □ No dead external links (GitHub, docs.microsoft.com)?
  □ Internal file references all valid?
```

---

## Questions to Ask When Reviewing Docs

1. **Clarity**: Would a new dev understand this without asking questions?
2. **Accuracy**: Does the code match what the docs say?
3. **Completeness**: Are edge cases / gotchas documented?
4. **Examples**: Do examples capture the real-world pattern?
5. **Timeliness**: Would this be helpful when needed, or buried in process?

---

*Last updated: 2 April 2026*
