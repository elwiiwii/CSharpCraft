# Test-Driven Development (TDD) Workflow

This guide describes the Red-Green-Refactor cycle and test organization patterns for the CSharpCraft & PSharp8 workspace.

---

## The Red-Green-Refactor Cycle

### Phase 1: Red 🔴 — Write a Failing Test

Start by writing a test that expresses your desired behavior. The test should **fail** because the feature doesn't exist yet.

**Example: Adding a new sprite cache method**

```csharp
[Fact]
public void GetSprite_WithInvalidId_ReturnsNull()
{
    // Arrange
    var cache = new LruCache<int, Sprite>(maxSize: 10);
    
    // Act
    var result = cache.Get(999);
    
    // Assert
    result.Should().BeNull();
}
```

**Why first?** Writing the test first forces you to:
- Think about the **interface** (what should be public?)
- Define **expected behavior** clearly
- Avoid over-engineering ("gold-plating")

---

### Phase 2: Green 🟢 — Write Minimal Code to Pass

Implement the **minimum code** needed to make the test pass. Don't optimize; don't generalize; just make it work.

```csharp
public class LruCache<TKey, TValue>
{
    private readonly Dictionary<TKey, TValue> _cache = new();
    private readonly int _maxSize;

    public LruCache(int maxSize)
    {
        _maxSize = maxSize ?? throw new ArgumentNullException(nameof(maxSize));
    }

    public TValue? Get(TKey key)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return value;
        }
        return null;  // Minimal implementation — just make the test pass
    }
}
```

**Why minimal?** 
- Keeps implementation simple and testable
- Prevents premature optimization
- Makes refactoring phase more focused

---

### Phase 3: Refactor 🟠 — Improve Code Quality

Now that the test passes, **refactor without changing behavior**. This is where you:
- Extract reusable methods
- Apply SOLID principles
- Optimize algorithms
- Improve readability

**Before (hard to read):**
```csharp
public TValue? Get(TKey key)
{
    if (_cache.TryGetValue(key, out var value))
    {
        // Move to front (LRU logic)
        _accessOrder.Remove(key);
        _accessOrder.Add(key);
        return value;
    }
    return null;
}
```

**After (clearer intent):**
```csharp
public TValue? Get(TKey key)
{
    if (!_cache.TryGetValue(key, out var value))
    {
        return null;
    }

    MarkAsRecentlyUsed(key);
    return value;
}

private void MarkAsRecentlyUsed(TKey key)
{
    _accessOrder.Remove(key);
    _accessOrder.Add(key);
}
```

**Why refactor?**
- Keeps tests focused (each test verifies one behavior)
- Enables safe refactoring (tests catch regressions)
- Improves maintainability without risk

---

## Test Organization Patterns

### Directory Structure

```
PSharp8.Tests/
├── Infrastructure/
│   ├── FnaCollection.cs              # xUnit [CollectionDefinition("Fna")]
│   ├── FnaFixture.cs                 # FNA game loop fixture (graphics + audio)
│   └── GraphicsTestBase.cs           # Base class for GPU tests
├── Audio/
│   └── AudioManagerTests.cs          # Pure logic + FNA audio tests
├── Graphics/
│   ├── GraphicsManagerTests.cs
│   ├── LruCacheTests.cs
│   ├── PaletteManagerTests.cs
│   ├── SpriteMapDataTests.cs
│   ├── SpriteSnapshotTests.cs
│   └── SpriteTextureManagerTests.cs
├── Input/
│   └── InputManagerTests.cs
├── Memory/, PMath/, Scene/           # Subsystem test folders
└── PSharp8.Tests.csproj

CSharpCraft.Tests/
├── Infrastructure/
│   ├── FnaCollection.cs              # xUnit [CollectionDefinition("Fna")]
│   └── FnaFixture.cs                 # FNA game loop fixture (backend config + graphics)
├── Settings/
│   └── GeneralSettingsTests.cs
├── Input/
│   └── InputEventTranslatorTests.cs
└── CSharpCraft.Tests.csproj
```

**Rule:** One test file per production class. Either namespace tests directly in the test project (e.g., `PSharp8.Tests.Graphics`) or nest them in subfolders with matching namespaces.

---

## Test Structure Template

Use the **Arrange-Act-Assert** pattern with a clear, predictable structure:

```csharp
using FluentAssertions;
using Xunit;

namespace PSharp8.Tests;

public class MyComponentTests
{
    [Fact]
    public void MethodName_GivenContext_ExpectedBehavior()
    {
        // Arrange: Set up test data and dependencies
        var mock = new Mock<IDependency>();
        mock.Setup(d => d.GetValue()).Returns(42);
        var sut = new MyComponent(mock.Object);  // sut = System Under Test

        // Act: Execute the method being tested
        var result = sut.DoSomething();

        // Assert: Verify the result matches expectations
        result.Should().Be(42);
        mock.Verify(d => d.GetValue(), Times.Once);
    }

    [Theory]
    [InlineData(0, "zero")]
    [InlineData(1, "one")]
    [InlineData(2, "two")]
    public void MethodName_WithVariousInputs_ProcessesCorrectly(int input, string expected)
    {
        // Arrange
        var sut = new MyComponent();

        // Act
        var result = sut.ConvertToWord(input);

        // Assert
        result.Should().Be(expected);
    }
}
```

