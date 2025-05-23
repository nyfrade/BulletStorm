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
        public float AttackSpeed = 1.0f;
        public float AttackCritChance = 0.1f;
        public int AttackDamage = 1;
        public float CoinPickupRange = 40f;

        public bool IsAlive => Health > 0;

        // --- Animação ---
        private int currentFrame = 0;
        private float animationTimer = 0f;
        private float frameTime = 0.12f; // tempo de cada frame em segundos
        private static readonly Rectangle[] walkDownFrames = new Rectangle[]
        {
            new Rectangle(24, 23, 15, 24),
            new Rectangle(88, 21, 15, 26),
            new Rectangle(152, 22, 15, 25),
            new Rectangle(216, 23, 15, 24),
            new Rectangle(280, 21, 15, 26),
            new Rectangle(344, 22, 15, 25)
        };

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
            {
                direction.Normalize();
                // Atualiza animação apenas se estiver se movendo
                animationTimer += delta;
                if (animationTimer >= frameTime)
                {
                    currentFrame = (currentFrame + 1) % walkDownFrames.Length;
                    animationTimer = 0f;
                }
            }
            else
            {
                // Se parado, volta para o primeiro frame
                currentFrame = 0;
                animationTimer = 0f;
            }

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
            {
                // Desenha o frame atual da animação de andar para baixo
                float scale = 2f; // Altere para o tamanho desejado
                spriteBatch.Draw(
                    texture,
                    Position,
                    walkDownFrames[currentFrame],
                    Color.White,
                    0f,
                    Vector2.Zero,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }
}

