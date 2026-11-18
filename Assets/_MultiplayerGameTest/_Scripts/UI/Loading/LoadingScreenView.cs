using TMPro;
using UnityEngine;

namespace _MultiplayerGameTest._Scripts.UI.Loading
{
    public class LoadingScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text detailsText;

        public void SetVisible(bool visible) => panel.SetActive(visible);
        public void SetTitle(string text) => titleText.text = text;
        public void SetDetails(string text) => detailsText.text = text;
    }
}