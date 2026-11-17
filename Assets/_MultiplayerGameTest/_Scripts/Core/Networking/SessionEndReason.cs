namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    public enum SessionEndReason
    {
        LocalRequest,
        SessionFull,
        VersionMismatch,
        DuplicatePlayer,
        ConnectionTimeout,
        Kicked,
        ConnectionLost,
        Unknown
    }
}