# Dodgy.EXE

A 3D arcade dodge and boss-survival game for Android and PC. Dodge a lava-arena boss, its turrets and its explosives by moving and backflipping, and reach a score of 351 to win.

Game Design (DMVR504), Fall 2026, ESLSCA University. Instructor: Dr. Arafa Taher.

## Run it

1. Clone this repository.
2. Open the folder in Unity Hub with Unity **6000.3.20f1**.
3. Open the main scene `Assets/_Recovery/0 (1).unity`.
4. Press Play, then choose **Play** in the menu.

For Android: switch the build target to Android, set landscape orientation, build an APK and install it on a phone.

## Controls

| Action | Keyboard | Touch |
|---|---|---|
| Move left / right | A / D or arrow keys | Swipe left or right |
| Backflip | Space, W or Up | FLIP button |
| Pause | Esc | Pause button (top right) |
| Menus | Mouse click | Tap |

## How to play

- Your score rises while you survive. Green orbs give +25 score and one orb for the shop, and vanish after 6 seconds.
- One hit from any projectile, explosion or boss attack ends the run.
- Orange aim lines and red circles show where attacks will land. The backflip lifts you over low shots but does not make you invulnerable.
- Reach 351 to win. Spend saved orbs on colours in the shop.

| Score | What happens |
|---|---|
| 0 | Thrower shots only |
| 50 | Side turrets and purple shots |
| 75 | Blue slow shots |
| 100 | Red split shots, double throwers, lane barriers |
| 125 | Bouncing shots |
| 150 | Yellow split shots and explosives |
| 201 | Boss arrives |
| 276 | Boss enters a faster second phase |
| 351 | You win |

The thresholds are fields on `GameManager`.

## Status

Done: full run from menu to win or lose, backflip, turrets, boss, explosives, orbs, pause, How to Play, shop colours, saved best score, Android APK.

Planned: power-up orbs, boss defeat sequence, endless mode, shop skins, object pooling.

See [TASK_BOARD.md](TASK_BOARD.md) and [TEST_PLAN.md](TEST_PLAN.md).

## Team

| Member | ID | Area |
|---|---|---|
| Vincent Sakr | 242500153 | Gameplay and models |
| Mohab Mohamed | 242500568 | Core scripts, menus and shop |
| Zein Eldin Amr | 242500413 | Android, HUD and testing |
| Anthony Edward | 242500465 | Arena and environment |

## Credits

- "Low Poly Lava Fighting Arena/Stage" by Umar (@123umar123), [Sketchfab](https://sketchfab.com/3d-models/low-poly-lava-fighting-arenastage-6c9918fa61014829ab10048aa6c3171b), CC BY 4.0. Modified with colliders, background, walls and lighting.
- Built with Unity 6000.3.20f1 and Blender.
