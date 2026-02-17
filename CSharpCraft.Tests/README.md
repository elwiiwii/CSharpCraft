# CSharpCraft.Tests

This is the xUnit test project for CSharpCraft, following SOLID principles and agile development practices.

## Structure

```
CSharpCraft.Tests/
├── Game/                 - Tests for game logic (scenes, entities, etc)
├── Pico8/                - Tests for Pico8 library services
└── README.md
```

## Running Tests

### Run all tests:
```bash
dotnet test CSharpCraft.Tests.csproj
```

### Run specific test class:
```bash
dotnet test --filter "ClassName"
```

### Run with verbose output:
```bash
dotnet test -v d
```

### Run with code coverage:
```bash
dotnet test /p:CollectCoverage=true
```

## Testing Best Practices

### 1. **Mock Dependencies**
Use Moq to mock Pico8 service interfaces, enabling tests without FNA dependencies:

```csharp
var graphicsMock = new Mock<IGraphicsEngine>();
var graphics = graphicsMock.Object;
```

### 2. **Test Arrange-Act-Assert Pattern**
```csharp
[Fact]
public void MyTest()
{
    // Arrange - Set up test data
    var input = new MyClass();
    
    // Act - Perform the action being tested
    var result = input.DoSomething();
    
    // Assert - Verify the outcome
    Assert.Equal(expected, result);
}
```

### 3. **Test Service Interfaces, Not Implementations**
Tests should work with `IGraphicsEngine`, `IInputManager`, etc., not specific implementations.

### 4. **Keep Tests Isolated**
Each test should be independent and not rely on other tests' state or order.

### 5. **Use Descriptive Test Names**
Method names should describe what is being tested and the expected outcome:
- `FixedMath_Addition_ProducesCorrectResult`
- `Scene_UpdateWithInput_TransitionsCorrectly`

## Common Mock Setups

### Mock Graphics Engine:
```csharp
var graphicsMock = new Mock<IGraphicsEngine>();
graphicsMock.SetupGet(x => x.Cell).Returns((8, 8));
graphicsMock.Setup(x => x.Cls(It.IsAny<int>()));
```

### Mock Input Manager:
```csharp
var inputMock = new Mock<IInputManager>();
inputMock.Setup(x => x.Btn(0)).Returns(true);      // Up button
inputMock.Setup(x => x.Btnp(4)).Returns(true);     // Confirm button
```

### Mock Scene Manager:
```csharp
var sceneManagerMock = new Mock<ISceneManager>();
sceneManagerMock.Setup(x => x.ScheduleScene(It.IsAny<Func<IScene>>()));
```

## Future Additions

- Integration tests for complete scenes
- Performance tests for physics/rendering
- Load tests for network functionality
- UI acceptance tests
- Snapshot tests for sprite rendering

## Useful Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4/wiki/Quickstart)
- [Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)
