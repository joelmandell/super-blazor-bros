# Super Blazor Bros

En Super Mario Bros-klon byggd med Blazor WebAssembly och .NET 10, med AI-genererade banor via Google Gemini.

## Funktioner

- 🎮 Klassisk Super Mario Bros-spelupplevelse
- 🤖 AI-genererade banor med Google Gemini 2.0 Flash
- 📱 Responsiv design med touch-kontroller
- 🎨 Pixel-art grafik i klassisk stil
- 🔊 Ljudeffekter och musik (kommande)

## Installation

**Förutsättningar:** .NET 10 SDK

1. Installera beroenden:
   ```bash
   cd BlazorApp
   dotnet restore
   ```

2. Starta utvecklingsservern:
   ```bash
   dotnet run
   ```

3. Öppna webbläsaren på `https://localhost:5001`

## AI-genererade banor

För att använda AI-funktionen behöver du en gratis API-nyckel från [Google AI Studio](https://aistudio.google.com/app/apikey).

1. Klicka på "Ange API-nyckel" i menyn
2. Klistra in din Gemini API-nyckel
3. Klicka på "✨ SKAPA AI-BANA" för att generera en ny bana

API-nyckeln sparas lokalt i din webbläsare.

## Kontroller

- **Pilarna / D-PAD**: Gå vänster/höger
- **SPACE / A**: Hoppa
- **SHIFT / B**: Springa/Skjuta eldkulor (när du har Fire Flower)

## Teknologi

- Blazor WebAssembly
- .NET 10
- C# 13
- Google Gemini AI
- HTML5 Canvas
- CSS3

## Bygga för produktion

```bash
cd BlazorApp
dotnet publish -c Release
```

Byggfilerna kommer att finnas i `BlazorApp/bin/Release/net10.0/publish/wwwroot/`-mappen.

## Utveckling

Projektet är byggt med Blazor WebAssembly och använder:
- C# för spellogik och AI-integration
- JavaScript för Canvas-rendering och tangentbordsinmatning
- JSInterop för kommunikation mellan C# och JavaScript

## Licens

MIT
