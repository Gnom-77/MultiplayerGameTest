# Multiplayer Game Test (Unity 6.3 LTS, Netcode for GameObjects)

A small cooperative prototype for 2-4 players: connect to a host, meet in a network lobby, load a level
together and interact with shared physics items.

- Unity 6.3 LTS (6000.3.28f1), Netcode for GameObjects (NGO), Unity Transport (UDP, IP:port)
- Dependency injection: VContainer
- Input: New Input System
- Topology: listen server (host-client). The host is the authority for the world state and the session.

**Time spent:**  ~20 hours

## 1. How to run

### Scenes
`Boot` -> `MainMenu` -> `Lobby` -> `Game`.

### Host and join
1. Enter a player name.
2. **Host**: press *Host*. You are moved to the lobby.
3. **Join**: enter the host IP and port, press *Join*. The host's IP on a LAN (or a forwarded port on the internet) is required.
4. The host presses **Start** in the lobby to load the game scene for everybody.

Default address and port come from the `NetworkSettings` asset (`Resources/Data`).

### Local testing in the editor
`Window > Multiplayer > Multiplayer Play Mode`, enable Player 2 / Player 3. Give every player a different name.

### Command line arguments

| Argument | Description |
|---|---|
| `-transport utp` | Transport selection (`utp` / `unity`). `steam` is recognized but not implemented. |
| `-address <ip>` | Default address shown in the menu. |
| `-port <port>` | Default port shown in the menu. |
| `-playerid <id>` | Overrides the persistent player id. Needed to run several instances of one build on the same PC. |
| `-buildversion <v>` | Overrides the build version sent in the handshake.  |
| `-stallload <ms>` | Test helper: blocks the main thread while the game scene loads (simulates a slow client). |

Example for a second instance of the same build on one PC:
`Game.exe -playerid second -address 127.0.0.1 -port 1234`

### Controls

| Key | Action |
|---|---|
| WASD / mouse / Space | Move / look / jump |
| Q | Spawn an item (host creates it) |
| E | Grab the item you look at / release the held item |
| Left mouse | Throw the held item |
| X | Delete the held item |
| Esc | Pause menu: player list, Resume, Leave |

## 2. Architecture
### Dependency injection
- `RootLifetimeScope` lives in the `Boot` scene and survives scene loads. It registers all global services.
  `EntryPoint` is the application entry point (it loads the main menu).
- Scene scopes (`MainMenuLifetimeScope`, `LobbyLifetimeScope`, `GameLifetimeScope`) use the root scope as parent
  and register the scene's view and presenter.
- Views are passive (no logic); presenters hold the logic and depend only on `Game.Core` interfaces.
- Network-spawned prefabs (player, items) are created by NGO, not by the container, so they cannot receive injection.
  Shared state they need is exposed through ScriptableObject assets (`PauseState`, `ItemSettings`), which the container
  also registers for the rest of the game.

### Extension points
- **New transport**: implement `INetworkTransportProvider` (configure `NetworkManager` for host/client) and register it in
  `RootLifetimeScope.RegisterTransport`. The transport is selected in one place by the `NetworkSettings` asset or `-transport`.
- **New item type**: create an item prefab (copy `Item`: `Rigidbody`, `NetworkObject`, `NetworkTransform`, `NetworkRigidbody`,
  `NetworkItem`), add it to the Network Prefabs List and to `ItemSettings.itemPrefabs`.
- **New rejection reason**: add a value to `SessionEndReason`, check it in `ConnectionApprovalHandler`, add a message in `SessionEndMessages`.

## 3. Known issues and what is not done
- **Not tested under artificial latency and packet loss.** The design is intended for it (host-validated requests, state in
  `NetworkVariable`s, interpolation), but I did not get to a measured test run with ~150 ms and packet loss, so possible
  tuning problems (interpolation smoothness, held item feel on the holder side) are unverified.
- **Steam is not implemented** (no Steam P2P transport, lobbies, Rich Presence or stats). The transport abstraction is in place:
  a Steam provider would implement `INetworkTransportProvider` (for example with a Facepunch-based NGO transport from
  `Unity-Technologies/multiplayer-community-contributions`), and the identity would come from a Steam `IPlayerIdentityProvider`.
- **Hard disconnects are detected by the transport timeout** (about 15 seconds by default). It can be shortened through the transport's
  disconnect timeout setting; a clean exit or Alt+F4 is detected immediately.
- **Connection approval and player-id uniqueness are tested only on a LAN or localhost.** There is no relay or NAT traversal with
  Unity Transport; internet play needs a forwarded port.