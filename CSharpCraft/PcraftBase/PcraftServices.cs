using CSharpCraft.PcraftBase.Crafting;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Draw;
using CSharpCraft.PcraftBase.Inventory;
using CSharpCraft.PcraftBase.Map;
using CSharpCraft.PcraftBase.Menu;
using CSharpCraft.PcraftBase.Physics;
using CSharpCraft.PcraftBase.Update;

namespace CSharpCraft.PcraftBase;

internal class PcraftServices
{
    private static PcraftServices _current = new();
    internal static void SetServices(PcraftServices s) => _current = s;

    #region Crafting
    // ---------------------------------------------------------------------------

    internal static bool CanCraft(List<InventorySlot> invent, Recipe recipe)
        => _current.OnCanCraft(invent, recipe);

    protected virtual bool OnCanCraft(List<InventorySlot> invent, Recipe recipe)
        => CraftingSystem.CanCraft(invent, recipe);

    internal static void Craft(List<InventorySlot> invent, Recipe recipe)
        => _current.OnCraft(invent, recipe);

    protected virtual void OnCraft(List<InventorySlot> invent, Recipe recipe)
        => CraftingSystem.Craft(invent, recipe);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Background
    // ---------------------------------------------------------------------------

    internal static bool Comp(int i, int j, BlendGroup blendGroup, Level level)
        => _current.OnComp(i, j, blendGroup, level);

    protected virtual bool OnComp(int i, int j, BlendGroup blendGroup, Level level)
        => BackDrawer.Comp(i, j, blendGroup, level);

    internal static int CornerOffset(bool sideH, bool sideV, bool diag, int rnd,
        int innerCorner, int hOnly, int vOnly, int outer)
        => _current.OnCornerOffset(sideH, sideV, diag, rnd, innerCorner, hOnly, vOnly, outer);

    protected virtual int OnCornerOffset(bool sideH, bool sideV, bool diag, int rnd,
        int innerCorner, int hOnly, int vOnly, int outer)
        => BackDrawer.CornerOffset(sideH, sideV, diag, rnd, innerCorner, hOnly, vOnly, outer);

    internal static void DrawBack(Level level, PlayerEntity player)
        => _current.OnDrawBack(level, player);

    protected virtual void OnDrawBack(Level level, PlayerEntity player)
        => BackDrawer.DrawBack(level, player);

    internal static int RndCenter(double i, double j, Level level)
        => _current.OnRndCenter(i, j, level);

    protected virtual int OnRndCenter(double i, double j, Level level)
        => BackDrawer.RndCenter(i, j, level);

    internal static int RndSand(double i, double j, Level level)
        => _current.OnRndSand(i, j, level);

    protected virtual int OnRndSand(double i, double j, Level level)
        => BackDrawer.RndSand(i, j, level);

    internal static int RndTree(double i, double j, Level level)
        => _current.OnRndTree(i, j, level);

    protected virtual int OnRndTree(double i, double j, Level level)
        => BackDrawer.RndTree(i, j, level);

    internal static void Spr4(double i, double j, int gi, int gj,
        int a, int b, int c, int d, int off, Func<double, double, int> f)
        => _current.OnSpr4(i, j, gi, gj, a, b, c, d, off, f);

    protected virtual void OnSpr4(double i, double j, int gi, int gj,
        int a, int b, int c, int d, int off, Func<double, double, int> f)
        => BackDrawer.Spr4(i, j, gi, gj, a, b, c, d, off, f);

    internal static void WatAnim(double i, double j, Level level)
        => _current.OnWatAnim(i, j, level);

    protected virtual void OnWatAnim(double i, double j, Level level)
        => BackDrawer.WatAnim(i, j, level);

    internal static F32 WatVal(double i, double j, Level level)
        => _current.OnWatVal(i, j, level);

    protected virtual F32 OnWatVal(double i, double j, Level level)
        => BackDrawer.WatVal(i, j, level);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Enemies
    // ---------------------------------------------------------------------------

    internal static void DrawEnemies(PlayerEntity player, Level level)
        => _current.OnDrawEnemies(player, level);

    protected virtual void OnDrawEnemies(PlayerEntity player, Level level)
        => EnemiesDrawer.DrawEnemies(player, level);

    internal static void DrawPlayer(F32 x, F32 y, F32 rot, F32 anim, F32 subAnim, bool isPlayer, PlayerEntity player, Level level)
        => _current.OnDrawPlayer(x, y, rot, anim, subAnim, isPlayer, player, level);

