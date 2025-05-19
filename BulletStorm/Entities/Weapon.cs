using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletStorm.Entities
{
    public class Weapon
    {
        public Vector2 Offset;
        public float Angle;
        public float OrbitRadius = 40f;
        public Texture2D Texture;

        public Weapon(Texture2D texture, float angle)
        {
            Texture = texture;
            Angle = angle;
        }

        public void Update(Vector2 playerPosition, float delta)
        {
            Angle += delta; // Rotação contínua
            Offset = new Vector2(
                (float)Math.Cos(Angle) * OrbitRadius,
                (float)Math.Sin(Angle) * OrbitRadius
            );
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 playerPosition)
        {
            spriteBatch.Draw(Texture, playerPosition + Offset, Color.White);
        }
    }
}