**Key points:**
- **`[Fact]`** for single test case
- **`[Theory]` + `[InlineData(...)]`** for parametrized tests
- **`sut`** = "System Under Test" (the class you're testing)
- **FluentAssertions** (`.Should().Be(...)`) reads like English
- **Moq** (`new Mock<T>()`) for mocking dependencies

---

## Organizing Large Test Classes

For test files with 20+ test methods, use `#region`/`#endregion` blocks to organize tests by behavior category. See [testing.instructions.md](../instructions/testing.instructions.md#test-organization-with-regions) for the complete pattern.

**Quick example:** Group constructor validation tests, happy-path tests, and error-case tests into separate regions for easy navigation:

```csharp
// --------------------------------------------------------------------------
#region Constructor null guards
// --------------------------------------------------------------------------

[Fact]
public void Constructor_ThrowsArgumentNullException_WhenDepIsNull() { ... }

// --------------------------------------------------------------------------
#endregion
#region Get method behavior
// --------------------------------------------------------------------------

// --- Happy path ---

[Fact]
public void Get_ReturnsCachedValue_WhenKeyExists() { ... }

// --- Error cases ---

[Fact]
public void Get_ReturnsNull_WhenKeyNotInCache() { ... }

// --------------------------------------------------------------------------
#endregion
```

This improves IDE navigation and makes large test files easier to maintain.

---

## Fixture Setup for Graphics Tests

FNA3D requires special initialization. Use `GraphicsFixture` for tests that need graphics resources:

```csharp
using FluentAssertions;
using Xunit;
using PSharp8.Tests.Infrastructure;

namespace PSharp8.Tests.Graphics;

public class SpriteTextureManagerTests : IClassFixture<GraphicsFixture>
{
    private readonly GraphicsFixture _fixture;

    public SpriteTextureManagerTests(GraphicsFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    [Fact]
    public void LoadSprite_WithValidId_ReturnsTexture()
    {
        // Arrange
        var spriteManager = new SpriteTextureManager(_fixture.GraphicsDevice);

        // Act
        var texture = spriteManager.LoadSprite(0);

        // Assert
        texture.Should().NotBeNull();
    }
}
```

**Why `IClassFixture<T>`?**
- xUnit re-uses the fixture for all tests in the class
- `GraphicsFixture` initializes FNA3D backend once
- Tests avoid expensive re-initialization overhead

---

## Common Test Patterns

### Testing Null Input (Constructor Guards)

```csharp
[Fact]
public void Constructor_WithNullDependency_ThrowsArgumentNullException()
{
    // Arrange & Act & Assert
    var act = () => new MyClass(dependency: null!);
    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("dependency");
}
```

### Testing State Changes

```csharp
[Fact]
public void Add_UpdatesCount()
{
    // Arrange
    var cache = new LruCache<int, string>(maxSize: 10);
    cache.Count.Should().Be(0);

    // Act
    cache.Add(1, "value");

    // Assert
    cache.Count.Should().Be(1);
}
```

### Testing Exceptions

```csharp
[Fact]
public void Parse_WithInvalidInput_ThrowsFormatException()
{
    // Arrange & Act & Assert
    var act = () => Parser.Parse("not a number");
    act.Should().Throw<FormatException>();
}
```

### Testing Collections

```csharp
[Fact]
public void GetAll_ReturnsAllItems()
{
    // Arrange
    var items = new[] { "a", "b", "c" };
    var sut = new ItemStore(items);

    // Act
    var result = sut.GetAll();

    // Assert
    result.Should().BeEquivalentTo(items);  // Order-independent comparison
    result.Should().HaveCount(3);           // Length assertion
    result.Should().Contain("a");           // Membership assertion
}
```

### Testing Mocked Dependencies

```csharp
[Fact]
public void ProcessItem_CallsLogger()
{
    // Arrange
    var loggerMock = new Mock<ILogger>();
    var processor = new ItemProcessor(loggerMock.Object);

    // Act
    processor.ProcessItem(new Item { Id = 1 });

    // Assert
    loggerMock.Verify(
        l => l.Log(It.IsAny<string>()),
        Times.Once,
        "Logger should be called exactly once"
    );
}
```

---

## Running Tests

### CLI (Recommended for CI/CD & Linux/Wayland)

```bash
# Run all tests
dotnet test CSharpCraft.slnx -c Debug

# Run specific project
dotnet test PSharp8.Tests/PSharp8.Tests.csproj -c Debug

# Run with verbose output
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug --verbosity=normal

# Run specific test file
dotnet test PSharp8.Tests/LruCacheTests.cs -c Debug
```

### VS Code Shell Task (Preferred within editor)

1. Open Command Palette: `Ctrl+Shift+P`
2. Search: "Tasks: Run Task"
3. Select: `test: CSharpCraft.Tests`

**Why not Testing view?**
- `.runsettings` not reliably applied before FNA3D startup
- Wayland/EGL fallback causes backend probe failures
- Shell task & CLI bypass this issue

---

## Integration Tests vs. Unit Tests

### Unit Tests (Isolated)
- Test a **single class** in isolation
- Mock external dependencies
- Fast (milliseconds)
- Most of your tests should be here

```csharp
[Fact]
public void PaletteManager_GetColor_ReturnsCachedPalette()
{
    var palette = new[] { Color.Red, Color.Blue };
    var mockDevice = new Mock<IGraphicsDevice>();
    var sut = new PaletteManager(mockDevice.Object, palette);
    
    var color = sut.GetColor(0);
    
    color.Should().Be(Color.Red);
}
```

### Integration Tests (Connected)
- Test **multiple components** together
- Use real FNA3D graphics device
- Slower (seconds)
- Verify boundary interactions

```csharp
[Fact]
public void RenderPipeline_WithSpritesAndPalette_RendersCorrectly()
{
    // This test uses real GraphicsDevice, real SpriteTextureManager, etc.
    var orchestrator = GameOrchestrator.CreateForTesting();
    var graphics = orchestrator.GraphicsManager;
    
    graphics.DrawSprite(spriteId: 0, x: 10, y: 20);
    
    // Verify rendered output (screenshot comparison, pixel checks, etc.)
}
```

---

## Test-First Development Workflow

### Step-by-Step Example: Adding Sprite Caching

1. **Red: Write the test**
   ```csharp
   [Fact]
   public void SpriteCache_EvictsOldestWhenFull()
   {
       var cache = new LruCache<int, Sprite>(maxSize: 2);
       cache.Add(1, sprite1);
       cache.Add(2, sprite2);
       
       cache.Add(3, sprite3);  // Should evict sprite1
       
       cache.Get(1).Should().BeNull();
       cache.Get(3).Should().NotBeNull();
   }
   ```

2. **Green: Implement minimally**
   ```csharp
   public void Add(TKey key, TValue value)
   {
       if (_cache.Count >= _maxSize)
       {
           var oldest = _accessOrder.First();
           _cache.Remove(oldest);
           _accessOrder.RemoveAt(0);
       }
       _cache[key] = value;
       _accessOrder.Add(key);
   }
   ```

3. **Refactor: Improve readability**
   ```csharp
   public void Add(TKey key, TValue value)
   {
       EvictIfAtCapacity();
       _cache[key] = value;
       _accessOrder.Add(key);
   }

   private void EvictIfAtCapacity()
   {
       if (_cache.Count >= _maxSize)
       {
           var oldest = _accessOrder.First();
           _cache.Remove(oldest);
           _accessOrder.RemoveAt(0);
       }
   }
   ```

4. **Repeat**: The next feature starts the cycle again with a new test.

---

## Debugging Failed Tests

### Check the Error Message First
```
Expected string to be "hello" with a length of 5, but "world" has a length of 5.
^-- This tells you exactly what went wrong
```

### Backend Issues (FNA3D)
If tests fail with FNA3D errors:
```bash
# Ensure .runsettings is applied
export FNA_PLATFORM_BACKEND=SDL3 FNA3D_FORCE_DRIVER=SDLGPU SDL_GPU_DRIVER=vulkan
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug
```

### Flaky Tests
- **Async timing issues**: Use `await Task.Delay(...)` or proper `ConfigureAwait(false)`
- **Randomized data**: Use fixed seeds or `[Theory]` with `[InlineData(...)]`
- **Shared state**: Ensure each test is independent (no static mutable state)

---

## Best Practices Checklist

When writing tests:
- [ ] Test name clearly describes the scenario and expected outcome
- [ ] Test is **independent** (doesn't rely on other tests)
- [ ] Test **fails for the right reason** (covers the happy path + edge cases)
- [ ] Arrange-Act-Assert structure is clear
- [ ] All dependencies are mocked (unit tests) or real (integration tests)
- [ ] No test data files or external I/O (pure logic)
- [ ] Test runs in < 100ms (or document why it's slower)
- [ ] FluentAssertions make assertions readable
- [ ] Constructor guards are tested (null checks)
- [ ] Run locally before pushing: `dotnet test ... -c Debug`

---

## More Reading

- **Microsoft: Unit testing best practices**: https://docs.microsoft.com/dotnet/core/testing/unit-testing-best-practices
- **Kent Beck (TDD inventor)**: https://www.kentbeck.com/
- **Test Pyramid**: https://martinfowler.com/bliki/TestPyramid.html (many unit tests, fewer integration tests)

---

*Last updated: 2 April 2026*
