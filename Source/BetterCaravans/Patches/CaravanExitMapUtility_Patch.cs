using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace BetterCaravans
{
    [HarmonyPatch]
    public static class Patch_CaravanExitMapUtility_ExitMapAndCreateCaravan
    {
        static MethodBase TargetMethod()
        {
            var methods = typeof(CaravanExitMapUtility)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.Name == "ExitMapAndCreateCaravan")
                .ToList();

            MethodInfo match = methods
                .FirstOrDefault(m =>
                {
                    ParameterInfo[] parameters = m.GetParameters();
                    if (parameters.Length != 6)
                    {
                        return false;
                    }
                    return parameters[0].ParameterType == typeof(IEnumerable<Pawn>) &&
                           parameters[1].ParameterType == typeof(Faction) &&
                           parameters[2].ParameterType == typeof(PlanetTile) &&
                           parameters[3].ParameterType == typeof(Direction8Way) &&
                           parameters[4].ParameterType == typeof(PlanetTile) &&
                           parameters[5].ParameterType == typeof(bool);
                });

            return match ?? methods.OrderByDescending(m => m.GetParameters().Length).First();
        }

        public static void Prefix(IEnumerable<Pawn> pawns, Faction faction, PlanetTile exitFromTile, Direction8Way dir, PlanetTile destinationTile, bool sendMessage)
        {
            CaravanFoodRestrictionController.HandleBeforeCaravanCreation(pawns);
        }

        public static void Postfix(IEnumerable<Pawn> pawns, Faction faction, PlanetTile exitFromTile, Direction8Way dir, PlanetTile destinationTile, bool sendMessage)
        {
            CaravanFoodRestrictionController.HandleAfterCaravanCreation();
        }
    }
}
