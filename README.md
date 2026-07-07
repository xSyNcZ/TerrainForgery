# TerrainForgery

TerrainForgery is a C# project for procedural generation of 3D printable tabletop terrain models.

## Current Milestone: MVP-0

MVP-0 is a simple console application. It generates a closed procedural hill mesh and exports it as an ASCII STL file.

There is no UI yet. The current code focuses on core geometry, hill generation, and STL export.

## Roadmap

See [ROADMAP.md](ROADMAP.md) for the planned milestones from MVP-0 to v1.0.

## Run the CLI

```powershell
dotnet run --project src/TerrainForgery.Cli
```

The command creates:

```text
samples/hill_test.stl
```

It also prints the output path and triangle count.

## Build and Test

```powershell
dotnet build
dotnet test
```
