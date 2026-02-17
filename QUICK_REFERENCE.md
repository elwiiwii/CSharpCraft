# CSharpCraft Quick Reference

## 🚀 Quick Start Commands

### Clone & Setup
```bash
cd ~/Documents/Source/CSharpCraft
git status  # Verify you're in the right place
```

### Build
```bash
# Build everything
dotnet build CSharpCraft.slnx -p:Platform=x64

# Build one project
dotnet build CSharpCraft.Game/ -p:Platform=x64

# Build with specific verbosity
dotnet build -v minimal              # Show only errors
dotnet build -v diagnostic           # Show everything
```

### Run
```bash
# Run the game
cd CSharpCraft
dotnet run -p:Platform=x64

# Run from solution root
dotnet run --project CSharpCraft/CSharpCraft.csproj -p:Platform=x64
```

### Test
```bash
# Run all tests
dotnet test CSharpCraft.Tests/

# Run specific test class
dotnet test --filter "FixedTests"

# Run with output
dotnet test CSharpCraft.Tests/ --logger "console;verbosity=normal"

# Run and show coverage (requires special setup)
dotnet test /p:CollectCoverage=true
```

### Clean/Rebuild
```bash
# Clean all builds
dotnet clean

# Full rebuild
dotnet clean && dotnet build -p:Platform=x64
```

---

## 📁 Where to Find Things

### Service Interfaces
```
CSharpCraft.Pico8/
  ├── IGraphicsEngine.cs     ← Graphics rendering
  ├── IAudioManager.cs       ← Audio playback
  ├── IInputManager.cs       ← Input handling
  ├── ISceneManager.cs       ← Scene lifecycle
  └── IGameClock.cs          ← Frame timing
```

### Game Scenes
```
CSharpCraft.Game/
  ├── TitleScreen.cs         ← Main menu
  └── Competitive/*.cs       ← Multiplayer scenes
```

### Game Logic
```
CSharpCraft.Game/Pcraft/
  ├── PcraftBase.cs          ← Main engine (2,259 lines)
  ├── Entity.cs              ← Game entities
  └── Level.cs               ← Level data
```

### Math
```
CSharpCraft.FixMath/
  ├── F32.cs                 ← 32-bit fixed point
  ├── F64.cs                 ← 64-bit fixed point
  └── FixedUtil.cs           ← Math utilities
```

### Tests
```
CSharpCraft.Tests/
  ├── Game/Fixed32Tests.cs   ← Math tests
  ├── Game/SceneInitializationTests.cs  ← DI pattern tests
  └── Pico8/InputManagerTests.cs        ← Input tests
```

### Assets
```
CSharpCraft.Game/Content/
  ├── Graphics/              ← Sprite images
  ├── Music/                 ← Background music
  └── Sfx/                   ← Sound effects
```

---

## 🔧 Common Tasks

### Add a New Scene
1. Create file: `CSharpCraft.Game/[Category]/MyScene.cs`
2. Implement `IScene` interface
3. Copy pattern from existing scene
4. Register in `Main.cs` scene list

### Fix a Bug in Pcraft
1. Find method in `CSharpCraft.Game/Pcraft/PcraftBase.cs`
2. Write failing test first in `CSharpCraft.Tests/Game/PcraftTests.cs`
3. Make fix
4. Verify test passes
5. Commit with test

### Add a New Test
1. Create file: `CSharpCraft.Tests/[Area]/NewTests.cs`
2. Use pattern from `Fixed32Tests.cs`
3. Follow naming: `Component_Scenario_Expected()`
4. Run: `dotnet test --filter "NewTests"`

### Work with Audio
- **Play music**: `Pico8Functions.Music(trackId)`
- **Play sfx**: `Pico8Functions.Sfx(sfxId)`
- **Mute all**: `Pico8Functions.Mute()`

