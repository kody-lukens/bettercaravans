using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace BetterCaravans
{
    [HarmonyPatch]
    public static class Patch_CaravanEnterMapUtility_Enter_Mode
    {
        static MethodBase TargetMethod()
        {
            return typeof(CaravanEnterMapUtility)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .First(m =>
                {
                    ParameterInfo[] p = m.GetParameters();
                    return m.Name == "Enter" &&
                           p.Length == 6 &&
                           p[0].ParameterType == typeof(Caravan) &&
                           p[1].ParameterType == typeof(Map) &&
                           p[2].ParameterType == typeof(CaravanEnterMode) &&
                           p[3].ParameterType == typeof(CaravanDropInventoryMode) &&
                           p[4].ParameterType == typeof(bool);
                });
        }

        public static void Postfix(Caravan caravan, Map map, CaravanEnterMode enterMode, CaravanDropInventoryMode dropInventoryMode, bool draftColonists, Predicate<IntVec3> enterCellValidator)
        {
            CaravanFoodRestrictionController.RestorePoliciesForPlayerSettlement(caravan, map);
        }
    }

    [HarmonyPatch]
    public static class Patch_CaravanEnterMapUtility_Enter_Custom
    {
        static MethodBase TargetMethod()
        {
            return typeof(CaravanEnterMapUtility)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .First(m =>
                {
                    ParameterInfo[] p = m.GetParameters();
                    return m.Name == "Enter" &&
                           p.Length == 5 &&
                           p[0].ParameterType == typeof(Caravan) &&
                           p[1].ParameterType == typeof(Map) &&
                           p[2].ParameterType == typeof(Func<Pawn, IntVec3>) &&
                           p[3].ParameterType == typeof(CaravanDropInventoryMode) &&
                           p[4].ParameterType == typeof(bool);
                });
        }

        public static void Postfix(Caravan caravan, Map map, Func<Pawn, IntVec3> enterCellGetter, CaravanDropInventoryMode dropInventoryMode, bool draftColonists)
        {
            CaravanFoodRestrictionController.RestorePoliciesForPlayerSettlement(caravan, map);
        }
    }
}
