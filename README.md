This project is a recreation of OMORI's base overworld and turn-based RPG battle system within Unity version 6000.3.0f1 using C#.

Implemented mechanics and systems include:
- Tile-based overworld player and enemy movement
- Room transitions with multiple entrances/exits
- Overworld enemy movement AI with custom A* pathfinding
- Battle encounter tables and enemy spawning
- Tiered emotion battle system with stat changes
- Flexible player and enemy battle skills using IBattleAction interface and Scriptable Objects
- Boss encounters with unique behaviors (e.g., a boss summoning a minion enemy at the end of each turn and consuming it for HP if not killed by the end of the next round of turns)
- Mouse-driven battle targeting and action selection
- Battle animation and damage sequencing

Additionally, menus that were intially controlled with keyboard are now controlled almost entirely through mouse input.
Controls are as follows:

--- General (Overworld & Battle) ---

Hold Escape: Quit
F11: Fullscreen

--- Overworld ---

WASD/Arrow Keys: Move
Shift/Control (press/toggle on/off): Sprint
E: Interact

--- Battle ---

Point and click with mouse: Select
Escape, Backspace, or Delete: Back

This project is a non-commercial educational recreation of systems inspired by OMORI. All original characters, art, music, and intellectual property belong to their respective owners.
