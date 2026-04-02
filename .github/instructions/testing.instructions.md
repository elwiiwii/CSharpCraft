---
description: 'Use these guidelines when generating or updating tests.'
applyTo: '**/*Tests.cs'
---
# CSharpCraft & PSharp8 Testing Guidelines

## Test framework
- **xUnit** with `[Fact]` for single-case tests and `[Theory]` + `[InlineData(...)]` for parametrized tests
- **FluentAssertions** for all assertions — never use `Assert.Equal` or bare `Assert`
- **Moq** for mocking interfaces and abstract classes

## Test naming
Use the pattern `Subject_ExpectedBehavior_GivenContext`:
```csharp
public void Get_ReturnsNull_ForMissingKey()
public void Put_DisposesOldValue_WhenReplacingExistingKey()
public void Constructor_ThrowsArgumentNullException_WhenDepIsNull()
```

`[Theory]` tests use the same naming pattern — the method name covers all cases, and each `[InlineData]` line gets a comment explaining what makes that case distinct:
```csharp
[Theory]
[InlineData(7, 8)]   // width not a multiple of 8
[InlineData(8, 7)]   // height not a multiple of 8
[InlineData(9, 8)]   // width off by one
public void Constructor_Throws_WhenSpritesheetDimensionsNotMultipleOf8(int w, int h)
{
    var act = () => new SpriteMapData(new Texture2D(_gd, w, h), map, "");
    act.Should().Throw<ArgumentException>().WithParameterName("spriteTexture");
}
```

## Namespace and class structure
- One test class per production class: `LruCache` → `LruCacheTests`
- Namespace matches test project: `PSharp8.Tests` or `CSharpCraft.Tests`
- Seal helper/fixture types that don't need subclassing

## Test Organization with Regions

For large test classes (20+ tests), use `#region`/`#endregion` to organize related tests into logical groups. Use this pattern consistently:

```csharp
// --------------------------------------------------------------------------
#region Constructor argument validation
// --------------------------------------------------------------------------

[Fact]
public void Constructor_ThrowsArgumentNullException_WhenDepIsNull()
{
    var act = () => new MyManager(dep: null!);
    act.Should().Throw<ArgumentNullException>().WithParameterName("dep");
}

[Fact]
public void Constructor_ThrowsArgumentNullException_WhenOtherDepIsNull()
{
    var act = () => new MyManager(dep: realDep, other: null!);
    act.Should().Throw<ArgumentNullException>().WithParameterName("other");
}

// --------------------------------------------------------------------------
#endregion
// --------------------------------------------------------------------------
#region Get method behavior
// --------------------------------------------------------------------------

// --- Cache hit scenario ---

[Fact]
public void Get_ReturnsCachedValue_WhenKeyExists()
{
    var sut = new LruCache<int, string>(10);
    sut.Put(1, "value");
    
    var result = sut.Get(1);
    result.Should().Be("value");
}

// --- Cache miss scenario ---

[Fact]
public void Get_ReturnsNull_WhenKeyNotInCache()
{
    var sut = new LruCache<int, string>(10);
    
    var result = sut.Get(999);
    result.Should().BeNull();
}

// --------------------------------------------------------------------------
#endregion
```

**Pattern details:**
- Top-level separators: `// --------------------------------------------------------------------------` (72 dashes)
- Region markers: `#region` / `#endregion` with the separator line both above and below
- Subsection markers (optional): `// --- Subsection Name ---` (two dashes on each side, no region marker)
- Regions should group tests by **behavior category**, not by test type
- Use subsections within a region to break up scenarios (e.g., "happy path" vs. "error cases")

**Benefits:**
- Improves IDE navigation (outline view and Ctrl+K Ctrl+1)
- Limits cognitive load in large test files
- Makes it easy to find related tests
- Collapsible in editors for readability

## Arrange-Act-Assert (AAA)
Write all tests in three distinct stages. Only add `// Arrange / // Act / // Assert` comments when the stages aren't obvious from the code itself.

## Constructor null guards
Always verify null guards throw with the correct parameter name:
```csharp
[Fact]
public void Constructor_ThrowsArgumentNullException_WhenDepIsNull()
{
    var act = () => new MyManager(dep: null!);
    act.Should().Throw<ArgumentNullException>()
       .WithParameterName("dep");
}
```

## Assertions style
```csharp
result.Should().Be(expected);
result.Should().BeNull();
result.Should().NotBeNull();
result.Should().BeSameAs(other);
collection.Should().HaveCount(3);
collection.Should().Contain(item);
collection.Should().BeEquivalentTo(expected);  // order-independent
act.Should().Throw<FormatException>();
```

## Mocking with Moq
```csharp
var mock = new Mock<IDependency>();
mock.Setup(d => d.GetValue()).Returns(42);
var sut = new MyClass(mock.Object);
// ...
mock.Verify(d => d.GetValue(), Times.Once);
```

## FNA tests (Graphics & Audio)

Tests that require `GraphicsDevice` or audio (`SoundEffect`) must use the FNA test fixtures. **This is critical on Linux/Wayland** because fixtures pre-configure the SDL3/Vulkan backend before `GraphicsDevice` is created.

### PSharp8 Graphics Tests
Use `[Collection("Fna")]` + `GraphicsTestBase`:
```csharp
[Collection("Fna")]
public class MyGraphicsTests(FnaFixture fixture) : GraphicsTestBase(fixture)
{
    [Fact]
    public void MyTest()
    {
        var texture = MakeSolid(8, 8, Black);
        // ... test GPU resources
    }
}
```

`GraphicsTestBase` provides:
- Pico-8 color constants (`Black`, `DarkBlue`, `White`, etc.)
- `MakeSolid(width, height, color)` helper for test textures
- Automatic texture/resource cleanup on `Dispose()`

### Audio Tests
Use `FnaFixture.CreateSilentSoundEffect()` when tests need `SoundEffect` instances without real audio files:
```csharp
public void MyAudioTest()
{
    var silence = FnaFixture.CreateSilentSoundEffect(durationMs: 100);
    // ... test audio logic
}
```

### Pure Logic Tests (No FNA)
DO NOT use `FnaFixture` for tests without GPU or audio dependencies — keep them fast and isolated:
```csharp
public class MyColorCalcTests  // No fixture needed
{
    [Fact]
    public void MyLogicTest()
    {
        var result = PaletteManager.Blend(color1, color2);
        result.Should().Be(expected);
    }
}
```

### FNA Fixture Details
- **[Collection("Fna")]**: Marks test class as part of the shared Fna collection
- **FnaFixture**: Creates a real FNA game loop, initializes GraphicsDevice and audio
- **FnaCollection.cs**: Mediates fixture sharing across test classes in that collection
See [FnaCollection.cs](../../PSharp8/PSharp8.Tests/Infrastructure/FnaCollection.cs) for collection definition

## Accessing internal fields in tests
PSharp8 declares `[assembly: InternalsVisibleTo("PSharp8.Tests")]` in `GlobalUsings.cs`. Use `internal` fields (e.g. `sut._currentInstance`) directly instead of reflection — it's faster, rename-safe, and compile-checked.

## Running tests
```bash
# Always use CLI or the VS Code shell task — never the Testing view
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug
dotnet test ../PSharp8/PSharp8.Tests/PSharp8.Tests.csproj -c Debug
```
