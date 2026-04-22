using UnityEngine;
using KSP.UI.Screens;

namespace SuicideBurn
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class SuicideBurnAddon : MonoBehaviour
    {
        private AutoLandController controller;
        private SuicideBurnUI ui;
        private ApplicationLauncherButton appButton;
        private Texture2D buttonTexture;

        public void Start()
        {
            controller = new AutoLandController(this);
            ui = new SuicideBurnUI(controller);
            TryCreateToolbarButton();
        }

        public void Update()
        {
            if (appButton == null && ApplicationLauncher.Ready)
            {
                TryCreateToolbarButton();
            }
        }

        public void OnGUI()
        {
            ui?.OnGUI();
        }

        public void OnDestroy()
        {
            if (appButton != null && ApplicationLauncher.Instance != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appButton);
                appButton = null;
            }

            controller?.Abort();
        }

        private void TryCreateToolbarButton()
        {
            if (!ApplicationLauncher.Ready || ApplicationLauncher.Instance == null)
            {
                return;
            }

            if (buttonTexture == null)
            {
                buttonTexture = BuildDefaultTexture();
            }

            appButton = ApplicationLauncher.Instance.AddModApplication(
                OnToggleOn,
                OnToggleOff,
                null,
                null,
                null,
                null,
                ApplicationLauncher.AppScenes.FLIGHT,
                buttonTexture);
        }

        private void OnToggleOn()
        {
            if (ui != null)
            {
                ui.IsVisible = true;
            }
        }

        private void OnToggleOff()
        {
            if (ui != null)
            {
                ui.IsVisible = false;
            }
        }

        private static Texture2D BuildDefaultTexture()
        {
            var tex = new Texture2D(38, 38, TextureFormat.ARGB32, false);
            var fill = new Color(0.9f, 0.25f, 0.2f, 1f);
            var colors = new Color[38 * 38];

            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = fill;
            }

            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }
    }
}
