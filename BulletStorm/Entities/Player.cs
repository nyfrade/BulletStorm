using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BulletStorm.Entities
{
    public class Player
    {
        public Vector2 Position;
        public float Speed = 200f;
        public int Health = 100;
        public int MaxHealth = 120;
        public float AttackSpeed = 1.0f;
        public float AttackCritChance = 0.1f;
        public int AttackDamage = 1;
        public bool IsAlive = true;
        public float Scale = 2f;

        // No início da classe Player:
        private float regenTimer = 0f;
        private float regenInterval = 0.5f; // Regenera a cada 0.5 segundos
        private int regenAmount = 5;        // Quantidade de vida regenerada por tick

        // No método Update (adicione este método se não existir):
        public void Update(GameTime gameTime)
        {
            if (!IsAlive || Health >= MaxHealth) return;

            regenTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (regenTimer >= regenInterval)
            {
                Health += regenAmount;
                if (Health > MaxHealth) Health = MaxHealth;
                regenTimer = 0f;
            }
        }


        private enum Direction
        {
            Down,
            Left,
            Right,
            Up
        }
        private Direction currentDirection = Direction.Down;

        private readonly List<Rectangle> downFrames = new()
        {
            new Rectangle(24, 23, 15, 24),
            new Rectangle(88, 21, 15, 26),
            new Rectangle(152, 22, 15, 25),
            new Rectangle(216, 23, 15, 24),
            new Rectangle(280, 21, 15, 26),
            new Rectangle(344, 22, 15, 25)
        };
        private readonly List<Rectangle> leftFrames = new()
        {
            new Rectangle(24, 86, 15, 25),
            new Rectangle(88, 84, 15, 27),
            new Rectangle(152, 85, 15, 26),
            new Rectangle(216, 86, 15, 26),
            new Rectangle(280, 85, 15, 26),
            new Rectangle(344, 84, 15, 27)
        };
        private readonly List<Rectangle> rightFrames = new()
        {
            new Rectangle(25, 150, 15, 25),
            new Rectangle(89, 148, 15, 27),
            new Rectangle(153, 149, 15, 26),
            new Rectangle(217, 150, 15, 26),
            new Rectangle(281, 149, 15, 26),
            new Rectangle(345, 148, 15, 27)
        };
        private readonly List<Rectangle> upFrames = new()
        {
            new Rectangle(24, 214, 15, 25),
            new Rectangle(88, 212, 15, 27),
            new Rectangle(152, 213, 15, 26),
            new Rectangle(216, 214, 15, 25),
            new Rectangle(280, 212, 15, 27),
            new Rectangle(344, 213, 15, 26)
        };

        private int currentFrame = 0;
        private float animationTimer = 0f;
        private float animationInterval = 0.12f;

        public Player(Vector2 startPosition)
        {
            Position = startPosition;
        }

        public void Move(Vector2 direction, float delta, int maxWidth, int maxHeight, int spriteWidth, int spriteHeight)
        {
            if (direction.LengthSquared() > 0)
            {
                direction.Normalize();
                Position += direction * Speed * delta;

                if (System.Math.Abs(direction.X) > System.Math.Abs(direction.Y))
                {
                    currentDirection = direction.X > 0 ? Direction.Right : Direction.Left;
                }
                else
                {
                    currentDirection = direction.Y > 0 ? Direction.Down : Direction.Up;
                }

                animationTimer += delta;
                if (animationTimer >= animationInterval)
                {
                    currentFrame = (currentFrame + 1) % downFrames.Count;
                    animationTimer = 0f;
                }
            }
            else
            {
                currentFrame = 0;
                animationTimer = 0f;
            }

            Position.X = MathHelper.Clamp(Position.X, 0, maxWidth - spriteWidth * Scale);
            Position.Y = MathHelper.Clamp(Position.Y, 0, maxHeight - spriteHeight * Scale);
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            Health -= amount;
            if (Health < 0) Health = 0;
            if (Health == 0) IsAlive = false;
        }

        public void Heal(int amount)
        {
            Health += amount;
            if (Health > MaxHealth) Health = MaxHealth;
        }

        public void UpgradeAttackSpeed(float amount)
        {
            AttackSpeed += amount;
        }

        public void UpgradeAttackCritChance(float amount)
        {
            AttackCritChance += amount;
        }

        public void UpgradeAttackDamage(int amount)
        {
            AttackDamage += amount;
        }

        public void UpgradeHealth(int amount)
        {
            MaxHealth += amount;
            Health += amount;
        }

        public void UpgradeSpeed(float amount)
        {
            Speed += amount;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            Rectangle sourceRect = downFrames[currentFrame];
            switch (currentDirection)
            {
                case Direction.Down:
                    sourceRect = downFrames[currentFrame];
                    break;
                case Direction.Left:
                    sourceRect = leftFrames[currentFrame];
                    break;
                case Direction.Right:
                    sourceRect = rightFrames[currentFrame];
                    break;
                case Direction.Up:
                    sourceRect = upFrames[currentFrame];
                    break;
            }

            spriteBatch.Draw(
                texture,
                new Vector2((int)Position.X, (int)Position.Y),
                sourceRect,
                Color.White,
                0f,
                Vector2.Zero,
                Scale,
                SpriteEffects.None,
                0f
            );
        }
    }
}

