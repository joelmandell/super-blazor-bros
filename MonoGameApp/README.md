# Super Blazor Bros - MonoGame Edition

A Super Mario Bros-inspired game built with MonoGame following the official MonoGame 2D game development tutorial patterns.

## Features

- 🎮 Classic Super Mario Bros gameplay
- 🖼️ Pixel-art graphics rendered with MonoGame SpriteBatch
- 🎯 Physics-based movement and collision detection
- 📊 HUD with score, coins, world, and time display
- 🏁 Complete level with flag pole ending
- 🔄 Game states: Menu, Playing, Game Over, Victory

## Technology Stack

- **MonoGame 3.8.2** - Cross-platform game framework
- **.NET 10** - Latest .NET runtime
- **C# 13** - Modern C# language features
- **MonoGame.Framework.DesktopGL** - Cross-platform desktop support

## Prerequisites

- .NET 10 SDK
- MonoGame 3.8.2 or later
- OpenGL support (provided by DesktopGL)

## Installation

1. Install dependencies:
   ```bash
   cd MonoGameApp
   dotnet restore
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the game:
   ```bash
   dotnet run
   ```

## Controls

- **Arrow Keys**: Move left/right
- **Space/Up Arrow**: Jump
- **Left Shift/Right Shift**: Run
- **Enter**: Start game / Confirm
- **Escape**: Exit game

## Project Structure

```
MonoGameApp/
├── Models/
│   ├── Constants.cs        # Game constants, physics, and colors
│   └── GameTypes.cs        # Game entities and enums
├── Services/
│   └── GeminiService.cs    # AI level generation (optional)
├── Game1.cs                # Main game class
├── Program.cs              # Application entry point
└── Content/                # Game assets (textures, fonts, etc.)
```

## Game Architecture

Following MonoGame best practices:

### Core Game Loop

1. **Initialize()**: Setup game state and level data
2. **LoadContent()**: Load textures and fonts
3. **Update(GameTime)**: Process input, physics, and game logic
4. **Draw(GameTime)**: Render game objects using SpriteBatch

### Rendering

The game uses MonoGame's `SpriteBatch` for efficient 2D rendering:
- Primitive shapes drawn using a 1x1 white pixel texture
- Colors applied during draw calls
- All rendering follows MonoGame 2D patterns

### Physics

Authentic NES-style physics implementation:
- Gravity: 0.25 units/frame
- Jump force: 6.6 units
- Friction: 90% velocity retention
- Two speed modes: walk (1.4) and run (2.6)

## Building for Production

### Windows
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

### Linux
```bash
dotnet publish -c Release -r linux-x64 --self-contained
```

### macOS
```bash
dotnet publish -c Release -r osx-x64 --self-contained
```

## Differences from JavaScript/Canvas Version

| Aspect | JavaScript/Canvas | MonoGame |
|--------|------------------|----------|
| Rendering | HTML5 Canvas API | MonoGame SpriteBatch |
| Game Loop | requestAnimationFrame | MonoGame GameTime |
| Input | DOM Events | MonoGame Input API |
| Platform | Web Browser | Desktop (Windows/Linux/macOS) |
| Performance | JavaScript Engine | Native .NET Runtime |

## MonoGame Tutorial Reference

This project follows the patterns from the official MonoGame documentation:
https://docs.monogame.net/articles/tutorials/building_2d_games/index.html

Key concepts applied:
- Game class inheritance
- Content pipeline usage
- SpriteBatch rendering
- Input handling with Keyboard
- Game state management
- Collision detection

## Future Enhancements

- [ ] Add sprite textures via Content Pipeline
- [ ] Implement sprite animations
- [ ] Add sound effects and music
- [ ] Create additional levels
- [ ] Add power-up system (mushroom, fire flower)
- [ ] Implement enemy AI improvements
- [ ] Add particle effects

## Development

### Running in Development Mode
```bash
dotnet run
```

### Hot Reload
MonoGame doesn't support hot reload for code changes. Restart the application after making changes.

### Debugging
Use Visual Studio, Visual Studio Code, or Rider for debugging support.

## License

MIT

## Credits

Built with MonoGame following the official 2D game development tutorials.
Inspired by Super Mario Bros (Nintendo).
