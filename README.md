# Stardew Valley Mod

A base SMAPI mod project for Stardew Valley.

## Requirements

- Stardew Valley
- SMAPI
- .NET 6 SDK

## Build

```sh
dotnet build
```

The `Pathoschild.Stardew.ModBuildConfig` package configures Stardew Valley and SMAPI references automatically. If it can't find your game install, set `GamePath` in `StardewValleyMod.csproj` to the Stardew Valley game folder.

## Project Structure

- `StardewValleyMod.csproj`: .NET 6 class library project configured for SMAPI.
- `ModEntry.cs`: SMAPI entry point with a basic input event handler.
- `manifest.json`: SMAPI mod metadata.
