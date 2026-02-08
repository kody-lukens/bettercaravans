using HarmonyLib;
using RimWorld.Planet;

namespace BetterCaravans
{
    [HarmonyPatch(typeof(CaravanExitMapUtility), "ExitMapAndCreateCaravan")]
    public static class CaravanExitMapUtility_Patch
    {
        public static void Prefix(object[] __args)
        {
            CaravanFoodRestrictionController.HandleBeforeCaravanCreation(__args);
        }

        public static void Postfix()
        {
            CaravanFoodRestrictionController.HandleAfterCaravanCreation();
        }
    }
}
