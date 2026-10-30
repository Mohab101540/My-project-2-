# Test Plan

Run in the Unity Editor, main scene `Assets/_Recovery/0 (1).unity`, Play Mode, Console open.
Record each result with date and tester. Only mark Pass after actually running it.

| ID | Test | Steps | Expected | Result |
|---|---|---|---|---|
| T0 | Compiles | Open project, wait for import | Console shows no compile errors | Scripts built OK outside Unity (2026-10-26); Editor check pending |
| T1 | Movement | Hold A/D and arrows; swipe on touch | Player moves both ways, stops at ±7 on X, never leaves arena | |
| T2 | Backflip | Press Space (also W, Up, Jump button) | Player flips once, lands on the lane, can flip again right after landing; can't flip in mid-air | Seen working in Play Mode 2026-10-26 (recovery scene) |
| T3 | Collectibles | Wait ~4 s; walk into green orb | Orb spawns on the player's line, disappears on touch, score +25 exactly once; uncollected orb vanishes after 6 s || Pass, Play Mode 2026-10-30 (Zein): orb on the lane, +25 once with a "+25" popup |
| T4 | Enemy AI and obstacles | Survive to 100+ | Thrower winds up and throws at player; purple at 50, red splits at 100; red/purple throwers appear at 100, leave at 200 || Partial, Play Mode 2026-10-30 (Zein): lane barrier appears at 100, can't be walked through, never pins the player, can be flipped over |
| T5 | Lose | Stand still in front of a throw | "GAME OVER / Score: N" shown, Restart button appears, everything freezes || Pass, Play Mode 2026-10-30 (Zein): Game Over screen with Score and Best, freeze, red flash + shake once |
| T6 | Win | Survive (and collect orbs) to score 351 | Turrets at 50, explosives (red discs) at 150, boss at 201 with orange aim lines before each attack, "BOSS ENRAGED!" at 276, removed at 351, "YOU WIN! / Score: N" shown, freeze, Restart visible. Tip: lower the thresholds on GameManager in the Inspector to test faster | |
| T7 | Restart | Press Restart after T5 and after T6, 3 times | Score 0, only main thrower, no boss, no leftover projectiles/orbs, controls work || Pass, Play Mode 2026-10-30 (Zein): 3 restarts in a row after Game Over, each at score 0 with no leftovers (restart after Win not run yet) |
| T8 | Cleanup | During play, watch Hierarchy | Projectile and Collectible clones don't pile up (projectiles gone within 10 s) || Pass, Play Mode 2026-10-30 (Zein): max 2 projectiles and 1 orb alive up to score 239 |
| T9 | UI readability | Fresh player plays once | Score, goal and result are understood without explanation || HUD checked in Play Mode 2026-10-30: Score, Best, goal and controls hint visible; phone check pending |

## Regression
After any change re-run T1, T3, T5, T6, T7.