### Work with Graphics
- **Clear screen**: `p8.Cls(colorIndex)`
- **Draw sprite**: `p8.Spr(spriteNum, x, y)`
- **Draw text**: `p8.Print(text, x, y, color)`
- **Draw shape**: `p8.Circ(x, y, radius, color)`

### Handle Input
- **Check held**: `p8.Btn(buttonId)` → bool
- **Check pressed**: `p8.Btnp(buttonId)` → bool
- **Button codes**: 0=up, 1=down, 2=left, 3=right, 4=confirm, 5=pause

---

## 📝 File Templates

### New Scene Template
```csharp
namespace CSharpCraft.Game.YourCategory;

public class YourScene : IScene
{
    public string SceneName => "Your Scene";
    public double Fps => 60;
    public (int w, int h) Resolution => (128, 128);
    public string SpriteImage => "";
    public string SpriteData => "";
    public string FlagData => "";
    public (int x, int y) MapDimensions => (0, 0);
    public string MapData => "";
    public Dictionary<string, List<SongInst>> Music => [];
    public Dictionary<string, Dictionary<int, string>> Sfx => [];

    private Pico8Functions? p8;

    public void Init(Pico8Functions p8)
    {
        this.p8 = p8;
    }

    public void Update()
    {
        if (p8.Btnp(4)) // Confirm
        {
            // Handle input
        }
    }

    public void Draw()
    {
        p8.Cls(0);
        p8.Print("Your Scene", 10, 10, 7);
    }

    public void Dispose() { }
}
```

### New Test Template
```csharp
using Xunit;
using Moq;
using CSharpCraft.Game;

namespace CSharpCraft.Tests.Game;

public class MyComponentTests
{
    [Fact]
    public void Component_Scenario_ReturnsExpected()
    {
        // Arrange
        var mockService = new Mock<IServiceInterface>();
        mockService.Setup(x => x.Method()).Returns(true);

        // Act
        var result = mockService.Object.Method();

        // Assert
        Assert.True(result);
        mockService.Verify(x => x.Method(), Times.Once);
    }
}
```

---

## 🎯 Key Concepts

### IScene Interface
Every game screen implements this:
- `Init()` - Called once when scene starts
- `Update()` - Called each frame for logic
- `Draw()` - Called each frame for rendering
- `Dispose()` - Called when scene ends

### Service Interfaces (Coming Soon)
- `IGraphicsEngine` - All drawing (replaces p8 drawing calls)
- `IAudioManager` - All audio (replaces p8 audio calls)
- `IInputManager` - All input (replaces p8 Btn/Btnp)
- `ISceneManager` - Scene transitions
- `IGameClock` - Timing/FPS

### Mocking with Moq
```csharp
var mock = new Mock<IService>();
mock.Setup(x => x.Method()).Returns(value);  // Setup return
var service = mock.Object;                    // Get object
service.Method();                              // Use it
mock.Verify(x => x.Method(), Times.Once);    // Verify called
```

---

## 🐛 Troubleshooting

### "Error CS0246: namespace not found"
- Check project references
- Verify `using` statements at top of file
- Run `dotnet build` to resolve

### "Error: FNA not found"
- Check FNA.Core path in relative reference
- Verify FNA folder exists at `../../../FNA/`

### "Circular dependency error"
- Check project references don't loop back
- Verify dependency direction: Game → Pico8 → FixMath

### "Test hangs indefinitely"
- FNA requires graphics initialization
- Use mocks instead of real FNA classes for unit tests
- Check if you're creating GraphicsDevice in test

### "Build takes too long"
- Run targeted builds: `dotnet build ProjectName/`
- Use `--no-restore` on repeat builds
- Check for bloated bin/obj folders: `dotnet clean`

---

## 📚 Documentation Files

