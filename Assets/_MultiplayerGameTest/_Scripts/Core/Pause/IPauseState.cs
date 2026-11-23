using System;

namespace _MultiplayerGameTest._Scripts.Core.Pause
{
    /// <summary>
    /// Shared "game is paused / cursor is free" flag between gameplay and UI.
    /// </summary>
    public interface IPauseState
    {
        bool IsPaused { get; }
        event Action Changed;
        void SetPaused(bool paused);
    }
}