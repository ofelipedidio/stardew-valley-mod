# Stardew Valley Mod

A base SMAPI mod project for Stardew Valley.

## Requirements

- Stardew Valley
- SMAPI
- .NET 6 SDK

## Build

Install Stardew Valley and SMAPI first, then build the project:

```sh
dotnet build
```

The build copies the mod into Stardew Valley's `Mods` folder and creates a release zip in `bin/Debug/net6.0/`.

If the build package can't find your game install, set `GamePath` in `StardewValleyMod.csproj` to the Stardew Valley game folder. On macOS with Steam, that is usually:

```xml
<GamePath>$(HOME)/Library/Application Support/Steam/steamapps/common/Stardew Valley/Contents/MacOS</GamePath>
```

## GitHub Actions

The CI workflow restores NuGet packages and validates `manifest.json`. It doesn't run a full mod compile because GitHub-hosted runners don't include Stardew Valley or SMAPI game files.

## Project Structure

- `StardewValleyMod.csproj`: .NET 6 class library project configured for SMAPI.
- `ModEntry.cs`: SMAPI entry point with a basic input event handler.
- `manifest.json`: SMAPI mod metadata.
