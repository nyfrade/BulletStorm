using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BulletStorm.Entities;
using BulletStorm.Managers;
using BulletStorm.GameState;
using BulletStorm.Map;

namespace BulletStorm
{
    public class Game1 : Game
    {
        private Map.ProceduralMapGenerator mapGenerator;
        private Dictionary<TileType, Texture2D> tileTextures;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private List<Quest> quests = new();
        private int projectileHits = 0; // For Sharpshooter quest
        private double quickDrawTimer = 0; // For Quick Draw quest
        private int quickDrawKills = 0;


        // Entities
        private Player player;
        private Texture2D playerTexture;
        private Texture2D enemyTexture;
        private Texture2D projectileTexture;
        private Texture2D portalTexture;
        private Texture2D coinTexture;

        // Weapon textures
        private Texture2D[] swordTextures = new Texture2D[7];
        private Texture2D[] gunTextures = new Texture2D[3];

        // Weapons and projectiles
        private List<Weapon> weapons = new();
        private List<Weapon> playerWeapons = new();
        private List<PlayerProjectile> playerProjectiles = new();

        private Portal portal;

        // Level manager
        private LevelManager levelManager = new();

        // Game state
        private GameState.GameState currentState = GameState.GameState.SafeHouse;
        private Phase currentPhase = Phase.Phase1;

        // Spawn
        private Random random = new();
        private float spawnTimer = 0f;
        private float spawnInterval = 1.0f;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            // Carregue as texturas dos tiles
            tileTextures = new Dictionary<TileType, Texture2D>
            {
                [TileType.Grass] = Content.Load<Texture2D>("grass"), // Adapte para o nome do seu asset
                [TileType.Tree] = Content.Load<Texture2D>("tree"),
                [TileType.Bush] = Content.Load<Texture2D>("bush"),
                [TileType.Rock] = Content.Load<Texture2D>("rock"),
                // ...adicione os restantes
            };

            // Gere o mapa procedural
            mapGenerator = new Map.ProceduralMapGenerator(30, 20);
            mapGenerator.Generate();
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            playerTexture = Content.Load<Texture2D>("player");
            enemyTexture = new Texture2D(GraphicsDevice, 1, 1);
            enemyTexture.SetData(new[] { Color.White });

            projectileTexture = new Texture2D(GraphicsDevice, 1, 1);
            projectileTexture.SetData(new[] { Color.White });

            portalTexture = new Texture2D(GraphicsDevice, 1, 1);
            portalTexture.SetData(new[] { Color.Black });

            coinTexture = new Texture2D(GraphicsDevice, 1, 1);
            coinTexture.SetData(new[] { Color.Gold });

            // Load weapon textures (replace with your actual asset names)
            for (int i = 0; i < 7; i++)
                swordTextures[i] = Content.Load<Texture2D>($"sword{i + 1}");
            for (int i = 0; i < 3; i++)
                gunTextures[i] = Content.Load<Texture2D>($"gun{i + 1}");

            // Create all weapons
            weapons = Weapon.CreateAllWeapons(swordTextures, gunTextures);

            // Player starts with the three basic weapons
            playerWeapons.Add(weapons[0]); // Blaze Edge
            playerWeapons.Add(weapons[1]); // Frostbite Saber
            playerWeapons.Add(weapons[2]); // Pulse Blaster

            // Initialize player in the center
            player = new Player(new Vector2(
                GraphicsDevice.Viewport.Width / 2f - playerTexture.Width / 2f,
                GraphicsDevice.Viewport.Height / 2f - playerTexture.Height / 2f
            ));

