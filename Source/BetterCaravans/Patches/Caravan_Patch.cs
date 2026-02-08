using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace BetterCaravans
{
    [HarmonyPatch(typeof(Caravan), "AddPawn", new[] { typeof(Pawn), typeof(bool) })]
    public static class Patch_Caravan_AddPawn
    {
        public static void Postfix(Caravan __instance, Pawn p, bool addCarriedPawnToWorldPawnsIfAny)
        {
            CaravanFoodRestrictionController.HandlePawnJoinedCaravan(__instance, p);
        }
    }

    [HarmonyPatch(typeof(Caravan), "AddPawnOrItem", new[] { typeof(Thing), typeof(bool) })]
    public static class Patch_Caravan_AddPawnOrItem
    {
        public static void Postfix(Caravan __instance, Thing thing, bool addCarriedPawnToWorldPawnsIfAny)
        {
            if (thing is Pawn pawn)
            {
                CaravanFoodRestrictionController.HandlePawnJoinedCaravan(__instance, pawn);
            }
        }
    }

    [HarmonyPatch(typeof(CaravanMaker), "MakeCaravan", new[] { typeof(IEnumerable<Pawn>), typeof(Faction), typeof(PlanetTile), typeof(bool) })]
    public static class Patch_CaravanMaker_MakeCaravan
    {
        public static void Postfix(Caravan __result, IEnumerable<Pawn> __0, Faction __1, PlanetTile __2, bool __3)
        {
            if (__0 == null)
            {
                return;
            }

            foreach (Pawn pawn in __0)
            {
                CaravanFoodRestrictionController.HandlePawnJoinedCaravan(__result, pawn);
            }
        }
    }

    [HarmonyPatch(typeof(Caravan), "RemovePawn", new[] { typeof(Pawn) })]
    public static class Patch_Caravan_RemovePawn
    {
        public static void Postfix(Caravan __instance, Pawn p)
        {
            CaravanFoodRestrictionController.HandlePawnLeftCaravan(__instance, p);
        }
    }
}
