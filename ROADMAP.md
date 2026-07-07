# TerrainForgery Roadmap

## Project Vision

TerrainForgery is planned as a desktop tool for procedurally generating 3D printable tabletop terrain and battlefield layouts. It should support both individual terrain pieces and complete playable battlefields, with a workflow focused on tabletop wargaming, STL export, scale-aware design, 3D printing, and approachable user editing.

The long-term goal is not to become a full 3D modeling suite. TerrainForgery should remain a focused generator and planning assistant: fast to use, deterministic when needed, easy to preview, and practical for preparing printable terrain.

## Development Principles

- [ ] Start with a small working core.
- [ ] Generate valid STL files before building complex UI.
- [ ] Keep geometry generation separated from UI.
- [ ] Keep exporters modular.
- [ ] Prefer deterministic generation through seeds.
- [ ] Make scale and printability first-class concepts.
- [ ] Avoid becoming a full Blender replacement.
- [ ] Keep editing simple, fast, and user-friendly.

## Version Roadmap

| Version | Milestone | Focus | Status |
| --- | --- | --- | --- |
| v0.0.1 | MVP-0 | Core STL generation | Current / In Progress |
| v0.1 | Desktop Preview App | Single-object preview and export | Planned |
| v0.2 | Scale System | Scale-aware generation | Planned |
| v0.3 | Board Size System | Inch-based board planning | Planned |
| v0.4 | Terrain Object Library | Terrain metadata and categories | Planned |
| v0.5 | Genre / Setting System | Thematic generation rules | Planned |
| v0.6 | Terrain Randomizer | Seeded terrain generation | Planned |
| v0.7 | Full Map Generator | Complete battlefield drafts | Planned |
| v0.8 | Segmented Export | 3D printer bed-aware export | Planned |
| v0.9 | User Editing Tools | Simple scene editing | Planned |
| v0.10 | Advanced Preview | Full scene visualization | Planned |
| v0.11 | Gameplay Tools | Playability analysis | Planned |
| v0.12 | Measurement and Range Tools | Tabletop distance helpers | Planned |
| v0.13 | 3D Printing Validation | Printability checks | Planned |
| v0.14 | Project Save / Load | Persistent project files | Planned |
| v0.15 | Preset Library | Reusable scale, board, map, and export presets | Planned |
| v0.16 | Better Buildings | Advanced building generators | Planned |
| v0.17 | Scene Layers | Layered scene management | Planned |
| v0.18 | Objective Layout Tools | Objective placement and validation | Planned |
| v0.19 | Deployment Zone Tools | Deployment planning | Planned |
| v0.20 | Project Sharing and Export Packages | Shareable project bundles | Planned |
| v1.0 | Public Release | Stable public GitHub release | Planned |

### v0.0.1 - MVP-0: Core STL Generation

**Goal:** deliver the first working milestone. The console application generates a simple hill model and exports it as an ASCII STL file.

**Scope:**

- Core mesh model
- `Vertex`, `Triangle`, and `Mesh`
- Procedural hill generator
- ASCII STL exporter
- CLI sample generation
- Basic unit tests
- Sample output file

**Expected result:** running the CLI creates `samples/hill_test.stl`.

**Status:** Current / In Progress

### v0.1 - Desktop Preview App

**Goal:** add the first desktop application with a 3D preview for a single generated object.

**Scope:**

- WPF application
- Basic 3D preview
- Camera orbit, zoom, pan, and reset
- Generate button
- Export STL button
- Parameter panel for one generator

**Initial generators:**

- Hill
- Crater
- Rock
- Simple wall
- Simple block / building placeholder

### v0.2 - Scale System

**Goal:** add model scale support so generated terrain has sensible dimensions for different miniature games.

**Scope:**

- Scale presets
- Custom scale
- Reference miniature size
- Base size presets
- Scale-aware doors, walls, windows, and scatter objects

**Scale presets:**

- 6 mm
- 10 mm
- 15 mm
- 20 mm
- 25 mm
- 28 mm
- 32 mm
- 35 mm
- 54 mm
- Custom

Scale must not be only a label. It should affect generated model dimensions.

### v0.3 - Board Size System

**Goal:** add battlefield board size selection in inches and convert dimensions to millimeters.

**Scope:**

- Board size presets
- Custom board size
- Inch-to-millimeter conversion
- Visible board boundaries
- Top-down board grid
- Optional inch grid
- Measurement helpers

**Board presets:**

