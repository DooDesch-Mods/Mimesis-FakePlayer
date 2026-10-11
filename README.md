# MIMESIS - FakePlayers

> 🛟 **Need help or found a bug?** Get support at [support.doodesch.de/fakeplayer](https://support.doodesch.de/fakeplayer).


> Spawns configurable fake players in your Mimesis session so a solo host can test multiplayer mods without needing real friends to join.

![Version](https://img.shields.io/badge/version-1.1.4-blue)
![Game](https://img.shields.io/badge/game-MIMESIS-purple)
![MelonLoader](https://img.shields.io/badge/MelonLoader-0.7.3+-green)
![Status](https://img.shields.io/badge/status-working-brightgreen)

FakePlayers joins simulated clients after your host avatar loads. Each client completes the normal login, room-entry and level-load handshake and appears as an idle player in the game. It pairs naturally with TooManyPlayers: raise that mod's cap, set how many fakes you want here, and start a session to test the higher limit.

---

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Limitations](#limitations)
- [Compatibility](#compatibility)
- [Building (developers)](#building-developers)
- [Credits / License](#credits--license)

---

## Features

- Spawns named idle player avatars with unique SteamIDs, player UIDs and session IDs.
- Uses the normal client admission checks. Fake players respect the active player cap and have no host privileges.
- Follows the host through room transitions and confirms the level load.
- Sends heartbeats and removes rejected or closed sessions through the game's SessionManager.
- Logs requests, confirmed joins, player-limit rejection and response timeouts.

---

## Requirements

| Component | Version |
|-----------|---------|
| MIMESIS | 0.3.2 |
| MelonLoader | 0.7.3+ |

---

## Installation

- **Recommended:** install through a Thunderstore mod manager (r2modman / Gale) and let it resolve MelonLoader for you.
- **Manual:**
  1. Download the latest `FakePlayers.dll` from the [releases page](https://github.com/DooDesch-Mods/Mimesis-FakePlayer/releases).
  2. Drop it into `MIMESIS/Mods/`.
  3. Launch the game once to generate the config file at `UserData/MelonPreferences.cfg`.

No extra assets are required - only `FakePlayers.dll`.

---

## Configuration

Stored in `UserData/MelonPreferences.cfg` under the `FakePlayers` category.

The host's `Enabled` and `FakePlayerCount` apply for the lobby. Joining clients with this mod automatically use those values and keep their local config. A host without FakePlayers creates no fake sessions. Changes apply to the next lobby.

| Option | Description | Default | Values/Range |
|--------|-------------|---------|--------------|
| `Enabled` | Enable or disable fake player emulation for testing (display name "Enable Fake Players"). When `false`, no fake players are created. | `true` | `true` / `false` |
| `FakePlayerCount` | Number of fake players to spawn (display name "Fake Player Count"). Default 3 gives a total of 4 players including yourself (the vanilla cap). Clamped on load: below 0 becomes 0, above 32762 becomes 32762. A value of 0 (or less) creates no fake players. | `3` | `0` - `32762` (clamped on load) |

---

## Usage

No keybinds and no in-game UI - FakePlayers works automatically:

1. Enable the mod and set `FakePlayerCount` in `UserData/MelonPreferences.cfg` (or via a config UI).
2. Host / start a session. Fake clients join once your host avatar has loaded.

They appear as idle players and count toward the normal player limit. They complete room transitions and stay connected with heartbeats, but they do not walk or play autonomously.

To test higher player counts, pair FakePlayers with TooManyPlayers (`DooDesch/TooManyPlayers` on Thunderstore). Raise that mod's max players, set `FakePlayerCount` accordingly (for example `7` for 8 total), then start a session to exercise the higher limit.

This is a host-only testing tool: it injects players on the local host machine, so run it on the machine that is hosting the test session.

---

## Limitations

- Fake players remain idle at the normal spawn points and do not play autonomously.
- This validates game admission, avatar creation and room transitions. It does not validate real Steam tickets, external relays or many physical Steam clients.
- Some multiplayer features may not behave correctly with fake players present.
- Intended for testing player-count and session logic, not for normal play.

---

## Compatibility

Built for Mimesis 0.3.2 / MelonLoader 0.7.3. This is a multiplayer/host testing tool: it must be run on the host machine, since it injects players locally during host registration.

---

## Building (developers)

```
dotnet build -c Release
```

Standalone mod (no MimicAPI). References resolve from `Workspace/lib/game` (game DLLs) and `Workspace/lib/melonloader`. Target framework is `netstandard2.1`. The shared deploy configuration defaults to `_Test/MIMESIS/Mods`.

---

## Credits / License

Author: DooDesch. Provided as-is under the MIT License. Contributions are welcome via pull requests.

Repository: <https://github.com/DooDesch-Mods/Mimesis-FakePlayer>

## AI disclosure

The mod icon is AI-generated.
