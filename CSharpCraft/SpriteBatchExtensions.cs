using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CSharpCraft;

public static class SpriteBatchExtensions
{
    public static void DrawLine(this SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);

        spriteBatch.Draw(texture,
            new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), (int)thickness),
            null,
            color,
            angle,
            new Vector2(0, 0.5f),
            SpriteEffects.None,
            0);
    }
}