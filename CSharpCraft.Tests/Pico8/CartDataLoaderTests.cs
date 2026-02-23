using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Tests.Pico8
{
    /// <summary>
    /// CartDataLoader Test Suite
    /// 
    /// TDD Phase: RED → GREEN → REFACTOR
    /// Tests define the data loading contract: parse sprites, flags, map data
    /// from IScene raw strings into an immutable CartData record.
    /// Extracted from GameOrchestrator.Reload() to follow SRP.
    /// All tests use FluentAssertions exclusively.
    /// </summary>
    public class CartDataLoaderTests
    {
        private readonly CartDataLoader _loader;
        private readonly List<Color> _colors;
        private readonly Dictionary<string, Texture2D> _textureDictionary;

        public CartDataLoaderTests()
        {
            _loader = new CartDataLoader();
            _colors = GameOrchestrator.DefaultColors;
            _textureDictionary = [];
        }

        /// <summary>
        /// Create a properly configured mock IScene with default empty data.
        /// </summary>
        private Mock<IScene> CreateMockScene(
            string spriteData = "",
            string spriteImage = "",
            string flagData = "",
            (int x, int y)? mapDimensions = null,
            string mapData = "",
            Dictionary<string, List<SongInst>>? music = null,
            Dictionary<string, Dictionary<int, string>>? sfx = null)
        {
            var mockScene = new Mock<IScene>();
            mockScene.Setup(s => s.SceneName).Returns("TestScene");
            mockScene.Setup(s => s.SpriteData).Returns(spriteData);
            mockScene.Setup(s => s.SpriteImage).Returns(spriteImage);
            mockScene.Setup(s => s.FlagData).Returns(flagData);
            mockScene.Setup(s => s.MapDimensions).Returns(mapDimensions ?? (0, 0));
            mockScene.Setup(s => s.MapData).Returns(mapData);
            mockScene.Setup(s => s.Music).Returns(music ?? new Dictionary<string, List<SongInst>>());
            mockScene.Setup(s => s.Sfx).Returns(sfx ?? new Dictionary<string, Dictionary<int, string>>());
            return mockScene;
        }

        #region NULL / ARGUMENT VALIDATION

        [Fact]
        public void Load_ThrowsArgumentNullException_WhenSceneIsNull()
        {
            var act = () => _loader.Load(null!, _colors, _textureDictionary);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("scene");
        }

        [Fact]
        public void Load_ThrowsArgumentNullException_WhenColorsIsNull()
        {
            var scene = CreateMockScene();
            var act = () => _loader.Load(scene.Object, null!, _textureDictionary);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("colors");
        }

        [Fact]
        public void Load_ThrowsArgumentNullException_WhenTextureDictionaryIsNull()
        {
            var scene = CreateMockScene();
            var act = () => _loader.Load(scene.Object, _colors, null!);
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("textureDictionary");
        }

        #endregion

        #region EMPTY DATA HANDLING

        [Fact]
        public void Load_WithEmptyScene_ReturnsEmptyCartData()
        {
            var scene = CreateMockScene();
            var result = _loader.Load(scene.Object, _colors, _textureDictionary);

            result.Should().NotBeNull();
            result.Sprites.Should().BeEmpty("no sprite data was provided");
            result.Flags.Should().BeEmpty("no flag data was provided");
            result.Map.Should().BeEmpty("no map data was provided");
            result.Music.Should().BeEmpty("no music data was provided");
            result.Sfx.Should().BeEmpty("no sfx data was provided");
        }

        #endregion

        #region SPRITE DATA PARSING

        [Fact]
        public void Load_ParsesSpriteData_FromHexString()
        {
            // "0" maps to color index 0 (black: 000000), "1" maps to color index 1 (dark blue: 1D2B53)
            var scene = CreateMockScene(spriteData: "01");
            var result = _loader.Load(scene.Object, _colors, _textureDictionary);

            result.Sprites.Should().HaveCount(2);
            result.Sprites[0].Should().Be(Pico8Utils.HexToColor("000000"),
                "hex '0' should map to PICO-8 color 0 (black)");
            result.Sprites[1].Should().Be(Pico8Utils.HexToColor("1D2B53"),
                "hex '1' should map to PICO-8 color 1 (dark blue)");
        }

        [Fact]
        public void Load_ParsesSpriteData_AllColors()
        {
            // Test parsing all 16 base colors (0-F)
            var scene = CreateMockScene(spriteData: "0123456789abcdef");
            var result = _loader.Load(scene.Object, _colors, _textureDictionary);

            result.Sprites.Should().HaveCount(16,
                "16 hex chars with n=1 should produce 16 Color values");
        }

        #endregion

        #region FLAG DATA PARSING

        [Fact]
        public void Load_ParsesFlagData_FromHexString()
        {
            // "00" = flag value 0, "ff" = flag value 255
            var scene = CreateMockScene(flagData: "00ff");
            var result = _loader.Load(scene.Object, _colors, _textureDictionary);

            result.Flags.Should().HaveCount(2);
            result.Flags[0].Should().Be(0, "hex '00' should parse to flag value 0");
            result.Flags[1].Should().Be(255, "hex 'ff' should parse to flag value 255");
        }

        [Fact]
        public void Load_WithEmptyFlagData_ReturnsEmptyFlags()
        {
            var scene = CreateMockScene(flagData: "");
            var result = _loader.Load(scene.Object, _colors, _textureDictionary);

            result.Flags.Should().BeEmpty("empty flag data should produce empty array");
        }

        #endregion

        #region MAP DATA PARSING

        [Fact]
        public void Load_ParsesMapData_WithCustomEncoding()
        {
            // PICO-8 map encoding: (char0 - 35) * 91 + (char1 - 35)
            // '#' is char 35, so "##" = (35-35)*91 + (35-35) = 0
            var scene = CreateMockScene(
                mapDimensions: (1, 1),
                mapData: "##");
            var result = _loader.Load(scene.Object, _colors, _textureDictionary);

            result.Map.Should().HaveCount(1);
            result.Map[0].Should().Be(0, "'##' encodes to sprite number 0");
        }

        [Fact]
        public void Load_ParsesMapData_MultipleEntries()
        {
            // Two map entries: "##" = 0, "$#" = (36-35)*91 + (35-35) = 91
            var scene = CreateMockScene(
                mapDimensions: (2, 1),
                mapData: "##$#");
            var result = _loader.Load(scene.Object, _colors, _textureDictionary);

            result.Map.Should().HaveCount(2);
            result.Map[0].Should().Be(0);
            result.Map[1].Should().Be(91);
        }

        [Fact]
        public void Load_ThrowsException_WhenMapDimensionsMismatch()
        {
            // Dimensions say 2x2 = 4 tiles, but data only has 2 chars = 1 tile
            var scene = CreateMockScene(
                mapDimensions: (2, 2),
                mapData: "##");
            var act = () => _loader.Load(scene.Object, _colors, _textureDictionary);

            act.Should().Throw<Exception>(
                "map dimensions (2x2=4) don't match data length (2 chars / 2 = 1 tile)");
        }

        [Fact]
        public void Load_WithEmptyMapData_ReturnsEmptyMap()
        {
            var scene = CreateMockScene(
                mapDimensions: (0, 0),
                mapData: "");
            var result = _loader.Load(scene.Object, _colors, _textureDictionary);

            result.Map.Should().BeEmpty("empty map data should produce empty array");
        }

        #endregion

        #region MUSIC AND SFX PASSTHROUGH

        [Fact]
        public void Load_PassesThroughMusicDictionary()
        {
            var music = new Dictionary<string, List<SongInst>>
            {
                ["track1"] = [new SongInst([], 0)]
            };
            var scene = CreateMockScene(music: music);
            var result = _loader.Load(scene.Object, _colors, _textureDictionary);

            result.Music.Should().BeSameAs(music,
                "music dictionary should be passed through from the scene");
        }

        [Fact]
        public void Load_PassesThroughSfxDictionary()
        {
            var sfx = new Dictionary<string, Dictionary<int, string>>
            {
                ["pack1"] = new Dictionary<int, string> { [0] = "sfx_data" }
            };
            var scene = CreateMockScene(sfx: sfx);
            var result = _loader.Load(scene.Object, _colors, _textureDictionary);

            result.Sfx.Should().BeSameAs(sfx,
                "sfx dictionary should be passed through from the scene");
        }

        #endregion

        #region INTERFACE CONFORMANCE

        [Fact]
        public void CartDataLoader_ImplementsICartDataLoader()
        {
            _loader.Should().BeAssignableTo<ICartDataLoader>(
                "CartDataLoader should implement the ICartDataLoader interface");
        }

        #endregion

        #region CARTDATA RECORD

        [Fact]
        public void CartData_Empty_ReturnsAllEmptyCollections()
        {
            var empty = CartData.Empty;

            empty.Sprites.Should().BeEmpty();
            empty.Flags.Should().BeEmpty();
            empty.Map.Should().BeEmpty();
            empty.Music.Should().BeEmpty();
            empty.Sfx.Should().BeEmpty();
        }

        [Fact]
        public void CartData_RecordEquality_WorksCorrectly()
        {
            var sprites = new Color[] { Color.Red };
            var flags = new int[] { 1 };
            var map = new int[] { 0 };
            var music = new Dictionary<string, List<SongInst>>();
            var sfx = new Dictionary<string, Dictionary<int, string>>();

            var data1 = new CartData(sprites, flags, map, music, sfx);
            var data2 = new CartData(sprites, flags, map, music, sfx);

            // Record equality checks reference equality for arrays, so same refs = equal
            data1.Should().Be(data2,
                "CartData records with same references should be equal");
        }

        #endregion
    }
}
