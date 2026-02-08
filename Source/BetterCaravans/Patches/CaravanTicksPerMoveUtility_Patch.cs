using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace BetterCaravans
{
    [HarmonyPatch]
    public static class Patch_CaravanTicksPerMoveUtility_GetTicksPerMove
    {
        static MethodBase TargetMethod()
        {
            var type = typeof(CaravanTicksPerMoveUtility);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.Name == "GetTicksPerMove")
                .ToList();

            return methods.First(m =>
            {
                ParameterInfo[] p = m.GetParameters();
                return p.Length == 2 &&
                       p[0].ParameterType == typeof(Caravan) &&
                       p[1].ParameterType == typeof(StringBuilder);
            });
        }

        public static void Postfix(Caravan caravan, StringBuilder explanation, ref int __result)
        {
            if (caravan == null)
            {
                return;
            }

            BetterCaravansSettings settings = BetterCaravansMod.Settings;
            if (!settings.affectAllCaravans && caravan.Faction != Faction.OfPlayer)
            {
                return;
            }

            float multiplier = Mathf.Clamp(settings.caravanSpeedMultiplier, 0.5f, 3.0f);
            if (Mathf.Approximately(multiplier, 1.0f))
            {
                return;
            }

            int adjusted = Mathf.Max(1, Mathf.RoundToInt(__result / multiplier));
            __result = adjusted;
        }
    }
}