- 12" x 12"
- 22" x 30"
- 24" x 24"
- 24" x 32"
- 30" x 44"
- 36" x 36"
- 44" x 60"
- 48" x 48"
- 48" x 72"
- Custom

### v0.4 - Terrain Object Library

**Goal:** add a library of basic terrain objects.

**Categories:**

- Buildings
- Ruins
- Hills
- Rocks
- Craters
- Walls
- Barricades
- Containers
- Scatter
- Roads
- Platforms
- Bridges
- Trees
- Pipes
- Industrial props
- Objective markers

**Object metadata:**

- Name
- Category
- Genre
- Scale compatibility
- Footprint size
- Height
- Complexity
- Print difficulty
- Line-of-sight blocking
- Cover value
- Walkable flag
- Scatter flag

### v0.5 - Genre / Setting System

**Goal:** add map genre and setting selection.

**Genres:**

- Sci-fi
- Cyberpunk
- Industrial
- Post-apocalyptic
- Fantasy
- Dark fantasy
- Medieval
- Modern urban
- Desert
- Jungle
- Arctic
- Alien planet
- Military
- Trench warfare
- Gothic sci-fi
- Steampunk

Genre should affect available building types, object shapes, damage level, decorations, scatter terrain, default density, and overall map atmosphere.

### v0.6 - Terrain Randomizer

**Goal:** add a deterministic seeded terrain randomizer.

**Generation modes:**

- Generate single object
- Generate scatter set
- Generate building set
- Generate full battlefield draft
- Generate symmetrical map
- Generate narrative map
- Generate dense urban map
- Generate sparse desert map
- Generate tournament-style layout

**Parameters:**

- Board size
- Scale
- Genre
- Terrain density
- Building density
- Scatter density
- Average object height
- Line-of-sight blocking amount
- Cover amount
- Symmetry
- Random seed
- Print segment size
- Walkable terrain amount
- Objective spacing

**Primary sliders:**

- Terrain density
- Building density
- Scatter density
- Verticality
- Ruination
- Symmetry
- Cover amount
- Line-of-sight blocking
- Complexity

The randomizer must be deterministic. The same seed and settings should generate the same scene.

### v0.7 - Full Map Generator

**Goal:** generate an entire battlefield.

**Modes:**

- Full board as one scene
- Separate printable objects
- Segmented board tiles
- Buildings only
- Scatter only
- Base board only
- Road layout only
- Objective layout only

**Example export files:**

- `board_base.stl`
- `building_001.stl`
- `building_002.stl`
- `scatter_001.stl`
- `wall_001.stl`
- `tile_A1.stl`
- `tile_A2.stl`

### v0.8 - Segmented Export

**Goal:** split boards and large objects into printable segments.

**Scope:**

- Maximum segment size
- Printer bed presets
- Export by layer
- Export by object
- Export by tile
- Automatic file naming

**Segment presets:**

- 100 x 100 mm
- 150 x 150 mm
- 180 x 180 mm
- 200 x 200 mm
- 220 x 220 mm
- Custom

**Target printer presets:**

- Bambu Lab A1 Mini
- Bambu Lab A1 / P1 / X1
- Prusa MK3 / MK4
- Elegoo Neptune
- Custom printer

### v0.9 - User Editing Tools

**Goal:** add simple and intuitive scene editing.

**Operations:**

- Select object
- Move object
- Rotate object
- Scale object
- Duplicate object
- Delete object
- Lock object
- Hide object
- Group objects
- Ungroup objects

**Special actions:**

- Randomize selected object
- Replace selected object with similar
- Regenerate selected object
- Freeze selected object
- Lock selected objects and reroll the rest
- Clear scatter only
- Regenerate buildings only

Editing should remain simple, fast, and friendly. This is not intended to be a full modeling application.

### v0.10 - Advanced Preview

**Goal:** add an advanced 3D preview for complete scenes.

**Scope:**

- Orbit camera
- Zoom
- Pan
- Top-down view
- Isometric view
- Grid view
- 3D print preview mode
- Gaming table preview mode
- Object highlighting
- Category coloring
- Layer visibility
- Reference miniature
- Reference base
- Cover height comparison
- Door height comparison

**Reference miniatures:**

- 28 mm human
- 32 mm heroic human
- 40 mm large infantry
- 50 mm heavy infantry
- 60 mm monster
- 100 mm vehicle placeholder

### v0.11 - Gameplay Tools

**Goal:** add tools for evaluating battlefield playability.

**Features:**

