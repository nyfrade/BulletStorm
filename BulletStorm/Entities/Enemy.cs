using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BulletStorm.Entities
{
    public enum EnemyType
    {
        Slime,
        SlimeFogo,
        Ogre,
        OgreBoss,
        Vampiro,
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

        // Novos stats para dificuldade e dano
        public int AttackDamage = 1;
        public int ProjectileDamage = 1;

        // For ranged enemies
        public List<EnemyProjectile> Projectiles = new();
        private float attackCooldown = 1f;
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

        // --- Spritesheets e frames para cada inimigo animado ---
        private static Texture2D slimeSpriteSheet, slimeFogoSpriteSheet, ogreSpriteSheet, ogreBossSpriteSheet, vampiroSpriteSheet, vampiroBossSpriteSheet;
        private static bool slimeFramesInitialized = false, slimeFogoFramesInitialized = false, ogreFramesInitialized = false, ogreBossFramesInitialized = false, vampiroFramesInitialized = false, vampiroBossFramesInitialized = false;
        private static List<Rectangle> slimeDownFrames, slimeUpFrames, slimeLeftFrames, slimeRightFrames;
        private static List<Rectangle> slimeFogoDownFrames, slimeFogoUpFrames, slimeFogoLeftFrames, slimeFogoRightFrames;
        private static List<Rectangle> ogreDownFrames, ogreUpFrames, ogreLeftFrames, ogreRightFrames;
        private static List<Rectangle> ogreBossDownFrames, ogreBossUpFrames, ogreBossLeftFrames, ogreBossRightFrames;
        private static List<Rectangle> vampiroDownFrames, vampiroUpFrames, vampiroLeftFrames, vampiroRightFrames;
        private static List<Rectangle> vampiroBossDownFrames, vampiroBossUpFrames, vampiroBossLeftFrames, vampiroBossRightFrames;

        // Controle de animação por instância
        private int animFrame = 0;
        private float animTimer = 0f;
        private float animInterval = 0.15f;
        private int animDirection = 0; // 0: down, 1: up, 2: left, 3: right

        // --- Fade ao morrer ---
        public float DeathFade { get; private set; } = 1f;
        public bool IsDying { get; private set; } = false;
        public void StartDying()
        {
            IsDying = true;
            DeathFade = 1f;
        }

        // Métodos estáticos para setar os spritesheets e inicializar frames
        public static void SetSlimeSpriteSheet(Texture2D texture)
        {
            slimeSpriteSheet = texture;
            if (!slimeFramesInitialized)
            {
                slimeDownFrames = new List<Rectangle>
                {
                    new Rectangle(24, 24, 17, 18), new Rectangle(88, 22, 17, 20), new Rectangle(152, 20, 18, 22),
                    new Rectangle(215, 18, 20, 24), new Rectangle(277, 17, 23, 25), new Rectangle(339, 17, 26, 25)
                };
                slimeUpFrames = new List<Rectangle>
                {
                    new Rectangle(24, 88, 17, 18), new Rectangle(88, 86, 17, 20), new Rectangle(151, 84, 18, 22),
                    new Rectangle(214, 82, 21, 24), new Rectangle(277, 81, 23, 25), new Rectangle(339, 81, 26, 25)
                };
                slimeLeftFrames = new List<Rectangle>
                {
                    new Rectangle(24, 153, 18, 17), new Rectangle(86, 150, 17, 20), new Rectangle(146, 148, 21, 22),
                    new Rectangle(210, 146, 21, 24), new Rectangle(270, 145, 26, 25), new Rectangle(337, 145, 25, 25)
                };
                slimeRightFrames = new List<Rectangle>
                {
                    new Rectangle(23, 217, 18, 17), new Rectangle(88, 214, 18, 20), new Rectangle(153, 212, 21, 22),
                    new Rectangle(217, 210, 21, 24), new Rectangle(280, 209, 26, 25), new Rectangle(342, 209, 25, 25)
                };
                slimeFramesInitialized = true;
            }
        }
        public static void SetSlimeFogoSpriteSheet(Texture2D texture)
        {
            slimeFogoSpriteSheet = texture;
            if (!slimeFogoFramesInitialized)
            {
                slimeFogoDownFrames = new List<Rectangle>
                {
                    new Rectangle(21, 20, 20, 22), new Rectangle(85, 16, 20, 26), new Rectangle(150, 14, 20, 28),
                    new Rectangle(215, 12, 21, 30), new Rectangle(277, 10, 23, 32), new Rectangle(339, 7, 26, 35)
                };
                slimeFogoUpFrames = new List<Rectangle>
                {
                    new Rectangle(24, 84, 20, 22), new Rectangle(88, 80, 20, 26), new Rectangle(151, 78, 20, 28),
                    new Rectangle(213,76, 22, 30), new Rectangle(277, 74, 23, 32), new Rectangle(339, 71, 26, 35)
                };
                slimeFogoLeftFrames = new List<Rectangle>
                {
                    new Rectangle(24, 145, 20, 25), new Rectangle(86, 140, 24, 30), new Rectangle(146, 139, 28, 31),
                    new Rectangle(210, 138, 29, 32), new Rectangle(270, 136, 26, 34), new Rectangle(337, 135, 25, 25)
                };
                slimeFogoRightFrames = new List<Rectangle>
                {
                    new Rectangle(21, 204, 20, 25), new Rectangle(82, 204, 24, 30), new Rectangle(146, 203, 28, 31),
                    new Rectangle(209, 202, 29, 32), new Rectangle(280, 200, 26, 34), new Rectangle(342, 199, 25, 35)
                };
                slimeFogoFramesInitialized = true;
            }
        }
        public static void SetOgreSpriteSheet(Texture2D texture)
        {
            ogreSpriteSheet = texture;
            if (!ogreFramesInitialized)
            {
                ogreDownFrames = new List<Rectangle>
                {
                    new Rectangle(10, 13, 37, 34), new Rectangle(72, 10, 38, 37), new Rectangle(138, 12, 37, 36),
                    new Rectangle(202, 13, 36, 34), new Rectangle(265, 10, 38, 36), new Rectangle(329, 9, 37, 37)
                };
                ogreUpFrames = new List<Rectangle>
                {
                    new Rectangle(20, 77, 34, 36), new Rectangle(82, 75, 37, 37), new Rectangle(148, 75, 35, 36),
                    new Rectangle(211, 78, 35, 33), new Rectangle(277, 76, 33, 34), new Rectangle(339, 76, 36, 37)
                };
                ogreLeftFrames = new List<Rectangle>
                {
                    new Rectangle(19, 142, 24, 34), new Rectangle(83, 140, 24, 36), new Rectangle(147, 140, 24, 36),
                    new Rectangle(212, 143, 25, 32), new Rectangle(276, 139, 23, 37), new Rectangle(341, 139, 20, 36)
                };
                ogreRightFrames = new List<Rectangle>
                {
                    new Rectangle(21, 204, 23, 36), new Rectangle(83, 203, 28, 36), new Rectangle(149, 204, 25, 37),
                    new Rectangle(213, 202, 26, 37), new Rectangle(276, 202, 25, 37), new Rectangle(339, 203, 31, 36)
                };
                ogreFramesInitialized = true;
            }
        }
        public static void SetOgreBossSpriteSheet(Texture2D texture)
        {
            ogreBossSpriteSheet = texture;
            if (!ogreBossFramesInitialized)
            {
                ogreBossDownFrames = new List<Rectangle>
                {
                    new Rectangle(9, 10, 37, 36), new Rectangle(73, 7, 37, 39), new Rectangle(137, 8, 37, 38),
                    new Rectangle(202, 10, 37, 36), new Rectangle(266, 7, 37, 39), new Rectangle(330, 8, 37, 38)
                };
                ogreBossUpFrames = new List<Rectangle>
                {
                    new Rectangle(18, 74, 37, 38), new Rectangle(82, 71, 37, 39), new Rectangle(146, 72, 37, 38),
                    new Rectangle(209, 74, 37, 36), new Rectangle(273, 71, 37, 39), new Rectangle(337, 72, 37, 38)
                };
                ogreBossLeftFrames = new List<Rectangle>
                {
                    new Rectangle(21, 134, 27, 40), new Rectangle(82, 135, 29, 39), new Rectangle(145, 139, 30, 40),
                    new Rectangle(209, 134, 31, 40), new Rectangle(274, 135, 29, 39), new Rectangle(339, 134, 28, 40)
                };
                ogreBossRightFrames = new List<Rectangle>
                {
                    new Rectangle(16, 198, 27, 40), new Rectangle(81, 199, 29, 39), new Rectangle(145, 198, 30, 40),
                    new Rectangle(208, 198, 32, 40), new Rectangle(273, 199, 29, 39), new Rectangle(337, 198, 28, 40)
                };
                ogreBossFramesInitialized = true;
            }
        }
        public static void SetVampiroSpriteSheet(Texture2D texture)
        {
            vampiroSpriteSheet = texture;
            if (!vampiroFramesInitialized)
            {
                vampiroDownFrames = new List<Rectangle>
                {
                    new Rectangle(20, 19, 23, 28), new Rectangle(85, 17, 21, 30), new Rectangle(147, 18, 25, 29),
                    new Rectangle(213, 19, 23, 28), new Rectangle(278, 17, 21, 30), new Rectangle(340, 18, 25, 29)
                };
                vampiroUpFrames = new List<Rectangle>
                {
                    new Rectangle(20, 83, 23, 28), new Rectangle(85, 81, 21, 30), new Rectangle(147, 82, 25, 29),
                    new Rectangle(213,83, 23, 28), new Rectangle(278, 81, 21, 30), new Rectangle(340, 82, 25, 29)
                };
                vampiroLeftFrames = new List<Rectangle>
                {
                    new Rectangle(22, 147, 20, 28), new Rectangle(86, 145, 18, 30), new Rectangle(150, 146, 21, 29),
                    new Rectangle(214, 147, 21, 28), new Rectangle(278, 145, 19, 30), new Rectangle(342, 146, 22, 29)
                };
                vampiroRightFrames = new List<Rectangle>
                {
                    new Rectangle(21, 211, 20, 28), new Rectangle(87, 209, 18, 30), new Rectangle(148, 210, 21, 29),
                    new Rectangle(212, 211, 21, 28), new Rectangle(278, 209, 19, 30), new Rectangle(339, 210, 22, 29)
                };
                vampiroFramesInitialized = true;
            }
        }
        public static void SetVampiroBossSpriteSheet(Texture2D texture)
        {
            vampiroBossSpriteSheet = texture;
            if (!vampiroBossFramesInitialized)
            {
                vampiroBossDownFrames = new List<Rectangle>
                {
                    new Rectangle(12, 19, 39, 28), new Rectangle(76, 17, 39, 30), new Rectangle(140, 18, 39, 29),
                    new Rectangle(205, 19, 39, 28), new Rectangle(269, 17, 39, 30), new Rectangle(333, 18, 39, 29)
                };
                vampiroBossUpFrames = new List<Rectangle>
                {
                    new Rectangle(12, 83, 39, 28), new Rectangle(76, 81, 39, 30), new Rectangle(140, 82, 39, 29),
                    new Rectangle(205,83, 39, 28), new Rectangle(269, 81, 39, 30), new Rectangle(333, 82, 39, 29)
                };
                vampiroBossLeftFrames = new List<Rectangle>
                {
                    new Rectangle(21, 146, 28, 29), new Rectangle(85, 144, 28, 31), new Rectangle(149, 145, 28, 30),
                    new Rectangle(213, 146, 28, 29), new Rectangle(277, 144, 28, 31), new Rectangle(341, 145, 28, 30)
                };
                vampiroBossRightFrames = new List<Rectangle>
                {
                    new Rectangle(14, 210, 28, 29), new Rectangle(78, 208, 28, 31), new Rectangle(142, 209, 28, 31),
                    new Rectangle(206, 210, 28, 29), new Rectangle(270, 208, 28, 31), new Rectangle(334, 209, 28, 30)
                };
                vampiroBossFramesInitialized = true;
            }
        }

        // Construtor atualizado para stats escaláveis
        public Enemy(Vector2 position, float speed, int health, EnemyType type, int phase = 1)
        {
            Position = position;
            Speed = speed;
            Health = health;
            Type = type;

            // Set color/size based on type
            switch (type)
            {
                case EnemyType.SlimeFogo: Color = Color.Orange; break;
                case EnemyType.Ogre: Color = Color.Green; Size = 40; break;
                case EnemyType.OgreBoss: Color = Color.DarkGreen; Size = 64; break;
                case EnemyType.Vampiro: Color = Color.Purple; Size = 36; break;
                case EnemyType.VampiroBoss: Color = Color.Gold; Size = 64; break;
                default: Color = Color.Red; break;
            }



            // Stats de ataque e dificuldade progressiva
            // Exemplo para cada tipo, ajuste conforme necessário:
            switch (type)
            {
                case EnemyType.Slime:
                    Health = 8 + phase * 2;
                    break;
                case EnemyType.SlimeFogo:
                    Health = 12 + phase * 3;
                    break;
                case EnemyType.Ogre:
                    Health = 20 + phase * 4;
                    break;
                case EnemyType.OgreBoss:
                    Health = 200 + phase * 20;
                    break;
                case EnemyType.Vampiro:
                    Health = 16 + phase * 3;
                    break;
                case EnemyType.VampiroBoss:
                    Health = 250 + phase * 20;
                    break;
                default:
                    Health = 10 + phase * 2;
                    break;
            }

        }

        public void Update(Vector2 playerPosition, GameTime gameTime, Texture2D projectileTexture, int phase =1, List<Enemy> allEnemies = null)
        {
            // Fade ao morrer
            if (IsDying)
            {
                DeathFade -= (float)gameTime.ElapsedGameTime.TotalSeconds * 2f;
                if (DeathFade < 0f) DeathFade = 0f;
                return;
            }

            if (!IsAlive) return;

            float distanceToPlayer = Vector2.Distance(Position, playerPosition);

            // --- Animação para cada tipo ---
            if (Type == EnemyType.Slime && slimeSpriteSheet != null)
                UpdateAnimation(playerPosition, gameTime, 6);
            else if (Type == EnemyType.SlimeFogo && slimeFogoSpriteSheet != null)
                UpdateAnimation(playerPosition, gameTime, 6);
            else if (Type == EnemyType.Ogre && ogreSpriteSheet != null)
                UpdateAnimation(playerPosition, gameTime, 6);
            else if (Type == EnemyType.OgreBoss && ogreBossSpriteSheet != null)
                UpdateAnimation(playerPosition, gameTime, 6);
            else if (Type == EnemyType.Vampiro && vampiroSpriteSheet != null)
                UpdateAnimation(playerPosition, gameTime, 6);
            else if (Type == EnemyType.VampiroBoss && vampiroBossSpriteSheet != null)
                UpdateAnimation(playerPosition, gameTime, 6);

            // Todos os inimigos podem disparar projéteis
            HandleRangedAttack(playerPosition, gameTime, projectileTexture, phase);

            switch (Type)
            {
                case EnemyType.Slime:
                case EnemyType.SlimeFogo:
                    if (Health <= 1 && distanceToPlayer < 200)
                        FleeFrom(playerPosition, gameTime, 1.2f);
                    else
                        MoveTowards(playerPosition, gameTime, 1.0f);
                    break;

                case EnemyType.Ogre:
                    if (distanceToPlayer > 250)
                        Patrol(gameTime, 0.7f);
                    else
                        MoveTowards(playerPosition, gameTime, 0.9f);
                    break;

                case EnemyType.OgreBoss:
                    specialTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    minionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (specialTimer > 8f)
                    {
                        Vector2 jumpOffset = new Vector2(rng.Next(-120, 120), rng.Next(-120, 120));
                        Position = playerPosition + jumpOffset;
                        specialTimer = 0f;
                    }
                    if (allEnemies != null && minionTimer > minionCooldown)
                    {
                        Vector2 minionPos = Position + new Vector2(rng.Next(-40, 40), rng.Next(-40, 40));
                        allEnemies.Add(new Enemy(minionPos, 90, 2, EnemyType.Ogre));
                        minionTimer = 0f;
                    }
                    MoveTowards(playerPosition, gameTime, 0.5f);
                    break;

                case EnemyType.Vampiro:
                    if (distanceToPlayer < 120)
                        FleeFrom(playerPosition, gameTime, 1.2f);
                    else if (distanceToPlayer < 300)
                        CirclePlayer(playerPosition, gameTime, 0.8f);
                    else
                        MoveTowards(playerPosition, gameTime, 0.7f);
                    break;

                case EnemyType.VampiroBoss:
                    specialTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (distanceToPlayer < 140 && specialTimer > 6f)
                    {
                        FleeFrom(playerPosition, gameTime, 2.5f);
                        specialTimer = 0f;
                    }
                    if (specialTimer > 7f)
                    {
                        FireRadialPattern(projectileTexture);
                        specialTimer = 0f;
                    }
                    if (distanceToPlayer < 180)
                        FleeFrom(playerPosition, gameTime, 1.1f);
                    else
                        MoveTowards(playerPosition, gameTime, 0.5f);
                    break;

                default:
                    MoveTowards(playerPosition, gameTime);
                    break;
            }

            foreach (var proj in Projectiles)
                proj.Update(gameTime);

            Projectiles.RemoveAll(p => !p.IsActive ||
                p.Position.X < -p.Size || p.Position.X > 2000 ||
                p.Position.Y < -p.Size || p.Position.Y > 2000);
        }


        private void UpdateAnimation(Vector2 playerPosition, GameTime gameTime, int frameCount)
        {
            Vector2 moveDir = playerPosition - Position;
            if (Math.Abs(moveDir.X) > Math.Abs(moveDir.Y))
                animDirection = moveDir.X > 0 ? 3 : 2;
            else
                animDirection = moveDir.Y > 0 ? 0 : 1;

            animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (animTimer >= animInterval)
            {
                animFrame = (animFrame + 1) % frameCount;
                animTimer = 0f;
            }
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || IsDying) return;
            Health -= amount;
            if (Health <= 0)
            {
                Health = 0;
                IsAlive = false;
            }
        }

        private void Patrol(GameTime gameTime, float speedMultiplier = 1.0f)
        {
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
            Vector2 toPlayer = playerPosition - Position;
            if (toPlayer == Vector2.Zero) return;
            toPlayer.Normalize();
            Vector2 perpendicular = new Vector2(-toPlayer.Y, toPlayer.X);

            Position += perpendicular * Speed * speedMultiplier * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        private void FireRadialPattern(Texture2D projectileTexture)
        {
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

        private void HandleRangedAttack(Vector2 playerPosition, GameTime gameTime, Texture2D projectileTexture, int phase = 1)
        {
            // Só dispara projéteis se a fase for maior que 1
            if (phase <= 1)
                return;

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
            Color fadeColor = Color.White * DeathFade;

            if (IsAlive || IsDying)
            {
                // Slime
                if (Type == EnemyType.Slime && slimeSpriteSheet != null)
                {
                    Rectangle sourceRect = slimeDownFrames[animFrame];
                    if (animDirection == 0) sourceRect = slimeDownFrames[animFrame];
                    else if (animDirection == 1) sourceRect = slimeUpFrames[animFrame];
                    else if (animDirection == 2) sourceRect = slimeLeftFrames[animFrame];
                    else if (animDirection == 3) sourceRect = slimeRightFrames[animFrame];

                    spriteBatch.Draw(slimeSpriteSheet, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), sourceRect, fadeColor);
                }
                // SlimeFogo
                else if (Type == EnemyType.SlimeFogo && slimeFogoSpriteSheet != null)
                {
                    Rectangle sourceRect = slimeFogoDownFrames[animFrame];
                    if (animDirection == 0) sourceRect = slimeFogoDownFrames[animFrame];
                    else if (animDirection == 1) sourceRect = slimeFogoUpFrames[animFrame];
                    else if (animDirection == 2) sourceRect = slimeFogoLeftFrames[animFrame];
                    else if (animDirection == 3) sourceRect = slimeFogoRightFrames[animFrame];

                    spriteBatch.Draw(slimeFogoSpriteSheet, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), sourceRect, fadeColor);
                }
                // Ogre
                else if (Type == EnemyType.Ogre && ogreSpriteSheet != null)
                {
                    Rectangle sourceRect = ogreDownFrames[animFrame];
                    if (animDirection == 0) sourceRect = ogreDownFrames[animFrame];
                    else if (animDirection == 1) sourceRect = ogreUpFrames[animFrame];
                    else if (animDirection == 2) sourceRect = ogreLeftFrames[animFrame];
                    else if (animDirection == 3) sourceRect = ogreRightFrames[animFrame];

                    spriteBatch.Draw(ogreSpriteSheet, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), sourceRect, fadeColor);
                }
                // OgreBoss
                else if (Type == EnemyType.OgreBoss && ogreBossSpriteSheet != null)
                {
                    Rectangle sourceRect = ogreBossDownFrames[animFrame];
                    if (animDirection == 0) sourceRect = ogreBossDownFrames[animFrame];
                    else if (animDirection == 1) sourceRect = ogreBossUpFrames[animFrame];
                    else if (animDirection == 2) sourceRect = ogreBossLeftFrames[animFrame];
                    else if (animDirection == 3) sourceRect = ogreBossRightFrames[animFrame];

                    spriteBatch.Draw(ogreBossSpriteSheet, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), sourceRect, fadeColor);
                }
                // Vampiro
                else if (Type == EnemyType.Vampiro && vampiroSpriteSheet != null)
                {
                    Rectangle sourceRect = vampiroDownFrames[animFrame];
                    if (animDirection == 0) sourceRect = vampiroDownFrames[animFrame];
                    else if (animDirection == 1) sourceRect = vampiroUpFrames[animFrame];
                    else if (animDirection == 2) sourceRect = vampiroLeftFrames[animFrame];
                    else if (animDirection == 3) sourceRect = vampiroRightFrames[animFrame];

                    spriteBatch.Draw(vampiroSpriteSheet, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), sourceRect, fadeColor);
                }
                // VampiroBoss
                else if (Type == EnemyType.VampiroBoss && vampiroBossSpriteSheet != null)
                {
                    Rectangle sourceRect = vampiroBossDownFrames[animFrame];
                    if (animDirection == 0) sourceRect = vampiroBossDownFrames[animFrame];
                    else if (animDirection == 1) sourceRect = vampiroBossUpFrames[animFrame];
                    else if (animDirection == 2) sourceRect = vampiroBossLeftFrames[animFrame];
                    else if (animDirection == 3) sourceRect = vampiroBossRightFrames[animFrame];

                    spriteBatch.Draw(vampiroBossSpriteSheet, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), sourceRect, fadeColor);
                }
                else
                {
                    spriteBatch.Draw(texture, new Rectangle((int)Position.X, (int)Position.Y, Size, Size), fadeColor);
                }
            }

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
