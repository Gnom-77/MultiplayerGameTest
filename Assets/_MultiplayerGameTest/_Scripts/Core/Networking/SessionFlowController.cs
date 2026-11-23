using System;
using _MultiplayerGameTest._Scripts.Core.Pause;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    /// <summary>
    /// Drives high-level app flow around the session:
    /// the host moves everyone to the lobby when the session starts,
    /// and any player is returned to the main menu when the session ends.
    /// </summary>
    public class SessionFlowController : IInitializable, IDisposable
    {
        private readonly INetworkSessionService _session;
        private readonly INetworkSceneService _scenes;
        private readonly SessionEndNotice _notice;
        private readonly IPauseState _pause;

        public SessionFlowController(INetworkSessionService session, INetworkSceneService scenes,
            SessionEndNotice notice, IPauseState pause)
        {
            _session = session;
            _scenes = scenes;
            _notice = notice;
            _pause = pause;
        }

        public void Initialize()
        {
            _session.StateChanged += OnStateChanged;
            _session.SessionEnded += OnSessionEnded;
        }

        public void Dispose()
        {
            _session.StateChanged -= OnStateChanged;
            _session.SessionEnded -= OnSessionEnded;
        }

        private void OnStateChanged(SessionState state)
        {
            if (state == SessionState.Connected && _session.IsHost)
                _scenes.LoadForAll(SceneNames.Lobby);
        }

        private void OnSessionEnded(SessionEndReason reason)
        {
            _pause.SetPaused(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (SceneManager.GetActiveScene().name == SceneNames.MainMenu)
                return;

            if (reason != SessionEndReason.LocalRequest)
                _notice.Set(reason);

            SceneManager.LoadScene(SceneNames.MainMenu);
        } 
    }
}