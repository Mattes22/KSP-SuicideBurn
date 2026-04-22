using System.Linq;

namespace SuicideBurn
{
    public static class TerrainScanner
    {
        public static double GetTerrainHeight(Vessel vessel)
        {
            if (vessel == null)
            {
                return 0d;
            }

            return vessel.terrainAltitude;
        }

        public static double GetAltitudeAboveTerrain(Vessel vessel)
        {
            if (vessel == null)
            {
                return 0d;
            }

            return vessel.altitude - vessel.terrainAltitude;
        }

        public static bool BodyHasAtmosphere(Vessel vessel)
        {
            return vessel?.mainBody?.atmosphere ?? false;
        }

        public static bool HasParachutes(Vessel vessel)
        {
            if (vessel == null)
            {
                return false;
            }

            return vessel.parts.Any(part => part.Modules.Contains("ModuleParachute"));
        }

        public static bool HasSufficientParachutes(Vessel vessel)
        {
            if (vessel == null)
            {
                return false;
            }

            var parachuteCount = vessel.parts.Count(part => part.Modules.Contains("ModuleParachute"));
            if (parachuteCount <= 0)
            {
                return false;
            }

            // Conservative simple heuristic for early deployment guidance.
            var requiredCount = System.Math.Max(1, (int)System.Math.Ceiling(vessel.totalMass / 5d));
            return parachuteCount >= requiredCount;
        }
    }
}
