using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Inventory;
using CSharpCraft.Pcraft.Map;
using CSharpCraft.Pcraft.Physics;
using PSharp8;

namespace CSharpCraft.Pcraft.Update;

internal static class EntityUpdater
{
    internal static (F32 dx, F32 dy, bool canAct) Update(WorldState state, F32 dx, F32 dy)
    {
        bool canAct = true;

        int fin = state.Entities.Count;
        for (int i = fin - 1; i >= 0; i--)
        {
            var e = state.Entities[i];

            if (e.HasCol)
            {
                (e.Vx, e.Vy) = CollisionSystem.ReflectCol(
                    e.X, e.Y, e.Vx, e.Vy,
                    (x2, y2) => MapOps.IsFree(x2, y2, state),
                    F32.FromFloat(0.9f));
            }

            e.X  += e.Vx;
            e.Y  += e.Vy;
            e.Vx *= F32.FromFloat(0.95f);
            e.Vy *= F32.FromFloat(0.95f);

            if (e.Timer != null && e.Timer.Value < F32.One)
            {
                state.Entities.RemoveAt(i);
                continue;
            }

            if (e.Timer != null)
                e.Timer = e.Timer.Value - F32.One;

            var dist = F32.Max(F32.Abs(e.X - state.Plx), F32.Abs(e.Y - state.Ply));

            if (e.GiveItem != null)
            {
                if (dist < F32.FromInt(5) && (e.Timer == null || e.Timer.Value < F32.FromInt(115)))
                {
                    var newit = new ItemStack(e.GiveItem, count: 1);
                    InventoryOps.AddItemInList(state.Invent, newit, -1);
                    state.Entities.RemoveAt(i);
                    var popup = new ItemEntity(PcraftData.EText, e.X, e.Y - F32.FromInt(5), F32.Zero, -F32.One);
                    popup.TextValue = F32.FromInt(InventoryOps.HowMany(state.Invent, newit));
                    popup.TextColor = 11;
                    popup.Timer     = F32.FromInt(20);
                    state.Entities.Add(popup);
                    Pico8.Sfx(18);
                }
            }
            else
            {
                if (e.HasCol)
                {
                    (dx, dy) = CollisionSystem.ReflectCol(
                        state.Plx, state.Ply, dx, dy,
                        (fx, fy) => CollisionSystem.EntColFree(fx, fy, e),
                        F32.Zero);
                }

                if (dist < F32.FromInt(12) && Pico8.Btn(5) && !state.Block5 && !state.Lb5)
                {
                    if (state.CurItem != null && state.CurItem.Type == PcraftData.PickupTool)
                    {
                        if (e.Type == PcraftData.Chest || e.Type.BeCraft)
                        {
                            var asStack = new ItemStack(e.Type, list: e.List);
                            InventoryOps.AddItemInList(state.Invent, asStack, 0);
                            state.CurItem = asStack;
                            state.Entities.RemoveAt(i);
                        }
                        canAct = false;
                    }
                    else
                    {
                        if (e.Type == PcraftData.Chest || e.Type.BeCraft)
                        {
                            state.ToogleMenu = 0;
                            state.CurMenu = new MenuState(e.Type, null, spr: 0, text: null, text2: null);
                            if (e.Type.BeCraft)
                                state.CurMenu.RecipeList = e.List;
                            Pico8.Sfx(13);
                        }
                        canAct = false;
                    }
                }
            }
        }

        return (dx, dy, canAct);
    }
}
