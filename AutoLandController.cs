using System.Collections;
using System;
using UnityEngine;

namespace SuicideBurn
{
    public enum LandingPhase
    {
        Idle,
        Deorbit,
        Coast,
        SuicideBurn,
        Touchdown
    }

    public class AutoLandController
    {
        private readonly MonoBehaviour host;
        private Coroutine landingRoutine;

        public LandingPhase Phase { get; private set; } = LandingPhase.Idle;
        public bool IsActive => landingRoutine != null;

        public AutoLandController(MonoBehaviour hostBehaviour)
        {
            host = hostBehaviour;
        }

        public void Activate()
        {
            if (IsActive || FlightGlobals.ActiveVessel == null)
            {
                return;
            }

            landingRoutine = host.StartCoroutine(LandingCoroutine());
        }

        public void Abort()
        {
            if (landingRoutine != null)
            {
                host.StopCoroutine(landingRoutine);
                landingRoutine = null;
            }

            var vessel = FlightGlobals.ActiveVessel;
            if (vessel != null)
            {
                vessel.ctrlState.mainThrottle = 0f;
                FlightInputHandler.state.mainThrottle = 0f;
                vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, false);
            }

            Phase = LandingPhase.Idle;
        }

        public double GetImpactTime()
        {
            return BurnCalculator.CalculateImpactTime(FlightGlobals.ActiveVessel);
        }

        public double GetSuicideBurnAltitude()
        {
            return BurnCalculator.CalculateSuicideBurnAltitude(FlightGlobals.ActiveVessel);
        }

        public double GetVerticalSpeed()
        {
            var vessel = FlightGlobals.ActiveVessel;
            return vessel?.verticalSpeed ?? 0d;
        }

        public double GetTwr()
        {
            return BurnCalculator.CalculateCurrentTwr(FlightGlobals.ActiveVessel);
        }

        private IEnumerator LandingCoroutine()
        {
            var vessel = FlightGlobals.ActiveVessel;
            if (vessel == null)
            {
                Abort();
                yield break;
            }

            vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);

            Phase = LandingPhase.Deorbit;
            yield return host.StartCoroutine(DoDeorbitBurn(vessel));

            Phase = LandingPhase.Coast;
            while (true)
            {
                var altitudeAboveTerrain = TerrainScanner.GetAltitudeAboveTerrain(vessel);
                var burnAltitude = BurnCalculator.CalculateSuicideBurnAltitude(vessel);

                if (ShouldDeployParachutes(vessel, altitudeAboveTerrain))
                {
                    vessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, true);
                    vessel.ActionGroups.SetGroup(KSPActionGroup.Gear, true);
                    vessel.ActionGroups.SetGroup(KSPActionGroup.Custom01, true);
                }

                if (altitudeAboveTerrain <= burnAltitude)
                {
                    break;
                }

                if (vessel.LandedOrSplashed)
                {
                    Phase = LandingPhase.Touchdown;
                    CompleteLanding(vessel);
                    yield break;
                }

                yield return null;
            }

            Phase = LandingPhase.SuicideBurn;
            while (!vessel.LandedOrSplashed)
            {
                PointRetrograde(vessel);
                vessel.ctrlState.mainThrottle = 1f;
                FlightInputHandler.state.mainThrottle = 1f;

                if (TerrainScanner.GetAltitudeAboveTerrain(vessel) < 2d)
                {
                    break;
                }

                yield return null;
            }

            Phase = LandingPhase.Touchdown;
            CompleteLanding(vessel);
        }

        private IEnumerator DoDeorbitBurn(Vessel vessel)
        {
            PointRetrograde(vessel);
            var targetPeriapsis = TerrainScanner.BodyHasAtmosphere(vessel) ? vessel.mainBody.atmosphereDepth * 0.8 : 10000d;

            while (vessel.orbit.PeA > targetPeriapsis && !vessel.LandedOrSplashed)
            {
                PointRetrograde(vessel);
                vessel.ctrlState.mainThrottle = 0.3f;
                FlightInputHandler.state.mainThrottle = 0.3f;
                yield return null;
            }

            vessel.ctrlState.mainThrottle = 0f;
            FlightInputHandler.state.mainThrottle = 0f;
        }

        private void PointRetrograde(Vessel vessel)
        {
            if (vessel.Autopilot != null)
            {
                vessel.Autopilot.SetMode(VesselAutopilot.AutopilotMode.Retrograde);
            }
        }

        private bool ShouldDeployParachutes(Vessel vessel, double altitudeAboveTerrain)
        {
            if (!TerrainScanner.BodyHasAtmosphere(vessel) || !TerrainScanner.HasParachutes(vessel))
            {
                return false;
            }

            if (!TerrainScanner.HasSufficientParachutes(vessel))
            {
                return false;
            }

            return altitudeAboveTerrain < 5000d;
        }

        private void CompleteLanding(Vessel vessel)
        {
            vessel.ctrlState.mainThrottle = 0f;
            FlightInputHandler.state.mainThrottle = 0f;
            landingRoutine = null;
        }
    }
}
