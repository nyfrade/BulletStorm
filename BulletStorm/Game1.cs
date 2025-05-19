using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BulletStorm.Entities;
using BulletStorm.Managers;
using BulletStorm.GameState;

namespace BulletStorm
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Entidades principais
        private Player player;
        private Texture2D playerTexture;
        private Texture2D enemyTexture;
        private Texture2D projectileTexture;
        private Texture2D portalTexture;
        private Texture2D coinTexture; // Add a texture for coins
        private Portal portal;

        // Gerenciador de níveis
        private LevelManager levelManager = new();

        // Estado do jogo
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
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            playerTexture = Content.Load<Texture2D>("player");
            enemyTexture = new Texture2D(GraphicsDevice, 1, 1);
            enemyTexture.SetData(new[] { Color.White });

            projectileTexture = new Texture2D(GraphicsDevice, 1, 1);
            projectileTexture.SetData(new[] { Color.White });

            portalTexture = new Texture2D(GraphicsDevice, 1, 1);
            portalTexture.SetData(new[] { Color.Black });

            coinTexture = new Texture2D(GraphicsDevice, 1, 1); // Simple placeholder
            coinTexture.SetData(new[] { Color.Gold });

            // Inicializa o player no centro
            player = new Player(new Vector2(
                GraphicsDevice.Viewport.Width / 2f - playerTexture.Width / 2f,
                GraphicsDevice.Viewport.Height / 2f - playerTexture.Height / 2f
            ));

            // Inicializa o portal (posição será definida ao fim do nível)
            portal = new Portal(
                new Rectangle(
                    (int)(GraphicsDevice.Viewport.Width / 2 - 32),
                    (int)(GraphicsDevice.Viewport.Height / 2 - 32),
                    64, 64
                ),
                portalTexture
            );
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

            // --- COIN PICKUP LOGIC ---
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
                    // Optionally: play sound or show effect
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
            levelManager.Coins.Clear(); // Clear coins when entering safe house
            portal.Active = false;
            currentPhase = Phase.Phase1;

            // Enter para começar o nível atual
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                currentState = GameState.GameState.CombatPhase1;
                currentPhase = Phase.Phase1;
            }

            // Se portal está ativo, permite trocar de nível
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
                        // Advanced AI and boss minion spawning
                        levelManager.Enemies[i].Update(player.Position, gameTime, projectileTexture, levelManager.Enemies);
                        if (!levelManager.Enemies[i].IsAlive)
                        {
                            // --- COIN DROP LOGIC ---
                            for (int c = 0; c < levelManager.Enemies[i].CoinsDropped; c++)
                            {
                                // Spread coins a bit randomly
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
                            // Advanced AI and boss minion spawning
                            levelManager.Enemies[i].Update(player.Position, gameTime, projectileTexture, levelManager.Enemies);
                            if (!levelManager.Enemies[i].IsAlive)
                            {
                                // --- COIN DROP LOGIC ---
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
                            // Advanced AI and boss minion spawning
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

        // Usa o LevelManager e Phase para decidir o tipo de inimigo a spawnar
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

            player.Draw(_spriteBatch, playerTexture);

            foreach (var enemy in levelManager.Enemies)
                enemy.Draw(_spriteBatch, enemyTexture, projectileTexture);

            // Draw coins
            foreach (var coin in levelManager.Coins)
                coin.Draw(_spriteBatch, coinTexture);

            portal.Draw(_spriteBatch);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
