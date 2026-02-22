using CSharpCraft.Pico8;
using static CSharpCraft.Pico8.Pico8;
using FixMath;
using Microsoft.Xna.Framework;

namespace CSharpCraft.Pcraft;

public class Visualiser : PcraftBase
{
    public override string SceneName => "visualiser";

    private (F32 x, F32 y) spawnCenter;

    public override void Init()
    {
        base.Init();
    }

    public void DrawZombieSpawnArea()
    {
        Vector2 playerWorld = new Vector2(spawnCenter.x.Float, spawnCenter.y.Float);

        const float spawnMargin = 50f;
        float screenMarginX = spawnMargin * CellWidth;
        float screenMarginY = spawnMargin * CellHeight;

        Vector2 playerScreen = WorldToScreen(playerWorld);
        Rectangle spawnArea = new Rectangle(
            (int)(playerScreen.X - screenMarginX),
            (int)(playerScreen.Y - screenMarginY),
            (int)(screenMarginX * 2),
            (int)(screenMarginY * 2)
        );

        Color spawnColor = GetColor(14);
        DrawRectOutline(
            new Vector2(spawnArea.X, spawnArea.Y),
            spawnArea.Width,
            spawnArea.Height,
            spawnColor,
            thickness: 0.4f * CellHeight
        );
    }

    public void DrawCameraBounds()
    {
        const float boundarySize = 32f;

        Vector2 targetCenterWorld = new Vector2(cmx.Float, cmy.Float);
        Vector2 targetCenterScreen = WorldToScreen(targetCenterWorld);

        float screenWidth = boundarySize * CellWidth;
        float screenHeight = boundarySize * CellHeight;

        DrawRectOutline(
            targetCenterScreen - new Vector2(screenWidth / 2, screenHeight / 2),
            screenWidth,
            screenHeight,
            GetColor(7),
            thickness: 0.4f * CellHeight
        );

        Vector2 currentCamWorld = new Vector2(clx.Float, cly.Float);
        Vector2 currentCamScreen = WorldToScreen(currentCamWorld);

        float camMarkerSize = 1 * CellHeight;
        DrawPixelScaled(
            currentCamScreen - new Vector2(camMarkerSize / 2, camMarkerSize / 2),
            GetColor(8),
            0,
            Vector2.Zero,
            new Vector2(camMarkerSize, camMarkerSize),
            0,
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

        F32 bx = Cos(prot);
        F32 by = Sin(prot);
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
        float screenRadiusX = attackRadius * CellWidth;
        float screenRadiusY = attackRadius * CellHeight;

        Color circleColor = nearEnemies is not null && nearEnemies.Count > 0 ?
            GetColor(8) :
            GetColor(7);

        DrawCircleOutline(
            circleCenterScreen,
            screenRadiusX,
            screenRadiusY,
            circleColor,
            thickness: 0.4f * CellHeight
        );

        DrawLine(playerScreen, circleCenterScreen, GetColor(8), 0.4f * CellHeight);
    }

    public void DrawZombieChaseRadius(Entity zombie)
    {
        const float baseRadius = 40f;

        const float chaseRadius = 70f;

        Vector2 zombieWorld = new Vector2(zombie.X.Float, zombie.Y.Float);
        Vector2 screenCenter = WorldToScreen(zombieWorld);

        float currentRadius = zombie.Step == enstep_Chase ? chaseRadius : baseRadius;
        Color radiusColor = zombie.Step == enstep_Chase ?
            GetColor(8) :
            GetColor(7);

        DrawDynamicCircle(
            screenCenter,
            currentRadius * CellWidth,
            currentRadius * CellHeight,
            radiusColor,
            thickness: 0.4f * CellHeight,
            isChasing: zombie.Step == enstep_Chase
        );
    }

    private void DrawDynamicCircle(Vector2 center,
                                 float radiusX, float radiusY, Color color,
                                 float thickness, bool isChasing)
    {
        DrawCircleOutline(center,
            40f * CellWidth, 40f * CellHeight,
            GetColor(7), thickness);

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
            GetColor(8) :
            GetColor(7);

        float screenRadiusX = attackRadius * CellWidth;
        float screenRadiusY = attackRadius * CellHeight;
        DrawCircleOutline(
            zombieScreen,
            screenRadiusX,
            screenRadiusY,
            circleColor,
            thickness: 0.4f * CellHeight
        );

        float facingTurns = zombie.Lrot.Float;
        Vector2 direction = AngleToDir(facingTurns);
        Vector2 endWorld = zombieWorld + direction * attackRadius;
        Vector2 endScreen = WorldToScreen(endWorld);
        DrawLine(zombieScreen, endScreen, GetColor(12), 0.4f * CellHeight);

        Vector2 toPlayerDir = playerWorld - zombieWorld;
        float distance = toPlayerDir.Length();

        if (distance > 0)
        {
            toPlayerDir /= distance;
            Vector2 clampedEndWorld = zombieWorld + toPlayerDir * Math.Min(distance, attackRadius);
            Vector2 clampedEndScreen = WorldToScreen(clampedEndWorld);

            DrawLine(zombieScreen, clampedEndScreen, GetColor(9), 0.4f * CellHeight);
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
            (worldPos.X - CameraOffsetX.Float) * CellWidth,
            (worldPos.Y - CameraOffsetY.Float) * CellHeight
        );
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);

        DrawPixelScaled(
            start,
            color,
            angle,
            new Vector2(0, 0.5f),
            new Vector2(edge.Length(), thickness),
            0,
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
                F32 r = Rnd(100);
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
                    Add(l.Ene, newe);
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
                Pal();
                Dplayer(plx, ply, prot, panim, banim, true);
            }
            else
            {
                if (IsIn(e, 72))
                {
                    Pal();
                    Pal(15, 3);
                    Pal(4, 1);
                    Pal(2, 8);
                    Pal(1, 1);

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
                float screenWidth = 16 * CellWidth;
                float screenHeight = 16 * CellHeight;

                if (gr == grrock || gr == grtree || gr == griron || gr == grgold || gr == grgem || gr == grhole)
                {
                    DrawRectOutline(
                    screenPos,
                    screenWidth,
                    screenHeight,
                    GetColor(7),
                    thickness: 0.3f * CellHeight
                    );
                }
            }
        }

        DrawPlayerAttackRange();
        foreach (Entity e in enemies)
        {
            if (e.Type == player)
            {
                Vector2 position = new((plx - CameraOffsetX - F32.Half).Float * CellWidth, (ply - CameraOffsetY - F32.Half).Float * CellHeight);
                Vector2 size = new(CellWidth, CellHeight);

                DrawPixelScaled(position, GetColor(11), 0, Vector2.Zero, size, 0, 0);
            }
            else if (IsIn(e, 72))
            {
                Vector2 position = new((e.X - CameraOffsetX - F32.Half).Float * CellWidth, (e.Y - CameraOffsetY - F32.Half).Float * CellHeight);
                Vector2 size = new(CellWidth, CellHeight);

                DrawPixelScaled(position, GetColor(11), 0, Vector2.Zero, size, 0, 0);
            }
        }
    }
}
