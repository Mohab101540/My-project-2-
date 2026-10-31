# Task Board

Statuses: Not Started / In Progress / Blocked / Done. "Done" means verified in Play Mode by the owner (and on a phone for Android tasks), not just written.
The shared team doc "Dodge Game — Team Tasks" mirrors this board.

## Done so far
- [x] B1 Backflip (Space/W/Up + FLIP button), repeatable after landing — Vincent
- [x] B2 Side turrets in the Hierarchy (TurretLeft/TurretRight), active from score 50 — Vincent
- [x] B3 Boss telegraphs (aim lines) + phase 2 at 276 — Vincent
- [x] B4 Explosive lobs with warning discs and fire patches from 150 — Vincent
- [x] B5 Difficulty thresholds on GameManager, win at 351 — Vincent
- [x] B6 Blue / bouncer / yellow projectiles in progression — Vincent
- [x] Collectible orb prefab (+25), win screen, freeze + restart
- [x] Player physics movement; fast shots can't pass through
- [x] Android controls first pass (FLIP button, swipe)
- [x] New Blender models: player, thrower, boss; black tracking turrets beside the boss
- [x] Editor speed-up (no reload on Play, DirectX 11); main scene set to `_Recovery/0 (1)`

## Open tasks

| ID | Owner | Description | Priority | Depends on | Acceptance criteria | Files / systems | Status | Testing |
|---|---|---|---|---|---|---|---|---|
| V1 | Vincent | Android performance: object pooling for projectiles, orbs, explosions; profile on a real phone | P1 | Z1 | Steady 60 FPS on the test phone through the boss fight; no GC spikes | projectile.cs, thrower.cs, BossController.cs, GameManager | Not Started | Profiler on device |
| V2 | Vincent | Power-up orbs: Shield (blocks one hit) and Slow-Time (half speed 3 s) | P2 | — | Shown on HUD while active, expire cleanly, cleared on restart | Collectible, GameManager, PlayerMovement | Not Started | T3, T7 |
| V3 | Vincent | Boss defeat sequence (flash, break apart, camera shake) before YOU WIN | P2 | — | Plays once, under 3 s, then win screen | BossController, GameManager | Not Started | T6 |
| V4 | Vincent | Script clean-up: Gameplay / Enemies / UI / Editor folders (moved inside Unity), remove leftover logs | P2 | — | No missing scripts, no Console spam | Assets/scripts | Not Started | T0 |
| V5 | Vincent | Endless mode: "Keep going" after the boss | P3 | V3 | Difficulty keeps rising until a hit | GameManager | Not Started | Manual |
| Z1 | Zein | Android build profile (package name, landscape, icon); build APK; install on a phone | P1 | — | APK installs and a full run plays on the phone | Build Profiles, Player Settings | In Progress: package name, landscape and icon set; needs Android Build Support installed, then APK on a phone | Device |
| Z2 | Zein | Fix FLIP button on Android: Canvas Scaler "Scale With Screen Size", safe area, tap never moves the player | P1 | Z1 | Easy to hit on 2 phones; tapping it never moves the player | GameUI, FlipButton, PlayerMovement | Code done, needs phone test: GameUI scales with screen size, HUD/FLIP inside SafeArea, fingers that start on a button never steer | Device |
| Z3 | Zein | Fix swipe on Android: tune Swipe Threshold, two-finger move + flip, Restart by touch | P1 | Z1 | Responsive on the phone; flip works while holding a swipe | PlayerMovement | Code done, needs phone test: steering follows one finger so a second finger can tap FLIP; Swipe Threshold still 0.03 | Device |
| Z4 | Zein | Android test pass: whole TEST_PLAN on the phone, log bugs | P1 | Z1–Z3 | Every test has a phone result | TEST_PLAN.md | Not Started | All |
| A1 | Zein | Verify win/lose freeze + Restart resets everything (3 restarts) | P1 | — | Each run starts at 0, no leftovers | GameManager | In Progress: lose path verified in Play Mode 2026-10-30 (3 restarts, each at 0, no leftovers); win path still to test | T5–T7 |
| A2 | Zein | Verify player physics (no wall jitter, every hit registers) | P2 | — | Verified in editor and on phone | player movement.cs, projectile.cs | Code done, needs phone test | T1, T4 |
| A3 | Zein | Projectile cleanup check with the Profiler | P2 | — | Projectile count bounded | projectile.cs | Done in Play Mode (2026-10-30): never more than 2 projectiles / 1 orb up to score 239; Profiler check with V1 | T8 |
| E1 | Zein | Arena obstacles: lane barriers from score 100 | P1 | C3 | Can't pass through; never forces an unavoidable hit | new Obstacle script/prefab, GameManager | Done (Play Mode 2026-10-30): LaneBarrier from 100, solid, backs off instead of pinning, can be flipped over | T4 |
| E2 | Zein | Audio: throw, hit, collect, boss, win, lose + music | P2 | — | Every event has a sound; works on phone speaker | AudioSources | Code done, needs phone test: GameAudio on GameManager, AudioSources under "Audio" in the Hierarchy, clips in Assets/Audio | Manual |
| E3 | Zein | Hit feedback: camera shake + red flash + phone vibration | P2 | — | Plays once per hit | Main Camera, UI | Code done, needs phone test (vibration): shake + red flash play once per hit in Play Mode | T5 |
| E4 | Zein | High score saved (PlayerPrefs), on HUD and end screens | P2 | D3, D5 | Survives closing the app | GameManager, UI | Done (Play Mode 2026-10-30): PlayerPrefs HighScore, Best on HUD and end screen | Manual |
| D5 | Zein | HUD: "Score: 120", goal "reach 351", controls hint | P1 | — | Readable on a phone | main scene UI | Done (Play Mode 2026-10-30) | T9 |
| D6 | Zein | "+25" popup on orb pickup | P2 | — | Visible every pickup | GameManager, UI | Done (Play Mode 2026-10-30) | T3 |
| D7 | Zein | Final test, 7 screenshots, 1-minute video | P1 | Z4, D1–D5 | Every checklist row has a real result | docs | Not Started | All |
| C1 | Anthony | Arena art theme + quick mock-up agreed with the team | P1 | — | Team agrees on one theme | — | Not Started | Review |
| C2 | Anthony | New themed floor; player's lane clearly marked | P1 | C1 | Lane obvious from the game camera | Plane, materials, Models | Not Started | Visual |
| C3 | Anthony | New themed walls (same collider positions, x/z = ±10) | P1 | C1 | Look finished; colliders unchanged | LeftWall/RightWall/FrontWall/Backwall | Not Started | Visual, T1 |
| C4 | Anthony | Environment/background/skybox around the arena | P2 | C1 | No empty dark areas on screen | Lighting, skybox, Models | Not Started | Visual |
| C5 | Anthony | Lighting pass; projectiles and orbs stand out on the new floor | P2 | C2 | Every projectile type easy to see | Directional Light, materials | Not Started | Visual |
| C6 | Anthony | Short hit/split/explosion particles | P3 | V1 | Gone within 2 s; no FPS drop on phone | new particle prefabs | Not Started | T8 |
| D1 | Mohab | Start menu: title, Play, Shop, Controls, Quit | P1 | — | Launch shows menu; Play starts a fresh run | GameManager, UI | Not Started | T9 |
| D2 | Mohab | Pause menu (Esc + on-screen pause button): Resume, Restart, Main Menu | P1 | A1 | Pause stops everything; Resume continues | GameManager, UI | Not Started | T9 |
| D3 | Mohab | Game Over / Win screens with score, Restart + Main Menu | P1 | A1 | Both work by touch | GameManager, main scene UI | Not Started | T5, T6 |
| D4 | Mohab | Controls / How to Play screen | P2 | D1 | New player can play without help | UI | Not Started | T9 |
| D8 | Mohab | Shop: spend orbs on colours/skins, saved (PlayerPrefs) | P3 | D1 | Purchase deducts orbs, item persists | new Shop script, UI | Not Started | Manual |
