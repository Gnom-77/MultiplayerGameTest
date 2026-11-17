using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _MultiplayerGameTest._Scripts.UI.MainMenu
{
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField addressInput;
        [SerializeField] private TMP_InputField portInput;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private TMP_Text statusText;

        public event Action HostClicked;
        public event Action JoinClicked;
        public event Action StopClicked;

        public string PlayerName => nameInput.text;
        public string Address => addressInput.text.Trim();
        public string PortText => portInput.text.Trim();

        private void Awake()
        {
            hostButton.onClick.AddListener(() => HostClicked?.Invoke());
            joinButton.onClick.AddListener(() => JoinClicked?.Invoke());
            stopButton.onClick.AddListener(() => StopClicked?.Invoke());
        }

        public void SetInputs(string playerName, string address, string port)
        {
            nameInput.text = playerName;
            addressInput.text = address;
            portInput.text = port;
        }

        public void SetStatus(string text) => statusText.text = text;

        public void SetIdle(bool idle)
        {
            hostButton.interactable = idle;
            joinButton.interactable = idle;
            nameInput.interactable = idle;
            addressInput.interactable = idle;
            portInput.interactable = idle;
            stopButton.gameObject.SetActive(!idle);
        }
    }
}