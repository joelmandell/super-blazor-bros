# MonoGame Migration Guide

## Overview

This document details the migration of Super Blazor Bros from a Blazor WebAssembly application using JavaScript and HTML5 Canvas to a MonoGame desktop application.

## Migration Reference

This migration follows the official MonoGame documentation:
**https://docs.monogame.net/articles/tutorials/building_2d_games/index.html**

## Architecture Changes

### Before: Blazor WebAssembly + JavaScript/Canvas

```
BlazorApp/
├── Pages/
│   └── Game.razor           # Blazor component with UI
├── Models/
│   ├── GameTypes.cs         # C# game models
│   └── Constants.cs         # Game constants
├── Services/
│   └── GeminiService.cs     # AI level generation
└── wwwroot/
    └── js/
        └── game.js          # JavaScript game engine
```

**Technology Stack:**
- Blazor WebAssembly (web frontend)
- C# for game logic and models
- JavaScript for rendering (Canvas API)
- JSInterop for communication
- HTML5 Canvas for graphics
- DOM Events for input

### After: MonoGame Desktop Application

```
MonoGameApp/
├── Game1.cs                 # Main game class
├── Program.cs               # Entry point
├── Models/
│   ├── GameTypes.cs         # Game models with MonoGame types
│   └── Constants.cs         # Game constants with MonoGame colors
├── Services/
│   └── GeminiService.cs     # AI level generation
└── Content/
    └── Content.mgcb         # MonoGame content pipeline
```

**Technology Stack:**
- MonoGame 3.8.2 (game framework)
- .NET 10 (runtime)
- C# 13 (language)
- MonoGame.Framework.DesktopGL (cross-platform)
- SpriteBatch for rendering
- MonoGame Input API

## Key Migration Steps

### 1. Project Setup

**Created MonoGame Project:**
```bash
dotnet new mgdesktopgl -n SuperBlazorBrosMonoGame -o MonoGameApp
```

**Updated Project File:**
- Target framework: net10.0
- Enabled ImplicitUsings
- Enabled Nullable reference types
- MonoGame packages: Framework.DesktopGL, Content.Builder.Task

### 2. Game Class Implementation

**Followed MonoGame Game Pattern:**

```csharp
public class Game1 : Game
{
    // Initialize(): Setup game state
    // LoadContent(): Load textures and resources
    // Update(GameTime): Game logic and input
    // Draw(GameTime): Render with SpriteBatch
}
```

**Key Implementations:**
- Game state management (Menu, Playing, Game Over, Victory)
- Player physics and movement
- Collision detection
- Camera system
- Entity updates
- HUD rendering

### 3. Rendering Migration

**From JavaScript Canvas API:**
```javascript
ctx.fillStyle = '#5C94FC';
ctx.fillRect(x, y, width, height);
```

**To MonoGame SpriteBatch:**
```csharp
_spriteBatch.Draw(_pixelTexture, 
    new Rectangle(x, y, width, height), 
    new Color(92, 148, 252));
```

**Rendering Approach:**
- Created 1x1 white pixel texture
- Draw colored rectangles using texture with color tint
- SpriteBatch for efficient batch rendering
- PointClamp sampling for pixel-perfect rendering

### 4. Input Migration

**From JavaScript DOM Events:**
```javascript
window.addEventListener('keydown', handleKeyDown);
```

**To MonoGame Keyboard API:**
```csharp
var keyboardState = Keyboard.GetState();
if (keyboardState.IsKeyDown(Keys.Space)) { }
```

**Input Handling:**
- Previous/current keyboard state comparison
- Key press detection (was up, now down)
- Continuous key hold detection
- Standard MonoGame input patterns

### 5. Game Loop Migration

**From JavaScript requestAnimationFrame:**
```javascript
function gameLoop() {
    update();
    render();
    requestAnimationFrame(gameLoop);
}
```

**To MonoGame GameTime:**
```csharp
protected override void Update(GameTime gameTime)
{
    // Update game logic at fixed timestep
    // Use gameTime.ElapsedGameTime for timing
}

protected override void Draw(GameTime gameTime)
{
    // Render using SpriteBatch
}
```

### 6. Physics and Collision

**JavaScript Implementation:**
```javascript
player.vel.y += GRAVITY;
player.pos.x += player.vel.x;
```

**MonoGame Implementation:**
```csharp
_player.Velocity = new Vector2(
    _player.Velocity.X, 
    _player.Velocity.Y + GameConstants.GRAVITY
);
_player.Position += _player.Velocity;
```

**Key Changes:**
- Used MonoGame Vector2 instead of custom Vector2D
- Same physics constants maintained
- Collision detection logic preserved
- Added proper null checks for nullable types

### 7. Type Migrations

**Vector2D → Vector2:**
```csharp
// Before (Blazor)
public class Vector2D
{
    public double X { get; set; }
    public double Y { get; set; }
}

// After (MonoGame)
using Microsoft.Xna.Framework;
// Use built-in Vector2
```

**Color Strings → Color Objects:**
```csharp
// Before (Blazor)
public const string SKY = "#5C94FC";

// After (MonoGame)
public static readonly Color SKY = new Color(92, 148, 252);
```

