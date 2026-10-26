# Dodge Game — Project Rules

Course: DMVR504 Game Design and Development, Fall 2026 — Assignment1
Team: Vincent, Zein, Anthony, Mohab
Repo: https://github.com/Mohab101540/My-project-2-
Unity: 6000.3.20f1 (URP 17.3, Input System 1.19)

## Concept
3D dodge game with a fixed camera. The player stands at the front of a walled arena
and moves left/right to dodge projectiles thrown at them. Survive long enough to
beat the boss phase and win; get hit once and you lose.

## Architecture (Assets/scripts)
| Script | Role |
|---|---|
| `GameManager.cs` | Score (time + collectibles), difficulty stages, boss spawn/despawn, collectible spawning, win/lose, restart |
| `player movement.cs` (`PlayerMovement`) | A/D or arrows, swipe on phones, arena clamp, backflip on Space/W/Up or the FLIP button (0.2 s cooldown after landing, only from the ground). Needs the Rigidbody on the player (set in the scene) |
| `thrower.cs` (`Thrower`) | Enemy AI: wind-up (`warningTime`) then throws at the player; Main thrower picks a projectile type by unlock state, and lobs explosives once unlocked |
| `projectile.cs` (`Projectile`) | Hit player = game over. Red splits into 3, Yellow into 5, Bouncer bounces up to 8 times, Explosive lands on a red warning disc and blasts 1.8 m. 10 s safety lifetime |
| `ExplosionHazard.cs` | Blast flash + fire patch left by an explosive; kills a grounded player for 2.5 s |
| `Turret.cs` | On `TurretLeft` (red model) and `TurretRight` (purple model) in the Hierarchy. Component starts disabled; GameManager enables it at score 50. Turns to the player, flashes red during a 0.6 s wind-up, fires every 2.4 s from the model's throwpoint |
| `BossController.cs` | Boss prefab: cycles Spread, Rapid, Double Spread, Burst, Death Barrage. Each attack is telegraphed (boss flashes orange + aim lines on the floor; aim is locked when the lines appear). Phase 2: faster attacks, faster shots, shorter telegraph |
| `Collectible.cs` | On the `Assets/prefabs/Collectible` prefab (green glowing orb). Spins, adds score on player touch, expires after 6 s |
| `Editor/DodgeSceneSetup.cs` | Menus under **Tools > Dodge Game**: *Set Up Scene* (turrets, FLIP button, player Rigidbody, Collectible prefab), *Apply New Models* (Blender models on player/thrower/boss, black look for RedThrower/PurpleThrower), *Speed Up Editor*. All safe to re-run |

Main scene: `Assets/_Recovery/0 (1).unity` (player `bobro`, tag `Player`) — first and only enabled scene in Build Settings.
Backup: `Assets/Scenes/SampleScene.unity` (player `Hamamayabro`), kept in Build Settings but disabled. Make gameplay changes in the main scene only.
Scene objects added for gameplay: `TurretLeft`, `TurretRight` (move them freely), `GameUI/FlipButton`, and a Rigidbody on `bobro` (gravity on, rotation + Z frozen). GameManager references the turrets and the Collectible prefab in the Inspector.

## Game flow (score thresholds are fields on GameManager)
- 0: main thrower, normal projectiles. Throw interval drops 0.5 s per 100 score (min 0.7 s). Green orbs every 4 s (+25).
- 50: purple (fast) projectiles; two side turrets appear ("TURRETS ONLINE!").
- 75: blue (slow) projectiles.
- 100: red (split) projectiles; red + purple extra throwers switch on ("DOUBLE THROWERS!").
- 125: bouncers ("BOUNCERS!").
- 150: yellow (split 5) + explosive lobs ("EXPLOSIVES! WATCH THE RED CIRCLES").
- 200: extra throwers switch off ("TRIAL COMPLETE!").
- 201: turrets stop, boss spawns ("BOSS BATTLE!").
- 276: boss phase 2 ("BOSS ENRAGED!").
- 351: boss removed, **win** ("YOU WIN!").
- Any projectile, blast or fire touching the player: **lose** ("GAME OVER").
- On win or lose, time freezes (`Time.timeScale = 0`) and the Restart button reloads the scene.

## Backflip hit rule
No invulnerability. The flip lifts the player about 2.5 m, which is what dodges low shots and fire patches;
anything that touches the player mid-flip still counts as a hit.

## Models
- `Assets/Models/Remodel_Player.fbx`, `Remodel_Thrower.fbx`, `Remodel_Boss.fbx` — made in Blender 5.2 by `Assets/Models/Source/make_models.py.txt` (run `blender -b -P make_models.py -- <out dir> <preview dir>`).
- The old meshes (LowPolyPlayer, LowPolyThrower, the boss cube) are still there, just switched off.

## Preservation rules
- Fix in place; don't rewrite or delete working scripts, scenes, prefabs or assets.
- Keep `.meta` files and GUIDs. Don't move assets without a reason.
- `Assets/_Recovery/` and `Assets/newdodge.unity` are backups — leave them unless the team agrees to remove them.
- Check `git status` before editing; never overwrite a teammate's uncommitted work.

## Git rules
- Only the human team commits, pushes, merges or opens PRs.
- No AI co-author trailers or generated-by signatures anywhere.

## Conventions
- New gameplay values go in public/serialized fields with sensible defaults so the scene keeps working without re-wiring.
- Avoid `Find*` calls in per-frame code; look up once in `Start`.
- Every spawned object must have a bounded lifetime or a cleanup path.

## Known issues / open work
See `TASK_BOARD.md`. Main gaps: no particle effects, no audio, no menus.

## Build and run
1. Open the folder in Unity Hub with 6000.3.20f1.
2. Open `Assets/_Recovery/0 (1).unity` (the main scene), press Play.
3. Controls: A/D or Left/Right arrows to move; Space, W or Up to backflip. On a phone: swipe left/right to move, tap the FLIP button (bottom-right) to backflip.
