using UnityEngine;

namespace WakeAIUp.Game
{
    /// <summary>
    /// Entry point for the game scene.
    /// Attach this to an empty GameObject in game.unity to bootstrap the entire UI.
    /// Creates the SpyGameManager singleton which builds all UI programmatically.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            // Set up the camera for UI rendering
            var cam = Camera.main;
            if (cam == null)
            {
                var camObj = new GameObject("MainCamera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            // Light background color matching our theme
            cam.backgroundColor = new Color(0.953f, 0.965f, 1f); // UITheme.Background
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.orthographic = true;

            // Create game manager
            if (SpyGameManager.Instance == null)
            {
                var managerObj = new GameObject("SpyGameManager");
                managerObj.AddComponent<SpyGameManager>();
            }
        }
    }
}
