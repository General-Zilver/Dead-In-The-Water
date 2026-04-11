# Dead In The Water

## Overview

**Dead In The Water** is a 2D underwater survival arcade game. The player controls a diver swimming through an ocean map where air is always running out. The goal is to survive, kill enemies, earn keys, and open chests to escape before you suffocate or die.

**Genre:** 2D Survival Arcade / Action  
**Engine:** Unity  
**Team:**  
- **John Payes**
- **Bryan Alaniz**

## Core Idea

You are trapped underwater with limited air. Enemies spawn around the map and become more dangerous as the run goes on. To win, the player must kill enough enemies to earn a key, then reach the matching chest and open it. While holding a key, enemy spawning increases, which turns the trip to the chest into the most dangerous part of the run.

## Game Loop

The game starts on a title screen where the player begins a run and selects a difficulty. During gameplay, the player manages air, avoids hazards, fights enemies, and works toward the next key and chest. The run ends in either a win screen after the final chest is opened or a game over screen if the player dies.

## Core Mechanics

### Movement
The player swims freely in all directions at a base speed. Movement itself does not consume extra air, but air drains passively at all times.

### Harpoon Gun
The player can fire a harpoon at walls, platforms, or dead enemies to pull toward that target at 3x base speed. Dead enemies stay on screen briefly, which gives the player a short chance to harpoon their position for movement. The bonus speed from a pull fades back down to normal over time.

### Whirlpools
Whirlpools catch the player in their current and spin them around at high speed, similar to a harpoon pull. The player can try to use that momentum to launch away, but staying too long results in death. On Medium, whirlpools are permanent. On Hard, they appear and disappear during the run. Easy has no whirlpools.

### Air Management
Air drains over time and acts as the main survival timer. To refill air, the player must remain still inside a geyser or in a bubble refill zone. This forces the player to stop moving in dangerous areas.

| Stat | Easy | Medium | Hard |
|---|---|---|---|
| Air Duration | 2:00 | 1:30 | 1:00 |
| Refill Time | 30s | 25s | 20s |

### Health
The player takes damage from enemies and other hazards such as jellyfish.

| Difficulty | Hits to Die |
|---|---|
| Easy | 10 |
| Medium | 5 |
| Hard | 3 |

### Scoring
Kills give a base of 10 points multiplied by current speed, so faster movement leads to better scoring. A chain-kill multiplier rewards consecutive kills within a short time window.

## Win and Lose Conditions

**Win Condition:** Kill the required number of enemies to obtain a key, then reach and open the corresponding chest. Easy requires 1 key and 1 chest. Medium requires 2. Hard requires 3.

**Lose Conditions:**
- Air reaches zero.
- Health reaches zero from enemy or hazard damage.
- The player gets sucked into a whirlpool on Medium or Hard.

## Difficulty Breakdown

### Easy
- Player swims much faster than enemies.
- 1 key and 1 chest to win.
- Fewer enemies and slower spawn rates.
- No whirlpools.
- Increased spawning while holding a key, but still manageable.

### Medium
- Player swims slightly faster than enemies.
- 2 keys and 2 chests to win.
- More enemies and faster spawn rates.
- Permanent whirlpools on the map.
- Enemy spawning jumps while holding a key and settles after a chest is opened.

### Hard
- Enemies swim slightly faster than the player.
- 3 keys and 3 chests to win.
- Enemies spawn in groups instead of one at a time.
- Whirlpools spawn and despawn during the run.
- Enemy pressure is highest while the player is carrying a key.

## UI Plan

### Title Screen / Start Menu
The title screen will show the game title, a start button, and difficulty selection.

### Gameplay UI
The gameplay screen will show:
- Air gauge
- Health bar
- Score
- Current key progress
- Chain-kill multiplier

The air gauge serves as the timer for the run.

### End Screens
A win screen appears after the player opens the final chest. A game over screen appears if the player runs out of air, loses all health, or dies to a whirlpool.

### Extra UI
A high-score table can appear after a run or from the main menu.

## Technical Requirements Coverage

| Requirement | Implementation |
|---|---|
| Player Movement | Free 2D swimming in all directions |
| Camera System | Camera follows the player through the map |
| Interaction System | Harpoon pulling, chest opening, refill zones, hazard avoidance |
| Item Collection | Keys that are earned through combat and used to open chests |
| Game Loop | Title Screen -> Gameplay -> Win or Game Over |
| Required UI | Title screen, gameplay HUD, win screen, game over screen |
| Gameplay Resources | Air, health, score, key progress |