- Deployment zones
- Objective markers
- Line-of-sight preview
- Cover zones
- High ground zones
- Open fire lanes
- Dead zones
- Density score
- Cover score
- Verticality score
- Symmetry score

**Example analyses:**

- Longest open fire lane
- Terrain density percentage
- Average cover distance
- Deployment safety
- Objective access
- Line-of-sight blocking amount

### v0.12 - Measurement and Range Tools

**Goal:** add measurement tools for tabletop games.

**Features:**

- Measure distance in inches
- Measure distance in millimeters
- 3D ruler
- Range bands
- 8" range band
- 16" range band
- 24" range band
- 32" range band
- Deployment distance preview
- Objective distance preview

### v0.13 - 3D Printing Validation

**Goal:** add basic 3D printing validation.

**Features:**

- Check if mesh is closed
- Check object dimensions
- Check minimum wall thickness
- Check overhang risk
- Check floating geometry
- Check very thin details
- Check base thickness
- Estimate print volume
- Warn when an object exceeds printer bed size

**Modes:**

- FDM printer mode
- Resin printer mode
- Support-free mode

### v0.14 - Project Save / Load

**Goal:** add project persistence.

**Scope:**

- Custom `.tforge` project file
- JSON scene description
- Save project
- Load project
- Export project settings
- Import project settings
- Preserve seed
- Preserve object positions
- Preserve locked objects
- Preserve generator settings

**Project data should include:**

- Scale
- Board size
- Genre
- Seed
- Objects
- Positions
- Generator settings
- Export settings

### v0.15 - Preset Library

**Goal:** add a library of reusable presets.

**Scale presets:**

- 28 mm heroic
- 32 mm heroic
- 15 mm sci-fi
- 6 mm epic

**Board presets:**

- Small skirmish
- Standard skirmish
- Large battle
- Classic square board
- Custom

**Map presets:**

- Dense sci-fi city
- Industrial complex
- Ruined village
- Desert outpost
- Alien rocks
- Trench battlefield
- Post-apocalyptic scrapyard
- Fantasy village
- Gothic cathedral ruins

**Export presets:**

- Export as full scene
- Export separate objects
- Export segmented board
- Export scatter pack only
- Export buildings only

### v0.16 - Better Buildings

**Goal:** add more advanced building generators.

**Sci-fi building types:**

- Hab block
- Bunker
- Generator station
- Comms tower
- Warehouse
- Modular corridor
- Landing pad
- Industrial platform
- Control room
- Power plant

**Fantasy building types:**

- Cottage
- Watchtower
- Ruined tower
- Chapel
- Tavern
- Stone house
- Castle wall
- Gatehouse
- Market stall

**Modern / urban building types:**

- Apartment block
- Shop
- Office building
- Garage
- Checkpoint
- Concrete barrier
- Bus stop
- Rooftop access

**Post-apocalyptic building types:**

- Scrap shack
- Destroyed house
- Fuel station
- Makeshift barricade
- Wrecked vehicle placeholder
- Broken tower
- Junk wall

**Industrial building types:**

- Pipe network
- Storage tank
- Factory wall
- Crane base
- Container stack
- Machinery block
- Ventilation unit
- Walkway

**Building parameters:**

- Width
- Depth
- Height
- Floors
- Wall thickness
- Door count
- Window count
- Damage amount
- Roof type
- Interior enabled
- Walkable roof
- Openable interior

### v0.17 - Scene Layers

**Goal:** add scene layers.

**Layers:**

- Board base
- Roads
- Buildings
- Large terrain
- Scatter
- Objectives
- Decorations
- Reference miniatures
- Grid
- Measurement guides

**Operations:**

- Hide layer
- Lock layer
- Export layer
- Regenerate layer
- Clear layer

### v0.18 - Objective Layout Tools

**Goal:** add tools for objective marker placement.

**Features:**

- Automatic objective placement
- Manual objective placement
- Distance validation
- Deployment zone distance validation
- Cover around objectives
- Symmetrical objective layout
- Narrative objective layout

**Presets:**

- 3 objectives centerline
- 5 objectives
- King of the hill
- Supply crates
- Control zones
- Deployment objectives
- Narrative scenario

### v0.19 - Deployment Zone Tools

**Goal:** add deployment zone tools.

**Features:**

- Deployment zone presets
- Custom deployment zones
- Deployment depth
- Corner deployment
- Diagonal deployment
- Line of sight from deployment
- Warning when deployment is too open

**Presets:**

- 8" deployment
- 10" deployment
- 12" deployment
- Corner deployment
- Diagonal deployment
- Custom

