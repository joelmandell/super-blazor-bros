# Super Blazor Bros

A Super Mario Bros-inspired game now built with **MonoGame** and .NET 10, following MonoGame 2D game development patterns.

## 🎮 MonoGame Version (NEW!)

The game has been migrated from JavaScript/Canvas to **MonoGame**, a professional cross-platform game framework.

### Features

- 🎮 Classic Super Mario Bros gameplay
- 🖼️ Native 2D rendering with MonoGame SpriteBatch
- 🎯 Authentic NES-style physics
- 🖥️ Cross-platform desktop support (Windows, Linux, macOS)
- 📊 HUD with score, coins, world, and time
- 🏁 Complete level with victory condition

### Quick Start

```bash
cd MonoGameApp
dotnet restore
dotnet run
```

**Controls:**
- Arrow Keys: Move
- Space/Up: Jump
- Shift: Run
- Enter: Start/Confirm
- Escape: Exit

For full documentation, see [MonoGameApp/README.md](MonoGameApp/README.md)

## 📚 Technologies

- **MonoGame 3.8.2** - Cross-platform game framework
- **.NET 10** - Latest .NET runtime
- **C# 13** - Modern C# language features
- **OpenGL** - Graphics rendering (via DesktopGL)

## 🏗️ Architecture

Following the official MonoGame 2D game tutorial patterns:
- Game class with proper Update/Draw loop
- SpriteBatch for efficient 2D rendering
- Keyboard input handling
- Game state management
- Collision detection system

Reference: https://docs.monogame.net/articles/tutorials/building_2d_games/index.html

## 🌐 Blazor WebAssembly Version (NEW!)

The Blazor app now runs with **MonoGame-style C# game engine as WebAssembly**!

### Blazor Features
- 🎮 **Pure C# game logic** running as WebAssembly (no JavaScript game code!)
- 🤖 AI-generated levels with Google Gemini 2.0 Flash
- 📱 Web-based with touch controls
- 🌐 Runs in browser via WebAssembly
- ⚡ MonoGame-inspired architecture adapted for the browser

### Running Blazor Version

```bash
cd BlazorApp
dotnet restore
dotnet run
```

Open browser at `http://localhost:5202`

## 🔄 Architecture Highlights

| Aspect | Blazor WebAssembly | MonoGame Desktop |
|--------|-------------------|------------------|
| Rendering | Canvas via JSInterop | MonoGame SpriteBatch |
| Game Loop | C# Timer (60 FPS) | MonoGame GameTime |
| Game Logic | **C# WebAssembly** | C# Native |
| Input | JSInterop + Keyboard | MonoGame Keyboard API |
| Platform | Web Browser | Desktop Native |
| Performance | WebAssembly | Native .NET Runtime |

**Key Achievement**: The Blazor version now runs **100% C# game logic** as WebAssembly, with only minimal JavaScript for Canvas rendering calls!

## 🚀 Building for Production

### MonoGame Desktop

Windows:
```bash
cd MonoGameApp
dotnet publish -c Release -r win-x64 --self-contained
```

Linux:
```bash
dotnet publish -c Release -r linux-x64 --self-contained
```

macOS:
```bash
dotnet publish -c Release -r osx-x64 --self-contained
```

### Blazor WebAssembly

```bash
cd BlazorApp
dotnet publish -c Release
```

## 📝 License

MIT
