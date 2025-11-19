# Sprite Download Instructions

## Manual Download Process

Since automated downloading from TCRF may be restricted, follow these steps to manually obtain the sprites:

### Step 1: Visit the TCRF Page

Navigate to: https://tcrf.net/Development:Super_Mario_World_%28SNES%29/Sprites

### Step 2: Identify Required Sprites

Look for the following sprite categories on the page:

#### Mario/Player Sprites
- Small Mario (standing, walking, jumping)
- Super Mario (big form)
- Fire Mario (if available)

#### Enemy Sprites
- Goomba
- Koopa Troopa (Green and Red)
- Rex (purple dinosaur enemy)
- Koopa Shell variations

#### Item Sprites
- Super Mushroom
- Fire Flower
- Coin
- Star (invincibility power-up)

#### Tile Sprites
- Ground/dirt tiles
- Brick blocks
- Question blocks (active and used)
- Pipe sections (top, middle, corners)
- Platform tiles

### Step 3: Download Each Sprite

For each sprite image on the TCRF page:

1. Right-click on the sprite image
2. Select "Save image as..." or "Save picture as..."
3. Choose the appropriate directory:
   - `characters/` for Mario/Luigi sprites
   - `enemies/` for enemy sprites
   - `items/` for power-ups and collectibles
   - `tiles/` for level tiles
4. Use descriptive filenames (lowercase with hyphens):
   - Examples: `mario-small.png`, `koopa-green.png`, `mushroom.png`

### Step 4: Organize Your Downloads

Ensure your sprite directory structure looks like this:

```
BlazorApp/wwwroot/sprites/
├── characters/
│   ├── mario-small.png
│   ├── mario-big.png
│   └── mario-fire.png
├── enemies/
│   ├── goomba.png
│   ├── koopa-green.png
│   ├── koopa-red.png
│   ├── rex.png
│   └── shell-green.png
├── items/
│   ├── mushroom.png
│   ├── fire-flower.png
│   ├── coin.png
│   └── star.png
└── tiles/
    ├── ground.png
    ├── brick.png
    ├── question-block.png
    ├── question-block-used.png
    ├── pipe-top-left.png
    ├── pipe-top-right.png
    ├── pipe-vertical.png
    └── platform.png
```

## Sprite Specifications

When downloading or creating sprites, ensure they meet these requirements:

### Format
- **File Type**: PNG with transparency (alpha channel)
- **Color Mode**: RGB/RGBA

### Dimensions
- **Tiles**: 16x16 pixels (native SNES size)
- **Small Mario**: 12x16 pixels
- **Big Mario**: 12x24 pixels
- **Enemies**: Varies (typically 16x16 to 16x24)
- **Items**: 16x16 pixels

### Quality Guidelines
- Use pixel-perfect sprites (no anti-aliasing on edges)
- Maintain authentic SNES/Super Mario World art style
- Ensure transparent backgrounds (not white backgrounds)
- Keep file sizes reasonable (< 50KB per sprite)

## Alternative Sources

If TCRF is not accessible, you can obtain sprites from:

1. **The Spriters Resource** (https://www.spriters-resource.com/)
   - Search for "Super Mario World"
   - Download complete sprite sheets
   - Extract individual sprites

2. **ROM Extraction**
   - Use sprite ripping tools with a Super Mario World ROM
   - Tools: Tile Molester, YY-CHR, or similar
   - Extract and export as PNG files

3. **Game Development Communities**
   - OpenGameArt.org (search for Mario-style sprites)
   - itch.io assets section
   - GitHub repositories with sprite collections

## Copyright Notice

**Important**: Super Mario World and all related sprites are copyrighted by Nintendo. 

- Sprites should only be used for personal, educational, or non-commercial projects
- Do not distribute sprite packs commercially
- Respect Nintendo's intellectual property rights
- This project is for educational purposes

## Verification

After downloading sprites, verify they work:

1. Run the Blazor app: `dotnet run` (from BlazorApp directory)
2. Open browser DevTools (F12)
3. Check Console for sprite loading messages
4. Start the game and confirm sprites appear

## Troubleshooting

**Sprites not appearing?**
- Check file names match exactly (case-sensitive on Linux)
- Verify PNG format with transparency
- Ensure files are in correct directories
- Check browser console for 404 errors

**Wrong size or quality?**
- Resize sprites to correct dimensions (16x16 for tiles)
- Use a pixel art editor like Aseprite, GIMP, or Photoshop
- Maintain pixel-perfect edges (disable interpolation)

## Need Help?

If you have issues downloading or setting up sprites:
1. Check the main README.md in the sprites directory
2. Review the console output when the game loads
3. Verify sprite file paths and names
4. Test with a single sprite first before adding all sprites
