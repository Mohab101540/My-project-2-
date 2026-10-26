# Changelog

## 2026-10-26
- Added collectible orbs: a green orb spawns every 4 s on the player's lane, gives +25 score, disappears after 6 s if missed (`Collectible.cs`, `GameManager.cs`).
- Added a win condition: surviving the boss phase (now score 351) shows "YOU WIN!".
- Game over and win now freeze the game and show the result with the final score; Restart unfreezes and reloads.
- Projectiles now destroy themselves after 10 s if they never hit anything.
- Added project docs: rules, task board, test plan, submission checklist.
- Player now moves through its Rigidbody in FixedUpdate; rotation and forward/back movement are locked so hits can't knock the player off the lane.
- Projectiles use continuous collision so fast ones (purple) can't pass through the player.
- Removed Console spam logged on every throw, bounce, split and boss attack.
- Backflip: Space, W, Up or the Jump button flips the player over low shots (1.5 s cooldown, ground only). Gravity is now on for the player, so jumps come back down; a Rigidbody is added at runtime if the scene's player has none (the recovery scene).
- Two side turrets appear at score 50: they track the player and flash red before each shot. They stop when the boss arrives.
- Boss attacks are now telegraphed: the boss flashes orange and draws aim lines on the floor showing where the shots will go. At score 276 it enters phase 2 (faster attacks and shots).
- Explosive lobs from score 150: a red disc marks where they land, the blast covers 1.8 m and leaves a fire patch for 2.5 s that you can backflip over.
- Blue (75), bouncer (125) and yellow (150) projectiles are now part of the progression; win moved to score 351 to give the boss fight 15 s.
- Fixed the extra throwers switching on and off every frame after score 200, which also hid the "BOSS BATTLE!" message.
- Main scene is now `Assets/_Recovery/0 (1).unity` (player bobro) and the only enabled scene in Build Settings; `SampleScene` is kept as a disabled backup.
- Side turrets now use copies of the red (left) and purple (right) thrower models and fire from each model's throw point.
- Backflip can be repeated: "on the ground" is now detected from floor contact instead of exact zero vertical speed, and landing always ends the flip.
- Android controls: a FLIP button appears bottom-right when no Jump button is assigned; swiping left/right moves the player, holding after a swipe keeps moving, swiping back reverses straight away.
- Gameplay objects now live in the main scene's Hierarchy instead of being built by code: TurretLeft (red model) and TurretRight (purple model) at x = -5 / 5 inside the arena, GameUI/FlipButton, and a Rigidbody on bobro. Orbs come from the new Assets/prefabs/Collectible prefab. Added the editor menu Tools > Dodge Game > Set Up Scene that creates these.
- Fixed backflip only working once: the editor had kept an old 30 s cooldown on the player. The setting is now `Flip Cooldown` (0.2 s after landing).
- New low-poly models made in Blender: player (blue suit, visor, backpack), thrower robot (orange, holding a bomb) and boss (purple horned mech with arm cannons and a glowing core). Files: Assets/Models/Remodel_*.fbx; the Blender script that builds them is Assets/Models/Source/make_models.py.txt. Old meshes are switched off, not deleted.
- The two throwers beside the boss (RedThrower, PurpleThrower) now look like black turrets with a red/purple barrel ring and turn to track the player.
- Editor speed: pressing Play no longer reloads all scripts (Enter Play Mode Options), and the Windows graphics API is set to DirectX 11 first (applies after restarting Unity). In-game frame rate measured at about 165 FPS before and after.
- New editor menus: Tools > Dodge Game > Apply New Models and Speed Up Editor.
