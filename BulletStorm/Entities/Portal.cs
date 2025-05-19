using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletStorm.Entities
{
    public class Portal
    {
        public Rectangle Rect;
        public Texture2D Texture;
        public bool Active;

        public Portal(Rectangle rect, Texture2D texture)
        {
            Rect = rect;
            Texture = texture;
            Active = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Active)
                spriteBatch.Draw(Texture, Rect, Color.Black);
        }

        public bool Intersects(Rectangle playerRect)
        {
            return Active && Rect.Intersects(playerRect);
        }
    }
}

