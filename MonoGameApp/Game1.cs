using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SuperBlazorBrosMonoGame.Models;

namespace SuperBlazorBrosMonoGame;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixelTexture = null!;

    // Game state
    private GameStatus _status = GameStatus.MENU;
    private GameStats _stats = new GameStats();
    private LevelData _levelData = null!;
    private Player _player = null!;
    private List<Entity> _entities = new List<Entity>();
    
    // Camera
    private float _cameraX = 0;
    
    // Input
    private KeyboardState _previousKeyboardState;
    private KeyboardState _currentKeyboardState;
    
    // Game timer
    private float _gameTimeCounter = 0f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        
        // Set window size based on scaled screen dimensions
        _graphics.PreferredBackBufferWidth = GameConstants.SCALED_SCREEN_WIDTH;
        _graphics.PreferredBackBufferHeight = GameConstants.SCALED_SCREEN_HEIGHT;
    }

    protected override void Initialize()
    {
        // Initialize level data with default level
        _levelData = new LevelData 
        { 
            Map = GameConstants.GetDefaultLevelMap(),
            Entities = GameConstants.GetDefaultEntities(),
            BackgroundColor = GameConstants.Colors.SKY
        };
        
        // Initialize player
        InitializePlayer();
        
        // Initialize entities list
        _entities = new List<Entity>(_levelData.Entities);

        base.Initialize();
    }

    private void InitializePlayer()
    {
        _player = new Player
        {
            Position = new Vector2(50, 100),
            Velocity = Vector2.Zero,
            Width = 12,
            Height = 16,
            PowerMode = "small",
            IsJumping = false,
            Grounded = false,
            Direction = 1
        };
        
        // Find safe starting position
        FindSafeStartPosition();
    }

    private void FindSafeStartPosition()
    {
        const int START_X = 50;
        int startTileX = START_X / GameConstants.TILE_SIZE;
        int[] groundTiles = { 1, 2, 3, 5, 6, 7, 8, 9 };
        
        for (int offset = 0; offset < 10; offset++)
        {
            int checkX = Math.Max(0, startTileX - offset);
            
            for (int y = 0; y < _levelData.Map.Length; y++)
            {
                if (_levelData.Map[y] != null && checkX < _levelData.Map[y].Length && 
                    Array.IndexOf(groundTiles, _levelData.Map[y][checkX]) >= 0)
                {
                    float groundTopY = y * GameConstants.TILE_SIZE;
                    float marioY = groundTopY - _player.Height;
                    
                    if (marioY < groundTopY)
                    {
                        _player.Position = new Vector2(checkX * GameConstants.TILE_SIZE + 2, marioY);
                        return;
                    }
                }
            }
        }
        
        // Fallback
        _player.Position = new Vector2(START_X, 12 * GameConstants.TILE_SIZE - _player.Height);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Create a 1x1 white pixel texture for drawing rectangles
        _pixelTexture = new Texture2D(GraphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        
        // Load font (we'll use a default for now, may need to add content later)
        // For now we'll just use pixel texture for everything
    }

    protected override void Update(GameTime gameTime)
    {
        _previousKeyboardState = _currentKeyboardState;
        _currentKeyboardState = Keyboard.GetState();

        // Menu navigation
        if (_status == GameStatus.MENU)
        {
            if (WasKeyPressed(Keys.Enter) || WasKeyPressed(Keys.Space))
            {
                StartGame();
            }
            if (WasKeyPressed(Keys.Escape))
            {
                Exit();
            }
        }
        else if (_status == GameStatus.PLAYING)
        {
            UpdateGameplay(gameTime);
            
            // Update game timer (1 second intervals)
            _gameTimeCounter += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_gameTimeCounter >= 1.0f)
            {
                _gameTimeCounter = 0f;
                _stats.Time--;
                if (_stats.Time <= 0)
                {
                    OnPlayerDie();
                }
            }
        }
        else if (_status == GameStatus.GAME_OVER || _status == GameStatus.VICTORY)
        {
            if (WasKeyPressed(Keys.Enter) || WasKeyPressed(Keys.Space))
            {
                _status = GameStatus.MENU;
            }
            if (WasKeyPressed(Keys.Escape))
            {
                Exit();
            }
        }

        base.Update(gameTime);
    }

    private void StartGame()
    {
        _stats = new GameStats 
        { 
            Score = 0, 
            Coins = 0, 
            Time = 400, 
            Lives = 3, 
            World = "YI1" // Yoshi's Island 1
        };
        
        _levelData = new LevelData 
        { 
            Map = GameConstants.GetDefaultLevelMap(),
            Entities = GameConstants.GetDefaultEntities(),
            BackgroundColor = GameConstants.Colors.SKY
        };
        
        InitializePlayer();
        _entities = new List<Entity>(_levelData.Entities);
        _cameraX = 0;
        _gameTimeCounter = 0f;
        _status = GameStatus.PLAYING;
    }

    private void UpdateGameplay(GameTime gameTime)
    {
        // Player horizontal movement
        if (_currentKeyboardState.IsKeyDown(Keys.Left))
        {
            _player.Velocity = new Vector2(_player.Velocity.X - GameConstants.ACCELERATION, _player.Velocity.Y);
            _player.Direction = -1;
        }
        if (_currentKeyboardState.IsKeyDown(Keys.Right))
        {
            _player.Velocity = new Vector2(_player.Velocity.X + GameConstants.ACCELERATION, _player.Velocity.Y);
            _player.Direction = 1;
        }
        
        // Apply friction
        _player.Velocity = new Vector2(_player.Velocity.X * GameConstants.FRICTION, _player.Velocity.Y);
        
        // Speed limits
        bool isRunning = _currentKeyboardState.IsKeyDown(Keys.LeftShift) || _currentKeyboardState.IsKeyDown(Keys.RightShift);
        float maxSpeed = isRunning ? GameConstants.MAX_RUN_SPEED : GameConstants.MAX_WALK_SPEED;
        if (Math.Abs(_player.Velocity.X) > maxSpeed)
        {
            _player.Velocity = new Vector2(maxSpeed * Math.Sign(_player.Velocity.X), _player.Velocity.Y);
        }
        
        // Jumping - SMW style with spin jump
        if ((_currentKeyboardState.IsKeyDown(Keys.Space) || _currentKeyboardState.IsKeyDown(Keys.Up)) && _player.Grounded)
        {
            // Check if A key is held for spin jump (SMW feature)
            bool spinJump = _currentKeyboardState.IsKeyDown(Keys.A);
            float jumpForce = spinJump ? GameConstants.SPIN_JUMP_FORCE : GameConstants.JUMP_FORCE;
            
            _player.Velocity = new Vector2(_player.Velocity.X, -jumpForce);
            _player.Grounded = false;
            _player.IsJumping = true;
            _player.IsSpinJumping = spinJump;
        }
        
        // Reset spin jump when grounded
        if (_player.Grounded)
        {
            _player.IsSpinJumping = false;
        }
        
        // Gravity
        if (!_player.Grounded)
        {
            _player.Velocity = new Vector2(_player.Velocity.X, _player.Velocity.Y + GameConstants.GRAVITY);
        }
        
        // Update position
        _player.Position += _player.Velocity;
        
        // Handle collisions
        HandleCollisions();
        
        // Update camera
        UpdateCamera();
        
        // Update entities
        UpdateEntities();
        
        // Check win condition
        if (_player.Position.X > (_levelData.Map[0].Length - 10) * GameConstants.TILE_SIZE)
        {
            _status = GameStatus.VICTORY;
        }
    }

    private void HandleCollisions()
    {
        _player.Grounded = false;
        int[] groundTiles = { 1, 2, 3, 5, 6, 7, 8, 9 };
        
        // Get the range of tiles the player overlaps
        int startX = Math.Max(0, (int)(_player.Position.X / GameConstants.TILE_SIZE));
        int endX = Math.Min((int)((_player.Position.X + _player.Width) / GameConstants.TILE_SIZE), 
                           (_levelData.Map[0]?.Length ?? 0) - 1);
        int startY = Math.Max(0, (int)(_player.Position.Y / GameConstants.TILE_SIZE));
        int endY = Math.Min((int)((_player.Position.Y + _player.Height) / GameConstants.TILE_SIZE), 
                           _levelData.Map.Length - 1);
        
        // Check all tiles the player overlaps
        for (int y = startY; y <= endY; y++)
        {
            if (y < 0 || y >= _levelData.Map.Length || _levelData.Map[y] == null) continue;
            
            for (int x = startX; x <= endX; x++)
            {
                if (x < 0 || x >= _levelData.Map[y].Length) continue;
                
                int tile = _levelData.Map[y][x];
                
                if (Array.IndexOf(groundTiles, tile) >= 0)
                {
                    float tileX = x * GameConstants.TILE_SIZE;
                    float tileY = y * GameConstants.TILE_SIZE;
                    float tileRight = tileX + GameConstants.TILE_SIZE;
                    float tileBottom = tileY + GameConstants.TILE_SIZE;
                    
                    // Check if player is actually colliding with this tile
                    if (_player.Position.X < tileRight && 
                        _player.Position.X + _player.Width > tileX &&
                        _player.Position.Y < tileBottom && 
                        _player.Position.Y + _player.Height > tileY)
                    {
                        // Calculate overlap on each side
                        float overlapLeft = (_player.Position.X + _player.Width) - tileX;
                        float overlapRight = tileRight - _player.Position.X;
                        float overlapTop = (_player.Position.Y + _player.Height) - tileY;
                        float overlapBottom = tileBottom - _player.Position.Y;
                        
                        // Find the minimum overlap to resolve collision
                        float minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), 
                                                    Math.Min(overlapTop, overlapBottom));
                        
                        // Resolve collision based on the side with minimum overlap
                        if (minOverlap == overlapTop && _player.Velocity.Y > 0)
                        {
                            // Collision from top (player falling onto ground)
                            _player.Position = new Vector2(_player.Position.X, tileY - _player.Height);
                            _player.Velocity = new Vector2(_player.Velocity.X, 0);
                            _player.Grounded = true;
                            _player.IsJumping = false;
                        }
                        else if (minOverlap == overlapBottom && _player.Velocity.Y < 0)
                        {
                            // Collision from bottom (player jumping into block)
                            _player.Position = new Vector2(_player.Position.X, tileBottom);
                            _player.Velocity = new Vector2(_player.Velocity.X, 0);
                        }
                        else if (minOverlap == overlapLeft && _player.Velocity.X > 0)
                        {
                            // Collision from left (moving right)
                            _player.Position = new Vector2(tileX - _player.Width, _player.Position.Y);
                            _player.Velocity = new Vector2(0, _player.Velocity.Y);
                        }
                        else if (minOverlap == overlapRight && _player.Velocity.X < 0)
                        {
                            // Collision from right (moving left)
                            _player.Position = new Vector2(tileRight, _player.Position.Y);
                            _player.Velocity = new Vector2(0, _player.Velocity.Y);
                        }
                    }
                }
            }
        }
        
        // Prevent falling through bottom of screen
        if (_player.Position.Y > GameConstants.SCREEN_HEIGHT)
        {
            OnPlayerDie();
        }
        
        // Prevent going off left edge
        _player.Position = new Vector2(Math.Max(0, _player.Position.X), _player.Position.Y);
    }

    private void UpdateCamera()
    {
        float targetX = _player.Position.X - GameConstants.SCREEN_WIDTH / 3;
        _cameraX = Math.Max(0, targetX);
    }

    private void UpdateEntities()
    {
        int[] groundTiles = { 1, 2, 3, 5, 6, 7, 8, 9, 10 };
        
        foreach (var entity in _entities)
        {
            if (entity.Dead) continue;
            
            // Update entity movement
            entity.Velocity = new Vector2(entity.Direction * 0.5f, entity.Velocity.Y);
            entity.Velocity = new Vector2(entity.Velocity.X, entity.Velocity.Y + GameConstants.GRAVITY);
            entity.Position += entity.Velocity;
            
            // Ground collision for enemies
            int tileX = (int)(entity.Position.X / GameConstants.TILE_SIZE);
            int tileY = (int)((entity.Position.Y + entity.Height) / GameConstants.TILE_SIZE);
            
            if (tileY >= 0 && tileY < _levelData.Map.Length)
            {
                var row = _levelData.Map[tileY];
                if (row != null && tileX >= 0 && tileX < row.Length)
                {
                    if (Array.IndexOf(groundTiles, row[tileX]) >= 0)
                    {
                        float groundY = tileY * GameConstants.TILE_SIZE;
                        if (entity.Position.Y + entity.Height > groundY)
                        {
                            entity.Position = new Vector2(entity.Position.X, groundY - entity.Height);
                            entity.Velocity = new Vector2(entity.Velocity.X, 0);
                        }
                    }
                }
            }
            
            // Collision with player
            if (CheckCollision(_player, entity))
            {
                if (_player.Velocity.Y > 0 && _player.Position.Y < entity.Position.Y)
                {
                    // Jump on enemy
                    entity.Dead = true;
                    
                    // SMW spin jump gives smaller bounce
                    if (_player.IsSpinJumping)
                    {
                        _player.Velocity = new Vector2(_player.Velocity.X, -2.5f);
                        _stats.Score += 200; // Bonus points for spin jump
                    }
                    else
                    {
                        _player.Velocity = new Vector2(_player.Velocity.X, -GameConstants.BOUNCE_FORCE);
                        _stats.Score += 100;
                    }
                }
                else if (!_player.IsSpinJumping)
                {
                    // Hit by enemy (spin jump protects you in SMW)
                    OnPlayerDie();
                }
            }
        }
    }

    private bool CheckCollision(Entity a, Entity b)
    {
        return a.Position.X < b.Position.X + b.Width &&
               a.Position.X + a.Width > b.Position.X &&
               a.Position.Y < b.Position.Y + b.Height &&
               a.Position.Y + a.Height > b.Position.Y;
    }

    private void OnPlayerDie()
    {
        _stats.Lives--;
        if (_stats.Lives < 0)
        {
            _status = GameStatus.GAME_OVER;
        }
        else
        {
            InitializePlayer();
            _cameraX = 0;
            _stats.Time = 400;
        }
    }

    private bool WasKeyPressed(Keys key)
    {
        return _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_levelData.BackgroundColor);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

        if (_status == GameStatus.MENU)
        {
            DrawMenu();
        }
        else if (_status == GameStatus.PLAYING)
        {
            DrawGame();
            DrawHUD();
        }
        else if (_status == GameStatus.GAME_OVER)
        {
            DrawGameOver();
        }
        else if (_status == GameStatus.VICTORY)
        {
            DrawVictory();
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawMenu()
    {
        string title = "SUPER MARIO WORLD";
        DrawText(title, new Vector2(GameConstants.SCALED_SCREEN_WIDTH / 2 - 200, 150), Color.Yellow, 2f);
        
        string subtitle = "YOSHI'S ISLAND 1";
        DrawText(subtitle, new Vector2(GameConstants.SCALED_SCREEN_WIDTH / 2 - 150, 220), Color.White, 1.5f);
        
        string start = "PRESS ENTER TO START";
        DrawText(start, new Vector2(GameConstants.SCALED_SCREEN_WIDTH / 2 - 150, 320), Color.White, 1f);
        
        string controls = "CONTROLS: ARROWS + SPACE + SHIFT";
        DrawText(controls, new Vector2(GameConstants.SCALED_SCREEN_WIDTH / 2 - 180, 400), new Color(144, 232, 152), 1f);
    }

    private void DrawGame()
    {
        // Render map
        DrawMap();
        
        // Render entities
        DrawEntities();
        
        // Render player
        DrawPlayer();
    }

    private void DrawMap()
    {
        int startX = (int)(_cameraX / GameConstants.TILE_SIZE);
        int endX = Math.Min(startX + (GameConstants.SCREEN_WIDTH / GameConstants.TILE_SIZE) + 1, _levelData.Map[0].Length);
        
        for (int y = 0; y < _levelData.Map.Length; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                int tile = _levelData.Map[y][x];
                if (tile == 0) continue;
                
                float screenX = (x * GameConstants.TILE_SIZE - _cameraX) * GameConstants.SCALE;
                float screenY = y * GameConstants.TILE_SIZE * GameConstants.SCALE;
                
                DrawTile(tile, new Vector2(screenX, screenY));
            }
        }
    }

    private void DrawTile(int tile, Vector2 position)
    {
        int size = GameConstants.ACTUAL_TILE_SIZE;
        Color color = Color.White;
        
        switch (tile)
        {
            case 1: // Ground - SMW style with more detail
                // Main ground color
                DrawRectangle(position, size, size, GameConstants.Colors.GROUND);
                // Add highlights and shadows for depth
                DrawRectangle(position, size, size / 4, GameConstants.Colors.GROUND_HIGHLIGHT);
                DrawRectangle(new Vector2(position.X, position.Y + size - size / 4), size, size / 4, GameConstants.Colors.GROUND_DARK);
                // Add some texture detail
                DrawRectangle(new Vector2(position.X + 2, position.Y + size / 3), size / 3, 2, GameConstants.Colors.GROUND_DARK);
                DrawRectangle(new Vector2(position.X + size - size / 3, position.Y + size / 2), size / 4, 2, GameConstants.Colors.GROUND_DARK);
                return;
            case 2: // Brick - SMW golden/yellow brick with more detail
                DrawRectangle(position, size, size, GameConstants.Colors.BRICK);
                // Border and details
                DrawRectangleOutline(position, size, size, GameConstants.Colors.BRICK_DARK, 2);
                // Inner detail lines (cross pattern)
                DrawRectangle(new Vector2(position.X + size / 2 - 1, position.Y + 4), 2, size - 8, GameConstants.Colors.BRICK_DARK);
                DrawRectangle(new Vector2(position.X + 4, position.Y + size / 2 - 1), size - 8, 2, GameConstants.Colors.BRICK_DARK);
                return;
            case 3: // Question Block - SMW style
                DrawRectangle(position, size, size, GameConstants.Colors.QUESTION);
                DrawRectangleOutline(position, size, size, GameConstants.Colors.QUESTION_DARK, 2);
                // Draw larger, more detailed question mark
                int qSize = size / 3;
                // Question mark stem
                DrawRectangle(new Vector2(position.X + size / 2 - qSize / 4, position.Y + size / 2 + qSize / 4), qSize / 2, qSize / 2, GameConstants.Colors.QUESTION_DARK);
                // Question mark curve
                DrawRectangle(new Vector2(position.X + size / 2 - qSize / 2, position.Y + size / 4), qSize, qSize, GameConstants.Colors.QUESTION_DARK);
                // Question mark dot
                DrawRectangle(new Vector2(position.X + size / 2 - qSize / 4, position.Y + size - size / 3), qSize / 2, qSize / 3, GameConstants.Colors.QUESTION_DARK);
                return;
            case 5: // Hard Block - SMW gray/brown block
                color = GameConstants.Colors.MESSAGE_BLOCK;
                DrawRectangle(position, size, size, color);
                DrawRectangleOutline(position, size, size, GameConstants.Colors.MESSAGE_BLOCK_OUTLINE, 2);
                return;
            case 6: // Pipe Left
            case 7: // Pipe Right
            case 8: // Pipe Top Left
            case 9: // Pipe Top Right
                // SMW pipes have more detail with highlights
                bool isTop = (tile == 8 || tile == 9);
                bool isLeft = (tile == 6 || tile == 8);
                
                DrawRectangle(position, size, size, GameConstants.Colors.PIPE);
                
                // Add highlight on left side, shadow on right
                if (isLeft)
                {
                    DrawRectangle(position, size / 4, size, GameConstants.Colors.PIPE_HIGHLIGHT);
                }
                else
                {
                    DrawRectangle(new Vector2(position.X + size - size / 4, position.Y), size / 4, size, GameConstants.Colors.PIPE_DARK);
                }
                
                // Pipe rim for top pieces
                if (isTop)
                {
                    DrawRectangle(new Vector2(position.X, position.Y), size, size / 3, GameConstants.Colors.PIPE_HIGHLIGHT);
                    DrawRectangleOutline(position, size, size / 3, GameConstants.Colors.PIPE_DARK, 1);
                }
                return;
            case 10: // Pole - SMW white pole with red stripes
                DrawRectangle(new Vector2(position.X + size / 2 - 3, position.Y), 6, size, Color.White);
                DrawRectangle(new Vector2(position.X + size / 2 - 3, position.Y), 6, size / 4, Color.Red);
                return;
            case 11: // Flag - SMW checkered flag
                DrawRectangle(new Vector2(position.X + size / 2, position.Y + 4), size / 2, size / 2, Color.White);
                // Checkered pattern
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if ((i + j) % 2 == 0)
                        {
                            DrawRectangle(new Vector2(position.X + size / 2 + i * size / 4, position.Y + 4 + j * size / 4), 
                                          size / 4, size / 4, Color.Black);
                        }
                    }
                }
                return;
            case 14: // Cloud
                DrawRectangle(position, size, size, GameConstants.Colors.CLOUD);
                DrawRectangleOutline(position, size, size, GameConstants.Colors.CLOUD_OUTLINE, 1);
                return;
            case 15: // Bush
                DrawRectangle(position, size, size, GameConstants.Colors.BUSH_GREEN);
                DrawRectangle(position, size, size / 3, GameConstants.Colors.BUSH_DARK);
                return;
            case 16: // Hill
                DrawRectangle(position, size, size, GameConstants.Colors.HILL_GREEN);
                DrawRectangle(position, size, size / 4, GameConstants.Colors.GRASS_TOP);
                return;
            case 17: // Castle
                DrawRectangle(position, size, size, new Color(180, 180, 180));
                DrawRectangleOutline(position, size, size, new Color(80, 80, 80), 2);
                return;
        }
        
        DrawRectangle(position, size, size, color);
    }

    private void DrawPlayer()
    {
        float screenX = (_player.Position.X - _cameraX) * GameConstants.SCALE;
        float screenY = _player.Position.Y * GameConstants.SCALE;
        float width = _player.Width * GameConstants.SCALE;
        float height = _player.Height * GameConstants.SCALE;
        
        // SMW Mario sprite - more detailed and colorful
        // Hat (red cap with white M emblem area)
        DrawRectangle(new Vector2(screenX + width * 0.15f, screenY), (int)(width * 0.7f), (int)(height * 0.25f), GameConstants.Colors.MARIO_RED);
        
        // Face/Head
        DrawRectangle(new Vector2(screenX + width * 0.2f, screenY + height * 0.25f), (int)(width * 0.6f), (int)(height * 0.3f), GameConstants.Colors.MARIO_SKIN);
        
        // Eyes (white)
        float eyeSize = width * 0.15f;
        DrawRectangle(new Vector2(screenX + width * 0.3f, screenY + height * 0.35f), (int)eyeSize, (int)(eyeSize * 0.8f), GameConstants.Colors.MARIO_WHITE);
        DrawRectangle(new Vector2(screenX + width * 0.55f, screenY + height * 0.35f), (int)eyeSize, (int)(eyeSize * 0.8f), GameConstants.Colors.MARIO_WHITE);
        
        // Pupils (black)
        DrawRectangle(new Vector2(screenX + width * 0.35f, screenY + height * 0.38f), (int)(eyeSize * 0.5f), (int)(eyeSize * 0.6f), GameConstants.Colors.BLACK);
        DrawRectangle(new Vector2(screenX + width * 0.6f, screenY + height * 0.38f), (int)(eyeSize * 0.5f), (int)(eyeSize * 0.6f), GameConstants.Colors.BLACK);
        
        // Body - Blue overalls
        DrawRectangle(new Vector2(screenX + width * 0.25f, screenY + height * 0.55f), (int)(width * 0.5f), (int)(height * 0.35f), GameConstants.Colors.MARIO_BLUE);
        
        // Overalls straps (blue lines on chest)
        DrawRectangle(new Vector2(screenX + width * 0.3f, screenY + height * 0.5f), (int)(width * 0.15f), (int)(height * 0.15f), GameConstants.Colors.MARIO_BLUE);
        DrawRectangle(new Vector2(screenX + width * 0.55f, screenY + height * 0.5f), (int)(width * 0.15f), (int)(height * 0.15f), GameConstants.Colors.MARIO_BLUE);
        
        // Arms (skin colored)
        DrawRectangle(new Vector2(screenX, screenY + height * 0.5f), (int)(width * 0.25f), (int)(height * 0.25f), GameConstants.Colors.MARIO_SKIN);
        DrawRectangle(new Vector2(screenX + width * 0.75f, screenY + height * 0.5f), (int)(width * 0.25f), (int)(height * 0.25f), GameConstants.Colors.MARIO_SKIN);
        
        // Gloves (white)
        DrawRectangle(new Vector2(screenX, screenY + height * 0.65f), (int)(width * 0.2f), (int)(height * 0.15f), GameConstants.Colors.MARIO_WHITE);
        DrawRectangle(new Vector2(screenX + width * 0.8f, screenY + height * 0.65f), (int)(width * 0.2f), (int)(height * 0.15f), GameConstants.Colors.MARIO_WHITE);
        
        // Legs/Shoes (brown)
        DrawRectangle(new Vector2(screenX + width * 0.25f, screenY + height * 0.9f), (int)(width * 0.2f), (int)(height * 0.1f), GameConstants.Colors.MARIO_BROWN);
        DrawRectangle(new Vector2(screenX + width * 0.55f, screenY + height * 0.9f), (int)(width * 0.2f), (int)(height * 0.1f), GameConstants.Colors.MARIO_BROWN);
    }

    private void DrawEntities()
    {
        foreach (var entity in _entities)
        {
            if (entity.Dead) continue;
            
            float screenX = (entity.Position.X - _cameraX) * GameConstants.SCALE;
            float screenY = entity.Position.Y * GameConstants.SCALE;
            float width = entity.Width * GameConstants.SCALE;
            float height = entity.Height * GameConstants.SCALE;
            
            if (entity.State == "rex")
            {
                // Rex - Purple dinosaur enemy from SMW
                // Body (purple)
                DrawRectangle(new Vector2(screenX, screenY + height * 0.3f), (int)width, (int)(height * 0.7f), GameConstants.Colors.REX);
                
                // Belly (yellow)
                DrawRectangle(new Vector2(screenX + width * 0.2f, screenY + height * 0.4f), (int)(width * 0.6f), (int)(height * 0.5f), GameConstants.Colors.REX_BELLY);
                
                // Head (purple)
                DrawRectangle(new Vector2(screenX + width * 0.2f, screenY), (int)(width * 0.6f), (int)(height * 0.4f), GameConstants.Colors.REX);
                
                // Eyes (white with black pupils)
                DrawRectangle(new Vector2(screenX + width * 0.3f, screenY + height * 0.15f), (int)(width * 0.15f), (int)(height * 0.15f), Color.White);
                DrawRectangle(new Vector2(screenX + width * 0.55f, screenY + height * 0.15f), (int)(width * 0.15f), (int)(height * 0.15f), Color.White);
                DrawRectangle(new Vector2(screenX + width * 0.35f, screenY + height * 0.18f), (int)(width * 0.08f), (int)(height * 0.1f), Color.Black);
                DrawRectangle(new Vector2(screenX + width * 0.6f, screenY + height * 0.18f), (int)(width * 0.08f), (int)(height * 0.1f), Color.Black);
                
                // Snout
                DrawRectangle(new Vector2(screenX + width * 0.65f, screenY + height * 0.25f), (int)(width * 0.2f), (int)(height * 0.1f), GameConstants.Colors.REX_BELLY);
                
                // Feet
                DrawRectangle(new Vector2(screenX + width * 0.1f, screenY + height * 0.9f), (int)(width * 0.25f), (int)(height * 0.1f), GameConstants.Colors.REX);
                DrawRectangle(new Vector2(screenX + width * 0.65f, screenY + height * 0.9f), (int)(width * 0.25f), (int)(height * 0.1f), GameConstants.Colors.REX);
            }
            else if (entity.State == "koopa")
            {
                // Koopa Troopa - Green turtle enemy from SMW
                // Shell (green with pattern)
                DrawRectangle(new Vector2(screenX, screenY + height * 0.4f), (int)width, (int)(height * 0.6f), GameConstants.Colors.KOOPA_SHELL);
                DrawRectangleOutline(new Vector2(screenX, screenY + height * 0.4f), (int)width, (int)(height * 0.6f), GameConstants.Colors.PIPE_DARK, 2);
                
                // Shell detail (hexagon pattern - simplified)
                DrawRectangle(new Vector2(screenX + width * 0.3f, screenY + height * 0.5f), (int)(width * 0.4f), (int)(height * 0.3f), Color.White);
                DrawRectangleOutline(new Vector2(screenX + width * 0.3f, screenY + height * 0.5f), (int)(width * 0.4f), (int)(height * 0.3f), GameConstants.Colors.PIPE_DARK, 1);
                
                // Head (yellow/orange)
                DrawRectangle(new Vector2(screenX + width * 0.25f, screenY), (int)(width * 0.5f), (int)(height * 0.45f), GameConstants.Colors.REX_BELLY);
                
                // Eyes
                DrawRectangle(new Vector2(screenX + width * 0.3f, screenY + height * 0.15f), (int)(width * 0.15f), (int)(height * 0.15f), Color.White);
                DrawRectangle(new Vector2(screenX + width * 0.55f, screenY + height * 0.15f), (int)(width * 0.15f), (int)(height * 0.15f), Color.White);
                DrawRectangle(new Vector2(screenX + width * 0.35f, screenY + height * 0.18f), (int)(width * 0.08f), (int)(height * 0.1f), Color.Black);
                DrawRectangle(new Vector2(screenX + width * 0.6f, screenY + height * 0.18f), (int)(width * 0.08f), (int)(height * 0.1f), Color.Black);
                
                // Feet (green)
                DrawRectangle(new Vector2(screenX, screenY + height * 0.9f), (int)(width * 0.3f), (int)(height * 0.1f), GameConstants.Colors.KOOPA_GREEN);
                DrawRectangle(new Vector2(screenX + width * 0.7f, screenY + height * 0.9f), (int)(width * 0.3f), (int)(height * 0.1f), GameConstants.Colors.KOOPA_GREEN);
            }
            else
            {
                // Fallback - draw as Goomba if state not specified
                DrawRectangle(new Vector2(screenX, screenY), (int)width, (int)height, new Color(228, 92, 16));
                DrawRectangle(new Vector2(screenX + width * 0.2f, screenY + height * 0.3f), (int)(width * 0.2f), (int)(height * 0.2f), Color.White);
                DrawRectangle(new Vector2(screenX + width * 0.6f, screenY + height * 0.3f), (int)(width * 0.2f), (int)(height * 0.2f), Color.White);
            }
        }
    }

    private void DrawHUD()
    {
        string mario = $"MARIO {_stats.Score:000000}";
        DrawText(mario, new Vector2(20, 10), Color.White, 1f);
        
        string coins = $"x{_stats.Coins:00}";
        DrawText(coins, new Vector2(200, 10), Color.White, 1f);
        
        string world = $"WORLD {_stats.World}";
        DrawText(world, new Vector2(350, 10), Color.White, 1f);
        
        string time = $"TIME {_stats.Time:000}";
        DrawText(time, new Vector2(550, 10), Color.White, 1f);
    }

    private void DrawGameOver()
    {
        string text = "GAME OVER";
        DrawText(text, new Vector2(GameConstants.SCALED_SCREEN_WIDTH / 2 - 120, GameConstants.SCALED_SCREEN_HEIGHT / 2), Color.White, 2f);
        
        string restart = "PRESS ENTER TO RETURN TO MENU";
        DrawText(restart, new Vector2(GameConstants.SCALED_SCREEN_WIDTH / 2 - 200, GameConstants.SCALED_SCREEN_HEIGHT / 2 + 60), Color.White, 1f);
    }

    private void DrawVictory()
    {
        string text = "LEVEL COMPLETE!";
        DrawText(text, new Vector2(GameConstants.SCALED_SCREEN_WIDTH / 2 - 150, GameConstants.SCALED_SCREEN_HEIGHT / 2), Color.White, 2f);
        
        string score = $"SCORE: {_stats.Score}";
        DrawText(score, new Vector2(GameConstants.SCALED_SCREEN_WIDTH / 2 - 100, GameConstants.SCALED_SCREEN_HEIGHT / 2 + 60), Color.Yellow, 1.5f);
        
        string restart = "PRESS ENTER TO RETURN TO MENU";
        DrawText(restart, new Vector2(GameConstants.SCALED_SCREEN_WIDTH / 2 - 200, GameConstants.SCALED_SCREEN_HEIGHT / 2 + 120), Color.White, 1f);
    }

    private void DrawRectangle(Vector2 position, int width, int height, Color color)
    {
        _spriteBatch.Draw(_pixelTexture, new Rectangle((int)position.X, (int)position.Y, width, height), color);
    }

    private void DrawRectangleOutline(Vector2 position, int width, int height, Color color, int thickness)
    {
        // Top
        DrawRectangle(position, width, thickness, color);
        // Bottom
        DrawRectangle(new Vector2(position.X, position.Y + height - thickness), width, thickness, color);
        // Left
        DrawRectangle(position, thickness, height, color);
        // Right
        DrawRectangle(new Vector2(position.X + width - thickness, position.Y), thickness, height, color);
    }

    private void DrawText(string text, Vector2 position, Color color, float scale)
    {
        // Simple pixel-based text rendering
        // For each character, draw a simple representation
        int charWidth = (int)(8 * scale);
        int charHeight = (int)(8 * scale);
        
        for (int i = 0; i < text.Length; i++)
        {
            Vector2 charPos = position + new Vector2(i * charWidth, 0);
            DrawRectangle(charPos, (int)(6 * scale), charHeight, color);
        }
    }
}
