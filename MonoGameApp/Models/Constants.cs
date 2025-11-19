using Microsoft.Xna.Framework;

namespace SuperBlazorBrosMonoGame.Models;

public static class GameConstants
{
    // Display
    public const int TILE_SIZE = 16;
    public const int SCALE = 3;
    public const int ACTUAL_TILE_SIZE = TILE_SIZE * SCALE;
    public const int SCREEN_WIDTH = 256;
    public const int SCREEN_HEIGHT = 240;
    public const int SCALED_SCREEN_WIDTH = SCREEN_WIDTH * SCALE;
    public const int SCALED_SCREEN_HEIGHT = SCREEN_HEIGHT * SCALE;

    // Physics - Super Mario World SNES feel (slightly floatier, faster)
    public const float GRAVITY = 0.22f; // Slightly lower for floatier feel
    public const float FRICTION = 0.92f; // Less friction for smoother movement
    public const float ACCELERATION = 0.15f; // Slightly faster acceleration
    public const float MAX_WALK_SPEED = 1.6f; // Faster walk speed
    public const float MAX_RUN_SPEED = 3.2f; // Much faster run speed (SMW is faster)
    public const float JUMP_FORCE = 7.2f; // Higher jump
    public const float BOUNCE_FORCE = 4.5f; // Higher bounce (SMW has pronounced bounces)
    public const float SPIN_JUMP_FORCE = 7.8f; // Spin jump is higher

    // Colors - Super Mario World 16-bit palette
    public static class Colors
    {
        // Sky and background (SMW lighter, brighter blue)
        public static readonly Color SKY = new Color(112, 176, 248); // Lighter, brighter sky
        
        // Ground tiles (SMW has more vibrant colors)
        public static readonly Color GROUND = new Color(240, 120, 40); // Brighter orange-brown
        public static readonly Color GROUND_DARK = new Color(184, 88, 24); // Shadow
        public static readonly Color GROUND_HIGHLIGHT = new Color(255, 168, 88); // Highlight
        
        // Blocks (SMW has more detailed, colorful blocks)
        public static readonly Color BRICK = new Color(248, 184, 0); // Golden brick
        public static readonly Color BRICK_DARK = new Color(192, 128, 0); // Shadow
        public static readonly Color QUESTION = new Color(248, 216, 104); // Brighter yellow
        public static readonly Color QUESTION_DARK = new Color(216, 160, 32); // Shadow
        
        // Pipes (SMW has brighter green pipes)
        public static readonly Color PIPE = new Color(88, 200, 112); // Brighter green
        public static readonly Color PIPE_DARK = new Color(40, 136, 64); // Darker green
        public static readonly Color PIPE_HIGHLIGHT = new Color(144, 232, 152); // Highlight
        
        // Enemies (Rex/Koopa colors for SMW)
        public static readonly Color REX = new Color(96, 120, 216); // Purple Rex
        public static readonly Color REX_BELLY = new Color(240, 208, 88); // Yellow belly
        public static readonly Color KOOPA_GREEN = new Color(88, 200, 112); // Green Koopa
        public static readonly Color KOOPA_SHELL = new Color(88, 200, 112);
        
        // Mario (SMW style with more vibrant colors)
        public static readonly Color MARIO_RED = new Color(240, 16, 24); // Bright red
        public static readonly Color MARIO_BLUE = new Color(40, 96, 216); // Blue overalls
        public static readonly Color MARIO_SKIN = new Color(255, 200, 144); // Peach skin
        public static readonly Color MARIO_BROWN = new Color(136, 80, 32); // Brown hair/shoes
        public static readonly Color MARIO_WHITE = new Color(255, 255, 255); // White gloves/eyes
        
        // Power-ups (SMW style)
        public static readonly Color CAPE_YELLOW = new Color(248, 208, 0); // Cape Feather
        public static readonly Color MUSHROOM_RED = new Color(240, 16, 24);
        public static readonly Color MUSHROOM_SPOTS = new Color(255, 255, 255);
        
        // Nature elements (SMW has more detailed scenery)
        public static readonly Color CLOUD = new Color(255, 255, 255);
        public static readonly Color CLOUD_OUTLINE = new Color(96, 168, 240);
        public static readonly Color BUSH_GREEN = new Color(88, 200, 112);
        public static readonly Color BUSH_DARK = new Color(40, 136, 64);
        public static readonly Color HILL_GREEN = new Color(120, 216, 128);
        public static readonly Color HILL_DARK = new Color(64, 160, 80);
        public static readonly Color GRASS_TOP = new Color(136, 224, 96); // Bright grass
        
        // Basic colors
        public static readonly Color BLACK = Color.Black;
        public static readonly Color WHITE = Color.White;
        
        // Coins and collectibles
        public static readonly Color COIN_GOLD = new Color(248, 216, 104);
        public static readonly Color COIN_OUTLINE = new Color(192, 128, 0);
        
