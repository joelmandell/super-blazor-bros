namespace SuperBlazorBros.Models;

public static class GameConstants
{
    // Display
    public const int TILE_SIZE = 16;
    public const int SCALE = 3;
    public const int ACTUAL_TILE_SIZE = TILE_SIZE * SCALE;
    public const int SCREEN_WIDTH = 256;
    public const int SCREEN_HEIGHT = 240;

    // Physics - Super Mario World SNES feel (slightly floatier, faster)
    public const double GRAVITY = 0.22;
    public const double FRICTION = 0.92;
    public const double ACCELERATION = 0.15;
    public const double MAX_WALK_SPEED = 1.6;
    public const double MAX_RUN_SPEED = 3.2;
    public const double JUMP_FORCE = 7.2;
    public const double BOUNCE_FORCE = 4.5;
    public const double SPIN_JUMP_FORCE = 7.8;

    // Colors - Super Mario World 16-bit palette
    public static class Colors
    {
        // Sky and background
        public const string SKY = "#70B0F8";
        
        // Ground tiles
        public const string GROUND = "#F07828";
        public const string GROUND_DARK = "#B85818";
        public const string GROUND_HIGHLIGHT = "#FFA858";
        
        // Blocks
        public const string BRICK = "#F8B800";
        public const string BRICK_DARK = "#C08000";
        public const string QUESTION = "#F8D868";
        public const string QUESTION_DARK = "#D8A020";
        
        // Pipes
        public const string PIPE = "#58C870";
        public const string PIPE_DARK = "#288840";
        public const string PIPE_HIGHLIGHT = "#90E898";
        
        // Enemies
        public const string REX = "#6078D8";
        public const string REX_BELLY = "#F0D058";
        public const string KOOPA_GREEN = "#58C870";
        public const string KOOPA_SHELL = "#58C870";
        
        // Mario
        public const string MARIO_RED = "#F01018";
        public const string MARIO_BLUE = "#2860D8";
        public const string MARIO_SKIN = "#FFC890";
        public const string MARIO_BROWN = "#885020";
        public const string MARIO_WHITE = "#FFFFFF";
        
        // Power-ups
        public const string CAPE_YELLOW = "#F8D000";
        public const string MUSHROOM_RED = "#F01018";
        public const string MUSHROOM_SPOTS = "#FFFFFF";
        
        // Nature
        public const string CLOUD = "#FFFFFF";
        public const string CLOUD_OUTLINE = "#60A8F0";
        public const string BUSH_GREEN = "#58C870";
        public const string BUSH_DARK = "#288840";
        public const string HILL_GREEN = "#78D880";
        public const string HILL_DARK = "#40A050";
        public const string GRASS_TOP = "#88E060";
        
        // Basic
        public const string BLACK = "#000000";
        public const string WHITE = "#FFFFFF";
        
        // Collectibles
        public const string COIN_GOLD = "#F8D868";
        public const string COIN_OUTLINE = "#C08000";
        
        // Message blocks
        public const string MESSAGE_BLOCK = "#A0A0A0";
        public const string MESSAGE_BLOCK_OUTLINE = "#505050";
    }

    // Level dimensions
    public const int LEVEL_WIDTH = 220;
    public const int LEVEL_HEIGHT = 15;

    // Generate default level map
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
            // SMW pipes
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
        PlaceGround(0, 85);
        PlaceGround(90, 25);
        PlaceGround(120, 80);
        PlaceGround(205, 15);

        // Scenery - Larger clouds
        foreach (var x in new[] { 10, 25, 60, 85, 110, 145, 175 })
        {
            Set(x, 3, TileType.CLOUD);
            Set(x + 1, 3, TileType.CLOUD);
            Set(x, 4, TileType.CLOUD);
            Set(x + 1, 4, TileType.CLOUD);
        }

        // Hills
        foreach (var x in new[] { 5, 55, 105, 155 })
        {
            Set(x, 11, TileType.HILL);
            Set(x + 1, 11, TileType.HILL);
            Set(x + 2, 11, TileType.HILL);
        }
        
        // Bushes
        foreach (var x in new[] { 15, 35, 65, 95, 125, 165 })
        {
            Set(x, 12, TileType.BUSH);
            Set(x + 1, 12, TileType.BUSH);
        }

        // Message block at start
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
        
        PlacePipe(32, 2);
        
