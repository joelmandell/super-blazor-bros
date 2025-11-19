# Sprite Integration Guide

This guide explains how to integrate Super Mario World sprites into the Blazor app.

## Overview

The game now supports rendering with authentic Super Mario World sprite images instead of colored rectangles. The sprite system is designed to:

1. **Automatically detect** available sprite images
2. **Gracefully fallback** to colored rectangles if sprites are missing
3. **Cache sprites** for optimal performance
4. **Support all game entities** (player, enemies, items, tiles)

## Quick Start

### Step 1: Download Sprites

Visit the TCRF development sprites page and download the sprites you need:
- **URL**: https://tcrf.net/Development:Super_Mario_World_%28SNES%29/Sprites
- **See**: `BlazorApp/wwwroot/sprites/download-sprites.md` for detailed instructions

### Step 2: Organize Sprites

Place downloaded sprites in the correct directories:

```
BlazorApp/wwwroot/sprites/
├── characters/
│   ├── mario-small.png      # Required
│   ├── mario-big.png         # Required
│   └── mario-fire.png        # Optional
├── enemies/
│   ├── goomba.png            # Recommended
│   ├── koopa-green.png       # Recommended
│   └── rex.png               # Recommended
├── items/
│   ├── mushroom.png          # Recommended
│   └── coin.png              # Recommended
└── tiles/
    ├── ground.png            # Recommended
    ├── brick.png             # Recommended
    └── question-block.png    # Recommended
```

### Step 3: Run the Game

```bash
cd BlazorApp
dotnet run
```

Open browser at `http://localhost:5202` and the game will automatically load available sprites!

## Architecture

### Components

1. **SpriteLoaderService** (`Services/SpriteLoaderService.cs`)
   - C# service that manages sprite metadata
   - Maps sprite names to URLs
   - Provides helper methods for entity-to-sprite mapping

2. **spriteLoader.js** (`wwwroot/js/spriteLoader.js`)
   - JavaScript module for loading and caching sprite images
   - Preloads sprites on game initialization
   - Provides fast access to cached images

3. **gameRendererWithSprites.js** (`wwwroot/js/gameRendererWithSprites.js`)
   - Enhanced renderer that supports sprite rendering
   - Falls back to colored rectangles if sprites unavailable
   - Handles sprite flipping for player direction

### How It Works

```
Game Initialization
        ↓
SpriteLoaderService.PreloadSpritesAsync()
        ↓
JavaScript: preloadSprites()
        ↓
Load each sprite image
        ↓
Cache in memory
        ↓
Game Rendering
        ↓
Check if sprite available
        ↓
    ┌──────┴──────┐
    ↓             ↓
Use Sprite    Use Colored
             Rectangle
```

## Using the Sprite System

### In C# Code

```csharp
// Inject the service
@inject SpriteLoaderService SpriteLoader

// Check if sprite is available
bool hasSprite = SpriteLoader.IsSpriteAvailable("mario-small");

// Get sprite URL
string url = SpriteLoader.GetSpriteUrl("mario-small");

// Get player sprite based on power mode
string sprite = SpriteLoader.GetPlayerSprite("big", "mario");
// Returns: "mario-big"

// Get entity sprite
string enemySprite = SpriteLoader.GetEntitySprite("goomba");
// Returns: "goomba"

// Get tile sprite
string tileSprite = SpriteLoader.GetTileSprite(1);
// Returns: "ground"
```

### In JavaScript

```javascript
import { preloadSprite, getSprite, isSpriteLoaded } from './spriteLoader.js';

// Preload a sprite
await preloadSprite('mario-small', 'sprites/characters/mario-small.png');

// Check if loaded
if (isSpriteLoaded('mario-small')) {
    const sprite = getSprite('mario-small');
    ctx.drawImage(sprite, x, y, width, height);
}
```

## Sprite Specifications

### File Format
- **Type**: PNG with alpha transparency
- **Bit Depth**: 32-bit RGBA or 24-bit RGB with alpha

### Dimensions