    protected virtual void OnDrawPlayer(F32 x, F32 y, F32 rot, F32 anim, F32 subAnim, bool isPlayer, PlayerEntity player, Level level)
        => EnemiesDrawer.DrawPlayer(x, y, rot, anim, subAnim, isPlayer, player, level);

    internal static void SortY(List<CharacterEntity> enemies)
        => _current.OnSortY(enemies);

    protected virtual void OnSortY(List<CharacterEntity> enemies)
        => EnemiesDrawer.SortY(enemies);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Entities
    // ---------------------------------------------------------------------------

    internal static void DrawEnt(Level level)
        => _current.OnDrawEnt(level);

    protected virtual void OnDrawEnt(Level level)
        => EntDrawer.DrawEnt(level);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Helpers
    // ---------------------------------------------------------------------------

    internal static void SetPal(int[] l)
        => _current.OnSetPal(l);

    protected virtual void OnSetPal(int[] l)
        => DrawHelpers.SetPal(l);

    internal static void PrintB(string t, double x, double y, double inCol, double outCol)
        => _current.OnPrintB(t, x, y, inCol, outCol);

    protected virtual void OnPrintB(string t, double x, double y, double inCol, double outCol)
        => DrawHelpers.PrintB(t, x, y, inCol, outCol);

    internal static void PrintC(string t, int x, int y, int c)
        => _current.OnPrintC(t, x, y, c);

    protected virtual void OnPrintC(string t, int x, int y, int c)
        => DrawHelpers.PrintC(t, x, y, c);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Hud
    // ---------------------------------------------------------------------------

    internal static void DrawBar(F32 px, F32 py, F32 v, F32 m, F32 c, F32 c2)
        => _current.OnDrawBar(px, py, v, m, c, c2);

    protected virtual void OnDrawBar(F32 px, F32 py, F32 v, F32 m, F32 c, F32 c2)
        => HudDrawer.DrawBar(px, py, v, m, c, c2);

    internal static void DrawHud(PlayerEntity player)
        => _current.OnDrawHud(player);

    protected virtual void OnDrawHud(PlayerEntity player)
        => HudDrawer.DrawHud(player);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Main
    // ---------------------------------------------------------------------------

    internal static void DrawMain(PlayerEntity player, Level level)
        => _current.OnDrawMain(player, level);

    protected virtual void OnDrawMain(PlayerEntity player, Level level)
        => PcraftDraw.DrawMain(player, level);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Menu
    // ---------------------------------------------------------------------------

    internal static void DrawChestPanels(ChestMenu menu)
        => _current.OnDrawChestPanels(menu);

    protected virtual void OnDrawChestPanels(ChestMenu menu)
        => MenuOverlayDrawer.DrawChestPanels(menu);

    internal static void DrawCraftingPanels(CraftingMenu menu, PlayerEntity player)
        => _current.OnDrawCraftingPanels(menu, player);

    protected virtual void OnDrawCraftingPanels(CraftingMenu menu, PlayerEntity player)
        => MenuOverlayDrawer.DrawCraftingPanels(menu, player);

    internal static void DrawInventoryMenu(InventoryMenu menu)
        => _current.OnDrawInventoryMenu(menu);

    protected virtual void OnDrawInventoryMenu(InventoryMenu menu)
        => MenuOverlayDrawer.DrawInventoryMenu(menu);

    internal static int DrawItemList(List<InventorySlot> list, string panelName, int sel, int off,
        int x, int y, int sx, int sy, int my)
        => _current.OnDrawItemList(list, panelName, sel, off, x, y, sx, sy, my);

    protected virtual int OnDrawItemList(List<InventorySlot> list, string panelName, int sel, int off,
        int x, int y, int sx, int sy, int my)
        => MenuOverlayDrawer.DrawItemList(list, panelName, sel, off, x, y, sx, sy, my);

    internal static void DrawItemVisual(int x, int y, int col, int? power, ItemDef type)
        => _current.OnDrawItemVisual(x, y, col, power, type);

    protected virtual void OnDrawItemVisual(int x, int y, int col, int? power, ItemDef type)
        => MenuOverlayDrawer.DrawItemVisual(x, y, col, power, type);

    internal static int DrawListCore(int sel, int off, int x, int y, int sx, int sy,
        int my, int tlist, Action<int, int, int> renderRow)
        => _current.OnDrawListCore(sel, off, x, y, sx, sy, my, tlist, renderRow);

    protected virtual int OnDrawListCore(int sel, int off, int x, int y, int sx, int sy,
        int my, int tlist, Action<int, int, int> renderRow)
        => MenuOverlayDrawer.DrawListCore(sel, off, x, y, sx, sy, my, tlist, renderRow);

    internal static void DrawPanel(string name, int x, int y, int sx, int sy)
        => _current.OnDrawPanel(name, x, y, sx, sy);

