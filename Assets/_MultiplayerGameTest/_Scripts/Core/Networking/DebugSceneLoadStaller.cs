using System;
using System.Threading;
using UnityEngine;

namespace _MultiplayerGameTest._Scripts.Core.Networking
{
    /// <summary>
    /// Test helper: blocks the main thread while the scene loads to simulate a slow client.
    /// Enabled with the command line argument: -stallload 8000 (milliseconds).
    /// </summary>
    public class DebugSceneLoadStaller : MonoBehaviour
    {
        private void Awake()
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-stallload" && int.TryParse(args[i + 1], out var milliseconds))
                {
                    Debug.LogWarning($"[Debug] Stalling scene load for {milliseconds} ms.");
                    Thread.Sleep(milliseconds);
                }
            }
        }
    }
}