### v0.20 - Project Sharing and Export Packages

**Goal:** add project package export.

**Features:**

- Export project ZIP
- Preview image
- `README.txt` inside export package
- Project file
- Grouped STL folders
- Settings JSON
- Object list

**Example package structure:**

```text
TerrainForgery_IndustrialMap_24x32.zip
|-- project.tforge
|-- preview.png
|-- README.txt
|-- board/
|   |-- tile_A1.stl
|   `-- tile_A2.stl
|-- buildings/
|   |-- building_001.stl
|   `-- building_002.stl
`-- scatter/
    |-- scatter_001.stl
    `-- scatter_002.stl
```

### v1.0 - Public Release

**Goal:** ship a stable public GitHub release.

**Minimum v1.0 scope:**

- Stable desktop application
- Scale presets
- Board size presets
- 3-5 genres
- 5-8 terrain object types
- Randomizer
- Full map generation
- Object separation
- STL export
- 3D preview
- Reference miniature
- Basic user editing
- Project save/load
- Sample files
- Documentation

**Release package should include:**

- Downloadable EXE
- README
- ROADMAP
- Screenshots
- Sample STL files
- Sample projects
- GitHub Releases
- Changelog
- License
- Short demo GIF or video

## Suggested Additional Features

### Game Style Presets

Users should choose generic game styles rather than specific branded game systems.

- Dense sci-fi skirmish
- Heroic fantasy battle
- Urban squad combat
- Mass battle
- RPG encounter
- Industrial firefight
- Desert outpost

Avoid using specific tabletop game brand names in the UI to reduce trademark and licensing risk.

### Terrain Pack Generator

Terrain pack generation should create coherent printable sets.

**Example: Industrial Starter Pack**

- 2 buildings
- 6 containers
- 8 barrels
- 4 barricades
- 2 generators

Other pack ideas:

- Fantasy village pack
- Post-apocalyptic barricade pack
- Sci-fi scatter pack
- Desert rocks pack

### Support-Free Mode

Support-free generation should prefer printable geometry:

- Avoid steep overhangs
- Avoid floating geometry
- Avoid thin antennas
- Avoid unsupported bridges
- Prefer printable angles

### FDM / Resin Mode

Printer modes should affect:

- Minimum wall thickness
- Detail amount
- Base thickness
- Segmentation
- Tolerances

### Tournament Symmetry

Symmetry options:

- Mirror symmetry
- Rotational symmetry
- Loose symmetry
- Narrative asymmetry

### Map Playability Score

TerrainForgery should eventually score maps for:

- Cover: good / too low / too high
- Line of sight: too open / balanced / too blocked
- Deployment safety
- Objective access
- Verticality
- Scatter density
- Symmetry

### Ruin Damage Generator

A damage slider from 0-100 should affect:

- Broken walls
- Missing floors
- Damaged roof
- Rubble
- Holes
- Cracks
- Uneven edges

### Modular Roads

Road elements:

- Straight road
- Corner road
- T-junction
- Crossroad
- Broken road
- Raised walkway

### Automatic Cover Placement

Cover tools should:

- Add cover around objectives
- Add cover near deployment
- Break long fire lanes

### Export Build Sheet

A future PDF or PNG build sheet could include:

- Map name
- Seed
- Scale
- Board size
- Object list
- Print files
- Suggested placement
- Top-down layout

## Suggested Application Layout

### Left Panel - Configuration

- Project actions
- Scale selection
- Board size
- Genre
- Generation mode
- Density sliders
- Seed controls

### Center - 3D Preview

- 3D scene
- Grid
- Board boundaries
- Miniature reference
- Selected object outline

### Right Panel - Selected Object Editor

- Object name
- Category
- Position
- Rotation
- Scale
- Regenerate
- Duplicate
- Delete
- Lock
- Export selected

### Bottom Panel - Status

- Triangle count
- Object count
- Board size
- Scale
- Estimated files
- Warnings

## Immediate Next Steps

1. MVP-0: Console-generated STL hill.
2. MVP-1: WPF preview for a single object.
3. MVP-2: Scale and board system.
4. MVP-3: Multiple terrain generators.
5. MVP-4: Random terrain pack generator.
6. MVP-5: Full map draft generator.

## Important Notes

- Do not implement everything at once.
- First generate correct STL files.
- Then add preview.
- Then add scale and board systems.
- Then add object library.
- Then add randomizer.
- Then add full map generation.
- Then add editing tools.
