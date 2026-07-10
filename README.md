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
- a millimeter scale grid in the preview,
- a parameter panel,
- terrain generator selection,
- terrain style presets,
- scale presets and custom scale that resize current dimensions,
- optional reference miniature preview with mouse dragging,
- with-base / without-base generation,
- smoothed no-base terrain edges,
- bounds fitting for generated objects,
- configurable outer wall thickness,
- target triangle budget control,
- seed randomization,
- STL export.

## Generation Parameters

`Resolution` is the terrain grid subdivision count. Higher values create more vertices, more triangles, and finer detail. If `Target triangles` is set, the app estimates a matching resolution automatically.

Seed digits currently influence terrain character:

- ones digit: roughness, from smoother to bumpier,
- tens digit: rockiness and ridge strength,
- hundreds digit: asymmetry and off-center shaping,
- thousands digit: detail scale, from broader forms to tighter features,
- remaining digits: deterministic noise phase and variation.

Style presets influence shape, smoothness, detail density, and preview color. Current presets are `Realistic`, `Stylized`, `Anime-inspired`, `Miniature-friendly`, `Low-poly`, and `Rugged / natural`.

Scale presets are not just labels. BoardMesh Studio uses 28 mm as the baseline scale. When the selected scale changes, the visible width, depth, height, reference miniature, base thickness, wall thickness, and noise dimensions are proportionally updated in millimeters for 6 mm, 10 mm, 15 mm, 20 mm, 25 mm, 28 mm, 32 mm, 35 mm, 54 mm, or a custom scale.

The reference miniature is optional. When enabled, it appears beside the generated object, defaults to 28 mm at 28 mm scale, can be dragged with the mouse in the 3D preview, and is never included in STL export.

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
