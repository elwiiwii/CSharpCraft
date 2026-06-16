using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Menu;
using PSharp8.Graphics;
using PSharp8.Input;

namespace CSharpCraft.PcraftBase.Update;

internal static class EntityUpdater
{
    internal static (F32 dx, F32 dy, bool canAct) Update(PlayerEntity player, Level level, F32 dx, F32 dy)
    {
        bool canAct = true;

        int fin = level.Ent.Count;
        for (int i = fin - 1; i >= 0; i--)
        {
            Entity e = level.Ent[i];

            // Physics: wall-bounce for dropped + placed items
            if (e is DroppedItemEntity or PlacedItemEntity)
            {
                (e.Vx, e.Vy) = PcraftServices.ReflectCol(
                    e.X, e.Y, e.Vx, e.Vy,
                    (x2, y2) => PcraftServices.IsFree(x2, y2, level),
                    F32.FromDouble(0.90));
            }

            e.X += e.Vx;
            e.Y += e.Vy;
            e.Vx *= F32.FromDouble(0.95);
            e.Vy *= F32.FromDouble(0.95);

            F32 dist = F32.Max(F32.Abs(e.X - player.X), F32.Abs(e.Y - player.Y));

            switch (e)
            {
                // Per-entity behaviour
                // Expiry
                case DroppedItemEntity dropped when dropped.Timer < F32.One:
                    level.Ent.RemoveAt(i);
                    continue;
                case DroppedItemEntity dropped:
                {
                    dropped.Timer -= F32.One;

                    // Pickup: only when close enough and timer has counted down
                    if (dist < F32.FromInt(5) && dropped.Timer < F32.FromInt(115))
                    {
                        StackableItem newit = new(dropped.Type, 1);
                        PcraftServices.AddItemInList(player.Invent, newit, player.Invent.Count);
                        level.Ent.RemoveAt(i);
                        TextPopupEntity popup = new(
                            F32.FromInt(PcraftServices.HowMany(player.Invent, newit)),
                            PicoColor._11Green,
                            e.X, e.Y - F32.FromInt(5), -F32.One);
                        level.Ent.Add(popup);
                        Pico8.Sfx(18);
                    }

                    break;
                }
                case PlacedItemEntity placed:
                {
                    // Player push-back
                    (dx, dy) = PcraftServices.ReflectCol(
                        player.X, player.Y, dx, dy,
                        (fx, fy) => PcraftServices.EntColFree(fx, fy, placed),
                        F32.Zero);

                    // Btn5 interaction
                    if (dist < F32.FromInt(12) && Pico8.Btnp(PicoButton.Primary, repeat: false) && !player.Block5)
                    {
                        if (player.CurItem is not null && player.CurItem.Type == PcraftData.PickupTool)
                        {
                            UnstackableItem asSlot = new(placed.Type);
                            PcraftServices.AddItemInList(player.Invent, asSlot, 0);
                            player.CurItem = asSlot;
                            level.Ent.RemoveAt(i);
                        }
                        else
                        {
                            player.CurMenu = placed.Type is BenchItemDef bench
                                ? new CraftingMenu(bench, player.Invent)
                                : new ChestMenu([], player.Invent);
                            Pico8.Sfx(13);
                        }
                        canAct = false;
                    }

                    break;
                }
                // Expiry
                case TextPopupEntity textPopup when textPopup.Timer < F32.One:
                    level.Ent.RemoveAt(i);
                    continue;
                case TextPopupEntity textPopup:
                    textPopup.Timer -= F32.One;
                    break;
            }
        }

        return (dx, dy, canAct);
    }
}
