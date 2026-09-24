# Changelog — gregMod.LargerCart

Format: [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added

- **Cart stabilization** (`StabilizeCart`, default on): Rigidbody mass ×5 and
  angularDrag ×10 against flying/tumbling under load. Multipliers
  (`CartMassMultiplier` 1–50, `CartAngularDragMultiplier` 1–100) in
  `Mods/gregMod.LargerCart/config.json`, once per scene, all logged.
- **Folding table** (`TableEnabled`, key `TableToggleKey`, default `T`):
  shelf plate + legs as trolley children (geometry from trolley bounds, material
  from trolley, no collider). Dies with scene change, rebuilt fresh.
- **Tray slots** (`TableTraySlots`, 4×3 grid at plate height): hooked into
  `positionsOnTrolley`/`usedPositions`, slots hang off the trolley
  (visible even with folded plate).

## [2.1.0] — 2026-09-24

### Added

- Trolley stabilization (mass/inertia, gently tuned: mass x2, angularDrag x4);
  config wiring (config → statics + patch hook).

## [2.0.0]

- Current state (slot capacity via `TrolleyArray`, JSON config).
