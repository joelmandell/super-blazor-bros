# Migration Notes: React to Blazor WebAssembly

## Overview

This project has been successfully migrated from React/TypeScript to Blazor WebAssembly targeting .NET 10.

## Key Changes

### Technology Stack

| Component | Before (React) | After (Blazor) |
|-----------|---------------|----------------|
| Frontend Framework | React 19 | Blazor WebAssembly |
| Language | TypeScript | C# 13 |
| Build Tool | Vite | .NET SDK |
| Package Manager | npm | NuGet |
| Runtime | JavaScript | WebAssembly |
| Target Framework | Node.js | .NET 10 |

### Architecture Changes

**Component Migration:**
- `App.tsx` → `Pages/Game.razor`
- `components/GameCanvas.tsx` → JavaScript game engine with JSInterop
- `services/geminiService.ts` → `Services/GeminiService.cs`
- `types.ts` → `Models/GameTypes.cs`
- `constants.ts` → `Models/Constants.cs`

**State Management:**
- React hooks (useState, useEffect) → Blazor component lifecycle and state
- JavaScript event handlers → Blazor event handlers with JSInterop

**Styling:**
- Tailwind CSS (CDN) → Custom CSS with game.css
- React inline styles → CSS classes

### File Structure

```
BlazorApp/
├── Models/
│   ├── Constants.cs        # Game constants and default level
│   └── GameTypes.cs        # Enums and data models
├── Services/
│   └── GeminiService.cs    # AI level generation service
├── Pages/
│   └── Game.razor          # Main game component
├── wwwroot/
│   ├── js/
│   │   └── game.js         # Canvas-based game engine
│   └── css/
│       └── game.css        # Game styling
└── Program.cs              # Application entry point
```

## Development

### Prerequisites

- .NET 10 SDK
- Modern web browser with WebAssembly support

### Running the Application

```bash
cd BlazorApp
dotnet run
```

The application will be available at `https://localhost:5202`

### Building for Production

```bash
cd BlazorApp
dotnet publish -c Release
```

Output will be in `bin/Release/net10.0/publish/wwwroot/`

## API Integration

The Gemini API integration has been ported to use C#'s HttpClient:
- TypeScript `@google/genai` → C# HttpClient with JSON serialization
- API calls use the same Gemini 2.0 Flash endpoint
- Request/response handling adapted for C# async/await patterns

## Known Differences

1. **JavaScript Interop**: Canvas rendering still uses JavaScript for performance, called via JSInterop
2. **Styling**: Custom CSS instead of Tailwind utility classes
3. **Build Process**: .NET build system instead of Vite
4. **Hot Reload**: Blazor hot reload instead of Vite HMR

## Benefits of Migration

1. ✅ **Type Safety**: Full C# type safety across the entire application
2. ✅ **Performance**: WebAssembly provides near-native performance
3. ✅ **Ecosystem**: Access to .NET libraries and NuGet packages
4. ✅ **Tooling**: Visual Studio / Rider integration
5. ✅ **Maintainability**: Single language (C#) for both frontend and backend logic

## Testing

All core functionality has been verified:
- ✅ Game loads and runs
- ✅ Keyboard controls work
- ✅ Touch controls work
- ✅ AI level generation functional
- ✅ State management working
- ✅ No security vulnerabilities (CodeQL verified)

## Future Enhancements

Potential improvements for the Blazor version:
- Add .NET-based audio service (currently planned but not implemented)
- Optimize WebAssembly bundle size
- Add server-side Blazor variant for better initial load
- Implement Blazor PWA features
- Add unit tests using xUnit/bUnit

## Support

For issues or questions, please refer to:
- [Blazor Documentation](https://learn.microsoft.com/aspnet/core/blazor/)
- [.NET 10 Documentation](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10)
