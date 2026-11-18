using System;
using System.Text;
using _MultiplayerGameTest._Scripts.Core;
using _MultiplayerGameTest._Scripts.Core.Networking;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.UI.Loading
{
    public class LoadingScreenPresenter : IStartable, IDisposable
    {
        private readonly LoadingScreenView _view;
        private readonly ISceneLoadState _state;
        private readonly IPlayerRoster _roster;
        private readonly StringBuilder _builder = new();

        public LoadingScreenPresenter(LoadingScreenView view, ISceneLoadState state, IPlayerRoster roster)
        {
            _view = view;
            _state = state;
            _roster = roster;
        }

        public void Start()
        {
            _state.Changed += Refresh;
            Refresh();
        }

        public void Dispose() => _state.Changed -= Refresh;

        private void Refresh()
        {
            _view.SetVisible(_state.IsLoading);
            if (!_state.IsLoading)
                return;

            _view.SetTitle(string.IsNullOrEmpty(_state.SceneName) ? "Loading..." : $"Loading {_state.SceneName}...");

            // Per-client progress is only known to the host.
            _builder.Clear();
            foreach (var pair in _state.ClientStatuses)
                _builder.Append(ResolveName(pair.Key)).Append(": ").AppendLine(pair.Value.ToString());

            _view.SetDetails(_builder.ToString());
        }

        private string ResolveName(ulong clientId)
        {
            var players = _roster.Players;
            for (var i = 0; i < players.Count; i++)
            {
                if (players[i].ClientId == clientId)
                    return players[i].Name;
            }

            return $"Client {clientId}";
        }
    }
}