        // Message blocks
        public static readonly Color MESSAGE_BLOCK = new Color(160, 160, 160);
        public static readonly Color MESSAGE_BLOCK_OUTLINE = new Color(80, 80, 80);
    }

    // Level dimensions
    public const int LEVEL_WIDTH = 220;
    public const int LEVEL_HEIGHT = 15;

    // Generate default level map - Super Mario World Yoshi's Island 1 inspired
    public static int[][] GetDefaultLevelMap()
    {
        var map = new int[LEVEL_HEIGHT][];
        for (int y = 0; y < LEVEL_HEIGHT; y++)
        {
            map[y] = new int[LEVEL_WIDTH];
            for (int x = 0; x < LEVEL_WIDTH; x++)
            {
                map[y][x] = (int)TileType.AIR;
            }
        }

        // Helper methods
        void Set(int x, int y, TileType t)
        {
            if (x >= 0 && x < LEVEL_WIDTH && y >= 0 && y < LEVEL_HEIGHT)
                map[y][x] = (int)t;
        }

        void FillRect(int x, int y, int w, int h, TileType t)
        {
            for (int iy = 0; iy < h; iy++)
                for (int ix = 0; ix < w; ix++)
                    Set(x + ix, y + iy, t);
        }

        void PlacePipe(int x, int h)
        {
            // SMW pipes are slightly different - taller tops
            Set(x, 13 - h, TileType.PIPE_TOP_L);
            Set(x + 1, 13 - h, TileType.PIPE_TOP_R);
            for (int i = 1; i < h; i++)
            {
                Set(x, 13 - h + i, TileType.PIPE_L);
                Set(x + 1, 13 - h + i, TileType.PIPE_R);
            }
        }

        void PlaceGround(int start, int width)
        {
            FillRect(start, 13, width, 2, TileType.GROUND);
        }

        void CreateStair(int x, int h, int dir)
        {
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    Set(x + (dir == 1 ? i : -i), 12 - j, TileType.HARD_BLOCK);
                }
            }
        }

        // --- Super Mario World: Yoshi's Island 1 Layout ---
        // The level is more open and has longer ground sections than SMB1
        
        // Main ground platforms
        PlaceGround(0, 85); // Long starting platform
        PlaceGround(90, 25); // Platform after first gap
        PlaceGround(120, 80); // Long middle section
        PlaceGround(205, 15); // Final section before goal

        // Scenery - Larger clouds (SMW style)
        foreach (var x in new[] { 10, 25, 60, 85, 110, 145, 175 })
        {
            Set(x, 3, TileType.CLOUD);
            Set(x + 1, 3, TileType.CLOUD);
            Set(x, 4, TileType.CLOUD);
            Set(x + 1, 4, TileType.CLOUD);
        }

        // Hills (rounder, more pronounced in SMW)
        foreach (var x in new[] { 5, 55, 105, 155 })
        {
            Set(x, 11, TileType.HILL);
            Set(x + 1, 11, TileType.HILL);
            Set(x + 2, 11, TileType.HILL);
        }
        
        // Bushes at ground level
        foreach (var x in new[] { 15, 35, 65, 95, 125, 165 })
        {
            Set(x, 12, TileType.BUSH);
            Set(x + 1, 12, TileType.BUSH);
        }

        // Message block at start (SMW has these)
        Set(10, 12, TileType.HARD_BLOCK);
        
        // Early section with coins
        Set(18, 10, TileType.QUESTION_BLOCK);
        Set(19, 9, TileType.COIN); // Coin above
        Set(20, 10, TileType.QUESTION_BLOCK);
        Set(24, 7, TileType.QUESTION_BLOCK);
        Set(24, 6, TileType.COIN); // Coin above
        
        // Floating coins
        for (int i = 0; i < 3; i++)
            Set(28 + i, 8, TileType.COIN);
        
        // First pipe (small)
        PlacePipe(32, 2);
        
        // Question blocks with coins above
        for (int i = 0; i < 5; i++)
        {
            Set(40 + i, 10, TileType.QUESTION_BLOCK);
            if (i % 2 == 1)
                Set(40 + i, 9, TileType.COIN); // Coins above some blocks
        }
        
        // Second pipe (medium)
        PlacePipe(50, 3);
        
        // Coin trail
        for (int i = 0; i < 4; i++)
            Set(54 + i, 7, TileType.COIN);
        
        // Floating blocks section
        FillRect(58, 8, 4, 1, TileType.BRICK);
        Set(60, 5, TileType.QUESTION_BLOCK);
        Set(60, 4, TileType.COIN); // Coin above
        
        // More floating coins
        for (int i = 0; i < 5; i++)
            Set(64 + i, 9, TileType.COIN);
        
        // Third pipe (tall)
        PlacePipe(70, 4);
        
        // Coins in arc pattern
        Set(76, 8, TileType.COIN);
        Set(78, 7, TileType.COIN);
        Set(80, 6, TileType.COIN);
        Set(82, 7, TileType.COIN);
        Set(84, 8, TileType.COIN);
        
        // Gap section with floating platforms
        FillRect(86, 10, 3, 1, TileType.HARD_BLOCK);
        
        // Coins around gap
        Set(89, 9, TileType.COIN);
        
        // After gap - more blocks
        Set(95, 10, TileType.QUESTION_BLOCK);
        Set(96, 9, TileType.COIN);
        Set(98, 10, TileType.QUESTION_BLOCK);
        Set(99, 9, TileType.COIN);
        Set(101, 10, TileType.QUESTION_BLOCK);
        
        // Fourth pipe
        PlacePipe(108, 3);
        
        // Coins around pipe
        Set(111, 10, TileType.COIN);
        Set(113, 9, TileType.COIN);
        Set(115, 8, TileType.COIN);
        
        // Mid-section floating platform
        FillRect(118, 8, 6, 1, TileType.BRICK);
        Set(121, 5, TileType.QUESTION_BLOCK);
        Set(121, 4, TileType.COIN);
        
        // Alternating blocks with coins
        for (int i = 0; i < 8; i++)
        {
            Set(130 + i, 10, i % 2 == 0 ? TileType.BRICK : TileType.QUESTION_BLOCK);
            if (i % 2 == 0 && i > 0 && i < 7)
                Set(130 + i, 9, TileType.COIN);
        }
        
        // More floating coins
        for (int i = 0; i < 4; i++)
            Set(140 + i, 8, TileType.COIN);
        
        // Fifth pipe (tall)
        PlacePipe(145, 4);
        
        // Coins on stairs
        CreateStair(155, 5, 1);
        Set(157, 10, TileType.COIN);
        Set(159, 8, TileType.COIN);
        FillRect(160, 8, 8, 1, TileType.HARD_BLOCK); // Platform on top
        // Coins on platform
        Set(162, 7, TileType.COIN);
        Set(164, 7, TileType.COIN);
        Set(166, 7, TileType.COIN);
        CreateStair(168, 5, -1);
        
        // More pipes
        PlacePipe(178, 3);
        
        // Final coin trail
        for (int i = 0; i < 5; i++)
            Set(182 + i, 9, TileType.COIN);
        
        PlacePipe(188, 4);
        
        // Pre-goal coins
        Set(193, 10, TileType.COIN);
        Set(194, 9, TileType.COIN);
        Set(195, 8, TileType.COIN);
        
        // Final stair to goal
        CreateStair(196, 8, 1);
        
        // Victory coins
        Set(205, 10, TileType.COIN);
        Set(207, 9, TileType.COIN);
        
        // Goal post (SMW style - tape and post)
        Set(210, 12, TileType.HARD_BLOCK);
        FillRect(210, 2, 1, 10, TileType.POLE);
        Set(210, 2, TileType.FLAG);
        
        // Castle/Giant gate at end
        Set(215, 12, TileType.CASTLE);
        Set(216, 12, TileType.CASTLE);

        return map;
    }

    // Generate default entities - Super Mario World Yoshi's Island 1 enemies
    public static List<Entity> GetDefaultEntities()
    {
        var entities = new List<Entity>();
        var entityId = 1000;
        
        // Ground level Y position (row 13 is ground, so entities at row 12)
        const int groundY = 12 * TILE_SIZE;
        
        // SMW Yoshi's Island 1 features Rex and Koopa enemies
        // Rex is the main enemy - purple dinosaur that shrinks when stomped
        var rexPositions = new[] { 25, 36, 48, 62, 76, 92, 112, 135, 150, 175, 190 };
        
        foreach (var x in rexPositions)
        {
            entities.Add(new Entity
            {
                Id = entityId++,
                Type = EntityType.KOOPA, // Using KOOPA type to represent Rex
                Position = new Microsoft.Xna.Framework.Vector2(x * TILE_SIZE, groundY),
                Velocity = new Microsoft.Xna.Framework.Vector2(-0.6f, 0),
                Width = 16,
                Height = 20, // Rex is slightly taller
                Dead = false,
                Grounded = false,
                Direction = -1,
                State = "rex" // Tag as rex for rendering
            });
        }
        
        // Add some Koopa Troopas (fewer than Rex)
        var koopaPositions = new[] { 55, 88, 125, 165, 185 };
        
        foreach (var x in koopaPositions)
        {
            entities.Add(new Entity
            {
                Id = entityId++,
                Type = EntityType.KOOPA,
                Position = new Microsoft.Xna.Framework.Vector2(x * TILE_SIZE, groundY),
                Velocity = new Microsoft.Xna.Framework.Vector2(-0.5f, 0),
                Width = 16,
                Height = 24, // Koopa with shell
                Dead = false,
                Grounded = false,
                Direction = -1,
                State = "koopa" // Tag as koopa for rendering
            });
        }
        
        return entities;
    }
}