        // Question blocks with coins above
        for (int i = 0; i < 5; i++)
        {
            Set(40 + i, 10, TileType.QUESTION_BLOCK);
            if (i % 2 == 1)
                Set(40 + i, 9, TileType.COIN); // Coins above some blocks
        }
        
        PlacePipe(50, 3);
        
        // Coin trail
        for (int i = 0; i < 4; i++)
            Set(54 + i, 7, TileType.COIN);
        
        FillRect(58, 8, 4, 1, TileType.BRICK);
        Set(60, 5, TileType.QUESTION_BLOCK);
        Set(60, 4, TileType.COIN); // Coin above
        
        // More floating coins
        for (int i = 0; i < 5; i++)
            Set(64 + i, 9, TileType.COIN);
        
        PlacePipe(70, 4);
        
        // Coins in arc pattern
        Set(76, 8, TileType.COIN);
        Set(78, 7, TileType.COIN);
        Set(80, 6, TileType.COIN);
        Set(82, 7, TileType.COIN);
        Set(84, 8, TileType.COIN);
        
        FillRect(86, 10, 3, 1, TileType.HARD_BLOCK);
        
        // Coins around gap
        Set(89, 9, TileType.COIN);
        
        Set(95, 10, TileType.QUESTION_BLOCK);
        Set(96, 9, TileType.COIN);
        Set(98, 10, TileType.QUESTION_BLOCK);
        Set(99, 9, TileType.COIN);
        Set(101, 10, TileType.QUESTION_BLOCK);
        
        PlacePipe(108, 3);
        
        // Coins around pipe
        Set(111, 10, TileType.COIN);
        Set(113, 9, TileType.COIN);
        Set(115, 8, TileType.COIN);
        
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
        
        PlacePipe(145, 4);
        
        // Coins on stairs
        CreateStair(155, 5, 1);
        Set(157, 10, TileType.COIN);
        Set(159, 8, TileType.COIN);
        FillRect(160, 8, 8, 1, TileType.HARD_BLOCK);
        // Coins on platform
        Set(162, 7, TileType.COIN);
        Set(164, 7, TileType.COIN);
        Set(166, 7, TileType.COIN);
        CreateStair(168, 5, -1);
        
        PlacePipe(178, 3);
        
        // Final coin trail
        for (int i = 0; i < 5; i++)
            Set(182 + i, 9, TileType.COIN);
        
        PlacePipe(188, 4);
        
        // Pre-goal coins
        Set(193, 10, TileType.COIN);
        Set(194, 9, TileType.COIN);
        Set(195, 8, TileType.COIN);
        
        CreateStair(196, 8, 1);
        
        // Victory coins
        Set(205, 10, TileType.COIN);
        Set(207, 9, TileType.COIN);
        
        Set(210, 12, TileType.HARD_BLOCK);
        FillRect(210, 2, 1, 10, TileType.POLE);
        Set(210, 2, TileType.FLAG);
        
        Set(215, 12, TileType.CASTLE);
        Set(216, 12, TileType.CASTLE);

        return map;
    }

    // Generate default entities - Super Mario World enemies
    public static List<Entity> GetDefaultEntities()
    {
        var entities = new List<Entity>();
        var entityId = 1000;
        
        const int groundY = 12 * TILE_SIZE;
        
        // Rex enemies
        var rexPositions = new[] { 25, 36, 48, 62, 76, 92, 112, 135, 150, 175, 190 };
        
        foreach (var x in rexPositions)
        {
            entities.Add(new Entity
            {
                Id = entityId++,
                Type = EntityType.KOOPA,
                Pos = new Vector2D(x * TILE_SIZE, groundY),
                Vel = new Vector2D(-0.6, 0),
                Width = 16,
                Height = 20,
                Dead = false,
                Grounded = false,
                Direction = -1,
                State = "rex"
            });
        }
        
        // Koopa Troopas
        var koopaPositions = new[] { 55, 88, 125, 165, 185 };
        
        foreach (var x in koopaPositions)
        {
            entities.Add(new Entity
            {
                Id = entityId++,
                Type = EntityType.KOOPA,
                Pos = new Vector2D(x * TILE_SIZE, groundY),
                Vel = new Vector2D(-0.5, 0),
                Width = 16,
                Height = 24,
                Dead = false,
                Grounded = false,
                Direction = -1,
                State = "koopa"
            });
        }
        
        return entities;
    }
}
