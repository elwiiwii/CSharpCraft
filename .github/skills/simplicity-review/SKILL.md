---
name: simplicity-review
description: |-
  Review code and designs for unnecessary complexity and over-engineering.
  Use when completing implementation, during code review, or when a design
  feels overcomplicated. Use proactively after implementing multi-file changes
  or when introducing new abstractions.

  Examples:
  - user completes a feature → audit for YAGNI violations, unnecessary interfaces, over-engineering
  - user introduces a new class or interface → challenge whether this abstraction is truly needed
  - design has deep inheritance → suggest composition, simpler alternatives
  - user proposes configuration system → check if simpler approach suffices
---

## Audit Checklist

Run through these checks before considering work complete:

### YAGNI (You Aren't Gonna Need It)
- Is every feature/abstraction justified by a current requirement?
- Are there any "we might need this later" constructs?
- Could the code be simpler by removing unused parameters, methods, or classes?

### Abstraction Review
- Does every `interface` have at least 2 current implementations? If not, consider removing it.
- Does every `virtual` method currently have at least one override? If not, make it non-virtual.
- Is the inheritance depth reasonable? Consider composition over inheritance.
- Are there factory patterns, strategies, or visitors that add complexity without clear benefit?

### File & Structure Review
- Could this be done in fewer files?
- Are there unnecessary partial classes, nested classes, or helper classes?
- Is the public API surface minimized?
- Could a simple method/function replace a class?

### Configuration Review
- Are there configurable parameters that nobody will ever change?
- Could sensible defaults replace configuration?
- Is there a simpler approach than the current settings system?

## Anti-Patterns to Flag

| Anti-Pattern | Simpler Alternative |
|---|---|
| Premature generalization (interfaces for single impl) | Concrete class, extract interface when needed |
| Deep inheritance chains | Composition, flat hierarchy |
| Over-configuration | Sensible hardcoded defaults |
| Gold-plating (extra features "for completeness") | Only what's needed now |
| Builder/Factory for simple construction | Constructor or factory method |
| Heavy abstraction layers | Direct approach, replace when proven necessary |

## When to Escalate

If you identify significant over-engineering that requires structural changes:
1. Document the complexity problem
2. Propose a simpler alternative
3. Invoke the `proactive-design` skill to present options to the user
