using System;
using UnityEngine;

namespace _MultiplayerGameTest._Scripts.Core.Pause
{
    [CreateAssetMenu(menuName = "Game/Pause State", fileName = "PauseState")]
    public class PauseState : ScriptableObject, IPauseState
    {
        public bool IsPaused { get; private set; }

        public event Action Changed;

        private void OnEnable() => IsPaused = false;

        public void SetPaused(bool paused)
        {
            if (IsPaused == paused)
                return;

            IsPaused = paused;
            Changed?.Invoke();
        }
    }
}