## MonoGame Patterns Applied

Following the official tutorial patterns:

### 1. Game Class Structure
```csharp
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }
    
    protected override void Initialize()
    {
        // Initialize game state
        base.Initialize();
    }
    
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        // Load content
    }
    
    protected override void Update(GameTime gameTime)
    {
        // Update logic
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();
        // Draw sprites
        _spriteBatch.End();
        base.Draw(gameTime);
    }
}
```

### 2. SpriteBatch Usage
```csharp
_spriteBatch.Begin(
    SpriteSortMode.Deferred, 
    BlendState.AlphaBlend, 
    SamplerState.PointClamp  // Pixel-perfect rendering
);

// Draw calls
_spriteBatch.Draw(texture, position, color);

_spriteBatch.End();
```

### 3. Input Handling
```csharp
_previousKeyboardState = _currentKeyboardState;
_currentKeyboardState = Keyboard.GetState();

private bool WasKeyPressed(Keys key)
{
    return _currentKeyboardState.IsKeyDown(key) && 
           _previousKeyboardState.IsKeyUp(key);
}
```

### 4. Game State Management
```csharp
public enum GameStatus
{
    MENU,
    LOADING,
    PLAYING,
    GAME_OVER,
    VICTORY
}

private GameStatus _status = GameStatus.MENU;
```

## Benefits of MonoGame

### 1. Performance
- Native .NET runtime (not JavaScript engine)
- Direct GPU access via OpenGL
- No browser overhead
- Better frame rates and responsiveness

### 2. Professional Framework
- Industry-standard game framework
- Used by commercial games
- Active community and support
- Cross-platform out of the box

### 3. Better Development Experience
- Single language (C#) throughout
- Strong typing and compile-time checks
- Better debugging tools
- Visual Studio/Rider integration

### 4. Distribution
- Standalone executables
- No web hosting required
- Desktop-native experience
- Easier deployment on Steam, itch.io, etc.

## Preserved Features

### ✅ Maintained
- Game physics and movement
- Collision detection
- Level data structure
- Enemy behavior
- Score and stats system
- Level completion detection
- Game over logic
- All game constants

### ✅ AI Level Generation
- GeminiService ported to MonoGame
- Same API integration
- LevelData structure maintained
- Can be extended for AI-generated levels

## Platform Support

### Supported Platforms
- ✅ Windows (x64, x86, ARM64)
- ✅ Linux (x64, ARM64)
- ✅ macOS (x64, ARM64)

### Build Commands
```bash
# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

## Testing Results

### Build Status
- ✅ Clean build successful
- ✅ Release build successful
- ✅ No compilation errors
- ✅ No compiler warnings (after fixes)

### Security Analysis
- ✅ CodeQL scan passed
- ✅ No security vulnerabilities found
- ✅ No unsafe code patterns

### Code Quality
- ✅ Follows MonoGame best practices
- ✅ Proper null safety
- ✅ Clean architecture
- ✅ Well-documented

## Future Enhancements

### Potential Improvements
1. **Graphics**
   - Add sprite textures via Content Pipeline
   - Implement sprite animations
   - Add particle effects
   - Create sprite sheets

2. **Audio**
   - Add background music
   - Implement sound effects
   - Use MonoGame audio API

3. **Content**
   - Create more levels
   - Add different enemies
   - Implement power-ups (mushroom, fire flower)
   - Add boss battles

4. **Polish**
   - Menu improvements
   - Pause functionality
   - Settings screen
   - Controller support
   - Better visual effects

## Comparison: Blazor vs MonoGame

| Feature | Blazor + JS/Canvas | MonoGame Desktop |
|---------|-------------------|------------------|
| Platform | Web Browser | Desktop Native |
| Performance | Good (WebAssembly) | Excellent (Native) |
| Distribution | Web Hosting | Standalone Exe |
| Development | C# + JavaScript | C# Only |
| Rendering | Canvas API | SpriteBatch/GPU |
| Input | DOM Events | MonoGame Input |
| Cross-platform | Any browser | Windows/Linux/macOS |
| Game Framework | Custom | Professional |
| Community | Smaller | Large |
| Commercial Use | Possible | Common |

## Conclusion

The migration from JavaScript/Canvas to MonoGame was successful, following all official MonoGame 2D game development patterns. The new implementation provides:

- ✅ Professional game framework
- ✅ Better performance
- ✅ Cross-platform desktop support
- ✅ Cleaner architecture
- ✅ All features preserved
- ✅ Room for growth

Both versions (Blazor and MonoGame) remain available in the repository, serving different deployment scenarios:
- **Blazor**: Web-based, easy sharing, no installation
- **MonoGame**: Desktop-native, better performance, professional distribution

## References

1. [MonoGame Official Documentation](https://docs.monogame.net/)
2. [Building 2D Games with MonoGame](https://docs.monogame.net/articles/tutorials/building_2d_games/index.html)
3. [MonoGame API Reference](https://docs.monogame.net/api/)
4. [MonoGame Community](https://community.monogame.net/)
