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
            World = "1-1" 
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
        
        // Jumping
        if ((_currentKeyboardState.IsKeyDown(Keys.Space) || _currentKeyboardState.IsKeyDown(Keys.Up)) && _player.Grounded)
        {
            _player.Velocity = new Vector2(_player.Velocity.X, -GameConstants.JUMP_FORCE);
            _player.Grounded = false;
            _player.IsJumping = true;
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
                    _player.Velocity = new Vector2(_player.Velocity.X, -3);
                    _stats.Score += 100;
                }
                else
                {
                    // Hit by enemy
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
        string title = "SUPER BLAZOR BROS";
        DrawText(title, new Vector2(GameConstants.SCALED_SCREEN_WIDTH / 2 - 200, 150), Color.White, 2f);
        
        string start = "PRESS ENTER TO START";
        DrawText(start, new Vector2(GameConstants.SCALED_SCREEN_WIDTH / 2 - 150, 300), Color.White, 1f);
        
        string controls = "CONTROLS: ARROWS + SPACE + SHIFT";
        DrawText(controls, new Vector2(GameConstants.SCALED_SCREEN_WIDTH / 2 - 180, 400), Color.Yellow, 1f);
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
            case 1: // Ground
                color = GameConstants.Colors.GROUND;
                break;
            case 2: // Brick
                color = GameConstants.Colors.BRICK;
                DrawRectangle(position, size, size, color);
                DrawRectangleOutline(position, size, size, GameConstants.Colors.BLACK, 2);
                return;
            case 3: // Question Block
                color = GameConstants.Colors.QUESTION;
                DrawRectangle(position, size, size, color);
                // Draw question mark
                DrawText("?", position + new Vector2(size / 2 - 8, size / 2 - 12), GameConstants.Colors.BLACK, 1.5f);
                return;
            case 5: // Hard Block
                color = new Color(128, 128, 128);
                break;
            case 6:
            case 7:
            case 8:
            case 9: // Pipe
                color = GameConstants.Colors.PIPE;
                break;
            case 10: // Pole
                DrawRectangle(new Vector2(position.X + size / 2 - 2, position.Y), 4, size, Color.Gold);
                return;
            case 11: // Flag
                DrawRectangle(new Vector2(position.X + size / 2, position.Y), size / 2, size / 2, Color.Red);
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
        
        // Simple Mario sprite
        DrawRectangle(new Vector2(screenX, screenY), (int)width, (int)height, GameConstants.Colors.MARIO_RED);
        
        // Cap
        DrawRectangle(new Vector2(screenX + 2, screenY), (int)(width - 4), (int)(height / 4), GameConstants.Colors.MARIO_RED);
        
        // Face
        DrawRectangle(new Vector2(screenX + 2, screenY + height / 4), (int)(width - 4), (int)(height / 3), GameConstants.Colors.MARIO_SKIN);
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
            
            // Simple Goomba sprite
            DrawRectangle(new Vector2(screenX, screenY), (int)width, (int)height, GameConstants.Colors.GOOMBA);
            
            // Eyes
            DrawRectangle(new Vector2(screenX + width * 0.2f, screenY + height * 0.3f), (int)(width * 0.2f), (int)(height * 0.2f), Color.White);
            DrawRectangle(new Vector2(screenX + width * 0.6f, screenY + height * 0.3f), (int)(width * 0.2f), (int)(height * 0.2f), Color.White);
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