            portal = new Portal(
                new Rectangle(
                    (int)(GraphicsDevice.Viewport.Width / 2 - 32),
                    (int)(GraphicsDevice.Viewport.Height / 2 - 32),
                    64, 64
                ),
                portalTexture
            );
            // Add 10 original quests
            quests.Add(new Quest(
                "Slime Slayer",
                "Kill 20 Slimes (Reward: Thunderbrand)",
                (lm, p, allWeapons) => lm.EnemiesKilled >= 20,
                (p, allWeapons, playerWeapons) => {
                    var w = allWeapons.Find(w => w.Name == "Thunderbrand");
                    if (w != null && !playerWeapons.Contains(w)) playerWeapons.Add(w);
                }
            ));
            quests.Add(new Quest(
                "Ogre Breaker",
                "Kill 10 Ogres (Reward: Venomspike)",
                (lm, p, allWeapons) => lm.Enemies.FindAll(e => e.Type == EnemyType.Ogre).Count >= 10,
                (p, allWeapons, playerWeapons) => {
                    var w = allWeapons.Find(w => w.Name == "Venomspike");
                    if (w != null && !playerWeapons.Contains(w)) playerWeapons.Add(w);
                }
            ));
            quests.Add(new Quest(
                "Coin Collector",
                "Collect 50 Coins (Reward: +2 Max Health)",
                (lm, p, allWeapons) => p.Coins >= 50,
                (p, allWeapons, playerWeapons) => p.UpgradeHealth(2)
            ));
            quests.Add(new Quest(
                "Boss Hunter",
                "Defeat 1 Boss (Reward: Void Cannon)",
                (lm, p, allWeapons) => lm.Enemies.FindAll(e => e.Type == EnemyType.OgreBoss || e.Type == EnemyType.VampiroBoss).Count == 0 && lm.BossSpawned,
                (p, allWeapons, playerWeapons) => {
                    var w = allWeapons.Find(w => w.Name == "Void Cannon");
                    if (w != null && !playerWeapons.Contains(w)) playerWeapons.Add(w);
                }
            ));
            quests.Add(new Quest(
                "Untouchable",
                "Survive a phase without taking damage (Reward: +0.2 Attack Speed)",
                (lm, p, allWeapons) => p.Health == p.MaxHealth && currentPhase == Phase.LevelComplete,
                (p, allWeapons, playerWeapons) => p.UpgradeAttackSpeed(0.2f)
            ));
            quests.Add(new Quest(
                "Sharpshooter",
                "Land 30 projectile hits (Reward: Star Piercer)",
                (lm, p, allWeapons) => projectileHits >= 30,
                (p, allWeapons, playerWeapons) => {
                    var w = allWeapons.Find(w => w.Name == "Star Piercer");
                    if (w != null && !playerWeapons.Contains(w)) playerWeapons.Add(w);
                }
            ));
            quests.Add(new Quest(
                "Quick Draw",
                "Kill 10 enemies in 30 seconds (Reward: +20 Move Speed)",
                (lm, p, allWeapons) => quickDrawKills >= 10,
                (p, allWeapons, playerWeapons) => p.UpgradeSpeed(20f)
            ));
            quests.Add(new Quest(
                "Vampire Vanquisher",
                "Kill 15 Vampiro or VampiroWarrior (Reward: Celestial Katana)",
                (lm, p, allWeapons) => lm.Enemies.FindAll(e => e.Type == EnemyType.Vampiro || e.Type == EnemyType.VampiroWarrior).Count >= 15,
                (p, allWeapons, playerWeapons) => {
                    var w = allWeapons.Find(w => w.Name == "Celestial Katana");
                    if (w != null && !playerWeapons.Contains(w)) playerWeapons.Add(w);
                }
            ));
            quests.Add(new Quest(
                "Wealth Hoarder",
                "Have 100 coins at once (Reward: +20 Coin Pickup Range)",
                (lm, p, allWeapons) => p.Coins >= 100,
                (p, allWeapons, playerWeapons) => p.UpgradeCoinPickupRange(20f)
            ));
            quests.Add(new Quest(
                "Master of Arms",
                "Unlock 5 different weapons (Reward: Shadow Reaver)",
                (lm, p, allWeapons) => playerWeapons.Count >= 5,
                (p, allWeapons, playerWeapons) => {
                    var w = allWeapons.Find(w => w.Name == "Shadow Reaver");
                    if (w != null && !playerWeapons.Contains(w)) playerWeapons.Add(w);
                }
            ));

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            switch (currentState)
            {
                case GameState.GameState.SafeHouse:
                    UpdateSafeHouse(gameTime);
                    break;
                case GameState.GameState.CombatPhase1:
                case GameState.GameState.WeaponChoice:
                case GameState.GameState.CombatPhase3:
                    UpdateLevelPhases(gameTime);
                    break;
                case GameState.GameState.LevelComplete:
                    UpdateLevelComplete(gameTime);
                    break;
            }

