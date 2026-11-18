using System;
using System.Text;
using _MultiplayerGameTest._Scripts.Core.Networking;
using UnityEngine;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.UI.Lobby
{
    public class LobbyPresenter : IStartable, IDisposable
    {
        private readonly LobbyView _view;
        private readonly IPlayerRoster _roster;
        private readonly INetworkSessionService _session;
        private readonly NetworkSettings _settings;
        private readonly StringBuilder _builder = new();

        public LobbyPresenter(LobbyView view, IPlayerRoster roster, INetworkSessionService session,
            NetworkSettings settings)
        {
            _view = view;
            _roster = roster;
            _session = session;
            _settings = settings;
        }

        public void Start()
        {
            Debug.Log("[Lobby] Presenter started.");
            _view.StartClicked += OnStartClicked;
            _view.LeaveClicked += OnLeaveClicked;
            _roster.Changed += Refresh;

            _view.SetStartVisible(_session.IsHost);
            Refresh();
        }

        public void Dispose()
        {
            _view.StartClicked -= OnStartClicked;
            _view.LeaveClicked -= OnLeaveClicked;
            _roster.Changed -= Refresh;
        }

        private void OnStartClicked()
        {
            Debug.Log("[Lobby] Start pressed.");
        }

        private void OnLeaveClicked() => _session.Shutdown();

        private void Refresh()
        {
            var players = _roster.Players;
            _builder.Clear();

            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];
                _builder.Append(i + 1).Append(". ").Append(player.Name);
                if (player.IsHost)
                    _builder.Append(" [Host]");
                if (player.ClientId == _roster.LocalClientId)
                    _builder.Append(" (You)");
                _builder.AppendLine();
            }

            _view.SetHeader($"Lobby  {players.Count}/{_settings.MaxPlayers}");
            _view.SetPlayers(_builder.ToString());
        }
    }
}