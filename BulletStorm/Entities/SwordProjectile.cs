using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletStorm.Entities
{
    public class SwordProjectile
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Texture2D Texture;
        public int Damage;
        public bool IsActive = true;

        public SwordProjectile(Vector2 position, Vector2 velocity, Texture2D texture, int damage)
        {
            Position = position;
            Velocity = velocity;
            Texture = texture;
            Damage = damage;
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
                spriteBatch.Draw(Texture, Position, Color.White);
        }
    }
}

