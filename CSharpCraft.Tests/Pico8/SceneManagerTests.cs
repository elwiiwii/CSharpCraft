using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;

namespace CSharpCraft.Tests.Pico8;

public class SceneManagerTests
{
    private SceneManager CreateManager() => new();

    private Mock<IScene> CreateMockScene(string name = "TestScene")
    {
        var mock = new Mock<IScene>();
        mock.Setup(s => s.SceneName).Returns(name);
        mock.Setup(s => s.Fps).Returns(60.0);
        mock.Setup(s => s.Resolution).Returns((128, 128));
        mock.Setup(s => s.SpriteData).Returns("");
        mock.Setup(s => s.SpriteImage).Returns("");
        mock.Setup(s => s.FlagData).Returns("");
        mock.Setup(s => s.MapDimensions).Returns((0, 0));
        mock.Setup(s => s.MapData).Returns("");
        mock.Setup(s => s.Music).Returns(new Dictionary<string, List<SongInst>>());
        mock.Setup(s => s.Sfx).Returns(new Dictionary<string, Dictionary<int, string>>());
        return mock;
    }

    // ── CurrentScene ──

    [Fact]
    public void CurrentScene_IsNull_Initially()
    {
        var manager = CreateManager();
        manager.CurrentScene.Should().BeNull();
    }

    [Fact]
    public void CurrentScene_ReturnsScene_AfterTransition()
    {
        var manager = CreateManager();
        var scene = CreateMockScene().Object;
        manager.TransitionToScene(scene);
        manager.CurrentScene.Should().BeSameAs(scene);
    }

    // ── TransitionToScene ──

    [Fact]
    public void TransitionToScene_SetsCurrentScene()
    {
        var manager = CreateManager();
        var scene = CreateMockScene("SceneA").Object;
        manager.TransitionToScene(scene);
        manager.CurrentScene.Should().BeSameAs(scene);
    }

    [Fact]
    public void TransitionToScene_OverwritesPreviousScene()
    {
        var manager = CreateManager();
        var sceneA = CreateMockScene("A").Object;
        var sceneB = CreateMockScene("B").Object;
        manager.TransitionToScene(sceneA);
        manager.TransitionToScene(sceneB);
        manager.CurrentScene.Should().BeSameAs(sceneB);
    }

    [Fact]
    public void TransitionToScene_ThrowsOnNull()
    {
        var manager = CreateManager();
        var act = () => manager.TransitionToScene(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── RegisterScene ──

    [Fact]
    public void RegisterScene_AddsSceneToRegistry()
    {
        var manager = CreateManager();
        var scene = CreateMockScene().Object;
        manager.RegisterScene(scene);
        manager.GetRegisteredScenes().Should().Contain(scene);
    }

    [Fact]
    public void RegisterScene_DoesNotAddDuplicates()
    {
        var manager = CreateManager();
        var scene = CreateMockScene().Object;
        manager.RegisterScene(scene);
        manager.RegisterScene(scene);
        manager.GetRegisteredScenes().Should().HaveCount(1);
    }

    [Fact]
    public void RegisterScene_AllowsMultipleDistinctScenes()
    {
        var manager = CreateManager();
        var sceneA = CreateMockScene("A").Object;
        var sceneB = CreateMockScene("B").Object;
        manager.RegisterScene(sceneA);
        manager.RegisterScene(sceneB);
        manager.GetRegisteredScenes().Should().HaveCount(2);
    }

    [Fact]
    public void RegisterScene_ThrowsOnNull()
    {
        var manager = CreateManager();
        var act = () => manager.RegisterScene(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── GetRegisteredScenes ──

    [Fact]
    public void GetRegisteredScenes_ReturnsEmptyInitially()
    {
        var manager = CreateManager();
        manager.GetRegisteredScenes().Should().BeEmpty();
    }

    [Fact]
    public void GetRegisteredScenes_ReturnsCopy()
    {
        var manager = CreateManager();
        var scene = CreateMockScene().Object;
        manager.RegisterScene(scene);

        var list1 = manager.GetRegisteredScenes();
        var list2 = manager.GetRegisteredScenes();
        list1.Should().NotBeSameAs(list2);
    }

    // ── ScheduleScene ──

    [Fact]
    public void ScheduleScene_StoresFactory()
    {
        var manager = CreateManager();
        var scene = CreateMockScene().Object;
        manager.ScheduleScene(() => scene);
        manager.GetAndClearScheduledScene().Should().NotBeNull();
    }

    [Fact]
    public void ScheduleScene_ThrowsOnNull()
    {
        var manager = CreateManager();
        var act = () => manager.ScheduleScene(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ScheduleScene_OverwritesPreviousSchedule()
    {
        var manager = CreateManager();
        var sceneA = CreateMockScene("A").Object;
        var sceneB = CreateMockScene("B").Object;
        manager.ScheduleScene(() => sceneA);
        manager.ScheduleScene(() => sceneB);
        var factory = manager.GetAndClearScheduledScene();
        factory!().Should().BeSameAs(sceneB);
    }

    // ── GetAndClearScheduledScene ──

    [Fact]
    public void GetAndClearScheduledScene_ReturnsNull_WhenNothingScheduled()
    {
        var manager = CreateManager();
        manager.GetAndClearScheduledScene().Should().BeNull();
    }

    [Fact]
    public void GetAndClearScheduledScene_ReturnsFactory()
    {
        var manager = CreateManager();
        var scene = CreateMockScene().Object;
        manager.ScheduleScene(() => scene);
        var factory = manager.GetAndClearScheduledScene();
        factory.Should().NotBeNull();
        factory!().Should().BeSameAs(scene);
    }

    [Fact]
    public void GetAndClearScheduledScene_ClearsAfterGet()
    {
        var manager = CreateManager();
        manager.ScheduleScene(() => CreateMockScene().Object);
        manager.GetAndClearScheduledScene();
        manager.GetAndClearScheduledScene().Should().BeNull();
    }

    [Fact]
    public void GetAndClearScheduledScene_DoesNotAffectCurrentScene()
    {
        var manager = CreateManager();
        var currentScene = CreateMockScene("Current").Object;
        manager.TransitionToScene(currentScene);
        manager.ScheduleScene(() => CreateMockScene("Next").Object);
        manager.GetAndClearScheduledScene();
        manager.CurrentScene.Should().BeSameAs(currentScene);
    }

    // ── Combined operations ──

    [Fact]
    public void RegisterAndTransition_WorkTogether()
    {
        var manager = CreateManager();
        var scene = CreateMockScene().Object;
        manager.RegisterScene(scene);
        manager.TransitionToScene(scene);
        manager.GetRegisteredScenes().Should().Contain(scene);
        manager.CurrentScene.Should().BeSameAs(scene);
    }

    [Fact]
    public void ScheduleAndTransition_AreIndependent()
    {
        var manager = CreateManager();
        var sceneA = CreateMockScene("A").Object;
        var sceneB = CreateMockScene("B").Object;
        manager.TransitionToScene(sceneA);
        manager.ScheduleScene(() => sceneB);
        // Current is still A, but a transition to B is scheduled
        manager.CurrentScene.Should().BeSameAs(sceneA);
        manager.GetAndClearScheduledScene()!().Should().BeSameAs(sceneB);
    }
}
