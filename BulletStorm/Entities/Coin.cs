using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletStorm.Entities
{
    public class Coin
    {
        public Vector2 Position;
        public int Value = 1;
        public int Size = 16;
        public bool Collected = false;

        public Coin(Vector2 position, int value = 1)
        {
            Position = position;
            Value = value;
        }

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Size, Size);

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (!Collected)
                spriteBatch.Draw(texture, Bounds, Color.Gold);
        }
    }
}
