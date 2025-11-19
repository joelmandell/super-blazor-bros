using Microsoft.JSInterop;

namespace SuperBlazorBros.Services;

/// <summary>
/// Service for loading and managing sprite images for the game.
/// Provides methods to preload sprites and check sprite availability.
/// </summary>
public class SpriteLoaderService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Dictionary<string, string> _spriteUrls = new();
    private bool _initialized = false;

    public SpriteLoaderService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
        InitializeSpriteUrls();
    }

    /// <summary>
    /// Initialize the mapping of sprite names to their URLs
    /// </summary>
    private void InitializeSpriteUrls()
    {
        // Character sprites
        _spriteUrls["mario-small"] = "sprites/characters/mario-small.png";
        _spriteUrls["mario-big"] = "sprites/characters/mario-big.png";
        _spriteUrls["mario-fire"] = "sprites/characters/mario-fire.png";
        _spriteUrls["luigi-small"] = "sprites/characters/luigi-small.png";
        _spriteUrls["luigi-big"] = "sprites/characters/luigi-big.png";

        // Enemy sprites
        _spriteUrls["goomba"] = "sprites/enemies/goomba.png";
        _spriteUrls["koopa-green"] = "sprites/enemies/koopa-green.png";
        _spriteUrls["koopa-red"] = "sprites/enemies/koopa-red.png";
        _spriteUrls["rex"] = "sprites/enemies/rex.png";
        _spriteUrls["shell-green"] = "sprites/enemies/shell-green.png";
        _spriteUrls["shell-red"] = "sprites/enemies/shell-red.png";

        // Item sprites
        _spriteUrls["mushroom"] = "sprites/items/mushroom.png";
        _spriteUrls["fire-flower"] = "sprites/items/fire-flower.png";
        _spriteUrls["coin"] = "sprites/items/coin.png";
        _spriteUrls["star"] = "sprites/items/star.png";

        // Tile sprites
        _spriteUrls["ground"] = "sprites/tiles/ground.png";
        _spriteUrls["brick"] = "sprites/tiles/brick.png";
        _spriteUrls["question-block"] = "sprites/tiles/question-block.png";
        _spriteUrls["question-block-used"] = "sprites/tiles/question-block-used.png";
        _spriteUrls["pipe-top-left"] = "sprites/tiles/pipe-top-left.png";
        _spriteUrls["pipe-top-right"] = "sprites/tiles/pipe-top-right.png";
        _spriteUrls["pipe-vertical"] = "sprites/tiles/pipe-vertical.png";
        _spriteUrls["platform"] = "sprites/tiles/platform.png";
    }

    /// <summary>
    /// Get the URL for a sprite by name
    /// </summary>
    public string GetSpriteUrl(string spriteName)
    {
        return _spriteUrls.TryGetValue(spriteName, out var url) ? url : string.Empty;
    }

    /// <summary>
    /// Check if a sprite is available
    /// </summary>
    public bool IsSpriteAvailable(string spriteName)
    {
        return _spriteUrls.ContainsKey(spriteName);
    }

    /// <summary>
    /// Get all sprite URLs for preloading
    /// </summary>
    public Dictionary<string, string> GetAllSpriteUrls()
    {
        return new Dictionary<string, string>(_spriteUrls);
    }

    /// <summary>
    /// Preload all sprites using JavaScript
    /// </summary>
    public async Task<Dictionary<string, bool>> PreloadSpritesAsync()
    {
        if (_initialized)
        {
            return new Dictionary<string, bool>();
        }

        var results = new Dictionary<string, bool>();
        
        foreach (var kvp in _spriteUrls)
        {
            try
            {
                var loaded = await CheckSpriteExistsAsync(kvp.Value);
                results[kvp.Key] = loaded;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking sprite {kvp.Key}: {ex.Message}");
                results[kvp.Key] = false;
            }
        }

        _initialized = true;
        return results;
    }

    /// <summary>
    /// Check if a sprite file exists by attempting to load it
    /// </summary>
    private async Task<bool> CheckSpriteExistsAsync(string url)
    {
        try
        {
            // Try to check if the image exists via JavaScript
            var exists = await _jsRuntime.InvokeAsync<bool>("checkImageExists", url);
            return exists;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get sprite mapping for a game entity type
    /// </summary>
    public string GetEntitySprite(string entityType, string state = "default")
    {
        return entityType.ToLower() switch
        {
            "goomba" => "goomba",
            "koopa" => state == "shell" ? "shell-green" : "koopa-green",
            "rex" => "rex",
            "mushroom" => "mushroom",
            "coin" => "coin",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Get sprite mapping for a player based on power mode
    /// </summary>
    public string GetPlayerSprite(string powerMode = "small", string character = "mario")
    {
        var prefix = character.ToLower();
        return powerMode.ToLower() switch
        {
            "small" => $"{prefix}-small",
            "big" => $"{prefix}-big",
            "fire" => $"{prefix}-fire",
            _ => $"{prefix}-small"
        };
    }

    /// <summary>
    /// Get sprite mapping for a tile type
    /// </summary>
    public string GetTileSprite(int tileType)
    {
        return tileType switch
        {
            1 => "ground",
            2 => "brick",
            3 => "question-block",
            4 => "question-block-used",
            5 => "pipe-top-left",
            6 => "pipe-top-right",
            7 => "pipe-vertical",
            8 => "platform",
            _ => string.Empty
        };
    }
}
