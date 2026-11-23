# Multiplayer Game Test (Unity 6.3 LTS, Netcode for GameObjects)

A small cooperative prototype for 2-4 players: connect to a host, meet in a network lobby, load a level
together and interact with shared physics items.

- Unity 6.3 LTS (6000.3.28f1), Netcode for GameObjects (NGO), Unity Transport (UDP, IP:port)
- Dependency injection: VContainer
- Input: New Input System
- Topology: listen server (host-client). The host is the authority for the world state and the session.

**Time spent:** ~20 hours

## 1. How to run

### Scenes
`Boot` -> `MainMenu` -> `Lobby` -> `Game`. Always start from `Boot` (scene index 0): it creates the root DI container.

### Host and join
1. Enter a player name.
2. **Host**: press *Host*. You are moved to the lobby.
3. **Join**: enter the host IP and port, press *Join*. On a LAN the host's local IP is enough. Over the internet the host
   needs a forwarded UDP port, or both players can use a virtual-LAN tool (e.g. Radmin VPN, ZeroTier) and use the host's virtual IP.
4. The host presses **Start** in the lobby to load the game scene for everybody.

Default address and port come from the `NetworkSettings` asset (`Resources/Data`).

### Local testing in the editor
`Window > Multiplayer > Multiplayer Play Mode`, enable Player 2 / Player 3. Give every player a different name.

### Command line arguments

| Argument | Description |
|---|---|
| `-transport utp` | Transport selection (`utp` / `unity`). `steam` is recognized but not implemented: it falls back to Unity Transport with a warning. |
| `-address <ip>` | Default address shown in the menu. |
| `-port <port>` | Default port shown in the menu. |
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

### Assemblies and dependencies

- **Game.Core**: interfaces and plain models with no dependency on NGO, transports or Steam
  (`INetworkSessionService`, `IPlayerRoster`, `ISceneLoadState`, `IPauseState`, `IPlayerIdentityProvider`, `ConnectionPayload`, ...).
- **Game.Networking**: NGO implementations: session lifecycle, connection approval, replicated player roster,
  scene load coordinator, player spawner, `INetworkTransportProvider`.
- **Game.Transport.Unity**: the Unity Transport provider, the only place that touches a concrete transport.
- **Game.Gameplay**: player controller, items, interaction.
- **Game.UI**: passive views and presenters (menu, lobby, loading screen, pause).
- **Game.Bootstrap**: composition root (`RootLifetimeScope`) and the entry point. The only place that knows which transport is used.

Dependencies: `Bootstrap -> {Core, Networking, Transport.Unity, Gameplay, UI}`, `Transport.Unity -> Networking -> Core`,
`Gameplay -> Core`, `UI -> Core`. `UI` and `Gameplay` do not reference any transport assembly, so replacing the transport
never touches game code. Gameplay still uses NGO types (`NetworkBehaviour`, RPCs): the game depends on the networking
library, not on the transport.

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

## 3. Networking decisions

### Authority overview

| Entity | Authority | Replicated through |
|---|---|---|
| Session (join rules) | Host | Connection approval, disconnect with a reason |
| Player list | Host | `NetworkList` on a host-spawned roster object |
| Scene loading | Host | NGO scene management + a custom "load finished" message |
| Player movement | Owner | `NetworkTransform` (Authority Mode = Owner), interpolation on |
| Free items | Host | Host-simulated physics, `NetworkTransform` + `NetworkRigidbody` |
| Held items | Holder (ownership) | `NetworkTransform`; holder id is a host-written `NetworkVariable` |

### Connection and approval
The client sends a JSON payload (player id, name, build version) as connection data. The host validates it in the approval
callback: payload, build version, player limit. A rejection carries the reason name, so the client shows
a precise message (session full, timeout, connection lost).

