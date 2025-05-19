using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BulletStorm.Entities
{
    public class PlayerProjectile
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Speed;
        public int Damage;
        public float CritChance;
        public float EffectDuration;
        public string EffectDescription;
        public Action<Enemy> OnHitEffect;
        public int Size = 12;
        public bool IsActive = true;

        public PlayerProjectile(
            Vector2 position,
            Vector2 direction,
            float speed,
            int damage,
            float critChance = 0f,
            float effectDuration = 0f,
            string effectDescription = "",
            Action<Enemy> onHitEffect = null)
        {
            Position = position;
            Direction = direction;
            Speed = speed;
            Damage = damage;
            CritChance = critChance;
            EffectDuration = effectDuration;
            EffectDescription = effectDescription;
            OnHitEffect = onHitEffect;
        }

        public void Update(GameTime gameTime)
        {
            Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (IsActive)
                spriteBatch.Draw(texture, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), Color.Cyan);
        }

        public void CheckCollisionWithEnemies(System.Collections.Generic.List<Enemy> enemies, Random rng)
        {
            if (!IsActive) return;

            Rectangle projRect = new Rectangle((int)Position.X, (int)Position.Y, Size, Size);

            foreach (var enemy in enemies)
            {
                Rectangle enemyRect = new Rectangle((int)enemy.Position.X, (int)enemy.Position.Y, enemy.Size, enemy.Size);
                if (enemy.IsAlive && projRect.Intersects(enemyRect))
                {
                    // Crit calculation
                    int finalDamage = Damage;
                    if (CritChance > 0f && rng.NextDouble() < CritChance)
                        finalDamage = (int)(Damage * 1.5f);

                    enemy.TakeDamage(finalDamage);

                    // Apply effect if any
                    OnHitEffect?.Invoke(enemy);

                    IsActive = false;
                    break;
                }
            }
        }
    }
}
