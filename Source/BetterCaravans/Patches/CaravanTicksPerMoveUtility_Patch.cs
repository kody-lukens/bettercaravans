using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace BetterCaravans
{
    [HarmonyPatch(typeof(CaravanTicksPerMoveUtility), "GetTicksPerMove")]
    public static class CaravanTicksPerMoveUtility_Patch
    {
        public static void Postfix(Caravan caravan, ref int __result)
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
