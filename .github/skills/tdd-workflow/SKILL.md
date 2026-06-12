---
name: tdd-workflow
description: |-
  Test-driven development workflow for C# with xUnit and FluentAssertions.
  Use when implementing new features, fixing bugs, or modifying behavior.
  Use proactively when user asks to implement, add, fix, change, or modify code.

  Examples:
  - user: "implement feature X" → write failing test first using xUnit + FluentAssertions
  - user: "fix bug Y" → write test reproducing the bug, then implement fix
  - user: "refactor Z" → ensure existing tests pass, refactor, verify tests still green
---

Test the behavior, not the implementation. A test's job is to document what the code must do and to catch regressions.

## Test Patterns

- **Framework**: xUnit v3 with `[Fact]` / `[Theory]`
- **Assertions**: FluentAssertions — `result.Should().Be(expected)`, `act.Should().Throw<T>()`
- **Naming**: `MethodOrBehavior_ExpectedOutcome_GivenCondition`
- **Classes**: `public sealed class [Subject]Tests`
- **Structure**: Arrange-Act-Assert with `#region` blocks grouping related tests
- **FNA tests**: Use `[Collection("Fna")]` with `FnaFixture` (spins up real GraphicsDevice)

For F32 comparisons:
```csharp
result.Should().Be(f32Value);
result.Float.Should().BeApproximately(expectedFloat, precision: 0.001f);
```

## TDD Cycle

### Red Phase
- Write a test that describes the desired behavior
- It should fail initially for the right reason (method doesn't exist, returns wrong value, etc.)
- If the test passes by accident (e.g., default value matches expected), that's fine — do not force a fail
- Follow existing naming and structural conventions from the codebase

### Green Phase
- Write the minimal implementation needed to pass the test
- Do not add functionality not covered by a test
- Do not optimize prematurely

### Refactor Phase
- Clean up the implementation while keeping tests green
- Run the test suite: `dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug`
- Ensure existing tests still pass
- Check that the implementation follows codebase patterns (check 2-3 existing files)

## Integration with Proactive Design

If a test reveals an unexpected problem (regression, design flaw, unexpected behavior):
1. Run `dotnet test` to see the full impact
2. Invoke the `proactive-design` skill for the escalation protocol
3. Do not fix in isolation — present the situation to the user with options
