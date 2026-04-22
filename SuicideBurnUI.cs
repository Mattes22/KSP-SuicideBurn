using System;
using UnityEngine;

namespace SuicideBurn
{
    public class SuicideBurnUI
    {
        private readonly AutoLandController controller;
        private Rect windowRect = new Rect(350, 120, 260, 200);
        private bool isVisible = true;

        public SuicideBurnUI(AutoLandController autoLandController)
        {
            controller = autoLandController;
        }

        public bool IsVisible
        {
            get => isVisible;
            set => isVisible = value;
        }

        public void OnGUI()
        {
            if (!isVisible || controller == null || HighLogic.LoadedScene != GameScenes.FLIGHT)
            {
                return;
            }

            windowRect = GUILayout.Window(
                GetWindowId(),
                windowRect,
                DrawWindow,
                "SuicideBurn");
        }

        private void DrawWindow(int id)
        {
            var impactTime = controller.GetImpactTime();
            var burnAltitude = controller.GetSuicideBurnAltitude();
            var verticalSpeed = controller.GetVerticalSpeed();
            var twr = controller.GetTwr();

            GUILayout.Label($"Phase: {controller.Phase}");
            GUILayout.Label($"Impact: {FormatSeconds(impactTime)}");
            GUILayout.Label($"Burn Alt: {FormatMeters(burnAltitude)}");
            GUILayout.Label($"VSpeed: {verticalSpeed:F1} m/s");
            GUILayout.Label($"TWR: {twr:F2}");
            GUILayout.Space(6);

            GUI.enabled = !controller.IsActive;
            if (GUILayout.Button("Activate SuicideBurn"))
            {
                controller.Activate();
            }

            GUI.enabled = controller.IsActive;
            if (GUILayout.Button("Abort"))
            {
                controller.Abort();
            }

            GUI.enabled = true;
            GUI.DragWindow();
        }

        private static int GetWindowId()
        {
            return typeof(SuicideBurnUI).FullName.GetHashCode();
        }

        private static string FormatSeconds(double value)
        {
            if (double.IsInfinity(value) || value > 9999d)
            {
                return "--";
            }

            return $"{Math.Max(0d, value):F1} s";
        }

        private static string FormatMeters(double value)
        {
            if (double.IsInfinity(value))
            {
                return "NO TWR";
            }

            return $"{Math.Max(0d, value):F0} m";
        }
    }
}
