using System;
using UnityEngine;

namespace _MultiplayerGameTest._Scripts.Core.Identity
{
    public class LocalPlayerIdentityProvider : IPlayerIdentityProvider
    {
        private const string IdKey = "player.id";
        private const string NameKey = "player.name";
        private const int MaxNameLength = 20;
        
        
        public string PlayerId { get; }
        public string PlayerName { get; private set; }
        
        public LocalPlayerIdentityProvider()
        {
            var id = PlayerPrefs.GetString(IdKey, string.Empty);
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString(IdKey, id);
            }

            var playerName = PlayerPrefs.GetString(NameKey, string.Empty);
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = $"Player{UnityEngine.Random.Range(1000, 10000)}";
                PlayerPrefs.SetString(NameKey, playerName);
            }

            PlayerPrefs.Save();
            PlayerId = id;
            PlayerName = playerName;
        }
        
        public void SetPlayerName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return;
            
            playerName = playerName.Trim();
            if (playerName.Length > MaxNameLength)
                playerName = playerName.Substring(0, MaxNameLength);

            PlayerName = playerName;
            PlayerPrefs.SetString(NameKey, playerName);
            PlayerPrefs.Save();
        }
    }
}