| Sprite Type | Recommended Size | Notes |
|-------------|-----------------|-------|
| Small Mario | 12x16 pixels | Small form |
| Big Mario | 12x24 pixels | Super form |
| Enemies | 16x16 or 16x24 | Varies by enemy |
| Items | 16x16 pixels | Standard |
| Tiles | 16x16 pixels | SNES tile size |

### Naming Convention

Use lowercase with hyphens:
- ✅ `mario-small.png`
- ✅ `koopa-green.png`
- ✅ `question-block.png`
- ❌ `MarioSmall.png`
- ❌ `Koopa_Green.PNG`

## Extending the System

### Adding New Sprites

1. **Update SpriteLoaderService.cs**:
```csharp
private void InitializeSpriteUrls()
{
    // Add your new sprite
    _spriteUrls["yoshi"] = "sprites/characters/yoshi.png";
}
```

2. **Add to appropriate directory**:
```bash
cp yoshi.png BlazorApp/wwwroot/sprites/characters/
```

3. **Use in renderer**:
```javascript
const sprite = getSprite('yoshi');
if (sprite) {
    ctx.drawImage(sprite, x, y, width, height);
}
```

### Creating Custom Renderers

You can create alternative renderers by implementing the same interface:

```javascript
export function initialize(dotNetRef) { }
export function render(renderData) { }
export function playSound(soundName) { }
export function setUseSpriteMode(enabled) { }
```

## Troubleshooting

### Sprites Not Loading

**Check browser console**:
- Look for 404 errors (file not found)
- Verify sprite URLs are correct

**Verify file paths**:
```bash
cd BlazorApp/wwwroot
ls -la sprites/characters/mario-small.png
```

**Check file names** (case-sensitive on Linux):
- Ensure lowercase with hyphens
- Verify `.png` extension

### Sprites Appear Blurry

**Disable image smoothing** in your renderer:
```javascript
ctx.imageSmoothingEnabled = false;
```

This is already done in `gameRendererWithSprites.js`.

### Performance Issues

**Preload sprites** during initialization:
```csharp
await SpriteLoader.PreloadSpritesAsync();
```

**Use sprite caching**:
- Sprites are automatically cached after first load
- No need to reload sprites each frame

## Testing

### Manual Testing

1. **Without sprites** (baseline):
   - Run game without any sprite files
   - Should show colored rectangles
   - Verify gameplay works

2. **With some sprites**:
   - Add just Mario sprites
   - Should show Mario as sprite, enemies as rectangles
   - Verify mixed rendering works

3. **With all sprites**:
   - Add all sprite categories
   - Should show all entities as sprites
   - Verify no fallbacks used

### Browser Console Checks

Open DevTools (F12) and look for:
```
Sprite loaded: mario-small
Sprite loaded: mario-big
Sprite mode: enabled
```

Or warnings:
```
Failed to load sprite: mario-fire from sprites/characters/mario-fire.png
```

## Performance Considerations

### Sprite Caching
- All sprites are cached in memory after first load
- Reduces file I/O and network requests
- Improves rendering performance

### Fallback System
- Missing sprites don't break the game
- Fallback rectangles are fast to render
- No performance penalty for mixed mode

### Optimization Tips
1. Keep sprite file sizes small (< 50KB each)
2. Use PNG compression
3. Preload sprites during game initialization
4. Avoid loading sprites during gameplay

## Copyright Notice

⚠️ **Important**: Super Mario World sprites are copyrighted by Nintendo.

- Use only for personal, educational, or non-commercial projects
- Do not distribute commercially
- Respect intellectual property rights
- This implementation is for educational purposes

## Additional Resources

- **TCRF Sprites Page**: https://tcrf.net/Development:Super_Mario_World_%28SNES%29/Sprites
- **Sprite Documentation**: `BlazorApp/wwwroot/sprites/README.md`
- **Download Guide**: `BlazorApp/wwwroot/sprites/download-sprites.md`

## Support

If you encounter issues:
1. Check the documentation in `sprites/README.md`
2. Review browser console for errors
3. Verify sprite file paths and names
4. Test with minimal sprites first
5. Ensure PNG format with transparency
