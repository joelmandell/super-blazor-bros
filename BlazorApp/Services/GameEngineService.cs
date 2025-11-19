using SuperBlazorBros.Models;
using Microsoft.JSInterop;
using System.Timers;

namespace SuperBlazorBros.Services;

/// <summary>
/// MonoGame-style game engine adapted for Blazor WebAssembly.
/// Runs all game logic in C# as WebAssembly, uses JSInterop only for rendering.
/// </summary>
public class GameEngineService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _renderModule;
    private System.Timers.Timer? _gameLoopTimer;
    private System.Timers.Timer? _gameTimeTimer;
    private DotNetObjectReference<GameEngineService>? _objRef;
    
    // Game state
    private GameStatus _status = GameStatus.MENU;
    private GameStats _stats = new();
    private LevelData _levelData = new() { Map = GameConstants.GetDefaultLevelMap() };
    private Player _player = null!;
    private List<Entity> _entities = new();
    
    // Camera
    private double _cameraX = 0;
    
    // Input state
    private readonly HashSet<string> _keysDown = new();
    private readonly object _lockObject = new();
    
    // Events for UI updates
    public event Action? OnStateChanged;
    public event Action<GameStats>? OnStatsChanged;
    
    public GameStatus Status => _status;
    public GameStats Stats => _stats;
    
    public GameEngineService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    public async Task InitializeAsync()
    {
        _objRef = DotNetObjectReference.Create(this);
        _renderModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/gameRendererWebGL.js");
            
        await _renderModule.InvokeVoidAsync("initialize", _objRef);
    }
    
    public async Task StartGameAsync(LevelData? customLevel = null)
    {
        _stats = new GameStats 
        { 
            Score = 0, 
            Coins = 0, 
            Time = 400, 
            Lives = 3, 
            World = customLevel != null ? "1-AI" : "1-1"
        };
        
        if (customLevel == null)
        {
            _levelData = new LevelData 
            { 
                Map = GameConstants.GetDefaultLevelMap(),
                Entities = GameConstants.GetDefaultEntities()
            };
        }
        else
        {
            _levelData = customLevel;
        }
        
        InitializePlayer();
        _entities = new List<Entity>(_levelData.Entities);
        _cameraX = 0;
        _status = GameStatus.PLAYING;
        
        // Start game loop at 60 FPS
        _gameLoopTimer?.Dispose();
        _gameLoopTimer = new System.Timers.Timer(1000.0 / 60.0);
        _gameLoopTimer.Elapsed += (s, e) => GameLoop();
        _gameLoopTimer.AutoReset = true;
        _gameLoopTimer.Start();
        
        // Start game time countdown
        _gameTimeTimer?.Dispose();
        _gameTimeTimer = new System.Timers.Timer(1000);
        _gameTimeTimer.Elapsed += (s, e) => {
            if (_status == GameStatus.PLAYING)
            {
                _stats.Time--;
                if (_stats.Time <= 0)
                {
                    OnPlayerDie();
                }
                OnStatsChanged?.Invoke(_stats);
            }
        };
        _gameTimeTimer.AutoReset = true;
        _gameTimeTimer.Start();
        
        OnStateChanged?.Invoke();
    }
    
    public void StopGame()
    {
        _gameLoopTimer?.Stop();
        _gameTimeTimer?.Stop();
    }
    
    public void SetStatus(GameStatus status)
    {
        _status = status;
        if (status != GameStatus.PLAYING)
        {
            StopGame();
        }
        OnStateChanged?.Invoke();
    }
    
    private void InitializePlayer()
    {
        _player = new Player
        {
            Pos = new Vector2D { X = 50, Y = 100 },
            Vel = new Vector2D { X = 0, Y = 0 },
            Width = 12,
            Height = 16,
            PowerMode = "small",
            IsJumping = false,
            Grounded = false,
            Direction = 1
        };
        
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
                    double groundTopY = y * GameConstants.TILE_SIZE;
                    double marioY = groundTopY - _player.Height;
                    
                    if (marioY < groundTopY)
                    {
                        _player.Pos = new Vector2D { X = checkX * GameConstants.TILE_SIZE + 2, Y = marioY };
                        return;
                    }
                }
            }
        }
        
        // Fallback
        _player.Pos = new Vector2D { X = START_X, Y = 12 * GameConstants.TILE_SIZE - _player.Height };
    }
    
    private void GameLoop()
    {
        if (_status != GameStatus.PLAYING) return;
        
        lock (_lockObject)
        {
            try
            {
                UpdateGameplay();
                _ = RenderAsync(); // Fire and forget
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Game loop error: {ex.Message}");
            }
        }
    }
    
    private void UpdateGameplay()
    {
        // Player horizontal movement
        if (_keysDown.Contains("ArrowLeft") || _keysDown.Contains("KeyA"))
        {
            _player.Vel.X -= GameConstants.ACCELERATION;
            _player.Direction = -1;
        }
        if (_keysDown.Contains("ArrowRight") || _keysDown.Contains("KeyD"))
        {
            _player.Vel.X += GameConstants.ACCELERATION;
            _player.Direction = 1;
        }
        
        // Apply friction
        _player.Vel.X *= GameConstants.FRICTION;
        
        // Max speed
        bool isRunning = _keysDown.Contains("ShiftLeft") || _keysDown.Contains("ShiftRight");
        double maxSpeed = isRunning ? GameConstants.MAX_RUN_SPEED : GameConstants.MAX_WALK_SPEED;
        _player.Vel.X = Math.Clamp(_player.Vel.X, -maxSpeed, maxSpeed);
        
        // Jumping - SMW style with spin jump
        if ((_keysDown.Contains("Space") || _keysDown.Contains("ArrowUp") || _keysDown.Contains("KeyW")) && 
            _player.Grounded && !_player.IsJumping)
        {
            // Check if A key is held for spin jump (SMW feature)
            bool spinJump = _keysDown.Contains("KeyA");
            double jumpForce = spinJump ? GameConstants.SPIN_JUMP_FORCE : GameConstants.JUMP_FORCE;
            
            _player.Vel.Y = -jumpForce;
            _player.IsJumping = true;
            _player.Grounded = false;
            _player.IsSpinJumping = spinJump;
            
            // Play jump sound
            _ = PlaySoundAsync("jump");
        }
        
        // Reset spin jump when grounded
        if (_player.Grounded)
        {
            _player.IsSpinJumping = false;
        }
        
        // Apply gravity
        _player.Vel.Y += GameConstants.GRAVITY;
        _player.Vel.Y = Math.Min(_player.Vel.Y, 10.0); // Max fall speed
        
        // Update position
        _player.Pos.X += _player.Vel.X;
        _player.Pos.Y += _player.Vel.Y;
        
        // Collision detection
        HandleCollisions();
        
        // Update camera
        UpdateCamera();
        
        // Update entities
        UpdateEntities();
        
        // Check victory
        CheckVictoryCondition();
    }
    
    private void HandleCollisions()
    {
        _player.Grounded = false;
        int[] groundTiles = { 1, 2, 3, 5, 6, 7, 8, 9 };
        
        int startX = Math.Max(0, (int)(_player.Pos.X / GameConstants.TILE_SIZE));
        int endX = Math.Min((int)((_player.Pos.X + _player.Width) / GameConstants.TILE_SIZE), (_levelData.Map[0]?.Length ?? 0) - 1);
        int startY = Math.Max(0, (int)(_player.Pos.Y / GameConstants.TILE_SIZE));
        int endY = Math.Min((int)((_player.Pos.Y + _player.Height) / GameConstants.TILE_SIZE), _levelData.Map.Length - 1);
        
        for (int y = startY; y <= endY; y++)
        {
            if (y < 0 || y >= _levelData.Map.Length || _levelData.Map[y] == null) continue;
            
            for (int x = startX; x <= endX; x++)
            {
                if (x < 0 || x >= _levelData.Map[y].Length) continue;
                
                int tile = _levelData.Map[y][x];
                
                if (Array.IndexOf(groundTiles, tile) >= 0)
                {
                    double tileX = x * GameConstants.TILE_SIZE;
                    double tileY = y * GameConstants.TILE_SIZE;
                    double tileRight = tileX + GameConstants.TILE_SIZE;
                    double tileBottom = tileY + GameConstants.TILE_SIZE;
                    
                    if (_player.Pos.X < tileRight && 
                        _player.Pos.X + _player.Width > tileX &&
                        _player.Pos.Y < tileBottom && 
                        _player.Pos.Y + _player.Height > tileY)
                    {
                        double overlapLeft = (_player.Pos.X + _player.Width) - tileX;
                        double overlapRight = tileRight - _player.Pos.X;
                        double overlapTop = (_player.Pos.Y + _player.Height) - tileY;
                        double overlapBottom = tileBottom - _player.Pos.Y;
                        
                        double minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), 
                                                      Math.Min(overlapTop, overlapBottom));
                        
                        if (minOverlap == overlapTop && _player.Vel.Y > 0)
                        {
                            _player.Pos.Y = tileY - _player.Height;
                            _player.Vel.Y = 0;
                            _player.Grounded = true;
                            _player.IsJumping = false;
                        }
                        else if (minOverlap == overlapBottom && _player.Vel.Y < 0)
                        {
                            _player.Pos.Y = tileBottom;
                            _player.Vel.Y = 0;
                            
                            // Check if hitting question block from below
                            if (tile == 3) // Question block
                            {
                                // Change to hit question block
                                _levelData.Map[y][x] = 4;
                                
                                // Spawn mushroom
                                SpawnMushroom(x, y);
                                _ = PlaySoundAsync("powerup-appears");
                            }
                        }
                        else if (minOverlap == overlapLeft && _player.Vel.X > 0)
                        {
                            _player.Pos.X = tileX - _player.Width;
                            _player.Vel.X = 0;
                        }
                        else if (minOverlap == overlapRight && _player.Vel.X < 0)
                        {
                            _player.Pos.X = tileRight;
                            _player.Vel.X = 0;
                        }
                    }
                }
                else if (tile == 12) // Coin tile
                {
                    // Check if player is overlapping coin
                    double tileX = x * GameConstants.TILE_SIZE;
                    double tileY = y * GameConstants.TILE_SIZE;
                    double tileRight = tileX + GameConstants.TILE_SIZE;
                    double tileBottom = tileY + GameConstants.TILE_SIZE;
                    
                    if (_player.Pos.X < tileRight && 
                        _player.Pos.X + _player.Width > tileX &&
                        _player.Pos.Y < tileBottom && 
                        _player.Pos.Y + _player.Height > tileY)
                    {
                        _levelData.Map[y][x] = 0;
                        _stats.Coins++;
                        _stats.Score += 100;
                        OnStatsChanged?.Invoke(_stats);
                        _ = PlaySoundAsync("coin");
                    }
                }
            }
        }
        
        if (_player.Pos.Y > GameConstants.SCREEN_HEIGHT)
        {
            OnPlayerDie();
        }
        
        _player.Pos.X = Math.Max(0, _player.Pos.X);
    }
    
    private void UpdateCamera()
    {
        double targetCameraX = _player.Pos.X - GameConstants.SCREEN_WIDTH / 2 + _player.Width / 2;
        targetCameraX = Math.Max(0, targetCameraX);
        _cameraX += (targetCameraX - _cameraX) * 0.1;
    }
    
    private void UpdateEntities()
    {
        int[] groundTiles = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        
        for (int i = _entities.Count - 1; i >= 0; i--)
        {
            var entity = _entities[i];
            
            if (entity.Dead) continue;
            
            // Handle mushroom power-ups
            if (entity.Type == EntityType.MUSHROOM)
            {
                // Mushroom movement
                entity.Vel.X = entity.Direction * 1.2;
                entity.Pos.X += entity.Vel.X;
                entity.Vel.Y += GameConstants.GRAVITY;
                entity.Pos.Y += entity.Vel.Y;
                
                // Ground collision
                int tileX = (int)(entity.Pos.X / GameConstants.TILE_SIZE);
                int tileY = (int)((entity.Pos.Y + entity.Height) / GameConstants.TILE_SIZE);
                
                if (tileY >= 0 && tileY < _levelData.Map.Length)
                {
                    if (_levelData.Map[tileY] != null && tileX >= 0 && tileX < _levelData.Map[tileY].Length)
                    {
                        if (Array.IndexOf(groundTiles, _levelData.Map[tileY][tileX]) >= 0)
                        {
                            double groundY = tileY * GameConstants.TILE_SIZE;
                            if (entity.Pos.Y + entity.Height > groundY)
                            {
                                entity.Pos.Y = groundY - entity.Height;
                                entity.Vel.Y = 0;
                            }
                        }
                    }
                }
                
                // Player collision with mushroom
                if (CheckCollision(_player, entity))
                {
                    _entities.RemoveAt(i);
                    _player.PowerMode = "big";
                    _player.Height = 24; // Grow taller
                    _stats.Score += 1000;
                    OnStatsChanged?.Invoke(_stats);
                    _ = PlaySoundAsync("powerup");
                    continue;
                }
            }
            // Handle enemies (Goomba, Koopa, Rex)
            else if (entity.Type == EntityType.GOOMBA || entity.Type == EntityType.KOOPA)
            {
                entity.Vel.X = entity.Direction * 0.6;
                entity.Pos.X += entity.Vel.X;
                entity.Vel.Y += GameConstants.GRAVITY;
                entity.Pos.Y += entity.Vel.Y;
                
                // Ground collision for enemies
                int tileX = (int)(entity.Pos.X / GameConstants.TILE_SIZE);
                int tileY = (int)((entity.Pos.Y + entity.Height) / GameConstants.TILE_SIZE);
                
                if (tileY >= 0 && tileY < _levelData.Map.Length)
                {
                    if (_levelData.Map[tileY] != null && tileX >= 0 && tileX < _levelData.Map[tileY].Length)
                    {
                        if (Array.IndexOf(groundTiles, _levelData.Map[tileY][tileX]) >= 0)
                        {
                            double groundY = tileY * GameConstants.TILE_SIZE;
                            if (entity.Pos.Y + entity.Height > groundY)
                            {
                                entity.Pos.Y = groundY - entity.Height;
                                entity.Vel.Y = 0;
                            }
                        }
                    }
                }
                
                // Player collision with enemy
                if (CheckCollision(_player, entity))
                {
                    if (_player.Vel.Y > 0 && _player.Pos.Y < entity.Pos.Y)
                    {
                        // Jump on enemy
                        entity.Dead = true;
                        
                        // SMW spin jump gives smaller bounce
                        if (_player.IsSpinJumping)
                        {
                            _player.Vel.Y = -2.5;
                            _stats.Score += 200; // Bonus points for spin jump
                        }
                        else
                        {
                            _player.Vel.Y = -GameConstants.BOUNCE_FORCE;
                            _stats.Score += 100;
                        }
                        OnStatsChanged?.Invoke(_stats);
                        _ = PlaySoundAsync("stomp");
                    }
                    else if (!_player.IsSpinJumping)
                    {
                        // Hit by enemy (spin jump protects you in SMW)
                        OnPlayerDie();
                    }
                }
            }
        }
    }
    
    private bool CheckCollision(Entity a, Entity b)
    {
        return a.Pos.X < b.Pos.X + b.Width &&
               a.Pos.X + a.Width > b.Pos.X &&
               a.Pos.Y < b.Pos.Y + b.Height &&
               a.Pos.Y + a.Height > b.Pos.Y;
    }
    
    private void SpawnMushroom(int blockX, int blockY)
    {
        var mushroom = new Entity
        {
            Id = _entities.Count + 1000,
            Type = EntityType.MUSHROOM,
            Pos = new Vector2D 
            { 
                X = blockX * GameConstants.TILE_SIZE, 
                Y = (blockY - 1) * GameConstants.TILE_SIZE 
            },
            Vel = new Vector2D { X = 1.2, Y = -2 },
            Width = 16,
            Height = 16,
            Dead = false,
            Direction = 1,
            State = "mushroom"
        };
        
        _entities.Add(mushroom);
    }
    
    private void CheckVictoryCondition()
    {
        int maxX = (_levelData.Map[0]?.Length ?? 0) * GameConstants.TILE_SIZE;
        if (_player.Pos.X >= maxX - 50)
        {
            _status = GameStatus.VICTORY;
            StopGame();
            OnStateChanged?.Invoke();
        }
    }
    
    private void OnPlayerDie()
    {
        _stats.Lives--;
        if (_stats.Lives < 0)
        {
            _status = GameStatus.GAME_OVER;
            StopGame();
        }
        else
        {
            InitializePlayer();
            _stats.Time = 400;
        }
        OnStatsChanged?.Invoke(_stats);
        OnStateChanged?.Invoke();
    }
    
    private async Task RenderAsync()
    {
        if (_renderModule == null) return;
        
        try
        {
            // Simplified render data to avoid serialization issues
            var renderData = new
            {
                player = new {
                    pos = new { x = _player.Pos.X, y = _player.Pos.Y },
                    width = _player.Width,
                    height = _player.Height,
                    direction = _player.Direction,
                    powerMode = _player.PowerMode
                },
                entities = _entities.Where(e => !e.Dead).Select(e => new {
                    pos = new { x = e.Pos.X, y = e.Pos.Y },
                    width = e.Width,
                    height = e.Height,
                    type = (int)e.Type,
                    state = e.State
                }).ToList(),
                cameraX = _cameraX,
                level = _levelData.Map
            };
            
            await _renderModule.InvokeVoidAsync("render", renderData);
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, stop the game
            StopGame();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Render error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
    
    private async Task PlaySoundAsync(string soundName)
    {
        if (_renderModule == null) return;
        
        try
        {
            await _renderModule.InvokeVoidAsync("playSound", soundName);
        }
        catch (Exception ex)
        {
            // Sound errors are non-critical, just log
            Console.WriteLine($"Sound error: {ex.Message}");
        }
    }
    
    [JSInvokable]
    public void OnKeyDown(string code)
    {
        lock (_lockObject)
        {
            _keysDown.Add(code);
        }
    }
    
    [JSInvokable]
    public void OnKeyUp(string code)
    {
        lock (_lockObject)
        {
            _keysDown.Remove(code);
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        _gameLoopTimer?.Dispose();
        _gameTimeTimer?.Dispose();
        
        if (_renderModule != null)
        {
            await _renderModule.DisposeAsync();
        }
        
        _objRef?.Dispose();
    }
}
