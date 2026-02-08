using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace BetterCaravans
{
    [HarmonyPatch]
    public static class Patch_CaravanArrivalAction_Enter_Arrived
    {
        static MethodBase TargetMethod()
        {
            return typeof(CaravanArrivalAction_Enter)
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .First(m =>
                {
                    ParameterInfo[] p = m.GetParameters();
                    return m.Name == "Arrived" && p.Length == 1 && p[0].ParameterType == typeof(Caravan);
                });
        }

        public static void Postfix(CaravanArrivalAction_Enter __instance, Caravan caravan)
        {
            MapParent mapParent = AccessTools.Field(typeof(CaravanArrivalAction_Enter), "mapParent")?.GetValue(__instance) as MapParent;

            CaravanFoodRestrictionController.RestorePoliciesForPlayerSettlement(caravan, mapParent);
        }
    }
}
