using System;
using _MultiplayerGameTest._Scripts.Core.Identity;
using _MultiplayerGameTest._Scripts.Core.Networking;
using _MultiplayerGameTest._Scripts.UI.MainMenu;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.UI
{
    public class MainMenuPresenter : IStartable, IDisposable
    {
        private readonly MainMenuView _view;
        private readonly INetworkSessionService _session;
        private readonly IPlayerIdentityProvider _identity;
        private readonly LaunchOptions _options;

        public MainMenuPresenter(MainMenuView view, INetworkSessionService session,
            IPlayerIdentityProvider identity, LaunchOptions options)
        {
            _view = view;
            _session = session;
            _identity = identity;
            _options = options;
        }

        public void Start()
        {
            _view.SetInputs(_identity.PlayerName, _options.Address, _options.Port.ToString());
            _view.SetIdle(_session.State == SessionState.Idle);

            _view.HostClicked += OnHostClicked;
            _view.JoinClicked += OnJoinClicked;
            _view.StopClicked += OnStopClicked;
            _session.StateChanged += OnStateChanged;
            _session.SessionEnded += OnSessionEnded;
        }

        public void Dispose()
        {
            _view.HostClicked -= OnHostClicked;
            _view.JoinClicked -= OnJoinClicked;
            _view.StopClicked -= OnStopClicked;
            _session.StateChanged -= OnStateChanged;
            _session.SessionEnded -= OnSessionEnded;
        }

        private void OnHostClicked()
        {
            if (!TryReadPort(out var port))
                return;

            _identity.SetPlayerName(_view.PlayerName);
            _view.SetStatus(string.Empty);
            _session.StartHost(port);
        }

        private void OnJoinClicked()
        {
            if (!TryReadPort(out var port))
                return;

            _identity.SetPlayerName(_view.PlayerName);
            _view.SetStatus(string.Empty);
            _session.Join(new ConnectionTarget(_view.Address, port));
        }

        private void OnStopClicked() => _session.Shutdown();

        private bool TryReadPort(out ushort port)
        {
            if (ushort.TryParse(_view.PortText, out port) && port >= 1024)
                return true;

            _view.SetStatus("Invalid port (1024-65535).");
            return false;
        }

        private void OnStateChanged(SessionState state)
        {
            _view.SetIdle(state == SessionState.Idle);

            switch (state)
            {
                case SessionState.Connecting:
                    _view.SetStatus("Connecting...");
                    break;
                case SessionState.Connected:
                    _view.SetStatus(_session.IsHost ? "Hosting. Waiting for players..." : "Connected to host.");
                    break;
            }
        }

        private void OnSessionEnded(SessionEndReason reason)
        {
            _view.SetStatus(reason == SessionEndReason.LocalRequest
                ? string.Empty
                : SessionEndMessages.Get(reason));
        }
    }
}