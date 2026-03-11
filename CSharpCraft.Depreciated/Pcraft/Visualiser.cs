using CSharpCraft.Pico8;
using FixMath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft.Pcraft;

public class Visualiser : PcraftBase
{
    public override string SceneName => "visualiser";

    private (F32 x, F32 y) spawnCenter;

    public override void Init(Pico8Functions pico8)
    {
        base.Init(pico8);
    }

    public void DrawZombieSpawnArea()
    {
        Vector2 playerWorld = new Vector2(spawnCenter.x.Float, spawnCenter.y.Float);

        const float spawnMargin = 50f;
        float screenMarginX = spawnMargin * p8.Cell.Width;
        float screenMarginY = spawnMargin * p8.Cell.Height;

        Vector2 playerScreen = WorldToScreen(playerWorld);
        Rectangle spawnArea = new Rectangle(
            (int)(playerScreen.X - screenMarginX),
            (int)(playerScreen.Y - screenMarginY),
            (int)(screenMarginX * 2),
            (int)(screenMarginY * 2)
        );

        Color spawnColor = p8.Colors[14];
        DrawRectOutline(
            new Vector2(spawnArea.X, spawnArea.Y),
            spawnArea.Width,
            spawnArea.Height,
            spawnColor,
            thickness: 0.4f * p8.Cell.Height
        );
    }

    public void DrawCameraBounds()
    {
        const float boundarySize = 32f;

        Vector2 targetCenterWorld = new Vector2(cmx.Float, cmy.Float);
        Vector2 targetCenterScreen = WorldToScreen(targetCenterWorld);

        float screenWidth = boundarySize * p8.Cell.Width;
        float screenHeight = boundarySize * p8.Cell.Height;

        DrawRectOutline(
            targetCenterScreen - new Vector2(screenWidth / 2, screenHeight / 2),
            screenWidth,
            screenHeight,
            p8.Colors[7],
            thickness: 0.4f * p8.Cell.Height
        );

        Vector2 currentCamWorld = new Vector2(clx.Float, cly.Float);
        Vector2 currentCamScreen = WorldToScreen(currentCamWorld);

        float camMarkerSize = 1 * p8.Cell.Height;
        p8.Batch.Draw(
            p8.Pixel,
            currentCamScreen - new Vector2(camMarkerSize / 2, camMarkerSize / 2),
            null,
            p8.Colors[8],
            0,
            Vector2.Zero,
            new Vector2(camMarkerSize, camMarkerSize),
            SpriteEffects.None,
            0
        );
    }

    public void DrawRectOutline(Vector2 topLeft, float width, float height,
                                     Color color, float thickness)
    {
        DrawLine(topLeft, topLeft + new Vector2(width, 0), color, thickness);
        DrawLine(topLeft + new Vector2(width, 0), topLeft + new Vector2(width, height), color, thickness);
        DrawLine(topLeft + new Vector2(0, height), topLeft + new Vector2(width, height), color, thickness);
        DrawLine(topLeft, topLeft + new Vector2(0, height), color, thickness);
    }

    public void DrawPlayerAttackRange()
    {
        Vector2 playerWorld = new Vector2(plx.Float, ply.Float);

        F32 bx = p8.Cos(prot);
        F32 by = p8.Sin(prot);
        F32 hitx = plx + bx * 8;
        F32 hity = ply + by * 8;

        Vector2 hitWorld = new Vector2(hitx.Float, hity.Float);
        Vector2 attackDir = hitWorld - playerWorld;

        if (attackDir == Vector2.Zero) return;

        attackDir.Normalize();

        Vector2 circleCenterWorld = playerWorld + attackDir * 8f;

        Vector2 circleCenterScreen = WorldToScreen(circleCenterWorld);
        Vector2 playerScreen = WorldToScreen(playerWorld);

        const float attackRadius = 10f;
        float screenRadiusX = attackRadius * p8.Cell.Width;
        float screenRadiusY = attackRadius * p8.Cell.Height;

        Color circleColor = nearEnemies is not null && nearEnemies.Count > 0 ?
            p8.Colors[8] :
            p8.Colors[7];

        DrawCircleOutline(
            circleCenterScreen,
            screenRadiusX,
            screenRadiusY,
            circleColor,
            thickness: 0.4f * p8.Cell.Height
        );

        DrawLine(playerScreen, circleCenterScreen, p8.Colors[8], 0.4f * p8.Cell.Height);
    }

