using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BetterCaravans
{
    public static class CaravanFoodRestrictionController
    {
        private static readonly Dictionary<Pawn, FoodPolicy> PreviousPolicies = new Dictionary<Pawn, FoodPolicy>();
        private static bool didSwitchThisLaunch;

        public static void HandleBeforeCaravanCreation(object[] args)
        {
            BetterCaravansSettings settings = BetterCaravansMod.Settings;
            if (!settings.autoSwitchFoodRestriction)
            {
                return;
            }

            if (settings.onlySwitchOnInstant && !InstantCaravanFormController.InstantLaunchInProgress)
            {
                return;
            }

            List<Pawn> pawns = ExtractPawns(args);
            if (pawns == null || pawns.Count == 0)
            {
                return;
            }

            FoodPolicy caravanPolicy = GetOrCreateCaravanPolicy();
            if (caravanPolicy == null)
            {
                return;
            }

            didSwitchThisLaunch = false;
            foreach (Pawn pawn in pawns)
            {
                if (pawn?.foodRestriction == null)
                {
                    continue;
                }

                if (settings.restorePreviousRestriction)
                {
                    PreviousPolicies[pawn] = pawn.foodRestriction.CurrentFoodPolicy;
                }

                pawn.foodRestriction.CurrentFoodPolicy = caravanPolicy;
                didSwitchThisLaunch = true;
            }
        }

        public static void HandleAfterCaravanCreation()
        {
            if (!didSwitchThisLaunch)
            {
                PreviousPolicies.Clear();
                return;
            }

            if (!BetterCaravansMod.Settings.restorePreviousRestriction)
            {
                PreviousPolicies.Clear();
                return;
            }

            foreach (KeyValuePair<Pawn, FoodPolicy> entry in PreviousPolicies.ToList())
            {
                Pawn pawn = entry.Key;
                if (pawn?.foodRestriction == null)
                {
                    continue;
                }

                pawn.foodRestriction.CurrentFoodPolicy = entry.Value;
            }

            PreviousPolicies.Clear();
        }

        private static FoodPolicy GetOrCreateCaravanPolicy()
        {
            FoodRestrictionDatabase database = Current.Game?.foodRestrictionDatabase;
            if (database == null)
            {
                return null;
            }

            FoodPolicy existing = database.AllFoodRestrictions.FirstOrDefault(r => r.label == "Caravan");
            if (existing != null)
            {
                return existing;
            }

            FoodPolicy created = database.MakeNewFoodRestriction();
            created.label = "Caravan";
            return created;
        }

        private static List<Pawn> ExtractPawns(object[] args)
        {
            if (args == null)
            {
                return null;
            }

            foreach (object arg in args)
            {
                if (arg is List<Pawn> pawnList)
                {
                    return pawnList;
                }

                if (arg is IEnumerable<Pawn> pawnEnumerable)
                {
                    return pawnEnumerable.ToList();
                }
            }

            return null;
        }
    }
}
