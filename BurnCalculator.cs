using System;
using System.Linq;
using UnityEngine;

namespace SuicideBurn
{
    public static class BurnCalculator
    {
        public static double CalculateSuicideBurnAltitude(Vessel vessel)
        {
            if (vessel == null)
            {
                return 0d;
            }

            var verticalSpeed = Math.Max(0d, -vessel.verticalSpeed);
            var availableAcceleration = GetAvailableNetDeceleration(vessel);

            if (availableAcceleration <= 0d)
            {
                return double.PositiveInfinity;
            }

            var stoppingDistance = (verticalSpeed * verticalSpeed) / (2d * availableAcceleration);
            var safetyMargin = Math.Max(20d, verticalSpeed * 0.5d);
            return stoppingDistance + safetyMargin;
        }

        public static double CalculateImpactTime(Vessel vessel)
        {
            if (vessel == null)
            {
                return double.PositiveInfinity;
            }

            var altitudeAboveTerrain = TerrainScanner.GetAltitudeAboveTerrain(vessel);
            if (altitudeAboveTerrain <= 0d)
            {
                return 0d;
            }

            var verticalSpeed = -vessel.verticalSpeed;
            if (verticalSpeed <= 0.1d)
            {
                return double.PositiveInfinity;
            }

            return altitudeAboveTerrain / verticalSpeed;
        }

        public static double CalculateBurnDuration(Vessel vessel, double deltaV)
        {
            if (vessel == null || deltaV <= 0d)
            {
                return 0d;
            }

            var thrust = GetAvailableThrust(vessel);
            var mass = Math.Max(vessel.totalMass, 0.001);
            if (thrust <= 0d)
            {
                return double.PositiveInfinity;
            }

            var acceleration = thrust / mass;
            return deltaV / acceleration;
        }

        public static double CalculateCurrentTwr(Vessel vessel)
        {
            if (vessel == null)
            {
                return 0d;
            }

            var gravity = GetSurfaceGravity(vessel);
            if (gravity <= 0d)
            {
                return 0d;
            }

            var weight = vessel.totalMass * gravity;
            if (weight <= 0d)
            {
                return 0d;
            }

            return GetAvailableThrust(vessel) / weight;
        }

        public static double GetSurfaceGravity(Vessel vessel)
        {
            return vessel?.mainBody?.GeeASL * PhysicsGlobals.GravitationalAcceleration ?? 0d;
        }

        private static double GetAvailableNetDeceleration(Vessel vessel)
        {
            var thrust = GetAvailableThrust(vessel);
            var mass = Math.Max(vessel.totalMass, 0.001);
            var gravity = GetSurfaceGravity(vessel);
            var thrustAcceleration = thrust / mass;
            return thrustAcceleration - gravity;
        }

        private static double GetAvailableThrust(Vessel vessel)
        {
            if (vessel == null)
            {
                return 0d;
            }

            return vessel.parts
                .SelectMany(part => part.Modules.OfType<ModuleEngines>())
                .Where(engine => engine.isEnabled)
                .Sum(engine => (double)engine.maxThrust);
        }
    }
}
