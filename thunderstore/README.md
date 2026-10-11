# MIMESIS - FakePlayers

> 🛟 **Need help or found a bug?** Get support at [support.doodesch.de/fakeplayer](https://support.doodesch.de/fakeplayer).


> Registers configurable fake players in your Mimesis session so a solo host can test multiplayer mods without needing real friends to join.

![Version](https://img.shields.io/badge/version-1.1.3-blue)
![Game](https://img.shields.io/badge/game-MIMESIS-purple)
![MelonLoader](https://img.shields.io/badge/MelonLoader-0.7.3+-green)
![Status](https://img.shields.io/badge/status-working-brightgreen)

FakePlayers injects dummy player sessions right after you register as the host, so you can exercise player-count and session logic on your own machine. It pairs naturally with TooManyPlayers: raise that mod's cap, set how many fakes you want here, and start a session to test the higher limit.

## Features

- Injects fake players once, automatically, right after your own host registration succeeds.
- Creates virtual player sessions through the game's local/offline session system, each with a unique SteamID, a unique player UID, and a name like `FakePlayer1`, `FakePlayer2`, and so on.
- Lets fake players bypass the default 4-player room cap while real players still hit the normal limit.
- Logs room-entry and spawn-point failures.
- Gives each fake session a unique session ID to avoid collisions between simultaneous fakes.
- Logs extensively across the join pipeline so you can trace whether fakes actually entered the room.

## Requirements / Dependencies

| Component | Version |
|-----------|---------|
| MIMESIS | 0.3.2 |
| MelonLoader | 0.7.3+ |

MelonLoader is the only hard dependency. TooManyPlayers is recommended for testing higher player counts but optional.

## Installation

- **Recommended:** install through a Thunderstore mod manager (r2modman / Gale), which resolves MelonLoader automatically.
- **Manual:** download the package, then drop `FakePlayers.dll` into `MIMESIS/Mods/`. Launch the game once to generate the config file at `UserData/MelonPreferences.cfg`. No extra assets are required.

## Configuration

Stored in `UserData/MelonPreferences.cfg` under the `FakePlayers` category.

The host's `Enabled` and `FakePlayerCount` apply for the lobby. Joining clients with this mod automatically use those values and keep their local config. A host without FakePlayers creates no fake sessions. Changes apply to the next lobby.

| Option | Description | Default | Values/Range |
|--------|-------------|---------|--------------|
| `Enabled` | Enable or disable fake player emulation for testing. When `false`, no fake players are created. | `true` | `true` / `false` |
| `FakePlayerCount` | Number of fake players to spawn. Default 3 gives a total of 4 players including yourself (the vanilla cap). Clamped on load: below 0 becomes 0, above 32762 becomes 32762. A value of 0 (or less) creates no fake players. | `3` | `0` - `32762` (clamped on load) |

## Usage

No keybinds and no in-game UI - FakePlayers works automatically:

1. Enable the mod and set `FakePlayerCount` in `UserData/MelonPreferences.cfg`.
2. Host / start a session. Fake players are injected once, right after your host registration.

They appear in the session and count toward the player limit, but they are registered sessions only: they do not move, respond to events, or interact with the world.

To test higher player counts, pair FakePlayers with TooManyPlayers (`DooDesch/TooManyPlayers` on Thunderstore): raise that mod's max players, set `FakePlayerCount` accordingly (for example `7` for 8 total), then start a session.

This is a host-only testing tool: run it on the machine that is hosting the test session.

## Limitations

- Fake players are session-registered only - they will not spawn, move, or interact in-game.
- They do not respond to game events.
- Some multiplayer features may not behave correctly with fake players present.
- Intended for testing player-count and session logic, not for normal play.

## Links

- Source and releases: <https://github.com/DooDesch-Mods/Mimesis-FakePlayer>

## AI disclosure

The mod icon is AI-generated.

Built against MIMESIS 0.3.2 / MelonLoader 0.7.3.
