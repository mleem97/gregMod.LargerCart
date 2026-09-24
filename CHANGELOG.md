# Changelog — gregMod.LargerCart

Format: [Keep a Changelog](https://keepachangelog.com/de/1.0.0/).

## [Unreleased]

### Added

- **Cart-Stabilisierung** (`StabilizeCart`, Default an): Rigidbody-Masse ×5 und
  AngularDrag ×10 gegen Wegfliegen/Taumeln unter Last. Multiplikatoren
  (`CartMassMultiplier` 1–50, `CartAngularDragMultiplier` 1–100) in
  `Mods/gregMod.LargerCart/config.json`, einmal pro Szene, alles geloggt.
- **Klapp-Tisch** (`TableEnabled`, Taste `TableToggleKey`, Default `T`):
  Ablageplatte + Beine als Trolley-Kind (Geometrie aus Trolley-Bounds, Material
  vom Trolley, kein Collider). Stirbt mit Szenenwechsel, wird neu gebaut.
- **Tray-Slots** (`TableTraySlots`, 4×3-Raster auf Plattenhöhe): in
  `positionsOnTrolley`/`usedPositions` eingehängt, Slots hängen am Trolley
  (sichtbar auch bei eingeklappter Platte).

## [2.0.0]

- Aktueller Stand (Slot-Kapazität via `TrolleyArray`, JSON-Config).