            // Coin pickup logic
            for (int i = levelManager.Coins.Count - 1; i >= 0; i--)
            {
                var coin = levelManager.Coins[i];
                float distance = Vector2.Distance(
                    player.Position + new Vector2(playerTexture.Width / 2, playerTexture.Height / 2),
                    coin.Position + new Vector2(coin.Size / 2, coin.Size / 2)
                );
                if (!coin.Collected && distance <= player.CoinPickupRange)
                {
                    player.Coins += coin.Value;
                    coin.Collected = true;
                    levelManager.Coins.RemoveAt(i);
                }
            }

            // --- Weapon orbit and firing logic ---
            foreach (var weapon in playerWeapons)
            {
                weapon.Update(player.Position, (float)gameTime.ElapsedGameTime.TotalSeconds);

                weapon.FireTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                float fireInterval = 1f / weapon.FireRate;

                if (weapon.FireTimer >= fireInterval)
                {
                    // Find nearest enemy
                    Enemy nearest = null;
                    float minDist = float.MaxValue;
                    foreach (var enemy in levelManager.Enemies)
                    {
                        if (!enemy.IsAlive) continue;
                        float dist = Vector2.Distance(player.Position + weapon.Offset, enemy.Position + new Vector2(enemy.Size / 2, enemy.Size / 2));
                        if (dist < minDist)
                        {
                            minDist = dist;
                            nearest = enemy;
                        }
                    }

                    if (nearest != null)
                    {
                        Vector2 firePos = player.Position + weapon.Offset;
                        Vector2 target = nearest.Position + new Vector2(nearest.Size / 2, nearest.Size / 2);
                        Vector2 direction = target - firePos;
                        if (direction != Vector2.Zero)
                            direction.Normalize();

                        playerProjectiles.Add(new PlayerProjectile(
                            firePos,
                            direction,
                            weapon.ProjectileSpeed,
                            weapon.Damage,
                            weapon.CritChance,
                            weapon.EffectDuration,
                            weapon.EffectDescription,
                            weapon.OnHitEffect
                        ));
                    }
                    weapon.FireTimer = 0f;
                }
            }

            // --- Update and collision for player projectiles ---
            for (int i = playerProjectiles.Count - 1; i >= 0; i--)
            {
                var proj = playerProjectiles[i];
                proj.Update(gameTime);
                proj.CheckCollisionWithEnemies(levelManager.Enemies, random);

                // Remove if not active or off-screen
                if (!proj.IsActive ||
                    proj.Position.X < -proj.Size || proj.Position.X > GraphicsDevice.Viewport.Width + proj.Size ||
                    proj.Position.Y < -proj.Size || proj.Position.Y > GraphicsDevice.Viewport.Height + proj.Size)
                {
                    playerProjectiles.RemoveAt(i);
                }
            }

            base.Update(gameTime);
        }

        private void UpdateSafeHouse(GameTime gameTime)
        {
            levelManager.Enemies.Clear();
            levelManager.EnemiesKilled = 0;
            levelManager.Phase3Kills = 0;
            levelManager.BossSpawned = false;
            levelManager.Coins.Clear();
            portal.Active = false;
            currentPhase = Phase.Phase1;

            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                currentState = GameState.GameState.CombatPhase1;
                currentPhase = Phase.Phase1;
            }

