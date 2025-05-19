using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BulletStorm.Entities
{
    public class Player
    {
        public Vector2 Position;
        public float Speed = 200f;
        public int Health = 10;
        public int MaxHealth = 10;

        // --- Player Stats ---
        public int Coins = 0;
        public float AttackSpeed = 1.0f;         // Attacks per second
        public float AttackCritChance = 0.1f;    // 10% crit chance (0.0 - 1.0)
        public int AttackDamage = 1;             // Base damage
        public float CoinPickupRange = 40f;      // Pixels

        public bool IsAlive => Health > 0;

        public Player(Vector2 startPosition)
        {
            Position = startPosition;
            Speed = 200f;
            Health = MaxHealth = 10;
            Coins = 0;
            AttackSpeed = 1.0f;
            AttackCritChance = 0.1f;
            AttackDamage = 1;
            CoinPickupRange = 40f;
        }

        public void Move(Vector2 direction, float delta, int maxWidth, int maxHeight, int spriteWidth, int spriteHeight)
        {
            if (direction != Vector2.Zero)
                direction.Normalize();

            Position += direction * Speed * delta;
            Position.X = Math.Clamp(Position.X, 0, maxWidth - spriteWidth);
            Position.Y = Math.Clamp(Position.Y, 0, maxHeight - spriteHeight);
        }

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health < 0) Health = 0;
        }

        public void Heal(int amount)
        {
            Health += amount;
            if (Health > MaxHealth) Health = MaxHealth;
        }

        // --- Upgrade Methods ---
        public void UpgradeAttackSpeed(float amount) => AttackSpeed += amount;
        public void UpgradeAttackCritChance(float amount) => AttackCritChance = Math.Clamp(AttackCritChance + amount, 0f, 1f);
        public void UpgradeAttackDamage(int amount) => AttackDamage += amount;
        public void UpgradeHealth(int amount) { MaxHealth += amount; Health += amount; }
        public void UpgradeSpeed(float amount) => Speed += amount;
        public void UpgradeCoinPickupRange(float amount) => CoinPickupRange += amount;

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (IsAlive)
                spriteBatch.Draw(texture, Position, Color.White);
        }
    }
}