    public void DrawZombieChaseRadius(Entity zombie)
    {
        const float baseRadius = 40f;

        const float chaseRadius = 70f;

        Vector2 zombieWorld = new Vector2(zombie.X.Float, zombie.Y.Float);
        Vector2 screenCenter = WorldToScreen(zombieWorld);

        float currentRadius = zombie.Step == enstep_Chase ? chaseRadius : baseRadius;
        Color radiusColor = zombie.Step == enstep_Chase ?
            p8.Colors[8] :
            p8.Colors[7];

        DrawDynamicCircle(
            screenCenter,
            currentRadius * p8.Cell.Width,
            currentRadius * p8.Cell.Height,
            radiusColor,
            thickness: 0.4f * p8.Cell.Height,
            isChasing: zombie.Step == enstep_Chase
        );
    }

    private void DrawDynamicCircle(Vector2 center,
                                 float radiusX, float radiusY, Color color,
                                 float thickness, bool isChasing)
    {
        DrawCircleOutline(center,
            40f * p8.Cell.Width, 40f * p8.Cell.Height,
            p8.Colors[7], thickness);

        if (isChasing)
        {
            DrawCircleOutline(center,
                radiusX, radiusY,
                color, thickness);
        }
    }

    public void DrawCircleOutline(Vector2 center, float radiusX, float radiusY,
                                       Color color, float thickness,
                                       int segments = 32)
    {
        Vector2 lastPoint = Vector2.Zero;

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * MathHelper.TwoPi;
            Vector2 point = new Vector2(
                center.X + (float)Math.Cos(angle) * radiusX,
                center.Y + (float)Math.Sin(angle) * radiusY
            );

            if (i > 0)
            {
                DrawLine(lastPoint, point, color, thickness);
            }
            lastPoint = point;
        }
    }

    public void DrawZombieAttackRange(Entity zombie)
    {
        const float attackRadius = 10f;

        Vector2 zombieWorld = new Vector2(zombie.X.Float, zombie.Y.Float);
        Vector2 playerWorld = new Vector2(plx.Float, ply.Float);

        float distp = Vector2.Distance(zombieWorld, playerWorld);

        Vector2 zombieScreen = WorldToScreen(zombieWorld);

        Color circleColor = distp < attackRadius ?
            p8.Colors[8] :
            p8.Colors[7];

        float screenRadiusX = attackRadius * p8.Cell.Width;
        float screenRadiusY = attackRadius * p8.Cell.Height;
        DrawCircleOutline(
            zombieScreen,
            screenRadiusX,
            screenRadiusY,
            circleColor,
            thickness: 0.4f * p8.Cell.Height
        );

        float facingTurns = zombie.Lrot.Float;
        Vector2 direction = AngleToDir(facingTurns);
        Vector2 endWorld = zombieWorld + direction * attackRadius;
        Vector2 endScreen = WorldToScreen(endWorld);
        DrawLine(zombieScreen, endScreen, p8.Colors[12], 0.4f * p8.Cell.Height);

        Vector2 toPlayerDir = playerWorld - zombieWorld;
        float distance = toPlayerDir.Length();

        if (distance > 0)
        {
            toPlayerDir /= distance;
            Vector2 clampedEndWorld = zombieWorld + toPlayerDir * Math.Min(distance, attackRadius);
            Vector2 clampedEndScreen = WorldToScreen(clampedEndWorld);

            DrawLine(zombieScreen, clampedEndScreen, p8.Colors[9], 0.4f * p8.Cell.Height);
        }
    }

    private Vector2 AngleToDir(float turns)
    {
        float radians = turns * MathHelper.TwoPi;
        return new Vector2(
            (float)Math.Cos(radians),
            -(float)Math.Sin(radians)
        );
    }

    private Vector2 WorldToScreen(Vector2 worldPos)
    {
        return new Vector2(
            (worldPos.X - p8.CameraOffset.x.Float) * p8.Cell.Width,
            (worldPos.Y - p8.CameraOffset.y.Float) * p8.Cell.Height
        );
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);

        p8.Batch.Draw(
            p8.Pixel,
            start,
            null,
            color,
            angle,
            new Vector2(0, 0.5f),
            new Vector2(edge.Length(), thickness),
            SpriteEffects.None,
            0
        );
    }

    protected override void FillEne(Level l)
    {
        spawnCenter = (plx, ply);
        l.Ene = [Entity(player, F32.Zero, F32.Zero, F32.Zero, F32.Zero)];
        enemies = l.Ene;
        for (F32 i = F32.Zero; i < levelsx; i++)
        {
            for (F32 j = F32.Zero; j < levelsy; j++)
            {
                Ground c = GetDirectGr(i, j);
                F32 r = p8.Rnd(100);
                F32 ex = i * 16 + 8;
                F32 ey = j * 16 + 8;
                F32 dist = F32.Max(F32.Abs(ex - plx), F32.Abs(ey - ply));
                if (r < 3 && c != grwater && c != grrock && !c.IsTree && dist > 50)
                {
                    Entity newe = Entity(zombi, ex, ey, F32.Zero, F32.Zero);
                    newe.Life = F32.FromInt(10);
                    newe.Prot = F32.Zero;
                    newe.Lrot = F32.Zero;
                    newe.Panim = F32.Zero;
                    newe.Banim = F32.Zero;
                    newe.Dtim = F32.Zero;
                    newe.Step = 0;
                    newe.Ox = F32.Zero;
                    newe.Oy = F32.Zero;
                    p8.Add(l.Ene, newe);
                }
            }
        }
    }

    protected override void Denemies()
    {
        Sorty(enemies);

        foreach (Entity e in enemies)
        {
            if (e.Type == player)
            {
                p8.Pal();
                Dplayer(plx, ply, prot, panim, banim, true);
            }
            else
            {
                if (IsIn(e, 72))
                {
                    p8.Pal();
                    p8.Pal(15, 3);
                    p8.Pal(4, 1);
                    p8.Pal(2, 8);
                    p8.Pal(1, 1);

                    Dplayer(e.X, e.Y, e.Prot, e.Panim, e.Banim, false);

                    DrawZombieChaseRadius(e);
                    DrawZombieAttackRange(e);
                }
            }
        }

        DrawZombieSpawnArea();
        DrawCameraBounds();

        F32 ci = F32.Floor((clx - 64) / 16);
        F32 cj = F32.Floor((cly - 64) / 16);

        for (F32 i = ci - 1; i <= ci + 8; i++)
        {
            for (F32 j = cj - 1; j <= cj + 8; j++)
            {
                Ground gr = GetDirectGr(i, j);
                if (gr is null)
                {
                    continue;
                }

                F32 gi = i * 16;
                F32 gj = j * 16;

                Vector2 worldPos = new Vector2(gi.Float, gj.Float);
                Vector2 screenPos = WorldToScreen(worldPos);
                float screenWidth = 16 * p8.Cell.Width;
                float screenHeight = 16 * p8.Cell.Height;

                if (gr == grrock || gr == grtree || gr == griron || gr == grgold || gr == grgem || gr == grhole)
                {
                    DrawRectOutline(
                    screenPos,
                    screenWidth,
                    screenHeight,
                    p8.Colors[7],
                    thickness: 0.3f * p8.Cell.Height
                    );
                }
            }
        }

        DrawPlayerAttackRange();
        foreach (Entity e in enemies)
        {
            if (e.Type == player)
            {
                Vector2 position = new((plx - p8.CameraOffset.x - F32.Half).Float * p8.Cell.Width, (ply - p8.CameraOffset.y - F32.Half).Float * p8.Cell.Height);
                Vector2 size = new(p8.Cell.Width, p8.Cell.Height);

                p8.Batch.Draw(p8.Pixel, position, null, p8.Colors[11], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }
            else if (IsIn(e, 72))
            {
                Vector2 position = new((e.X - p8.CameraOffset.x - F32.Half).Float * p8.Cell.Width, (e.Y - p8.CameraOffset.y - F32.Half).Float * p8.Cell.Height);
                Vector2 size = new(p8.Cell.Width, p8.Cell.Height);

                p8.Batch.Draw(p8.Pixel, position, null, p8.Colors[11], 0, Vector2.Zero, size, SpriteEffects.None, 0);
            }
        }
    }
}
