# Changelog — gregMod.LargerCart

Format: [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added

- **Cart stabilization** (`StabilizeCart`, default on): Rigidbody mass ×5 and
  angular drag ×10 against flying away/tumbling under load. Multipliers
  (`CartMassMultiplier` 1–50, `CartAngularDragMultiplier` 1–100) in
  `Mods/gregMod.LargerCart/config.json`, once per scene, everything logged.
- **Folding table** (`TableEnabled`, key `TableToggleKey`, default `T`):
  shelf + legs as trolley child (geometry from trolley bounds, material
  from the trolley, no collider). Destroyed on scene change, rebuilt.
- **Tray slots** (`TableTraySlots`, 4×3 grid at shelf height): hooked into
  `positionsOnTrolley`/`usedPositions`, slots attached to the trolley
  (visible even when the shelf is folded).

## [2.1.0] — 2026-09-24

### Added

<<<<<<< Updated upstream
- Trolley stabilization (mass/inertia, tuned more gently: mass x2,
  angular drag x4); config wiring (config → statics + patch hook).

## [2.0.0]

- Current state (slot capacity via `TrolleyArray`, JSON config).
