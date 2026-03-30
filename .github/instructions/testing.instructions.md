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
- Group related tests with `// --- Section ---` comments (see `PaletteManagerTests.cs`)
- Seal helper/fixture types that don't need subclassing

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
Tests that require `GraphicsDevice` or audio (`SoundEffect`) must use `[Collection("Fna")]` + `FnaFixture`:
```csharp
[Collection("Fna")]
public class MyFnaTests(FnaFixture fixture) : GraphicsTestBase(fixture)
{
}
```

Use `FnaFixture.CreateSilentSoundEffect()` when tests need `SoundEffect` instances without real audio files.

DO NOT use `FnaFixture` in tests that don't need GPU or audio — keep pure logic tests fast and dependency-free.

## Accessing internal fields in tests
PSharp8 declares `[assembly: InternalsVisibleTo("PSharp8.Tests")]` in `GlobalUsings.cs`. Use `internal` fields (e.g. `sut._currentInstance`) directly instead of reflection — it's faster, rename-safe, and compile-checked.

## Running tests
```bash
# Always use CLI or the VS Code shell task — never the Testing view
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug
dotnet test ../PSharp8/PSharp8.Tests/PSharp8.Tests.csproj -c Debug
```