    protected virtual void OnDrawPanel(string name, int x, int y, int sx, int sy)
        => MenuOverlayDrawer.DrawPanel(name, x, y, sx, sy);

    internal static void DrawRequireList(Recipe recip, int x, int y, int sx, int sy, PlayerEntity player)
        => _current.OnDrawRequireList(recip, x, y, sx, sy, player);

    protected virtual void OnDrawRequireList(Recipe recip, int x, int y, int sx, int sy, PlayerEntity player)
        => MenuOverlayDrawer.DrawRequireList(recip, x, y, sx, sy, player);

    internal static void ItemName(int x, int y, InventorySlot item, int col)
        => _current.OnItemName(x, y, item, col);

    protected virtual void OnItemName(int x, int y, InventorySlot item, int col)
        => MenuOverlayDrawer.ItemName(x, y, item, col);

    // ---------------------------------------------------------------------------
    #endregion
    #region Inventory
    // ---------------------------------------------------------------------------
    
    internal static void AddItemInList(List<InventorySlot> list, InventorySlot item, int pos)
        => _current.OnAddItemInList(list, item, pos);

    protected virtual void OnAddItemInList(List<InventorySlot> list, InventorySlot item, int pos)
        => InventoryOps.AddItemInList(list, item, pos);

    internal static InventorySlot? FindByType(List<InventorySlot> list, ItemDef type)
        => _current.OnFindByType(list, type);

    protected virtual InventorySlot? OnFindByType(List<InventorySlot> list, ItemDef type)
        => InventoryOps.FindByType(list, type);
    
    internal static StackableItem? FindStackable(List<InventorySlot> list, StackableItem query)
        => _current.OnFindStackable(list, query);

    protected virtual StackableItem? OnFindStackable(List<InventorySlot> list, StackableItem query)
        => InventoryOps.FindStackable(list, query);

    internal static int HowMany(List<InventorySlot> list, InventorySlot query)
        => _current.OnHowMany(list, query);

    protected virtual int OnHowMany(List<InventorySlot> list, InventorySlot query)
        => InventoryOps.HowMany(list, query);

    internal static int Loop(int sel, int count)
        => _current.OnLoop(sel, count);

    protected virtual int OnLoop(int sel, int count)
        => InventoryOps.Loop(sel, count);

    internal static void RemInList(List<InventorySlot> list, InventorySlot elem)
        => _current.OnRemInList(list, elem);

    protected virtual void OnRemInList(List<InventorySlot> list, InventorySlot elem)
        => InventoryOps.RemInList(list, elem);

    // ---------------------------------------------------------------------------
    #endregion
    #region Map - LevelManager
    // ---------------------------------------------------------------------------

    internal static void AddItem(ItemDef mat, int minCount, int maxCount, F32 hitX, F32 hitY, List<Entity> entities,
        F32? playerFacing = null, double dropChance = 1.0)
        => _current.OnAddItem(mat, minCount, maxCount, hitX, hitY, entities, playerFacing, dropChance);

    protected virtual void OnAddItem(ItemDef mat, int minCount, int maxCount, F32 hitX, F32 hitY, List<Entity> entities,
        F32? playerFacing = null, double dropChance = 1.0)
        => LevelManager.AddItem(mat, minCount, maxCount, hitX, hitY, entities, playerFacing, dropChance);

    internal static Level CreateLevel(int x, int y, int sx, int sy, LevelTheme theme, PlayerEntity player)
        => _current.OnCreateLevel(x, y, sx, sy, theme, player);

    protected virtual Level OnCreateLevel(int x, int y, int sx, int sy, LevelTheme theme, PlayerEntity player)
        => LevelManager.CreateLevel(x, y, sx, sy, theme, player);

    internal static void FillEne(Level level, PlayerEntity player)
        => _current.OnFillEne(level, player);

    protected virtual void OnFillEne(Level level, PlayerEntity player)
        => LevelManager.FillEne(level, player);

    internal static void ResetLevel(PlayerEntity player)
        => _current.OnResetLevel(player);

    protected virtual void OnResetLevel(PlayerEntity player)
        => LevelManager.ResetLevel(player);

    internal static void SetLevel(Level level, PlayerEntity player)
        => _current.OnSetLevel(level, player);

    protected virtual void OnSetLevel(Level level, PlayerEntity player)
        => LevelManager.SetLevel(level, player);

    internal static void UpGround(Level level, PlayerEntity player)
        => _current.OnUpGround(level, player);

    protected virtual void OnUpGround(Level level, PlayerEntity player)
        => LevelManager.UpGround(level, player);