| File | Purpose | Length |
|------|---------|--------|
| **ARCHITECTURE.md** | Technical deep-dive, roadmap, design patterns | 650+ lines |
| **DEVELOPER_GUIDE.md** | How-to guide, examples, best practices | 500+ lines |
| **IMPLEMENTATION_SUMMARY.md** | What was done, metrics, next steps | 700+ lines |
| **VERIFICATION_CHECKLIST.md** | Verification of completion | 400+ lines |
| **QUICK_REFERENCE.md** | This file - quick lookup | 300+ lines |

**Start here**: QUICK_REFERENCE.md (this file)  
**Learn structure**: DEVELOPER_GUIDE.md  
**Deep dive**: ARCHITECTURE.md  
**See what's done**: IMPLEMENTATION_SUMMARY.md  

---

## 🔗 Project Dependencies

```
game code
    ↓ uses
IGraphicsEngine, IAudioManager, IInputManager
    ↓ implemented by
Pico8Functions (coordinator)
    ↓ uses
FNA graphics, audio, input
    ↓
external libraries (XNA graphics)
```

**Key Rule**: Never import up the dependency chain  
- ✅ Game can use Pico8
- ❌ Pico8 cannot use Game  
- ✅ Tests can use both
- ❌ Tests cannot import CSharpCraft executable

---

## 🎓 Learning Path

**For New Members:**
1. Read this file (5 min) - Quick Reference
2. Explore project structure (10 min) - File tree
3. Read DEVELOPER_GUIDE.md (30 min) - How things work
4. Look at one scene (15 min) - Understand structure
5. Run tests (5 min) - See framework in action
6. Modify a test (20 min) - Hands-on practice

**For Contributors:**
1. Pick a small issue
2. Write a test that fails
3. Make code to pass test
4. Run all tests to verify
5. Commit with test included

**For Architecture Review:**
1. Read ARCHITECTURE.md - System design
2. Review service interfaces - API contracts
3. Check project dependencies - No cycles
4. Examine test patterns - Testability approach
5. Plan next phase - What to refactor next

---

## ✨ Quick Win Tasks (Good First Issues)

### Easy (30 min)
- [ ] Add new test for Fixed32 division
- [ ] Document one key method
- [ ] Fix a compiler warning
- [ ] Update a README section

### Medium (1-2 hours)
- [ ] Create new test class with 3 tests
- [ ] Add a new scene following pattern
- [ ] Extract one method into its own class
- [ ] Write documentation example

### Harder (2-4 hours)
- [ ] Implement one service interface class
- [ ] Extract related methods into new class
- [ ] Add comprehensive test coverage to one module
- [ ] Refactor one big method into smaller ones

---

## 🚀 Before Your First Commit

- [ ] Code compiles without errors
- [ ] Tests pass: `dotnet test`
- [ ] Code follows patterns from existing files
- [ ] Comments added for complex logic
- [ ] Commit message is descriptive
- [ ] No debug code left in
- [ ] One logical change per commit

---

## 📞 Getting Help

### Documentation
- See above - pick the right doc for your question
- Code comments - Most complex code has doc comments
- Examples - Look at similar code elsewhere

### Common Questions
- **"Where do I add a scene?"** → Read DEVELOPER_GUIDE.md "Adding a New Scene"
- **"How do I write a test?"** → See CSharpCraft.Tests/ examples
- **"How do I use Moq?"** → Check InputManagerTests.cs
- **"What's the architecture?"** → Read ARCHITECTURE.md
- **"What was changed?"** → See IMPLEMENTATION_SUMMARY.md

---

## 🎉 You're All Set!

You now have:
- ✅ Modular, testable architecture
- ✅ Clear project structure
- ✅ Service interfaces for dependency injection
- ✅ xUnit + Moq testing framework  
- ✅ 1,850+ lines of documentation
- ✅ Example code for common tasks
- ✅ Zero compilation errors

**Next**: Pick a task and start contributing!

---

**Last Updated**: 16 February 2026  
**Version**: 1.0

Questions? Check the documentation files!
