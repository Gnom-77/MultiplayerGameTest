using System;
using System.Text;
using _MultiplayerGameTest._Scripts.Core.Networking;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.Core.Pause
{
    public class PausePresenter : IStartable, IDisposable
    {
        private readonly PauseView _view;
        private readonly IPauseState _pause;
        private readonly IPlayerRoster _roster;
        private readonly INetworkSessionService _session;
        private readonly StringBuilder _builder = new();

        public PausePresenter(PauseView view, IPauseState pause, IPlayerRoster roster,
            INetworkSessionService session)
        {
            _view = view;
            _pause = pause;
            _roster = roster;
            _session = session;
        }

        public void Start()
        {
            _view.ResumeClicked += OnResumeClicked;
            _view.LeaveClicked += OnLeaveClicked;
            _pause.Changed += Refresh;
            _roster.Changed += Refresh;
            Refresh();
        }

        public void Dispose()
        {
            _view.ResumeClicked -= OnResumeClicked;
            _view.LeaveClicked -= OnLeaveClicked;
            _pause.Changed -= Refresh;
            _roster.Changed -= Refresh;
        }

        private void OnResumeClicked() => _pause.SetPaused(false);

        private void OnLeaveClicked() => _session.Shutdown();

        private void Refresh()
        {
            _view.SetVisible(_pause.IsPaused);
            if (!_pause.IsPaused)
                return;

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

            _view.SetPlayers(_builder.ToString());
        }
    }
}