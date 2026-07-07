# BoardMesh Studio

BoardMesh Studio is a C# project for procedural generation of 3D printable tabletop terrain models.

## Current Milestone: v0.1 Desktop Preview

v0.1 adds a WPF desktop preview app for generating, viewing, and exporting a single terrain object.

The current code focuses on core geometry, procedural terrain generators, STL export, and a first native WPF `Viewport3D` preview.

## Roadmap

See [ROADMAP.md](ROADMAP.md) for the planned milestones from MVP-0 to v1.0.

## Run the Desktop Preview

```powershell
dotnet run --project src/BoardMeshStudio.App
```

The desktop app includes:

- a 3D preview for a generated terrain mesh,
- orbit, zoom, pan, and reset camera controls,
- a parameter panel,
- terrain generator selection,
- STL export.

## Run the CLI

```powershell
dotnet run --project src/BoardMeshStudio.Cli
```

The command creates:

```text
samples/hill_test.stl
```

It also prints the output path and triangle count.

## Build and Test

```powershell
dotnet build BoardMeshStudio.sln
dotnet test BoardMeshStudio.sln
```
