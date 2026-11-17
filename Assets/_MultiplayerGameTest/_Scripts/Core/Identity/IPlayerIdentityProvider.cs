namespace _MultiplayerGameTest._Scripts.Core.Identity
{
    public interface IPlayerIdentityProvider
    {
        string PlayerId { get; }
        string PlayerName { get; }
        void SetPlayerName(string playerName);
    }
}