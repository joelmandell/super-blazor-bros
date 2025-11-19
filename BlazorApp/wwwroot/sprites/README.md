# Super Mario World Sprites

This directory contains sprite images for the Super Mario World game in the Blazor app.

## Sprite Sources

The sprites for this game should be downloaded from:
**https://tcrf.net/Development:Super_Mario_World_%28SNES%29/Sprites**

This page contains development sprites and unused graphics from Super Mario World (SNES).

## Directory Structure

```
sprites/
├── characters/     # Mario, Luigi sprites in different states
├── enemies/        # Goomba, Koopa, Rex, and other enemy sprites
├── items/          # Mushroom, Fire Flower, coins, etc.
├── tiles/          # Ground tiles, blocks, pipes, etc.
└── README.md       # This file
```

## Required Sprites

### Characters (characters/)
- `mario-small.png` - Small Mario sprite
- `mario-big.png` - Super Mario sprite
- `mario-fire.png` - Fire Mario sprite (optional)
- `luigi-small.png` - Small Luigi sprite (optional)
- `luigi-big.png` - Super Luigi sprite (optional)

### Enemies (enemies/)
- `goomba.png` - Goomba enemy sprite
- `koopa-green.png` - Green Koopa Troopa
- `koopa-red.png` - Red Koopa Troopa
- `rex.png` - Rex (purple dinosaur enemy from SMW)
- `shell-green.png` - Green Koopa shell
- `shell-red.png` - Red Koopa shell

### Items (items/)
- `mushroom.png` - Super Mushroom power-up
- `fire-flower.png` - Fire Flower power-up (optional)
- `coin.png` - Coin collectible
- `star.png` - Super Star (optional)

### Tiles (tiles/)
- `ground.png` - Ground/dirt tile
- `brick.png` - Brick block
- `question-block.png` - Question block (unbroken)
- `question-block-used.png` - Used question block
- `pipe-top-left.png` - Pipe top left section
- `pipe-top-right.png` - Pipe top right section
- `pipe-vertical.png` - Pipe vertical section
- `platform.png` - Platform tile

## Sprite Specifications

All sprites should follow these specifications:
- **Format**: PNG with transparency
- **Tile Size**: 16x16 pixels (standard SMW tile size)
- **Character Size**: Typically 12x16 pixels (small) or 12x24 pixels (big)
- **Enemy Size**: Varies by enemy type
- **Transparency**: Use PNG alpha channel for transparency

## Manual Download Instructions

Since automated downloading may be restricted:

1. Visit https://tcrf.net/Development:Super_Mario_World_%28SNES%29/Sprites
2. Right-click on each sprite image you need
3. Select "Save image as..."
4. Save to the appropriate subdirectory:
   - Character sprites → `characters/`
   - Enemy sprites → `enemies/`
   - Item sprites → `items/`
   - Tile sprites → `tiles/`
5. Rename files according to the naming convention above

## Alternative Sprite Sources

If you cannot access TCRF, you can also:
1. Extract sprites from Super Mario World ROM using sprite ripping tools
2. Use open-source sprite sheets from game development communities
3. Create your own sprites matching the 16-bit SNES style

**Note**: Ensure you have the right to use any sprites you download. Original Super Mario World graphics are copyrighted by Nintendo.

## Integration with Game

Once sprites are downloaded:
1. Place them in the appropriate directories
2. The game's sprite loading service will automatically detect and load them
3. The renderer will use these sprites instead of colored rectangles
4. Sprite animations will be handled by the game engine

## Sprite Naming Convention

Use lowercase with hyphens:
- Good: `mario-small.png`, `koopa-green.png`
- Bad: `MarioSmall.png`, `Koopa_Green.PNG`

## Testing

After adding sprites:
1. Run the Blazor app: `dotnet run` (from BlazorApp directory)
2. Open browser at `http://localhost:5202`
3. Verify sprites are loading correctly in the browser console
4. Start the game and check that sprites appear instead of rectangles

## Troubleshooting

If sprites don't appear:
- Check browser console for loading errors
- Verify file names match exactly (case-sensitive)
- Ensure PNG files have proper transparency
- Check that sprites are in the correct directories
- Verify file sizes are reasonable (< 100KB per sprite)
