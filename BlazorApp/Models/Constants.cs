namespace SuperBlazorBros.Models;

public static class GameConstants
{
    // Display
    public const int TILE_SIZE = 16;
    public const int SCALE = 3;
    public const int ACTUAL_TILE_SIZE = TILE_SIZE * SCALE;
    public const int SCREEN_WIDTH = 256;
    public const int SCREEN_HEIGHT = 240;

    // Physics - Adjusted for authentic NES feel (approx 60FPS logic)
    public const double GRAVITY = 0.25;
    public const double FRICTION = 0.90;
    public const double ACCELERATION = 0.12;
    public const double MAX_WALK_SPEED = 1.4;
    public const double MAX_RUN_SPEED = 2.6;
    public const double JUMP_FORCE = 6.6;
    public const double BOUNCE_FORCE = 3.5;

    // Colors
    public static class Colors
    {
        public const string SKY = "#5C94FC";
        public const string GROUND = "#C84C0C";
        public const string BRICK = "#B83410";
        public const string QUESTION = "#F8D820";
        public const string PIPE = "#00A800";
        public const string PIPE_DARK = "#006000";
        public const string GOOMBA = "#E45C10";
        public const string MARIO_RED = "#F83800";
        public const string MARIO_SKIN = "#FFA044";
        public const string MARIO_BROWN = "#887000";
        public const string FIRE_WHITE = "#F8F8F8";
        public const string FIRE_RED = "#F83800";
        public const string MUSHROOM_RED = "#E45C10";
        public const string MUSHROOM_SKIN = "#FFA044";
        public const string FLOWER_ORANGE = "#E45C10";
        public const string FIREBALL_CENTER = "#FFA044";
        public const string FIREBALL_OUTER = "#F83800";
        public const string BLACK = "#000000";
        public const string WHITE = "#FFFFFF";
        public const string CLOUD = "#FFFFFF";
        public const string BUSH = "#00A800";
        public const string HILL = "#008000";
        public const string HILL_OUTLINE = "#004000";
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
            FillRect(x, 13 - h, 2, h, TileType.PIPE_L);
            Set(x, 13 - h, TileType.PIPE_TOP_L);
            Set(x + 1, 13 - h, TileType.PIPE_TOP_R);
            for (int i = 1; i <= h; i++)
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

        // --- 1-1 Approximation ---
        PlaceGround(0, 69);
        PlaceGround(71, 15);
        PlaceGround(89, 64);
        PlaceGround(155, 65);

        // Scenery - Clouds
        foreach (var x in new[] { 8, 19, 56, 67, 103, 114, 152, 163 })
            Set(x, 3, TileType.CLOUD);
        foreach (var x in new[] { 27, 36, 75, 84, 123, 132, 171, 180 })
            Set(x, 4, TileType.CLOUD);

        // Scenery - Hills and Bushes
        foreach (var x in new[] { 0, 48, 96, 144, 192 })
            Set(x, 10, TileType.HILL);
        foreach (var x in new[] { 16, 64, 112, 160 })
            Set(x, 11, TileType.HILL);
        foreach (var x in new[] { 11, 59, 107, 155 })
            Set(x, 12, TileType.BUSH);
        foreach (var x in new[] { 23, 71, 119, 167 })
            Set(x, 12, TileType.BUSH);

        // Structures - First section
        Set(16, 9, TileType.QUESTION_BLOCK);
        Set(20, 9, TileType.BRICK);
        Set(21, 9, TileType.QUESTION_BLOCK);
        Set(22, 9, TileType.BRICK);
        Set(23, 9, TileType.QUESTION_BLOCK);
        Set(24, 9, TileType.BRICK);
        Set(22, 5, TileType.QUESTION_BLOCK);

        // Pipes
        PlacePipe(28, 2);
        PlacePipe(38, 3);
        PlacePipe(46, 4);
        PlacePipe(57, 4);

        // More blocks
        Set(64, 8, TileType.INVISIBLE_BLOCK);
        Set(77, 9, TileType.BRICK);
        Set(78, 9, TileType.QUESTION_BLOCK);
        Set(79, 9, TileType.BRICK);

        FillRect(80, 5, 8, 1, TileType.BRICK);
        FillRect(91, 5, 3, 1, TileType.BRICK);
        Set(94, 5, TileType.QUESTION_BLOCK);
        Set(94, 9, TileType.BRICK);

        Set(100, 9, TileType.BRICK);
        Set(101, 9, TileType.BRICK);
        Set(105, 9, TileType.QUESTION_BLOCK);
        Set(106, 9, TileType.QUESTION_BLOCK);
        Set(109, 9, TileType.QUESTION_BLOCK);
        Set(109, 5, TileType.QUESTION_BLOCK);

        Set(118, 9, TileType.BRICK);
        Set(119, 5, TileType.BRICK);
        Set(120, 5, TileType.BRICK);
        Set(121, 5, TileType.BRICK);

        Set(129, 5, TileType.BRICK);
        Set(130, 5, TileType.BRICK);
        Set(129, 9, TileType.PIPE_L);
        Set(130, 9, TileType.BRICK);

        // Stairs
        CreateStair(134, 4, 1);
        CreateStair(143, 4, -1);
        CreateStair(148, 4, 1);
        CreateStair(155, 4, -1);
        CreateStair(181, 8, 1);

        // Flag and castle
        Set(198, 12, TileType.HARD_BLOCK);
        FillRect(198, 2, 1, 10, TileType.POLE);
        Set(198, 2, TileType.FLAG);
        Set(202, 12, TileType.CASTLE);

        return map;
    }
}
