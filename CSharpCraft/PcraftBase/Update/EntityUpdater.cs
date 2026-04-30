using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Menu;

namespace CSharpCraft.PcraftBase.Update;

internal static class EntityUpdater
{
    internal static (F32 dx, F32 dy, bool canAct) Update(PlayerEntity player, Level level, F32 dx, F32 dy)
    {
        bool canAct = true;

        int fin = level.Ent.Count;
        for (int i = fin - 1; i >= 0; i--)
        {
            var e = level.Ent[i];

            if (e.HasCol)
            {
                (e.Vx, e.Vy) = PcraftServices.ReflectCol(
                    e.X, e.Y, e.Vx, e.Vy,
                    (x2, y2) => PcraftServices.IsFree(x2, y2, level),
                    F32.FromDouble(0.90));
            }

            e.X  += e.Vx;
            e.Y  += e.Vy;
            e.Vx *= F32.FromDouble(0.95);
            e.Vy *= F32.FromDouble(0.95);

            if (e.Timer != null && e.Timer.Value < F32.One)
            {
                level.Ent.RemoveAt(i);
                continue;
            }

            if (e.Timer != null)
                e.Timer = e.Timer.Value - F32.One;

            var dist = F32.Max(F32.Abs(e.X - player.X), F32.Abs(e.Y - player.Y));

            if (e.GiveItem != null)
            {
                if (dist < F32.FromInt(5) && (e.Timer == null || e.Timer.Value < F32.FromInt(115)))
                {
                    var newit = new ItemStack(e.GiveItem, count: 1);
                    PcraftServices.AddItemInList(player.Invent, newit, -1);
                    level.Ent.RemoveAt(i);
                    var popup = new ItemEntity(PcraftData.EText, e.X, e.Y - F32.FromInt(5), F32.Zero, -F32.One);
                    popup.TextValue = F32.FromInt(PcraftServices.HowMany(player.Invent, newit));
                    popup.TextColor = 11;
                    popup.Timer     = F32.FromInt(20);
                    level.Ent.Add(popup);
                    Pico8.Sfx(18);
                }
            }
            else
            {
                if (e.HasCol)
                {
                    (dx, dy) = PcraftServices.ReflectCol(
                        player.X, player.Y, dx, dy,
                        (fx, fy) => PcraftServices.EntColFree(fx, fy, e),
                        F32.Zero);
                }

                if (dist < F32.FromInt(12) && Pico8.Btn(5) && !player.Block5 && !player.Lb5)
                {
                    if (player.CurItem != null && player.CurItem.Type == PcraftData.PickupTool)
                    {
                        if (e.Type is PlaceableItemDef)
                        {
                            var asStack = new ItemStack(e.Type);
                            PcraftServices.AddItemInList(player.Invent, asStack, 0);
                            player.CurItem = asStack;
                            level.Ent.RemoveAt(i);
                        }
                        canAct = false;
                    }
                    else
                    {
                        if (e.Type is PlaceableItemDef)
                        {
                            if (e.Type is BenchItemDef bench)
                                player.CurMenu = new CraftingMenu(bench, player.Invent);
                            else
                                player.CurMenu = new ChestMenu([], player.Invent);
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
