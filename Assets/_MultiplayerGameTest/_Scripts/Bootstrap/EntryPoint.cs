using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace _MultiplayerGameTest._Scripts.Bootstrap
{
    public sealed class EntryPoint : IStartable
    {
        private const string MainMenuScene = "MainMenu";
        
        public void Start()
        {
            Debug.Log("Root container ready, loading main menu scene");
            SceneManager.LoadScene(MainMenuScene);
        }
    }
}