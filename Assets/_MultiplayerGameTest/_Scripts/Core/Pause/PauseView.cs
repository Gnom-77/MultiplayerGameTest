using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _MultiplayerGameTest._Scripts.Core.Pause
{
    public class PauseView :  MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text playersText;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button leaveButton;

        public event Action ResumeClicked;
        public event Action LeaveClicked;

        private void Awake()
        {
            resumeButton.onClick.AddListener(() => ResumeClicked?.Invoke());
            leaveButton.onClick.AddListener(() => LeaveClicked?.Invoke());
        }

        public void SetVisible(bool visible) => panel.SetActive(visible);
        public void SetPlayers(string text) => playersText.text = text;
    }
}