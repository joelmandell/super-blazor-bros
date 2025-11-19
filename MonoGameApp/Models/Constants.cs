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

    // Physics - Adjusted for authentic NES feel (approx 60FPS logic)
    public const float GRAVITY = 0.25f;
    public const float FRICTION = 0.90f;
    public const float ACCELERATION = 0.12f;
    public const float MAX_WALK_SPEED = 1.4f;
    public const float MAX_RUN_SPEED = 2.6f;
    public const float JUMP_FORCE = 6.6f;
    public const float BOUNCE_FORCE = 3.5f;

    // Colors
    public static class Colors
    {
        public static readonly Color SKY = new Color(92, 148, 252); // #5C94FC
        public static readonly Color GROUND = new Color(200, 76, 12); // #C84C0C
        public static readonly Color BRICK = new Color(184, 52, 16); // #B83410
        public static readonly Color QUESTION = new Color(248, 216, 32); // #F8D820
        public static readonly Color PIPE = new Color(0, 168, 0); // #00A800
        public static readonly Color PIPE_DARK = new Color(0, 96, 0); // #006000
        public static readonly Color GOOMBA = new Color(228, 92, 16); // #E45C10
        public static readonly Color MARIO_RED = new Color(248, 56, 0); // #F83800
        public static readonly Color MARIO_SKIN = new Color(255, 160, 68); // #FFA044
        public static readonly Color MARIO_BROWN = new Color(136, 112, 0); // #887000
        public static readonly Color FIRE_WHITE = new Color(248, 248, 248); // #F8F8F8
        public static readonly Color FIRE_RED = new Color(248, 56, 0); // #F83800
        public static readonly Color MUSHROOM_RED = new Color(228, 92, 16); // #E45C10
        public static readonly Color MUSHROOM_SKIN = new Color(255, 160, 68); // #FFA044
        public static readonly Color FLOWER_ORANGE = new Color(228, 92, 16); // #E45C10
        public static readonly Color FIREBALL_CENTER = new Color(255, 160, 68); // #FFA044
        public static readonly Color FIREBALL_OUTER = new Color(248, 56, 0); // #F83800
        public static readonly Color BLACK = Color.Black;
        public static readonly Color WHITE = Color.White;
        public static readonly Color CLOUD = Color.White;
        public static readonly Color BUSH = new Color(0, 168, 0); // #00A800
        public static readonly Color HILL = new Color(0, 128, 0); // #008000
        public static readonly Color HILL_OUTLINE = new Color(0, 64, 0); // #004000
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
