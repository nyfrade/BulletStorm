using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media; 
using BulletStorm.Entities;
using BulletStorm.Managers;
using BulletStorm.GameState;
using Teste;
using System.IO;



namespace BulletStorm
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private GameplayScreen _gameplayScreen;
        private string[] creditLines;

        Song song;

        // --- MENU E SCORES ---
        private enum MainMenuState { Menu, InputName, Playing, Scores, Credits }
        private MainMenuState menuState = MainMenuState.Menu;
        private MouseState previousMouseState;
        private int selectedMenuIndex = 0;
        private string[] menuOptions = new[] { "Start", "Scores", "Creditos" };
        private string sessionName = "";
        private string inputSessionName = "";
        private List<(string Name, int Score, int Enemies, int Time)> scores = new();
        private bool showInputCursor = true;
        private double cursorBlinkTimer = 0;

        private double gameTimer = 0;      // Timer que reinicia ao morrer
        private double sessionTimer = 0;   // Timer da sessão inteira

        private KeyboardState previousKeyboardState;

        // Entities
        private Player player;
        private Texture2D playerTexture;
        private Texture2D enemyTexture;

        // Apenas espadas
        private Texture2D[] swordTextures = new Texture2D[6];

        // Weapons
        private List<Weapon> weapons = new();
        private List<Weapon> playerWeapons = new();
        private List<SwordProjectile> swordProjectiles = new();
        private Microsoft.Xna.Framework.Audio.SoundEffect swordSound;

        //mensagens
        private float phaseMessageTimer = 3.0f;
        private string phaseMessage = "Fase 1";
        private bool showPhaseMessage = true;

        // Level manager
        private LevelManager levelManager = new();

        // Game state
        private Gamestate currentState = Gamestate.Phase1;
        private Phase currentPhase = Phase.Phase1;
        private int phase1Kills = 0;
        private int phase2Kills = 0;
        private bool boostFase3Aplicado = false;



        // Spawn
        private Random random = new();
        private float spawnTimer = 0f;
        private float spawnInterval = 1.0f;

        // Mensagem de boost
        private string boostMessage = "";
        private double boostMessageTimer = 0;

        //arama enemigo 
        private Texture2D projectileTexture;

        // Fim de jogo
        private bool gameEnded = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true; // Permite redimensionar a janela
        }
        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            song = Content.Load<Song>("background-music");
            MediaPlayer.Play(song);
            MediaPlayer.IsRepeating = true;

            _gameplayScreen = new GameplayScreen();
            _gameplayScreen.LoadContent(Content, GraphicsDevice);

            playerTexture = Content.Load<Texture2D>("Unarmed_Walk_full");
            Enemy.SetOgreSpriteSheet(Content.Load<Texture2D>("orc1_walk_full"));
            Enemy.SetSlimeSpriteSheet(Content.Load<Texture2D>("Slime1_Walk_full"));
            Enemy.SetSlimeFogoSpriteSheet(Content.Load<Texture2D>("Slime3_Walk_full"));
            Enemy.SetOgreSpriteSheet(Content.Load<Texture2D>("orc1_walk_full"));
            Enemy.SetOgreBossSpriteSheet(Content.Load<Texture2D>("orc3_walk_full"));
            Enemy.SetVampiroSpriteSheet(Content.Load<Texture2D>("Vampires1_Walk_full"));
            Enemy.SetVampiroBossSpriteSheet(Content.Load<Texture2D>("Vampires3_Walk_full"));

            enemyTexture = new Texture2D(GraphicsDevice, 1, 1);
            enemyTexture.SetData(new[] { Color.White });

            // Carregue a textura do projétil
            projectileTexture = Content.Load<Texture2D>("Bullet");

            // Carrega apenas texturas de espada
            for (int i = 0; i < swordTextures.Length; i++)
                swordTextures[i] = Content.Load<Texture2D>($"sword{i + 1}");

            // Cria apenas armas de espada
            weapons = Weapon.CreateAllWeapons(swordTextures);

            // Player começa com apenas uma espada
            playerWeapons.Add(weapons[0]);

            // Inicializa player no centro
            player = new Player(new Vector2(
                GraphicsDevice.Viewport.Width / 2f - playerTexture.Width / 2f,
                GraphicsDevice.Viewport.Height / 2f - playerTexture.Height / 2f
            ));
            swordSound = Content.Load<Microsoft.Xna.Framework.Audio.SoundEffect>("sword-sound");

            // Substitua a inicialização de creditLines no LoadContent por:
            creditLines = new[]
            {
                "Desenvolvedores:",
                "Anthony Frade",
                "Valezka Naia",
                "",
                "Audio:",
                "Efeitos sonoros por Karim Nessim (Pixabay)",
                "Musica de fundo por Maksym Dudchyk (Pixabay)",
                "",
                "Imagens (Spritesheets):",
                "Inimigos:",
                "Free Slime Mobs - Pixel Art Top-Down (CraftPix)",
                "Free Top-Down Orc Game Character (CraftPix)",
                "Free Vampire 4-Direction Character (CraftPix)",
                "",
                "Personagem principal (jogador):",
                "Free Base 4-Direction Female Character (CraftPix)",
                "",
                "Armas:",
                "Sprites de armas obtidos em recursos livres no CraftPix",
                "",
                "Background:",
                "Emberwild-terrain-pack (itch.io)",
                "Small-grass-tileset (itch.io)"
            };

        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            MouseState currentMouseState = Mouse.GetState();
            // Lançar espada ao pressionar Space
            if (currentKeyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space) && playerWeapons.Count > 0)
            {
                // Encontra o inimigo mais próximo
                Enemy nearestEnemy = null;
                float minDist = float.MaxValue;
                foreach (var enemy in levelManager.Enemies)
                {
                    if (!enemy.IsAlive) continue;
                    float dist = Vector2.Distance(player.Position, enemy.Position);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearestEnemy = enemy;
                    }
                }

                if (nearestEnemy != null)
                {
                    // Usa a primeira espada do player
                    var weapon = playerWeapons[0];
                    Vector2 direction = nearestEnemy.Position - player.Position;
                    if (direction != Vector2.Zero)
                        direction.Normalize();

                    float speed = 400f; // Velocidade do projétil
                    var proj = new SwordProjectile(
                        player.Position + weapon.Offset, // posição inicial
                        direction * speed,
                        weapon.Texture,
                        weapon.Damage
                    );
                    swordProjectiles.Add(proj);
                    swordSound?.Play();


                    // Remove a espada lançada do orbit 
                    // playerWeapons.RemoveAt(0);
                }
            }



            // Sempre permite F11 para fullscreen, independente do estado do menu
            if (currentKeyboardState.IsKeyDown(Keys.F11) && previousKeyboardState.IsKeyUp(Keys.F11))
            {
                _graphics.IsFullScreen = !_graphics.IsFullScreen;

                if (_graphics.IsFullScreen)
                {
                    var screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                    var screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                    _graphics.PreferredBackBufferWidth = screenWidth;
                    _graphics.PreferredBackBufferHeight = screenHeight;
                }
                else
                {
                    _graphics.PreferredBackBufferWidth = 800;
                    _graphics.PreferredBackBufferHeight = 600;
                }

                _graphics.ApplyChanges();
            }

            // MENU PRINCIPAL
            if (menuState == MainMenuState.Menu)
            {
                // Navegação por teclado
                if (currentKeyboardState.IsKeyDown(Keys.Down) && previousKeyboardState.IsKeyUp(Keys.Down))
                    selectedMenuIndex = (selectedMenuIndex + 1) % menuOptions.Length;
                if (currentKeyboardState.IsKeyDown(Keys.Up) && previousKeyboardState.IsKeyUp(Keys.Up))
                    selectedMenuIndex = (selectedMenuIndex - 1 + menuOptions.Length) % menuOptions.Length;

                // Clique do mouse
                for (int i = 0; i < menuOptions.Length; i++)
                {
                    Rectangle rect = new Rectangle(
                        GraphicsDevice.Viewport.Width / 2 - 100,
                        150 + i * 60,
                        200,
                        50
                    );

                    if (rect.Contains(currentMouseState.Position) && currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                    {
                        selectedMenuIndex = i;
                        if (i == 0) // Start
                        {
                            inputSessionName = "";
                            menuState = MainMenuState.InputName;
                        }
                        else if (i == 1) // Scores
                        {
                            menuState = MainMenuState.Scores;
                        }
                        else if (i == 2) // Creditos
                        {
                            menuState = MainMenuState.Credits;
                        }
                    }
                }

                // Teclado: Enter
                if (currentKeyboardState.IsKeyDown(Keys.Enter) && previousKeyboardState.IsKeyUp(Keys.Enter))
                {
                    if (selectedMenuIndex == 0) // Start
                    {
                        inputSessionName = "";
                        menuState = MainMenuState.InputName;
                    }
                    else if (selectedMenuIndex == 1) // Scores
                    {
                        menuState = MainMenuState.Scores;
                    }
                    else if (selectedMenuIndex == 2) // Creditos
                    {
                        menuState = MainMenuState.Credits;
                    }
                }

                previousKeyboardState = currentKeyboardState;
                previousMouseState = currentMouseState;
                return;
            }


            // INPUT DE NOME DA SESSÃO
            if (menuState == MainMenuState.InputName)
            {
                foreach (var key in currentKeyboardState.GetPressedKeys())
                {
                    if (previousKeyboardState.IsKeyUp(key))
                    {
                        if (key == Keys.Back && inputSessionName.Length > 0)
                            inputSessionName = inputSessionName.Substring(0, inputSessionName.Length - 1);
                        else if (key == Keys.Enter && inputSessionName.Length > 0)
                        {
                            sessionName = inputSessionName;
                            menuState = MainMenuState.Playing;
                            ResetGame();
                        }
                        else if (key == Keys.Escape)
                        {
                            menuState = MainMenuState.Menu;
                        }
                        else
                        {
                            string c = key.ToString();
                            if (c.Length == 1 && inputSessionName.Length < 12)
                                inputSessionName += c;
                            else if (key >= Keys.A && key <= Keys.Z && inputSessionName.Length < 12)
                                inputSessionName += c;
                            else if (key >= Keys.D0 && key <= Keys.D9 && inputSessionName.Length < 12)
                                inputSessionName += (char)('0' + (key - Keys.D0));
                            else if (key == Keys.Space && inputSessionName.Length < 12)
                                inputSessionName += " ";
                        }
                    }
                }
                cursorBlinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (cursorBlinkTimer > 0.5)
                {
                    showInputCursor = !showInputCursor;
                    cursorBlinkTimer = 0;
                }
                previousKeyboardState = currentKeyboardState;
                return;
            }

            // SCORES E CRÉDITOS
            if (menuState == MainMenuState.Scores || menuState == MainMenuState.Credits)
            {
                if (currentKeyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
                    menuState = MainMenuState.Menu;
                previousKeyboardState = currentKeyboardState;
                return;
            }

            previousKeyboardState = currentKeyboardState;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Atualiza os timers
            double delta = gameTime.ElapsedGameTime.TotalSeconds;
            sessionTimer += delta;
            if (!gameEnded)
                gameTimer += delta;

            // Mensagem de fase temporária
            if (showPhaseMessage)
            {
                phaseMessageTimer -= (float)delta;
                if (phaseMessageTimer <= 0)
                    showPhaseMessage = false;
            }

            if (gameEnded)
            {
                int score = levelManager.EnemiesKilled * 100 - (int)gameTimer;
                if (score < 0) score = 0;
                scores.Add((sessionName, score, levelManager.EnemiesKilled, (int)gameTimer));
                SalvarScoreNoArquivo(sessionName, score, levelManager.EnemiesKilled, (int)gameTimer);
                menuState = MainMenuState.Menu;
                previousKeyboardState = currentKeyboardState;
                return;
            }


            switch (currentState)
            {
                case Gamestate.Phase1:
                case Gamestate.Phase2:
                case Gamestate.Phase3:
                case Gamestate.Phase4:
                case Gamestate.Phase5:
                    UpdateLevelPhases(gameTime);
                    break;
                case Gamestate.LevelComplete:
                    UpdateLevelComplete(gameTime);
                    break;
            }

            // --- Weapon orbit and contact damage logic ---
            foreach (var weapon in playerWeapons)
            {
                weapon.Update(player.Position, (float)gameTime.ElapsedGameTime.TotalSeconds);

                // Colisão da espada com inimigos
                Rectangle weaponRect = new Rectangle(
                    (int)(player.Position.X + weapon.Offset.X),
                    (int)(player.Position.Y + weapon.Offset.Y),
                    weapon.Texture.Width * 2, // 2x scale
                    weapon.Texture.Height * 2 // 2x scale
                );

                foreach (var enemy in levelManager.Enemies)
                {
                    Rectangle enemyRect = new Rectangle(
                        (int)enemy.Position.X,
                        (int)enemy.Position.Y,
                        enemy.Size,
                        enemy.Size
                    );

                    if (enemy.IsAlive && weaponRect.Intersects(enemyRect))
                    {
                        enemy.TakeDamage(weapon.Damage);
                        swordSound?.Play();
                        // Opcional: cooldown para não causar dano múltiplo por frame
                    }
                }
            }

            // --- Colisão do jogador com inimigos e projéteis ---
            Rectangle playerRect = new Rectangle((int)player.Position.X, (int)player.Position.Y, 15 * 2, 27 * 2);
            foreach (var enemy in levelManager.Enemies)
            {
                Rectangle enemyRect = new Rectangle((int)enemy.Position.X, (int)enemy.Position.Y, enemy.Size, enemy.Size);

                if (enemy.IsAlive && enemyRect.Intersects(playerRect))
                {
                    player.TakeDamage(enemy.AttackDamage);
                }

                foreach (var proj in enemy.Projectiles)
                {
                    Rectangle projRect = new Rectangle((int)proj.Position.X, (int)proj.Position.Y, proj.Size, proj.Size);
                    if (proj.IsActive && projRect.Intersects(playerRect))
                    {
                        player.TakeDamage(enemy.ProjectileDamage);
                        proj.IsActive = false;
                    }
                }
            }

            // Reset se o player morrer
            if (!player.IsAlive)
            {
                currentState = Gamestate.Phase1;
                currentPhase = Phase.Phase1;
                levelManager.EnemiesKilled = 0;
                levelManager.Phase3Kills = 0;
                levelManager.BossSpawned = false;
                levelManager.Enemies.Clear();
                player = new Player(new Vector2(
                    GraphicsDevice.Viewport.Width / 2f - playerTexture.Width / 2f,
                    GraphicsDevice.Viewport.Height / 2f - playerTexture.Height / 2f
                ));
                playerWeapons.Clear();
                playerWeapons.Add(weapons[0]);
                showPhaseMessage = true;
                phaseMessage = "Fase 1";
                phaseMessageTimer = 3.0f;
                gameEnded = false;
                gameTimer = 0; // Reinicia o timer de jogo
                return;
            }

            // Regeneração de vida do player
            player.Update(gameTime);
            previousMouseState = currentMouseState;

            // Atualiza projéteis de espada
            for (int i = swordProjectiles.Count - 1; i >= 0; i--)
            {
                var proj = swordProjectiles[i];
                proj.Update(gameTime);

                // Colisão com inimigos
                foreach (var enemy in levelManager.Enemies)
                {
                    if (enemy.IsAlive && proj.IsActive)
                    {
                        Rectangle projRect = new Rectangle((int)proj.Position.X, (int)proj.Position.Y, proj.Texture.Width, proj.Texture.Height);
                        Rectangle enemyRect = new Rectangle((int)enemy.Position.X, (int)enemy.Position.Y, enemy.Size, enemy.Size);
                        if (projRect.Intersects(enemyRect))
                        {
                            enemy.TakeDamage(proj.Damage);
                            proj.IsActive = false;
                        }
                    }
                }

                // Remove projétil se sair da tela ou colidir
                if (!proj.IsActive ||
                    proj.Position.X < 0 || proj.Position.X > GraphicsDevice.Viewport.Width ||
                    proj.Position.Y < 0 || proj.Position.Y > GraphicsDevice.Viewport.Height)
                {
                    swordProjectiles.RemoveAt(i);
                }
            }

            base.Update(gameTime);
        }

        //Metodo para salvar a Score num arquivo de texto 
        private void SalvarScoreNoArquivo(string nome, int score, int inimigos, int tempo)
        {
            string linha = $"{nome};{score};{inimigos};{tempo}";
            string caminho = "scores.txt";
            try
            {
                File.AppendAllText(caminho, linha + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Opcional: tratar erro de escrita
            }
        }


        private void ResetGame()
        {
            currentState = Gamestate.Phase1;
            currentPhase = Phase.Phase1;
            levelManager.EnemiesKilled = 0;
            levelManager.Phase3Kills = 0;
            levelManager.BossSpawned = false;
            levelManager.Enemies.Clear();
            player = new Player(new Vector2(
                GraphicsDevice.Viewport.Width / 2f - playerTexture.Width / 2f,
                GraphicsDevice.Viewport.Height / 2f - playerTexture.Height / 2f
            ));
            playerWeapons.Clear();
            playerWeapons.Add(weapons[0]);
            showPhaseMessage = true;
            phaseMessage = "Fase 1";
            phaseMessageTimer = 3.0f;
            gameEnded = false;
            gameTimer = 0;
            sessionTimer = 0;
            phase1Kills = 0;
            phase2Kills = 0;

        }

        private void UpdateLevelPhases(GameTime gameTime)
        {
            switch (currentPhase)
            {
                case Phase.Phase1:
                    UpdatePlayerMovement(gameTime);
                    spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (spawnTimer >= spawnInterval && levelManager.Enemies.Count < GetMaxEnemiesForPhase())
                    {
                        SpawnRandomEnemy();
                        spawnTimer = 0f;
                    }
                    for (int i = levelManager.Enemies.Count - 1; i >= 0; i--)
                    {
                        levelManager.Enemies[i].Update(player.Position, gameTime, projectileTexture, (int)currentPhase + 1, levelManager.Enemies);
                        if (!levelManager.Enemies[i].IsAlive && !levelManager.Enemies[i].IsDying)
                        {
                            levelManager.Enemies[i].StartDying();
                        }
                        if (levelManager.Enemies[i].IsDying && levelManager.Enemies[i].DeathFade <= 0f)
                        {
                            levelManager.Enemies.RemoveAt(i);
                            levelManager.EnemiesKilled++;
                            phase1Kills++;
                        }
                    }
                    if (phase1Kills >= 20)
                    {
                        levelManager.Enemies.Clear();
                        currentPhase = Phase.Phase2;
                        phaseMessage = "Fase 2";
                        phaseMessageTimer = 3.0f;
                        showPhaseMessage = true;
                        phase2Kills = 0; // reset para a próxima fase
                    }
                    break;

                case Phase.Phase2:
                    UpdatePlayerMovement(gameTime);

                    // Garante que o jogador só ganha uma nova arma uma vez
                    if (playerWeapons.Count < 2)
                    {
                        int idx;
                        do
                        {
                            idx = random.Next(weapons.Count);
                        } while (playerWeapons.Contains(weapons[idx]));
                        playerWeapons.Add(weapons[idx]);
                    }

                    // Spawn e atualização dos inimigos
                    spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (spawnTimer >= spawnInterval && levelManager.Enemies.Count < GetMaxEnemiesForPhase())
                    {
                        SpawnRandomEnemy();
                        spawnTimer = 0f;
                    }
                    for (int i = levelManager.Enemies.Count - 1; i >= 0; i--)
                    {
                        levelManager.Enemies[i].Update(player.Position, gameTime, projectileTexture, (int)currentPhase + 1, levelManager.Enemies);
                        if (!levelManager.Enemies[i].IsAlive && !levelManager.Enemies[i].IsDying)
                        {
                            levelManager.Enemies[i].StartDying();
                        }
                        if (levelManager.Enemies[i].IsDying && levelManager.Enemies[i].DeathFade <= 0f)
                        {
                            levelManager.Enemies.RemoveAt(i);
                            levelManager.EnemiesKilled++;
                            phase2Kills++;
                        }
                    }

                    // Só avança para a Phase3 após 30 kills nesta fase
                    if (phase2Kills >= 30)
                    {
                        levelManager.Enemies.Clear();
                        currentPhase = Phase.Phase3;
                        phaseMessage = "Fase 3";
                        phaseMessageTimer = 3.0f;
                        showPhaseMessage = true;
                        levelManager.Phase3Kills = 0;
                    }
                    break;

                case Phase.Phase3:
                    UpdatePlayerMovement(gameTime);

                    // Boost só uma vez ao entrar na Fase 3
                    if (!boostFase3Aplicado)
                    {
                        int boostType = random.Next(4);
                        switch (boostType)
                        {
                            case 0:
                                player.UpgradeAttackDamage(1);
                                boostMessage = "Boost: +1 Dano!";
                                break;
                            case 1:
                                player.UpgradeAttackSpeed(0.3f);
                                boostMessage = "Boost: +0.3 Velocidade de Ataque!";
                                break;
                            case 2:
                                player.UpgradeAttackCritChance(0.1f);
                                boostMessage = "Boost: +10% Chance de Critico!";
                                break;
                            case 3:
                                player.UpgradeHealth(2);
                                boostMessage = "Boost: +2 Vida Maxima!";
                                break;
                        }
                        boostMessageTimer = 3.0; // 3 segundos
                        boostFase3Aplicado = true;
                    }
                    else
                    {
                        boostMessageTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                        if (boostMessageTimer <= 0)
                        {
                            boostMessage = "";
                        }
                    }

                    // --- Adicione o spawn de inimigos aqui ---
                    spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (spawnTimer >= spawnInterval && levelManager.Enemies.Count < GetMaxEnemiesForPhase())
                    {
                        SpawnRandomEnemy();
                        spawnTimer = 0f;
                    }

                    for (int i = levelManager.Enemies.Count - 1; i >= 0; i--)
                    {
                        levelManager.Enemies[i].Update(player.Position, gameTime, projectileTexture, (int)currentPhase + 1, levelManager.Enemies);
                        if (!levelManager.Enemies[i].IsAlive && !levelManager.Enemies[i].IsDying)
                        {
                            levelManager.Enemies[i].StartDying();
                        }
                        if (levelManager.Enemies[i].IsDying && levelManager.Enemies[i].DeathFade <= 0f)
                        {
                            levelManager.Enemies.RemoveAt(i);
                            levelManager.EnemiesKilled++;
                            levelManager.Phase3Kills++;
                        }
                    }
                    if (levelManager.Phase3Kills > 30) // ou >= 31, conforme sua regra
                    {
                        currentPhase = Phase.Phase4;
                        phaseMessage = "Fase 4";
                        phaseMessageTimer = 3.0f;
                        showPhaseMessage = true;
                    }
                    break;

                case Phase.Phase4:
                    UpdatePlayerMovement(gameTime);

                    // Spawn de inimigos (igual à Phase1, mas pode customizar se quiser)
                    spawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (spawnTimer >= spawnInterval && levelManager.Enemies.Count < GetMaxEnemiesForPhase())
                    {
                        SpawnRandomEnemy();
                        spawnTimer = 0f;
                    }

                    for (int i = levelManager.Enemies.Count - 1; i >= 0; i--)
                    {
                        levelManager.Enemies[i].Update(player.Position, gameTime, projectileTexture, (int)currentPhase + 1, levelManager.Enemies);
                        if (!levelManager.Enemies[i].IsAlive && !levelManager.Enemies[i].IsDying)
                        {
                            levelManager.Enemies[i].StartDying();
                        }
                        if (levelManager.Enemies[i].IsDying && levelManager.Enemies[i].DeathFade <= 0f)
                        {
                            levelManager.Enemies.RemoveAt(i);
                            levelManager.EnemiesKilled++;
                        }
                    }

                    // Quando matar 60 inimigos, avança para a próxima fase
                    if (levelManager.EnemiesKilled >= 60)
                    {
                        levelManager.Enemies.Clear();
                        currentPhase = Phase.Phase5;
                        levelManager.BossSpawned = false;
                        phaseMessage = "Fase 5";
                        phaseMessageTimer = 3.0f;
                        showPhaseMessage = true;
                    }
                    break;

                case Phase.Phase5:
                    UpdatePlayerMovement(gameTime);
                    if (!levelManager.BossSpawned)
                    {
                        Vector2 bossPos = new Vector2(GraphicsDevice.Viewport.Width / 2, 50);
                        EnemyType bossType = random.Next(2) == 0 ? EnemyType.OgreBoss : EnemyType.VampiroBoss;
                        levelManager.Enemies.Add(new Enemy(bossPos, 40f, 1000, bossType, 5));
                        levelManager.BossSpawned = true;
                    }
                    else
                    {
                        for (int i = levelManager.Enemies.Count - 1; i >= 0; i--)
                        {
                            levelManager.Enemies[i].Update(player.Position, gameTime, projectileTexture, (int)currentPhase + 1,levelManager.Enemies);
                            if (!levelManager.Enemies[i].IsAlive && !levelManager.Enemies[i].IsDying)
                            {
                                levelManager.Enemies[i].StartDying();
                            }
                            if (levelManager.Enemies[i].IsDying && levelManager.Enemies[i].DeathFade <= 0f)
                            {
                                levelManager.Enemies.RemoveAt(i);
                                currentPhase = Phase.LevelComplete;
                                currentState = Gamestate.LevelComplete;
                                phaseMessage = "Parabens! Você completou todas as fases!";
                                phaseMessageTimer = float.MaxValue;
                                showPhaseMessage = true;
                                gameEnded = true;
                            }
                        }
                    }
                    break;
            }
        }


        private void UpdateLevelComplete(GameTime gameTime)
        {
            UpdatePlayerMovement(gameTime);

            if (Keyboard.GetState().GetPressedKeys().Length > 0)
            {
                currentState = Gamestate.Phase1;
                currentPhase = Phase.Phase1;
                levelManager.EnemiesKilled = 0;
                levelManager.Phase3Kills = 0;
                levelManager.BossSpawned = false;
                levelManager.Enemies.Clear();
                phaseMessage = "Fase 1";
                phaseMessageTimer = 3.0f;
                showPhaseMessage = true;
                gameEnded = false;
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

            int frameWidth = 15;
            int frameHeight = 27;

            player.Move(
                movement,
                delta,
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height,
                frameWidth,
                frameHeight
            );
        }

        // Dificuldade progressiva: menos inimigos no início, mais a cada fase
        private int GetMaxEnemiesForPhase()
        {
            int phaseNum = (int)currentPhase + 1;
            if (phaseNum >= 3)
                return 12 + (phaseNum - 3) * 4; // Mais inimigos a partir da fase 3
            else
                return 4 + (phaseNum - 1) * 3;
        }


        private void SpawnRandomEnemy()
        {
            EnemyType[] types;
            int phaseNum = (int)currentPhase + 1;

            if (currentPhase == Phase.Phase1 || currentPhase == Phase.Phase2)
            {
                types = new EnemyType[]
                {
                    EnemyType.Slime, EnemyType.SlimeFogo
                   
                };
            }
            else
            {
                types = new EnemyType[]
                {
                    EnemyType.Slime, EnemyType.SlimeFogo,
                    EnemyType.Ogre,
                    EnemyType.Vampiro
                };
            }

            EnemyType type = types[random.Next(types.Length)];
            Vector2 pos = new Vector2(
                random.Next(0, GraphicsDevice.Viewport.Width - 32),
                random.Next(0, GraphicsDevice.Viewport.Height - 32)
            );
            float speed = 80f + random.Next(-20, 20) + phaseNum * 5;
            int health = 8 + random.Next(3) + phaseNum * 3; // Mais vida base

            levelManager.Enemies.Add(new Enemy(pos, speed, health, type, phaseNum));
        }

        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            SpriteFont font = Content.Load<SpriteFont>("DefaultFont");

            // MENU PRINCIPAL
            if (menuState == MainMenuState.Menu)
            {
                string titulo = "BULLETSTORM";
                Vector2 tituloSize = font.MeasureString(titulo);
                _spriteBatch.DrawString(font, titulo, new Vector2(GraphicsDevice.Viewport.Width / 2 - tituloSize.X / 2, 60), Color.White);

                for (int i = 0; i < menuOptions.Length; i++)
                {
                    Color color = (i == selectedMenuIndex) ? Color.Yellow : Color.White;
                    string text = menuOptions[i];
                    Vector2 size = font.MeasureString(text);
                    Rectangle rect = new Rectangle(GraphicsDevice.Viewport.Width / 2 - 100, 150 + i * 60, 200, 50);
                    _spriteBatch.Draw(enemyTexture, rect, color * 0.2f);
                    _spriteBatch.DrawString(font, text, new Vector2(GraphicsDevice.Viewport.Width / 2 - size.X / 2, 165 + i * 60), color);
                }
                _spriteBatch.End();
                return;
            }

            // INPUT DE NOME
            if (menuState == MainMenuState.InputName)
            {
                string prompt = "Digite o nome da sessao:";
                Vector2 promptSize = font.MeasureString(prompt);
                _spriteBatch.DrawString(font, prompt, new Vector2(GraphicsDevice.Viewport.Width / 2 - promptSize.X / 2, 180), Color.White);

                string displayName = inputSessionName + (showInputCursor ? "|" : "");
                Vector2 nameSize = font.MeasureString(displayName);
                _spriteBatch.DrawString(font, displayName, new Vector2(GraphicsDevice.Viewport.Width / 2 - nameSize.X / 2, 240), Color.Yellow);
                _spriteBatch.End();
                return;
            }

            // SCORES
            if (menuState == MainMenuState.Scores)
            {
                string titulo = "SCORES";
                Vector2 tituloSize = font.MeasureString(titulo);
                _spriteBatch.DrawString(font, titulo, new Vector2(GraphicsDevice.Viewport.Width / 2 - tituloSize.X / 2, 60), Color.White);

                for (int i = 0; i < scores.Count; i++)
                {
                    string scoreText = $"{i + 1}. {scores[i].Name} - {scores[i].Score} pts - {scores[i].Enemies} inimigos - {TimeSpan.FromSeconds(scores[i].Time):mm\\:ss}";
                    _spriteBatch.DrawString(font, scoreText, new Vector2(200, 120 + i * 30), Color.Yellow);
                }
                _spriteBatch.DrawString(font, "Pressione ESC para voltar", new Vector2(200, 400), Color.White);
                _spriteBatch.End();
                return;
            }

            // CRÉDITOS
            if (menuState == MainMenuState.Credits)
            {
                string titulo = "CREDITOS";
                Vector2 tituloSize = font.MeasureString(titulo);
                _spriteBatch.DrawString(font, titulo, new Vector2(GraphicsDevice.Viewport.Width / 2 - tituloSize.X / 2, 60), Color.White);

                float y = 120;
                foreach (var line in creditLines)
                {
                    Vector2 size = font.MeasureString(line);
                    _spriteBatch.DrawString(font, line, new Vector2(GraphicsDevice.Viewport.Width / 2 - size.X / 2, y), Color.Yellow);
                    y += 28;
                }

                _spriteBatch.DrawString(font, "Pressione ESC para voltar", new Vector2(200, 500), Color.White);
                _spriteBatch.End();
                return;
            }


            // --- JOGO NORMAL ---
            _gameplayScreen.Draw(_spriteBatch);

            if (!string.IsNullOrEmpty(boostMessage))
            {
                Vector2 size = font.MeasureString(boostMessage);
                _spriteBatch.DrawString(
                    font,
                    boostMessage,
                    new Vector2(GraphicsDevice.Viewport.Width / 2 - size.X / 2, 10),
                    Color.Yellow
                );
            }

            if (showPhaseMessage)
            {
                Vector2 size = font.MeasureString(phaseMessage);
                _spriteBatch.DrawString(
                    font,
                    phaseMessage,
                    new Vector2(GraphicsDevice.Viewport.Width / 2 - size.X / 2, 60),
                    Color.White
                );
            }

            // Desenha projéteis de espada
            foreach (var swordProj in swordProjectiles)
                swordProj.Draw(_spriteBatch);

            player.Draw(_spriteBatch, playerTexture);

            foreach (var weapon in playerWeapons)
                weapon.Draw(_spriteBatch, player.Position);

            foreach (var enemy in levelManager.Enemies)
                enemy.Draw(_spriteBatch, enemyTexture, projectileTexture);

            // Barra de vida do player
            float barWidth = 200f;
            float barHeight = 20f;
            float healthPercent = Math.Max(0, Math.Min(1, (float)player.Health / player.MaxHealth));
            Rectangle healthBarBg = new Rectangle((GraphicsDevice.Viewport.Width - (int)barWidth) / 2, GraphicsDevice.Viewport.Height - 40, (int)barWidth, (int)barHeight);
            Rectangle healthBar = new Rectangle(healthBarBg.X, healthBarBg.Y, (int)(barWidth * healthPercent), (int)barHeight);
            _spriteBatch.Draw(enemyTexture, healthBarBg, Color.DarkRed);
            _spriteBatch.Draw(enemyTexture, healthBar, Color.LimeGreen);

            // Timers
            string gameTimeStr = $"Tempo de Jogo: {TimeSpan.FromSeconds(gameTimer):mm\\:ss}";
            string sessionTimeStr = $"Tempo de Sessao: {TimeSpan.FromSeconds(sessionTimer):hh\\:mm\\:ss}";
            _spriteBatch.DrawString(font, gameTimeStr, new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(font, sessionTimeStr, new Vector2(10, 35), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
    
