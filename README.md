# InteractableHighlight

Risk of Rain 2 makes interactables hard to spot from a distance. With this mod, chests, multishop terminals, printers, scrappers, shrines, broken drones, the teleporter, barrels, items on the ground and every other interactable glow softly in a colour that tells you what they are, so you can find them from across the map.

- **Never through walls:** the glow is drawn on the object itself, so anything in front of it hides it.
- **Vanilla selection unchanged:** the outline that shows which shop terminal or interactable you are aiming at works exactly as before.
- **Only what you can still use:** opened chests, bought terminals and used shrines stop glowing (so do interactables locked during the teleporter event).

## Installation
Install with a Thunderstore-compatible mod manager, or copy the `plugins/InteractableHighlight` folder of the release zip into `BepInEx/plugins/`. Requires BepInEx and RoR2BepInExPack.

## Multiplayer
Client-side: each player chooses. Nobody else needs the mod.

## Configuration
`BepInEx/config/com.hugocacciatori.InteractableHighlight.cfg` (changes apply while playing):
- `General.Enabled`, `General.Strength` (0 turns it off), `General.MaxDistance` (metres, 0 = unlimited, the default).
- `Colors.<Category>` for Chest, LargeChest, LegendaryChest, LunarChest, VoidChest, Barrel, Shop, Printer, Scrapper, Shrine, Drone, Teleporter, Altar, Pickup and Other. Set a colour's alpha to 0 to switch that category off.