    // ---------------------------------------------------------------------------
    #endregion
    #region Map - MapGenerator
    // ---------------------------------------------------------------------------

    internal static (int holeX, int holeY) CreateMap(Level level, PlayerEntity player)
        => _current.OnCreateMap(level, player);

    protected virtual (int holeX, int holeY) OnCreateMap(Level level, PlayerEntity player)
        => MapGenerator.CreateMap(level, player);

    internal static TileId[,] CreateMapStep(int sx, int sy, TileId a, TileId b, TileId c, TileId d, TileId e)
        => _current.OnCreateMapStep(sx, sy, a, b, c, d, e);

    protected virtual TileId[,] OnCreateMapStep(int sx, int sy, TileId a, TileId b, TileId c, TileId d, TileId e)
        => MapGenerator.CreateMapStep(sx, sy, a, b, c, d, e);

    internal static void CountTypes(TileId[,] tiles, int sx, int sy, int[] typecount)
        => _current.OnCountTypes(tiles, sx, sy, typecount);

    protected virtual void OnCountTypes(TileId[,] tiles, int sx, int sy, int[] typecount)
        => MapGenerator.CountTypes(tiles, sx, sy, typecount);

    internal static F32[][] InitRndWat()
        => _current.OnInitRndWat();

    protected virtual F32[][] OnInitRndWat()
        => MapGenerator.InitRndWat();

    internal static F32[,] Noise(int sx, int sy, F32 startScale, F32 scaleMod, int featStep)
        => _current.OnNoise(sx, sy, startScale, scaleMod, featStep);

    protected virtual F32[,] OnNoise(int sx, int sy, F32 startScale, F32 scaleMod, int featStep)
        => MapGenerator.Noise(sx, sy, startScale, scaleMod, featStep);

    // ---------------------------------------------------------------------------
    #endregion
    #region Map - MapOps
    // ---------------------------------------------------------------------------

    internal static Tile GetDirectTile(int i, int j, Level level)
        => _current.OnGetDirectTile(i, j, level);

    protected virtual Tile OnGetDirectTile(int i, int j, Level level)
        => MapOps.GetDirectTile(i, j, level);

    internal static Tile GetTile(F32 x, F32 y, Level level)
        => _current.OnGetTile(x, y, level);

    protected virtual Tile OnGetTile(F32 x, F32 y, Level level)
        => MapOps.GetTile(x, y, level);

    internal static (int i, int j) GetMCoord(F32 x, F32 y)
        => _current.OnGetMCoord(x, y);

    protected virtual (int i, int j) OnGetMCoord(F32 x, F32 y)
        => MapOps.GetMCoord(x, y);

    internal static bool IsFree(F32 x, F32 y, Level level)
        => _current.OnIsFree(x, y, level);

    protected virtual bool OnIsFree(F32 x, F32 y, Level level)
        => MapOps.IsFree(x, y, level);

    internal static bool IsFreeEnem(F32 x, F32 y, Level level)
        => _current.OnIsFreeEnem(x, y, level);

    protected virtual bool OnIsFreeEnem(F32 x, F32 y, Level level)
        => MapOps.IsFreeEnem(x, y, level);

    internal static bool OutOfBounds(int i, int j, Level level)
        => _current.OnOutOfBounds(i, j, level);

    protected virtual bool OnOutOfBounds(int i, int j, Level level)
        => MapOps.OutOfBounds(i, j, level);

    internal static void SetTile(F32 x, F32 y, Tile tile, Level level)
        => _current.OnSetTile(x, y, tile, level);

    protected virtual void OnSetTile(F32 x, F32 y, Tile tile, Level level)
        => MapOps.SetTile(x, y, tile, level);

    // ---------------------------------------------------------------------------
    #endregion
    #region Math
    // ---------------------------------------------------------------------------

    internal static F32 GetLen(F32 x, F32 y)
        => _current.OnGetLen(x, y);

    protected virtual F32 OnGetLen(F32 x, F32 y)
        => PcraftMath.GetLen(x, y);

    internal static F32 GetInvLen(F32 x, F32 y)
        => _current.OnGetInvLen(x, y);

    protected virtual F32 OnGetInvLen(F32 x, F32 y)
        => PcraftMath.GetInvLen(x, y);

    internal static F32 GetRot(F32 dx, F32 dy)
        => _current.OnGetRot(dx, dy);

    protected virtual F32 OnGetRot(F32 dx, F32 dy)
        => PcraftMath.GetRot(dx, dy);

    internal static F32 NormGetRot(F32 dx, F32 dy)
        => _current.OnNormGetRot(dx, dy);

