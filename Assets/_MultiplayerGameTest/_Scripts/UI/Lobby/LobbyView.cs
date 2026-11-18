using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _MultiplayerGameTest._Scripts.UI.Lobby
{
    public class LobbyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text playersText;
        [SerializeField] private Button startButton;
        [SerializeField] private Button leaveButton;

        public event Action StartClicked;
        public event Action LeaveClicked;

        private void Awake()
        {
            startButton.onClick.AddListener(() => StartClicked?.Invoke());
            leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());
        }

        public void SetHeader(string text) => headerText.text = text;
        public void SetPlayers(string text) => playersText.text = text;
        public void SetStartVisible(bool visible) => startButton.gameObject.SetActive(visible);
    }
}