### Lobby and scene loading
The host keeps a replicated roster (`NetworkList`). `SceneLoadCoordinator` tracks a status for every client
(Loading / Loaded / Dropped / TimedOut). A disconnect counts as Dropped, a client that exceeds the load timeout is
disconnected by the host, and when nobody is Loading anymore the host sends one message so all loading screens close at
the same moment. A client that drops during loading therefore never blocks the others.

### Late join
A late client loads the current scene by itself (the others do not wait) and shows its own loading screen during NGO
synchronization. Items, the holder of each item and the roster are spawned objects or `NetworkVariable`/`NetworkList`
values, so they arrive with the spawn and the late joiner sees the actual state at once.

### Player model: owner-authoritative
The owning client simulates its own character and `NetworkTransform` sends position and yaw; remote players interpolate.
Why: under ~150 ms latency the local controls stay responsive without prediction or reconciliation, and remote players
move smoothly because of interpolation. The trade-off is that the host does not validate movement (not cheat-resistant),
which is acceptable for a cooperative prototype. Camera pitch is local-only, since other players do not need it.

### Items: host-simulated physics, ownership handoff on grab
- **Spawn**: a client sends a request; the host checks the sender, a cooldown and an item limit, then spawns the item
  without an owner, so it belongs to the host and stays after its creator leaves.
- **Free item**: physics runs only on the host; clients receive an interpolated transform. A thrown item flies the same
  way for everyone and does not teleport on landing, because nobody else simulates it.
- **Grab**: the host validates the sender, that the player holds nothing, the distance (with a latency tolerance) and that the
  item is free. RPCs are processed one at a time, so when two players grab the same item, the first request wins and
  the second is ignored. Clients do not predict a grab; they react to the host-written holder variable, so there is no desync.
  On success the item's ownership goes to the holder, who moves it in front of the camera.
- **Release / throw / delete**: the host checks that the sender holds exactly this item. Ownership returns to the host.
  For a throw, the client only sends a direction; the speed is decided by the host.
- **Holder leaves**: the host releases the item and it falls on the spot.

### What goes over the network
Player position and yaw (owner to others, interpolated), item transforms only while they move, and small state values
(holder id, roster entries) only when they change. Requests (spawn, grab, release, throw, delete) are small RPCs to the host.

### Session end
A client leaving (Alt+F4 or network loss) does not break the game: its player and roster entry are removed and held
items are released. When the host leaves, all clients return to the main menu with a message. No host migration.

## 4. Known issues and what is not done

- **Not tested under artificial latency and packet loss.** The design is intended for it (host-validated requests, state in
  `NetworkVariable`s, interpolation), but I did not get to a measured test run with ~150 ms and packet loss, so possible
  tuning problems (interpolation smoothness, held item feel on the holder side) are unverified. Next step: run the
  Unity Transport network simulator and re-check grab races, throws and late join.
- **Steam is not implemented** (no Steam P2P transport, lobbies, Rich Presence or stats). The transport abstraction is in place:
  a Steam provider would implement `INetworkTransportProvider` (for example with a Facepunch-based NGO transport from
  `Unity-Technologies/multiplayer-community-contributions`), and the identity would come from a Steam `IPlayerIdentityProvider`.
- **Optional extras not implemented:** host kick, ping in the player list and player color change. Kick would be a host-only
  request that calls `DisconnectClient` with a "kicked" reason; ping would be read from the transport by the host and replicated
  through the roster; color would be a host-owned `NetworkVariable` (palette index), so late joiners get it with the spawn.
- **Movement is not validated by the host** (owner-authoritative). A server-authoritative model with client prediction and
  reconciliation would fix it if cheating mattered.
- **Player id is a locally stored GUID**, so it is not tamper-proof. A platform identity (such as a Steam id) would replace it.
- **Hard disconnects are detected by the transport timeout** (about 15 seconds by default). It can be shortened through the transport's
  disconnect timeout setting; a clean exit or Alt+F4 is detected immediately.
- **Connection approval and player-id uniqueness are tested only on a LAN or localhost.** There is no relay or NAT traversal with
  Unity Transport; internet play needs a forwarded port or a virtual-LAN tool.