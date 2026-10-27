# Changelog

## [Unreleased]

### Fixed

- Use the MIMESIS 0.4.0 current-room protocol when fake clients join. Remove the deleted Logistics room protocol.

## [1.1.4] - 2026-10-11

- Spawn visible idle avatars through the normal client join and level-load handshake.
- Respect the active player cap so FakePlayers can validate TooManyPlayers.
- Keep fake clients connected with heartbeats and follow host room transitions.
- Remove rejected and closed sessions through SessionManager and show their configured names.

## [1.1.3] - 2026-10-11

- Use the host's fake-player settings for the lobby without changing clients' local config.
- Clean up virtual sessions when leaving a session so a new host session uses its own fake-player count.

## [1.1.2] - 2026-10-11

- Fake sessions use the MIMESIS 0.3.2 login signature.
- Real players use the game's room entry checks, including player-limit mods.
- Spawn-point fallback can select an existing point when the game's first lookup returns none.