            if (portal.Active)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.N))
                {
                    if (levelManager.CurrentLevel < 2)
                        levelManager.CurrentLevel++;
                    currentState = GameState.GameState.SafeHouse;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.B))
                {
                    if (levelManager.CurrentLevel > 1)
                        levelManager.CurrentLevel--;
                    currentState = GameState.GameState.SafeHouse;
                }
            }
        }

        private void UpdateLevelPhases(GameTime gameTime)
        {
            switch (currentPhase)
            {
                case Phase.Phase1:
                    UpdatePlayerMovement(gameTime);
                    spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (spawnTimer >= spawnInterval && levelManager.Enemies.Count < 10)
                    {
                        SpawnRandomEnemy();
                        spawnTimer = 0f;
                    }
                    for (int i = levelManager.Enemies.Count - 1; i >= 0; i--)
                    {
                        levelManager.Enemies[i].Update(player.Position, gameTime, projectileTexture, levelManager.Enemies);
                        if (!levelManager.Enemies[i].IsAlive)
                        {
                            for (int c = 0; c < levelManager.Enemies[i].CoinsDropped; c++)
                            {
                                Vector2 coinPos = levelManager.Enemies[i].Position + new Vector2(random.Next(-8, 8), random.Next(-8, 8));
                                levelManager.Coins.Add(new Coin(coinPos));
                            }
                            levelManager.Enemies.RemoveAt(i);
                            levelManager.EnemiesKilled++;
                        }
                    }
                    if (levelManager.EnemiesKilled >= 50)
                    {
                        levelManager.Enemies.Clear();
                        currentPhase = Phase.WeaponChoice;
                        currentState = GameState.GameState.WeaponChoice;
                    }
                    break;

                case Phase.WeaponChoice:
                    var state = Keyboard.GetState();
                    if (state.IsKeyDown(Keys.D1) || state.IsKeyDown(Keys.D2) || state.IsKeyDown(Keys.D3))
                    {
                        currentPhase = Phase.Phase3;
                        currentState = GameState.GameState.CombatPhase3;
                        levelManager.Phase3Kills = 0;
                        levelManager.BossSpawned = false;
                        levelManager.Enemies.Clear();
                    }
                    break;

                case Phase.Phase3:
                    UpdatePlayerMovement(gameTime);
                    if (levelManager.Phase3Kills < 100)
                    {
                        spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (spawnTimer >= spawnInterval && levelManager.Enemies.Count < 10)
                        {
                            SpawnRandomEnemy();
                            spawnTimer = 0f;
                        }
                        for (int i = levelManager.Enemies.Count - 1; i >= 0; i--)
                        {
                            levelManager.Enemies[i].Update(player.Position, gameTime, projectileTexture, levelManager.Enemies);
                            if (!levelManager.Enemies[i].IsAlive)
                            {
                                for (int c = 0; c < levelManager.Enemies[i].CoinsDropped; c++)
                                {
                                    Vector2 coinPos = levelManager.Enemies[i].Position + new Vector2(random.Next(-8, 8), random.Next(-8, 8));
                                    levelManager.Coins.Add(new Coin(coinPos));
                                }
                                levelManager.Enemies.RemoveAt(i);
                                levelManager.Phase3Kills++;
                            }
                        }
                    }
                    else if (!levelManager.BossSpawned)
                    {
                        Vector2 bossPos = new Vector2(GraphicsDevice.Viewport.Width / 2, 50);
                        EnemyType bossType = (levelManager.CurrentLevel == 1) ? EnemyType.OgreBoss : EnemyType.VampiroBoss;
                        levelManager.Enemies.Add(new Enemy(bossPos, 40f, 100, bossType));
                        levelManager.BossSpawned = true;
                    }
                    else
                    {
                        for (int i = levelManager.Enemies.Count - 1; i >= 0; i--)
                        {
                            levelManager.Enemies[i].Update(player.Position, gameTime, projectileTexture, levelManager.Enemies);
                            if (!levelManager.Enemies[i].IsAlive)
                            {
                                for (int c = 0; c < levelManager.Enemies[i].CoinsDropped; c++)
                                {
                                    Vector2 coinPos = levelManager.Enemies[i].Position + new Vector2(random.Next(-8, 8), random.Next(-8, 8));
                                    levelManager.Coins.Add(new Coin(coinPos));
                                }
                                levelManager.Enemies.RemoveAt(i);
                                currentPhase = Phase.LevelComplete;
                                currentState = GameState.GameState.LevelComplete;
                                portal.Active = true;
                                portal.Rect = new Rectangle(
                                    (int)(GraphicsDevice.Viewport.Width / 2 - 32),
                                    (int)(GraphicsDevice.Viewport.Height / 2 - 32),
                                    64, 64);
                            }
                        }
                    }
                    break;
            }
        }

        private void UpdateLevelComplete(GameTime gameTime)
        {
            UpdatePlayerMovement(gameTime);

            Rectangle playerRect = new Rectangle(
                (int)player.Position.X, (int)player.Position.Y,
                playerTexture.Width, playerTexture.Height);

            if (portal.Active && portal.Intersects(playerRect))
            {
                var state = Keyboard.GetState();
                if (state.IsKeyDown(Keys.N) && levelManager.CurrentLevel < 2)
                {
                    levelManager.CurrentLevel++;
                    currentState = GameState.GameState.SafeHouse;
                }
                else if (state.IsKeyDown(Keys.B) && levelManager.CurrentLevel > 1)
                {
                    levelManager.CurrentLevel--;
                    currentState = GameState.GameState.SafeHouse;
                }
            }
        }

        private void UpdatePlayerMovement(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();
            Vector2 movement = Vector2.Zero;

            if (keyboardState.IsKeyDown(Keys.W)) movement.Y -= 1;
            if (keyboardState.IsKeyDown(Keys.S)) movement.Y += 1;
            if (keyboardState.IsKeyDown(Keys.A)) movement.X -= 1;
            if (keyboardState.IsKeyDown(Keys.D)) movement.X += 1;

            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            player.Move(movement, delta, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, playerTexture.Width, playerTexture.Height);
        }

        private void SpawnRandomEnemy()
        {
            EnemyType[] types;

            if (levelManager.CurrentLevel == 1)
            {
                if (currentPhase == Phase.Phase1)
                {
                    types = new EnemyType[]
                    {
                        EnemyType.Slime, EnemyType.SlimeAgua, EnemyType.SlimeFogo
                    };
                }
                else if (currentPhase == Phase.WeaponChoice)
                {
                    types = new EnemyType[]
                    {
                        EnemyType.Slime, EnemyType.SlimeAgua, EnemyType.SlimeFogo,
                        EnemyType.Ogre, EnemyType.OgreWarrior
                    };
                }
                else // Phase3
                {
                    types = new EnemyType[]
                    {
                        EnemyType.Slime, EnemyType.SlimeAgua, EnemyType.SlimeFogo,
                        EnemyType.Ogre, EnemyType.OgreWarrior,
                        EnemyType.Vampiro, EnemyType.VampiroWarrior
                    };
                }
            }
            else // currentLevel == 2
            {
                types = new EnemyType[]
                {
                    EnemyType.Slime, EnemyType.SlimeAgua, EnemyType.SlimeFogo,
                    EnemyType.Ogre, EnemyType.OgreWarrior,
                    EnemyType.Vampiro, EnemyType.VampiroWarrior
                };
            }

            EnemyType type = types[random.Next(types.Length)];
            Vector2 pos = new Vector2(
                random.Next(0, GraphicsDevice.Viewport.Width - 32),
                random.Next(0, GraphicsDevice.Viewport.Height - 32)
            );
            float speed = 80f + random.Next(-20, 20);
            int health = 2 + random.Next(3);

            levelManager.Enemies.Add(new Enemy(pos, speed, health, type));
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            // Desenhe o mapa procedural
            for (int x = 0; x < mapGenerator.Width; x++)
            {
                for (int y = 0; y < mapGenerator.Height; y++)
                {
                    var tile = mapGenerator.Tiles[x, y];
                    if (tile.Type != TileType.Empty)
                    {
                        _spriteBatch.Draw(tileTextures[tile.Type], tile.Position, Color.White);
                    }
                }
            }

            player.Draw(_spriteBatch, playerTexture);

            foreach (var weapon in playerWeapons)
                weapon.Draw(_spriteBatch, player.Position);

            foreach (var proj in playerProjectiles)
                proj.Draw(_spriteBatch, projectileTexture);

            foreach (var enemy in levelManager.Enemies)
                enemy.Draw(_spriteBatch, enemyTexture, projectileTexture);

            foreach (var coin in levelManager.Coins)
                coin.Draw(_spriteBatch, coinTexture);

            portal.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
