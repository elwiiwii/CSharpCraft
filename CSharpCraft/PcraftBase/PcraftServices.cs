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
    private static PcraftServices _current = new PcraftServices();
    internal static void SetServices(PcraftServices s) => _current = s;

    #region Crafting
    // ---------------------------------------------------------------------------

    internal static bool CanCraft(List<ItemStack> invent, Recipe recipe)
        => _current.OnCanCraft(invent, recipe);

    protected virtual bool OnCanCraft(List<ItemStack> invent, Recipe recipe)
        => CraftingSystem.CanCraft(invent, recipe);

    internal static void Craft(List<ItemStack> invent, Recipe recipe)
        => _current.OnCraft(invent, recipe);

    protected virtual void OnCraft(List<ItemStack> invent, Recipe recipe)
        => CraftingSystem.Craft(invent, recipe);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Background
    // ---------------------------------------------------------------------------

    internal static bool Comp(int i, int j, GroundType gr, WorldState state)
        => _current.OnComp(i, j, gr, state);

    protected virtual bool OnComp(int i, int j, GroundType gr, WorldState state)
        => BackDrawer.Comp(i, j, gr, state);

    internal static int CornerOffset(bool sideH, bool sideV, bool diag, int rnd,
        int innerCorner, int hOnly, int vOnly, int outer)
        => _current.OnCornerOffset(sideH, sideV, diag, rnd, innerCorner, hOnly, vOnly, outer);

    protected virtual int OnCornerOffset(bool sideH, bool sideV, bool diag, int rnd,
        int innerCorner, int hOnly, int vOnly, int outer)
        => BackDrawer.CornerOffset(sideH, sideV, diag, rnd, innerCorner, hOnly, vOnly, outer);

    internal static void DrawBack(WorldState state)
        => _current.OnDrawBack(state);

    protected virtual void OnDrawBack(WorldState state)
        => BackDrawer.DrawBack(state);

    internal static int RndCenter(double i, double j, WorldState state)
        => _current.OnRndCenter(i, j, state);

    protected virtual int OnRndCenter(double i, double j, WorldState state)
        => BackDrawer.RndCenter(i, j, state);

    internal static int RndSand(double i, double j, WorldState state)
        => _current.OnRndSand(i, j, state);

    protected virtual int OnRndSand(double i, double j, WorldState state)
        => BackDrawer.RndSand(i, j, state);

    internal static int RndTree(double i, double j, WorldState state)
        => _current.OnRndTree(i, j, state);

    protected virtual int OnRndTree(double i, double j, WorldState state)
        => BackDrawer.RndTree(i, j, state);

    internal static void Spr4(double i, double j, int gi, int gj,
        int a, int b, int c, int d, int off, Func<double, double, int> f)
        => _current.OnSpr4(i, j, gi, gj, a, b, c, d, off, f);

    protected virtual void OnSpr4(double i, double j, int gi, int gj,
        int a, int b, int c, int d, int off, Func<double, double, int> f)
        => BackDrawer.Spr4(i, j, gi, gj, a, b, c, d, off, f);

    internal static void WatAnim(double i, double j, WorldState state)
        => _current.OnWatAnim(i, j, state);

    protected virtual void OnWatAnim(double i, double j, WorldState state)
        => BackDrawer.WatAnim(i, j, state);

    internal static F32 WatVal(double i, double j, WorldState state)
        => _current.OnWatVal(i, j, state);

    protected virtual F32 OnWatVal(double i, double j, WorldState state)
        => BackDrawer.WatVal(i, j, state);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Enemies
    // ---------------------------------------------------------------------------

    internal static void DrawEnemies(WorldState state)
        => _current.OnDrawEnemies(state);

    protected virtual void OnDrawEnemies(WorldState state)
        => EnemiesDrawer.DrawEnemies(state);

    internal static void DrawPlayer(F32 x, F32 y, F32 rot, F32 anim, F32 subAnim, bool isPlayer, WorldState state)
        => _current.OnDrawPlayer(x, y, rot, anim, subAnim, isPlayer, state);

    protected virtual void OnDrawPlayer(F32 x, F32 y, F32 rot, F32 anim, F32 subAnim, bool isPlayer, WorldState state)
        => EnemiesDrawer.DrawPlayer(x, y, rot, anim, subAnim, isPlayer, state);

    internal static void SortY(List<CharacterEntity> enemies)
        => _current.OnSortY(enemies);

    protected virtual void OnSortY(List<CharacterEntity> enemies)
        => EnemiesDrawer.SortY(enemies);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Entities
    // ---------------------------------------------------------------------------

    internal static void DrawEnt(WorldState state)
        => _current.OnDrawEnt(state);

    protected virtual void OnDrawEnt(WorldState state)
        => EntDrawer.DrawEnt(state);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Helpers
    // ---------------------------------------------------------------------------

    internal static void SetPal(int[] l)
        => _current.OnSetPal(l);

    protected virtual void OnSetPal(int[] l)
        => DrawHelpers.SetPal(l);

    internal static void PrintB(string t, double x, double y, double c)
        => _current.OnPrintB(t, x, y, c);

    protected virtual void OnPrintB(string t, double x, double y, double c)
        => DrawHelpers.PrintB(t, x, y, c);

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

    internal static void DrawHud(WorldState state)
        => _current.OnDrawHud(state);

    protected virtual void OnDrawHud(WorldState state)
        => HudDrawer.DrawHud(state);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Main
    // ---------------------------------------------------------------------------

    internal static void DrawMain(WorldState state, PcraftGame game)
        => _current.OnDrawMain(state, game);

    protected virtual void OnDrawMain(WorldState state, PcraftGame game)
        => PcraftDraw.DrawMain(state, game);

    // ---------------------------------------------------------------------------
    #endregion
    #region Draw - Menu
    // ---------------------------------------------------------------------------

    internal static void DrawChestPanels(ChestMenu menu)
        => _current.OnDrawChestPanels(menu);

    protected virtual void OnDrawChestPanels(ChestMenu menu)
        => MenuOverlayDrawer.DrawChestPanels(menu);

    internal static void DrawCraftingPanels(CraftingMenu menu, WorldState state)
        => _current.OnDrawCraftingPanels(menu, state);

    protected virtual void OnDrawCraftingPanels(CraftingMenu menu, WorldState state)
        => MenuOverlayDrawer.DrawCraftingPanels(menu, state);

    internal static void DrawInventoryMenu(InventoryMenu menu)
        => _current.OnDrawInventoryMenu(menu);

    protected virtual void OnDrawInventoryMenu(InventoryMenu menu)
        => MenuOverlayDrawer.DrawInventoryMenu(menu);

    internal static int DrawItemList(List<ItemStack> list, string panelName, int sel, int off,
        int x, int y, int sx, int sy, int my)
        => _current.OnDrawItemList(list, panelName, sel, off, x, y, sx, sy, my);

    protected virtual int OnDrawItemList(List<ItemStack> list, string panelName, int sel, int off,
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

    internal static void DrawRequireList(Recipe recip, int x, int y, int sx, int sy, WorldState state)
        => _current.OnDrawRequireList(recip, x, y, sx, sy, state);

    protected virtual void OnDrawRequireList(Recipe recip, int x, int y, int sx, int sy, WorldState state)
        => MenuOverlayDrawer.DrawRequireList(recip, x, y, sx, sy, state);

    internal static void ItemName(int x, int y, ItemStack item, int col)
        => _current.OnItemName(x, y, item, col);

    protected virtual void OnItemName(int x, int y, ItemStack item, int col)
        => MenuOverlayDrawer.ItemName(x, y, item, col);

    // ---------------------------------------------------------------------------
    #endregion
    #region Inventory
    // ---------------------------------------------------------------------------

    internal static void AddItemInList(List<ItemStack> list, ItemStack item, int pos)
        => _current.OnAddItemInList(list, item, pos);

    protected virtual void OnAddItemInList(List<ItemStack> list, ItemStack item, int pos)
        => InventoryOps.AddItemInList(list, item, pos);

    internal static void AddPlace(List<ItemStack> list, ItemStack item, int pos)
        => _current.OnAddPlace(list, item, pos);

    protected virtual void OnAddPlace(List<ItemStack> list, ItemStack item, int pos)
        => InventoryOps.AddPlace(list, item, pos);

    internal static int HowMany(List<ItemStack> list, ItemStack query)
        => _current.OnHowMany(list, query);

    protected virtual int OnHowMany(List<ItemStack> list, ItemStack query)
        => InventoryOps.HowMany(list, query);

    internal static ItemStack? IsInList(List<ItemStack> list, ItemStack query)
        => _current.OnIsInList(list, query);

    protected virtual ItemStack? OnIsInList(List<ItemStack> list, ItemStack query)
        => InventoryOps.IsInList(list, query);

    internal static int Loop(int sel, int count)
        => _current.OnLoop(sel, count);

    protected virtual int OnLoop(int sel, int count)
        => InventoryOps.Loop(sel, count);

    internal static void RemInList(List<ItemStack> list, ItemStack elem)
        => _current.OnRemInList(list, elem);

    protected virtual void OnRemInList(List<ItemStack> list, ItemStack elem)
        => InventoryOps.RemInList(list, elem);

    // ---------------------------------------------------------------------------
    #endregion
    #region Map - LevelManager
    // ---------------------------------------------------------------------------

    internal static void AddItem(ItemDef mat, int count, F32 hitX, F32 hitY, List<ItemEntity> entities)
        => _current.OnAddItem(mat, count, hitX, hitY, entities);

    protected virtual void OnAddItem(ItemDef mat, int count, F32 hitX, F32 hitY, List<ItemEntity> entities)
        => LevelManager.AddItem(mat, count, hitX, hitY, entities);

    internal static Level CreateLevel(int x, int y, int sx, int sy, bool isUnder, WorldState state)
        => _current.OnCreateLevel(x, y, sx, sy, isUnder, state);

    protected virtual Level OnCreateLevel(int x, int y, int sx, int sy, bool isUnder, WorldState state)
        => LevelManager.CreateLevel(x, y, sx, sy, isUnder, state);

    internal static void FillEne(Level level, WorldState state)
        => _current.OnFillEne(level, state);

    protected virtual void OnFillEne(Level level, WorldState state)
        => LevelManager.FillEne(level, state);

    internal static void ResetLevel(WorldState state, PcraftGame game)
        => _current.OnResetLevel(state, game);

    protected virtual void OnResetLevel(WorldState state, PcraftGame game)
        => LevelManager.ResetLevel(state, game);

    internal static void SetLevel(Level level, WorldState state)
        => _current.OnSetLevel(level, state);

    protected virtual void OnSetLevel(Level level, WorldState state)
        => LevelManager.SetLevel(level, state);

    internal static void UpGround(WorldState state)
        => _current.OnUpGround(state);

    protected virtual void OnUpGround(WorldState state)
        => LevelManager.UpGround(state);

    // ---------------------------------------------------------------------------
    #endregion
    #region Map - MapGenerator
    // ---------------------------------------------------------------------------

    internal static (int holeX, int holeY) CreateMap(WorldState state)
        => _current.OnCreateMap(state);

    protected virtual (int holeX, int holeY) OnCreateMap(WorldState state)
        => MapGenerator.CreateMap(state);

    internal static int[,] CreateMapStep(int sx, int sy, int a, int b, int c, int d, int e)
        => _current.OnCreateMapStep(sx, sy, a, b, c, d, e);

    protected virtual int[,] OnCreateMapStep(int sx, int sy, int a, int b, int c, int d, int e)
        => MapGenerator.CreateMapStep(sx, sy, a, b, c, d, e);

    internal static void CountTypes(int[,] tiles, int sx, int sy, int[] typecount)
        => _current.OnCountTypes(tiles, sx, sy, typecount);

    protected virtual void OnCountTypes(int[,] tiles, int sx, int sy, int[] typecount)
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

    internal static void ClearData(F32 x, F32 y, WorldState state)
        => _current.OnClearData(x, y, state);

    protected virtual void OnClearData(F32 x, F32 y, WorldState state)
        => MapOps.ClearData(x, y, state);

    internal static F32 DirGetData(int i, int j, F32 def, WorldState state)
        => _current.OnDirGetData(i, j, def, state);

    protected virtual F32 OnDirGetData(int i, int j, F32 def, WorldState state)
        => MapOps.DirGetData(i, j, def, state);

    internal static void DirSetData(int i, int j, F32 v, WorldState state)
        => _current.OnDirSetData(i, j, v, state);

    protected virtual void OnDirSetData(int i, int j, F32 v, WorldState state)
        => MapOps.DirSetData(i, j, v, state);

    internal static F32 GetData(F32 x, F32 y, F32 def, WorldState state)
        => _current.OnGetData(x, y, def, state);

    protected virtual F32 OnGetData(F32 x, F32 y, F32 def, WorldState state)
        => MapOps.GetData(x, y, def, state);

    internal static GroundType GetDirectGr(int i, int j, WorldState state)
        => _current.OnGetDirectGr(i, j, state);

    protected virtual GroundType OnGetDirectGr(int i, int j, WorldState state)
        => MapOps.GetDirectGr(i, j, state);

    internal static GroundType GetGr(F32 x, F32 y, WorldState state)
        => _current.OnGetGr(x, y, state);

    protected virtual GroundType OnGetGr(F32 x, F32 y, WorldState state)
        => MapOps.GetGr(x, y, state);

    internal static (int i, int j) GetMCoord(F32 x, F32 y)
        => _current.OnGetMCoord(x, y);

    protected virtual (int i, int j) OnGetMCoord(F32 x, F32 y)
        => MapOps.GetMCoord(x, y);

    internal static bool IsCool(F32 x, F32 y, WorldState state)
        => _current.OnIsCool(x, y, state);

    protected virtual bool OnIsCool(F32 x, F32 y, WorldState state)
        => MapOps.IsCool(x, y, state);

    internal static bool IsFree(F32 x, F32 y, WorldState state)
        => _current.OnIsFree(x, y, state);

    protected virtual bool OnIsFree(F32 x, F32 y, WorldState state)
        => MapOps.IsFree(x, y, state);

    internal static bool IsFreeEnem(F32 x, F32 y, WorldState state)
        => _current.OnIsFreeEnem(x, y, state);

    protected virtual bool OnIsFreeEnem(F32 x, F32 y, WorldState state)
        => MapOps.IsFreeEnem(x, y, state);

    internal static bool OutOfBounds(int i, int j, WorldState state)
        => _current.OnOutOfBounds(i, j, state);

    protected virtual bool OnOutOfBounds(int i, int j, WorldState state)
        => MapOps.OutOfBounds(i, j, state);

    internal static void SetData(F32 x, F32 y, F32 v, WorldState state)
        => _current.OnSetData(x, y, v, state);

    protected virtual void OnSetData(F32 x, F32 y, F32 v, WorldState state)
        => MapOps.SetData(x, y, v, state);

    internal static void SetGr(F32 x, F32 y, GroundType v, WorldState state)
        => _current.OnSetGr(x, y, v, state);

    protected virtual void OnSetGr(F32 x, F32 y, GroundType v, WorldState state)
        => MapOps.SetGr(x, y, v, state);

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

    internal static void UpdateCamera(WorldState state, F32 dx, F32 dy)
        => _current.OnUpdateCamera(state, dx, dy);

    protected virtual void OnUpdateCamera(WorldState state, F32 dx, F32 dy)
        => CameraUpdater.Update(state, dx, dy);

    // ---------------------------------------------------------------------------
    #endregion
    #region Update - Enemies
    // ---------------------------------------------------------------------------

    internal static void UpdateEnemies(WorldState state)
        => _current.OnUpdateEnemies(state);

    protected virtual void OnUpdateEnemies(WorldState state)
        => EnemyUpdater.Update(state);

    // ---------------------------------------------------------------------------
    #endregion
    #region Update - Entities
    // ---------------------------------------------------------------------------

    internal static (F32 dx, F32 dy, bool canAct) UpdateEntities(WorldState state, F32 dx, F32 dy)
        => _current.OnUpdateEntities(state, dx, dy);

    protected virtual (F32 dx, F32 dy, bool canAct) OnUpdateEntities(WorldState state, F32 dx, F32 dy)
        => EntityUpdater.Update(state, dx, dy);

    // ---------------------------------------------------------------------------
    #endregion
    #region Update - Main
    // ---------------------------------------------------------------------------

    internal static void UpdateMain(WorldState state, PcraftGame game)
        => _current.OnUpdateMain(state, game);

    protected virtual void OnUpdateMain(WorldState state, PcraftGame game)
        => PcraftUpdate.Update(state, game);

    // ---------------------------------------------------------------------------
    #endregion
    #region Update - Menu
    // ---------------------------------------------------------------------------

    internal static bool UpdateMenu(WorldState state, PcraftGame game)
        => _current.OnUpdateMenu(state, game);

    protected virtual bool OnUpdateMenu(WorldState state, PcraftGame game)
        => MenuUpdater.Update(state, game);

    // ---------------------------------------------------------------------------
    #endregion
    #region Update - Player
    // ---------------------------------------------------------------------------

    internal static void UpdatePlayer(WorldState state, PcraftGame game,
        F32 dx, F32 dy, bool canAct)
        => _current.OnUpdatePlayer(state, game, dx, dy, canAct);

    protected virtual void OnUpdatePlayer(WorldState state, PcraftGame game,
        F32 dx, F32 dy, bool canAct)
        => PlayerActionUpdater.Update(state, game, dx, dy, canAct);

    // ---------------------------------------------------------------------------
    #endregion
}
