using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Menu;

namespace CSharpCraft.PcraftBase.Update;

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
                (e.Vx, e.Vy) = PcraftServices.ReflectCol(
                    e.X, e.Y, e.Vx, e.Vy,
                    (x2, y2) => PcraftServices.IsFree(x2, y2, state),
                    F32.FromDouble(0.90));
            }

            e.X  += e.Vx;
            e.Y  += e.Vy;
            e.Vx *= F32.FromDouble(0.95);
            e.Vy *= F32.FromDouble(0.95);

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
                    PcraftServices.AddItemInList(state.Invent, newit, -1);
                    state.Entities.RemoveAt(i);
                    var popup = new ItemEntity(PcraftData.EText, e.X, e.Y - F32.FromInt(5), F32.Zero, -F32.One);
                    popup.TextValue = F32.FromInt(PcraftServices.HowMany(state.Invent, newit));
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
                    (dx, dy) = PcraftServices.ReflectCol(
                        state.Plx, state.Ply, dx, dy,
                        (fx, fy) => PcraftServices.EntColFree(fx, fy, e),
                        F32.Zero);
                }

                if (dist < F32.FromInt(12) && Pico8.Btn(5) && !state.Block5 && !state.Lb5)
                {
                    if (state.CurItem != null && state.CurItem.Type == PcraftData.PickupTool)
                    {
                        if (e.Type == PcraftData.Chest || e.Type.BeCraft)
                        {
                            var asStack = new ItemStack(e.Type, list: e.List);
                            PcraftServices.AddItemInList(state.Invent, asStack, 0);
                            state.CurItem = asStack;
                            state.Entities.RemoveAt(i);
                        }
                        canAct = false;
                    }
                    else
                    {
                        if (e.Type == PcraftData.Chest || e.Type.BeCraft)
                        {
                            if (e.Type.BeCraft)
                                state.CurMenu = new CraftingMenu(e.Type, e.List ?? [], state.Invent);
                            else
                                state.CurMenu = new ChestMenu([], state.Invent);
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
