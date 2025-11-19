namespace SuperBlazorBros.Models;

public enum EntityType
{
    PLAYER,
    GOOMBA,
    KOOPA,
    MUSHROOM,
    FLOWER,
    FIREBALL,
    PARTICLE,
    FLAG
}

public enum TileType
{
    AIR = 0,
    GROUND = 1,
    BRICK = 2,
    QUESTION_BLOCK = 3,
    QUESTION_BLOCK_HIT = 4,
    HARD_BLOCK = 5,
    PIPE_L = 6,
    PIPE_R = 7,
    PIPE_TOP_L = 8,
    PIPE_TOP_R = 9,
    POLE = 10,
    FLAG = 11,
    COIN = 12,
    INVISIBLE_BLOCK = 13,
    CLOUD = 14,
    BUSH = 15,
    HILL = 16,
    CASTLE = 17
}

public enum GameStatus
{
    MENU,
    LOADING,
    PLAYING,
    GAME_OVER,
    VICTORY
}

public class Vector2D
{
    public double X { get; set; }
    public double Y { get; set; }

    public Vector2D(double x = 0, double y = 0)
    {
        X = x;
        Y = y;
    }
}

public class Entity
{
    public int Id { get; set; }
    public EntityType Type { get; set; }
    public Vector2D Pos { get; set; } = new();
    public Vector2D Vel { get; set; } = new();
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Dead { get; set; }
    public bool Grounded { get; set; }
    public int Direction { get; set; } = 1; // -1 left, 1 right
    public string? State { get; set; }
    public int FrameTimer { get; set; }
}

public class Player : Entity
{
    public string PowerMode { get; set; } = "small"; // small, big, fire, cape
    public bool IsJumping { get; set; }
    public bool IsSpinJumping { get; set; } // SMW spin jump
    public int Invulnerable { get; set; }
}

public class GameStats
{
    public int Score { get; set; }
    public int Coins { get; set; }
    public string World { get; set; } = "1-1";
    public int Time { get; set; } = 400;
    public int Lives { get; set; } = 3;
}

public class LevelData
{
    public int[][] Map { get; set; } = Array.Empty<int[]>();
    public List<Entity> Entities { get; set; } = new();
    public string BackgroundColor { get; set; } = "#5C94FC";
}
