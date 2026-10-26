# Submission Checklist — DMVR504 Assignment1

Status values: Complete and verified / Implemented but not verified / Incomplete / Blocked.

| Requirement | Status | Notes |
|---|---|---|
| GitHub project link | Implemented but not verified | https://github.com/Mohab101540/My-project-2- — local main is 1 commit ahead and today's changes are uncommitted |
| Screenshots of main features | Incomplete | See list below |
| One-minute gameplay video | Incomplete | See plan below |
| Player movement | Implemented but not verified | A/D, arrows, touch |
| Collecting items | Implemented but not verified | Green orbs, added 2026-10-26 |
| Score updates | Implemented but not verified | Time + orbs |
| Enemy AI | Implemented but not verified | Throwers with wind-up, boss with 5 patterns |
| Obstacles | Implemented but not verified | Projectiles (split, fast, bouncing), side turrets, explosive lobs with fire patches |
| Win condition | Implemented but not verified | Reach 351 (survive the boss), added 2026-10-26 |
| Lose condition | Implemented but not verified | One hit |
| Scripts organized, no errors | Implemented but not verified | All in Assets/scripts; compiled outside Unity with 0 errors |
| Play Mode validation | Incomplete | Run TEST_PLAN.md |
| Final team review | Incomplete | All four members |

## Screenshots
1. Main gameplay (arena, player, thrower, HUD)
2. Player mid-move
3. Green orb + score before/after collecting
4. Projectiles in flight (red split, purple)
5. Double throwers / boss
6. YOU WIN screen
7. GAME OVER screen

Capture: in Play Mode, Win+Shift+S over the Game view.

## One-minute video plan
Record with Windows Game Bar (Win+Alt+R) on the Game view, or Unity Recorder.
- 0–5 s: press Play, show arena and score at 0.
- 5–15 s: move left/right, jump.
- 15–25 s: collect 2 orbs, score jumps by 25 each.
- 25–40 s: dodge thrower and split projectiles; double throwers at 100.
- 40–50 s: explosives (red discs) and boss battle from 201, aim lines before each attack.
- 50–60 s: win screen at 351 (or show a deliberate hit for GAME OVER), press Restart.

Tip: to hit both endings in 60 s, temporarily raise `scorePerSecond` on GameManager in the Inspector
for the recording only, or record win and lose as two clips and cut them together. Revert after.
