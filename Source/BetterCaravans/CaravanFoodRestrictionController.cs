using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace BetterCaravans
{
    public static class CaravanFoodRestrictionController
    {
        private const string CaravanPolicyLabel = "Caravan";
        private static BetterCaravansGameComponent GameComponent => Current.Game?.GetComponent<BetterCaravansGameComponent>();
        private static Game lastEnsuredGame;
        private static bool ensuredThisGame;

        public static void EnsureCaravanPolicy()
        {
            Game currentGame = Current.Game;
            if (currentGame == null)
            {
                return;
            }

            if (currentGame != lastEnsuredGame)
            {
                lastEnsuredGame = currentGame;
                ensuredThisGame = false;
            }

            if (ensuredThisGame)
            {
                return;
            }

            if (GetOrCreateCaravanPolicy() != null)
            {
                ensuredThisGame = true;
            }
        }

        public static void HandleBeforeCaravanCreation(object[] args)
        {
            HandleBeforeCaravanCreation(ExtractPawns(args));
        }

        public static void HandleBeforeCaravanCreation(IEnumerable<Pawn> pawns)
        {
            BetterCaravansSettings settings = BetterCaravansMod.Settings;
            if (!settings.autoSwitchFoodRestriction)
            {
                return;
            }

            List<Pawn> pawnList = pawns?.ToList();
            if (pawnList == null || pawnList.Count == 0)
            {
                return;
            }

            FoodPolicy caravanPolicy = GetOrCreateCaravanPolicy();
            if (caravanPolicy == null)
            {
                return;
            }

            foreach (Pawn pawn in pawnList)
            {
                ApplyCaravanPolicy(pawn, caravanPolicy, storePrevious: true);
            }
        }

        public static void HandleAfterCaravanCreation()
        {
            // No-op: restoration now happens when a pawn leaves a caravan.
        }

        public static void HandlePawnJoinedCaravan(Caravan caravan, Pawn pawn)
        {
            BetterCaravansSettings settings = BetterCaravansMod.Settings;
            if (!settings.autoSwitchFoodRestriction)
            {
                return;
            }

            if (caravan == null || pawn == null)
            {
                return;
            }

            if (caravan.Faction != Faction.OfPlayer)
            {
                return;
            }

            FoodPolicy caravanPolicy = GetOrCreateCaravanPolicy();
            if (caravanPolicy == null)
            {
                return;
            }

            ApplyCaravanPolicy(pawn, caravanPolicy, storePrevious: true);
        }

        public static void HandlePawnLeftCaravan(Caravan caravan, Pawn pawn)
        {
            // No-op: policy switch is permanent once a pawn joins a caravan.
        }

        public static void RestorePoliciesForPlayerSettlement(Caravan caravan, Map map)
        {
            if (caravan == null || map == null)
            {
                return;
            }

            if (caravan.Faction != Faction.OfPlayer)
            {
                return;
            }

            if (!IsPlayerSettlementMap(map))
            {
                return;
            }
            QueueRestore(caravan, map.Parent);
        }

        public static void RestorePoliciesForPlayerSettlement(Caravan caravan, MapParent mapParent)
        {
            if (mapParent == null || caravan == null)
            {
                return;
            }

            QueueRestore(caravan, mapParent);
        }

        private static FoodPolicy GetOrCreateCaravanPolicy()
        {
            FoodRestrictionDatabase database = Current.Game?.foodRestrictionDatabase;
            if (database == null)
            {
                return null;
            }

            FoodPolicy existing = database.AllFoodRestrictions.FirstOrDefault(r =>
                string.Equals(r.label?.Trim(), CaravanPolicyLabel, System.StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return existing;
            }

            FoodPolicy created = database.MakeNewFoodRestriction();
            created.label = CaravanPolicyLabel;
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

        private static void ApplyCaravanPolicy(Pawn pawn, FoodPolicy caravanPolicy, bool storePrevious)
        {
            if (pawn?.foodRestriction == null)
            {
                return;
            }

            Map pawnMap = pawn.Map;
            if (pawnMap != null && pawnMap.IsPlayerHome && pawnMap.Parent is Settlement settlement && settlement.Faction == Faction.OfPlayer)
            {
                return;
            }

            if (storePrevious && GameComponent != null)
            {
                GameComponent.RememberPolicy(pawn, pawn.foodRestriction.CurrentFoodPolicy);
            }

            pawn.foodRestriction.CurrentFoodPolicy = caravanPolicy;
        }

        private static void RestorePreviousPolicy(Pawn pawn, Map map)
        {
            if (pawn?.foodRestriction == null)
            {
                return;
            }

            if (pawn.IsCaravanMember())
            {
                return;
            }

            if (GameComponent != null && GameComponent.TryGetAndClearPolicy(pawn, out FoodPolicy previous))
            {
                pawn.foodRestriction.CurrentFoodPolicy = previous;
            }
        }

        private static bool IsPlayerSettlementMap(Map map)
        {
            if (!map.IsPlayerHome)
            {
                return false;
            }

            Settlement settlement = map.Parent as Settlement;
            return settlement != null && settlement.Faction == Faction.OfPlayer;
        }

        private static void QueueRestore(Caravan caravan, MapParent mapParent)
        {
            if (GameComponent == null)
            {
                return;
            }

            if (mapParent == null)
            {
                return;
            }

            GameComponent.QueueRestore(mapParent, caravan);
        }
    }
}
