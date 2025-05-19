using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BulletStorm.Entities
{
    public enum EnemyType
    {
        Slime,
        SlimeAgua,
        SlimeFogo,
        Ogre,
        OgreWarrior,
        OgreBoss,
        Vampiro,
        VampiroWarrior,
        VampiroBoss
    }

    public class Enemy
    {
        public Vector2 Position;
        public float Speed;
        public int Health;
        public bool IsAlive = true;
        public EnemyType Type;
        public Color Color = Color.Red;
        public int Size = 32;

        // For ranged enemies
        public List<EnemyProjectile> Projectiles = new();
        private float attackCooldown = 2f;
        private float attackTimer = 0f;

        // Advanced AI state
        private Vector2 patrolTarget = Vector2.Zero;
        private bool patrolForward = true;
        private float specialTimer = 0f; // For boss special moves
        private float minionCooldown = 6f; // OgreBoss minion spawn
        private float minionTimer = 0f;
        private Random rng = new();

        // Coin drop property
        public int CoinsDropped { get; set; } = 1;

        public Enemy(Vector2 position, float speed, int health, EnemyType type)
        {
            Position = position;
            Speed = speed;
            Health = health;
            Type = type;

            // Set color/size based on type
            switch (type)
            {
                case EnemyType.SlimeAgua: Color = Color.Blue; break;
                case EnemyType.SlimeFogo: Color = Color.Orange; break;
                case EnemyType.Ogre:
                case EnemyType.OgreWarrior: Color = Color.Green; Size = 40; break;
                case EnemyType.OgreBoss: Color = Color.DarkGreen; Size = 64; break;
                case EnemyType.Vampiro:
                case EnemyType.VampiroWarrior: Color = Color.Purple; Size = 36; break;
                case EnemyType.VampiroBoss: Color = Color.Gold; Size = 64; break;
                default: Color = Color.Red; break;
            }

            // Set coin drop amount based on enemy type
            switch (type)
            {
                case EnemyType.Slime:
                    CoinsDropped = 1;
                    break;
                case EnemyType.SlimeAgua:
                case EnemyType.SlimeFogo:
                    CoinsDropped = 2;
                    break;
                case EnemyType.Ogre:
                case EnemyType.Vampiro:
                    CoinsDropped = 3;
                    break;
                case EnemyType.OgreWarrior:
                case EnemyType.VampiroWarrior:
                    CoinsDropped = 5;
                    break;
                case EnemyType.OgreBoss:
                case EnemyType.VampiroBoss:
                    CoinsDropped = 20;
                    break;
                default:
                    CoinsDropped = 1;
                    break;
            }
        }

        public void Update(Vector2 playerPosition, GameTime gameTime, Texture2D projectileTexture, List<Enemy> allEnemies = null)
        {
            if (!IsAlive) return;

            float distanceToPlayer = Vector2.Distance(Position, playerPosition);

            switch (Type)
            {
                // --- Slimes: Flee if low health, else chase ---
                case EnemyType.Slime:
                case EnemyType.SlimeAgua:
                case EnemyType.SlimeFogo:
                    if (Health <= 1 && distanceToPlayer < 200)
                        FleeFrom(playerPosition, gameTime, 1.2f);
                    else
                        MoveTowards(playerPosition, gameTime, 1.0f);
                    break;

                // --- Ogres: Patrol if far, chase if close ---
                case EnemyType.Ogre:
                case EnemyType.OgreWarrior:
                    if (distanceToPlayer > 250)
                        Patrol(gameTime, 0.7f);
                    else
                        MoveTowards(playerPosition, gameTime, 0.9f);
                    break;

                // --- OgreBoss: Slow chase, ranged, jumps, spawns minions ---
                case EnemyType.OgreBoss:
                    specialTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    minionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    // Jump (teleport) near player every 8 seconds
                    if (specialTimer > 8f)
                    {
                        Vector2 jumpOffset = new Vector2(rng.Next(-120, 120), rng.Next(-120, 120));
                        Position = playerPosition + jumpOffset;
                        specialTimer = 0f;
                    }
                    // Spawn minion every minionCooldown seconds
                    if (allEnemies != null && minionTimer > minionCooldown)
                    {
                        Vector2 minionPos = Position + new Vector2(rng.Next(-40, 40), rng.Next(-40, 40));
                        allEnemies.Add(new Enemy(minionPos, 90, 2, EnemyType.OgreWarrior));
                        minionTimer = 0f;
                    }
                    // Slow chase and ranged attack
                    MoveTowards(playerPosition, gameTime, 0.5f);
                    HandleRangedAttack(playerPosition, gameTime, projectileTexture);
                    break;

                // --- Vampiros: Kite, shoot, circle player ---
                case EnemyType.Vampiro:
                case EnemyType.VampiroWarrior:
                    if (distanceToPlayer < 120)
                        FleeFrom(playerPosition, gameTime, 1.2f);
                    else if (distanceToPlayer < 300)
                    {
                        HandleRangedAttack(playerPosition, gameTime, projectileTexture);
                        CirclePlayer(playerPosition, gameTime, 0.8f);
                    }
                    else
                        MoveTowards(playerPosition, gameTime, 0.7f);
                    break;

                // --- VampiroBoss: Kite, shoot, dash, radial attack ---
                case EnemyType.VampiroBoss:
                    specialTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    // Dash away if player is close (every 6s)
                    if (distanceToPlayer < 140 && specialTimer > 6f)
                    {
                        FleeFrom(playerPosition, gameTime, 2.5f);
                        specialTimer = 0f;
                    }
                    // Radial bullet pattern every 7s
                    if (specialTimer > 7f)
                    {
                        FireRadialPattern(projectileTexture);
                        specialTimer = 0f;
                    }
                    // Kite and shoot
                    if (distanceToPlayer < 180)
                        FleeFrom(playerPosition, gameTime, 1.1f);
                    else
                    {
                        HandleRangedAttack(playerPosition, gameTime, projectileTexture);
                        MoveTowards(playerPosition, gameTime, 0.5f);
                    }
                    break;

                default:
                    MoveTowards(playerPosition, gameTime);
                    break;
            }

            // Update projectiles
            foreach (var proj in Projectiles)
                proj.Update(gameTime);

            Projectiles.RemoveAll(p => !p.IsActive ||
                p.Position.X < -p.Size || p.Position.X > 2000 ||
                p.Position.Y < -p.Size || p.Position.Y > 2000);
        }

        // --- Damage handling ---
        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            Health -= amount;
            if (Health <= 0)
            {
                Health = 0;
                IsAlive = false;
                // Optionally: play death animation, drop loot, etc.
            }
        }

        // --- Advanced AI helpers ---

        private void Patrol(GameTime gameTime, float speedMultiplier = 1.0f)
        {
            // Simple patrol between two points
            if (patrolTarget == Vector2.Zero)
                patrolTarget = Position + new Vector2(100, 0);

            if (Vector2.Distance(Position, patrolTarget) < 10f)
            {
                patrolForward = !patrolForward;
                patrolTarget = Position + (patrolForward ? new Vector2(100, 0) : new Vector2(-100, 0));
            }

            MoveTowards(patrolTarget, gameTime, speedMultiplier);
        }

        private void FleeFrom(Vector2 target, GameTime gameTime, float speedMultiplier = 1.0f)
        {
            Vector2 direction = Position - target;
            if (direction != Vector2.Zero)
                direction.Normalize();

            Position += direction * Speed * speedMultiplier * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        private void CirclePlayer(Vector2 playerPosition, GameTime gameTime, float speedMultiplier = 1.0f)
        {
            // Move perpendicular to the player direction (circle around)
            Vector2 toPlayer = playerPosition - Position;
            if (toPlayer == Vector2.Zero) return;
            toPlayer.Normalize();
            Vector2 perpendicular = new Vector2(-toPlayer.Y, toPlayer.X);

            Position += perpendicular * Speed * speedMultiplier * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        private void FireRadialPattern(Texture2D projectileTexture)
        {
            // Fire bullets in all directions (radial pattern)
            int bulletCount = 12;
            float angleStep = MathHelper.TwoPi / bulletCount;
            for (int i = 0; i < bulletCount; i++)
            {
                float angle = i * angleStep;
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Projectiles.Add(new EnemyProjectile(Position + new Vector2(Size / 2, Size / 2), dir));
            }
        }

        private void MoveTowards(Vector2 target, GameTime gameTime, float speedMultiplier = 1.0f)
        {
            Vector2 direction = target - Position;
            if (direction != Vector2.Zero)
                direction.Normalize();

            Position += direction * Speed * speedMultiplier * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        private void HandleRangedAttack(Vector2 playerPosition, GameTime gameTime, Texture2D projectileTexture)
        {
            attackTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (attackTimer >= attackCooldown)
            {
                Vector2 direction = playerPosition - Position;
                if (direction != Vector2.Zero)
                    direction.Normalize();

                Projectiles.Add(new EnemyProjectile(Position + new Vector2(Size / 2, Size / 2), direction));
                attackTimer = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Texture2D projectileTexture)
        {
            if (IsAlive)
                spriteBatch.Draw(texture, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), Color);

            foreach (var proj in Projectiles)
                proj.Draw(spriteBatch, projectileTexture);
        }
    }

    public class EnemyProjectile
    {
        public Vector2 Position;
        public Vector2 Direction;
        public float Speed = 200f;
        public bool IsActive = true;
        public int Size = 8;

        public EnemyProjectile(Vector2 position, Vector2 direction)
        {
            Position = position;
            Direction = direction;
        }

        public void Update(GameTime gameTime)
        {
            Position += Direction * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (IsActive)
                spriteBatch.Draw(texture, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), Color.Yellow);
        }
    }
}
