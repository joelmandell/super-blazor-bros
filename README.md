# Super Mario World - Blazor Edition

A Super Mario World (SNES 16-bit)-inspired game now built with **MonoGame** and .NET 10, recreating Yoshi's Island 1!

## 🎮 MonoGame Version - Super Mario World Style!

The game features **Super Mario World** graphics, gameplay, and the famous first level: **Yoshi's Island 1**.

### Features

- 🎮 Super Mario World gameplay with spin jump mechanic
- 🖼️ 16-bit SNES-style graphics with vibrant colors
- 🌀 **Spin Jump** - Hold A while jumping for higher jumps and enemy protection!
- 🎯 SMW-style physics (floatier, faster than NES)
- 🖥️ Cross-platform desktop support (Windows, Linux, macOS)
- 📊 HUD with score, coins, world (YI1), and time
- 🦖 **Rex and Koopa Troopa enemies** from Super Mario World
- 🏁 Yoshi's Island 1 level layout with goal tape

### Quick Start

```bash
cd MonoGameApp
dotnet restore
dotnet run
```

**Controls:**
- Arrow Keys: Move
- Space/Up: Jump
- **A**: Hold while jumping for **Spin Jump** (protects from enemies!)
- Shift: Run (SMW is faster!)
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

## 🌐 Blazor WebAssembly Version

The Blazor app now runs with **Super Mario World-style C# game engine as WebAssembly**!

### Blazor Features
- 🎮 **Pure C# game logic** running as WebAssembly (no JavaScript game code!)
- 🌀 Super Mario World gameplay with spin jump support
- 📱 Web-based with touch controls
- 🌐 Runs in browser via WebAssembly
- 🦖 **Rex and Koopa enemies** from Super Mario World
- ⚡ SMW-inspired 16-bit graphics and physics

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
