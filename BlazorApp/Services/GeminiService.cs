using System.Net.Http.Json;
using System.Text.Json;
using SuperBlazorBros.Models;

namespace SuperBlazorBros.Services;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent";

    public GeminiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LevelData?> GenerateLevelAsync(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("No API Key provided for Gemini");
            return null;
        }

        try
        {
            Console.WriteLine("[GeminiService] Sending request to Gemini...");

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new
                            {
                                text = GetLevelGenerationPrompt()
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 1.0,
                    maxOutputTokens = 8192
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{GEMINI_API_URL}?key={apiKey}",
                requestBody
            );

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[GeminiService] Error: {response.StatusCode}");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine("[GeminiService] Received response from Gemini");

            // Parse the Gemini response
            var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var textContent = geminiResponse
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(textContent))
            {
                Console.WriteLine("[GeminiService] No text content in response");
                return null;
            }

            // Extract JSON from markdown code block if present
            var jsonText = textContent;
            if (textContent.Contains("```json"))
            {
                var startIndex = textContent.IndexOf("```json") + 7;
                var endIndex = textContent.LastIndexOf("```");
                jsonText = textContent.Substring(startIndex, endIndex - startIndex).Trim();
            }
            else if (textContent.Contains("```"))
            {
                var startIndex = textContent.IndexOf("```") + 3;
                var endIndex = textContent.LastIndexOf("```");
                jsonText = textContent.Substring(startIndex, endIndex - startIndex).Trim();
            }

            // Parse the level data
            var levelResponse = JsonSerializer.Deserialize<LevelResponse>(jsonText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (levelResponse?.Map == null)
            {
                Console.WriteLine("[GeminiService] Failed to parse level data");
                return null;
            }

            // Validate and fix the level
            var (validatedMap, entities) = ValidateAndFixLevel(levelResponse.Map, levelResponse.EnemyPositions ?? new List<EnemyPosition>());

            return new LevelData
            {
                Map = validatedMap,
                Entities = entities,
                BackgroundColor = GameConstants.Colors.SKY
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GeminiService] Error generating level: {ex.Message}");
            return null;
        }
    }

    private (int[][] map, List<Entity> entities) ValidateAndFixLevel(int[][] map, List<EnemyPosition> enemyPositions)
    {
        const int TILE_SIZE = 16;
        var entities = new List<Entity>();

        // Ensure map has correct dimensions
        if (map == null || map.Length == 0 || map[0] == null || map[0].Length == 0)
        {
            Console.WriteLine("[ValidateAndFixLevel] Map is empty or has wrong dimensions!");
            map = CreateFallbackMap();
        }

        // Ensure starting position has ground
        for (int x = 0; x < 5; x++)
        {
            for (int y = map.Length - 2; y < map.Length; y++)
            {
                if (map[y][x] == 0)
                {
                    map[y][x] = 1; // Ground
                }
            }
        }

        // Process enemies
        var entityId = 1000;
        foreach (var enemyPos in enemyPositions)
        {
            var tileX = (int)(enemyPos.X / TILE_SIZE);
            var groundY = FindGroundLevel(map, tileX);
            var groundPixelY = groundY * TILE_SIZE;

            entities.Add(new Entity
            {
                Id = entityId++,
                Type = EntityType.GOOMBA,
                Pos = new Vector2D(enemyPos.X, groundPixelY - 16),
                Vel = new Vector2D(-0.5, 0),
                Width = 16,
                Height = 16,
                Dead = false,
                Grounded = false,
                Direction = -1
            });
        }

        // Add default enemies if none were provided
        if (entities.Count == 0)
        {
            Console.WriteLine("[ValidateAndFixLevel] No enemies from AI, generating default enemies...");
            var defaultEnemyCount = 20;
            for (int i = 0; i < defaultEnemyCount; i++)
            {
                var x = (i + 1) * (map[0].Length / (defaultEnemyCount + 1));
                var groundY = FindGroundLevel(map, x);
                var groundPixelY = groundY * TILE_SIZE;

                entities.Add(new Entity
                {
                    Id = 2000 + i,
                    Type = EntityType.GOOMBA,
                    Pos = new Vector2D(x * TILE_SIZE, groundPixelY - 16),
                    Vel = new Vector2D(-0.5, 0),
                    Width = 16,
                    Height = 16,
                    Dead = false,
                    Grounded = false,
                    Direction = -1
                });
            }
        }

        return (map, entities);
    }

    private int FindGroundLevel(int[][] map, int x)
    {
        // Check rows 13-14 (the actual ground level)
        for (int y = map.Length - 1; y >= map.Length - 2 && y >= 0; y--)
        {
            if (map[y][x] == 1)
            {
                return y;
            }
        }

        // If no ground found in bottom rows, check all rows from bottom up
        for (int y = map.Length - 1; y >= 0; y--)
        {
            if (map[y][x] == 1)
            {
                return y;
            }
        }

        // Default to bottom row if no ground found
        return map.Length - 1;
    }

    private int[][] CreateFallbackMap()
    {
        const int HEIGHT = 15;
        const int WIDTH = 150;
        var map = new int[HEIGHT][];

        for (int y = 0; y < HEIGHT; y++)
        {
            map[y] = new int[WIDTH];
            for (int x = 0; x < WIDTH; x++)
            {
                map[y][x] = y >= 13 ? 1 : 0; // Ground at bottom
            }
        }

        return map;
    }

    private string GetLevelGenerationPrompt()
    {
        return @"Generate a Super Mario Bros style level map that closely follows the original game's design principles.
The map should be a 2D array of integers (15 rows x 150 columns).

Tile Mapping:
0: Air (empty space)
1: Ground (solid brown ground tile)
2: Brick (breakable brick block)
3: Question Block (contains power-ups or coins)
5: Hard Block (indestructible gray block)
10: Pole (flagpole)
11: Flag (at top of pole)

Provide a JSON object with:
- map: number[][] (15 rows x 150 columns)
- enemyPositions: {x: number, y: number}[] (x and y in pixels, not tiles)

CRITICAL DESIGN RULES:
1. Rows 13 and 14 (bottom 2 rows) MUST be mostly type 1 (Ground)
2. First 10 columns MUST have solid ground so Mario starts safely
3. Place 15-25 enemies on ground level
4. Place 8-12 Question Blocks (type 3) at rows 9-11
5. Place 20-30 brick blocks (type 2) to create platforms
6. Place pole (type 10) at column 145, rows 2-13
7. Place flag (type 11) at row 2, column 145

Return only valid JSON with map and enemyPositions arrays.";
    }

    private class LevelResponse
    {
        public int[][]? Map { get; set; }
        public List<EnemyPosition>? EnemyPositions { get; set; }
    }

    private class EnemyPosition
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