    protected virtual F32 OnNormGetRot(F32 dx, F32 dy)
        => PcraftMath.NormGetRot(dx, dy);

    internal static F32 UpRot(F32 grot, F32 rot)
        => _current.OnUpRot(grot, rot);

    protected virtual F32 OnUpRot(F32 grot, F32 rot)
        => PcraftMath.UpRot(grot, rot);

    internal static (int flipX, int flipY) Mirror(F32 rot)
        => _current.OnMirror(rot);

    protected virtual (int flipX, int flipY) OnMirror(F32 rot)
        => PcraftMath.Mirror(rot);

    // ---------------------------------------------------------------------------
    #endregion
    #region Physics
    // ---------------------------------------------------------------------------

    internal static bool EntColFree(F32 x, F32 y, Entity e)
        => _current.OnEntColFree(x, y, e);

    protected virtual bool OnEntColFree(F32 x, F32 y, Entity e)
        => CollisionSystem.EntColFree(x, y, e);

    internal static bool IsIn(Entity e, F32 size, F32 clx, F32 cly)
        => _current.OnIsIn(e, size, clx, cly);

    protected virtual bool OnIsIn(Entity e, F32 size, F32 clx, F32 cly)
        => CollisionSystem.IsIn(e, size, clx, cly);

    internal static (F32 dx, F32 dy) ReflectCol(F32 x, F32 y, F32 dx, F32 dy,
        Func<F32, F32, bool> check, F32 dp, Entity? e = null)
        => _current.OnReflectCol(x, y, dx, dy, check, dp, e);

    protected virtual (F32 dx, F32 dy) OnReflectCol(F32 x, F32 y, F32 dx, F32 dy,
        Func<F32, F32, bool> check, F32 dp, Entity? e = null)
        => CollisionSystem.ReflectCol(x, y, dx, dy, check, dp, e);

    // ---------------------------------------------------------------------------
    #endregion
    #region Update - Camera
    // ---------------------------------------------------------------------------

    internal static void UpdateCamera(PlayerEntity player, F32 dx, F32 dy)
        => _current.OnUpdateCamera(player, dx, dy);

    protected virtual void OnUpdateCamera(PlayerEntity player, F32 dx, F32 dy)
        => CameraUpdater.Update(player, dx, dy);

    // ---------------------------------------------------------------------------
    #endregion
    #region Update - Enemies
    // ---------------------------------------------------------------------------

    internal static List<CharacterEntity> UpdateEnemies(PlayerEntity player, Level level)
        => _current.OnUpdateEnemies(player, level);

    protected virtual List<CharacterEntity> OnUpdateEnemies(PlayerEntity player, Level level)
        => EnemyUpdater.Update(player, level);

    // ---------------------------------------------------------------------------
    #endregion
    #region Update - Entities
    // ---------------------------------------------------------------------------

    internal static (F32 dx, F32 dy, bool canAct) UpdateEntities(PlayerEntity player, Level level, F32 dx, F32 dy)
        => _current.OnUpdateEntities(player, level, dx, dy);

    protected virtual (F32 dx, F32 dy, bool canAct) OnUpdateEntities(PlayerEntity player, Level level, F32 dx, F32 dy)
        => EntityUpdater.Update(player, level, dx, dy);

    // ---------------------------------------------------------------------------
    #endregion
    #region Update - Main
    // ---------------------------------------------------------------------------

    internal static void UpdateMain(PlayerEntity player)
        => _current.OnUpdateMain(player);

    protected virtual void OnUpdateMain(PlayerEntity player)
        => PcraftUpdate.Update(player);

    // ---------------------------------------------------------------------------
    #endregion
    #region Update - Menu
    // ---------------------------------------------------------------------------

    internal static (bool consumed, bool needsReset) UpdateMenu(PlayerEntity player)
        => _current.OnUpdateMenu(player);

    protected virtual (bool consumed, bool needsReset) OnUpdateMenu(PlayerEntity player)
        => MenuUpdater.Update(player);

    // ---------------------------------------------------------------------------
    #endregion
    #region Update - Player
    // ---------------------------------------------------------------------------

    internal static void UpdatePlayer(PlayerEntity player,
        F32 dx, F32 dy, bool canAct, List<CharacterEntity> nearEnemies)
        => _current.OnUpdatePlayer(player, dx, dy, canAct, nearEnemies);

    protected virtual void OnUpdatePlayer(PlayerEntity player,
        F32 dx, F32 dy, bool canAct, List<CharacterEntity> nearEnemies)
        => PlayerActionUpdater.Update(player, dx, dy, canAct, nearEnemies);

    // ---------------------------------------------------------------------------
    #endregion
}
