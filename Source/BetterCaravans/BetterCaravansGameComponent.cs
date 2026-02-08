using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace BetterCaravans
{
    public class BetterCaravansGameComponent : GameComponent
    {
        private Dictionary<int, int> pawnPolicyMap = new Dictionary<int, int>();
        private readonly HashSet<int> pendingRestoreTiles = new HashSet<int>();

        public BetterCaravansGameComponent(Game game)
        {
        }

        public override void StartedNewGame()
        {
            CaravanFoodRestrictionController.EnsureCaravanPolicy();
        }

        public override void LoadedGame()
        {
            CaravanFoodRestrictionController.EnsureCaravanPolicy();
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref pawnPolicyMap, "pawnPolicyMap", LookMode.Value, LookMode.Value);
            if (pawnPolicyMap == null)
            {
                pawnPolicyMap = new Dictionary<int, int>();
            }
        }

        public void RememberPolicy(Pawn pawn, FoodPolicy policy)
        {
            if (pawn == null || policy == null)
            {
                return;
            }

            int pawnId = pawn.thingIDNumber;
            if (!pawnPolicyMap.ContainsKey(pawnId))
            {
                pawnPolicyMap[pawnId] = policy.id;
            }
        }

        public bool TryGetAndClearPolicy(Pawn pawn, out FoodPolicy policy)
        {
            policy = null;
            if (pawn == null)
            {
                return false;
            }

            int pawnId = pawn.thingIDNumber;
            if (!pawnPolicyMap.TryGetValue(pawnId, out int policyId))
            {
                return false;
            }

            FoodRestrictionDatabase database = Current.Game?.foodRestrictionDatabase;
            if (database != null)
            {
                policy = database.AllFoodRestrictions.Find(p => p.id == policyId);
            }

            if (policy == null)
            {
                return false;
            }

            pawnPolicyMap.Remove(pawnId);
            return true;
        }

        public bool TryGetStoredPolicyId(Pawn pawn, out int policyId)
        {
            policyId = -1;
            if (pawn == null)
            {
                return false;
            }
            return pawnPolicyMap.TryGetValue(pawn.thingIDNumber, out policyId);
        }

        public void QueueRestore(MapParent mapParent, Caravan caravan)
        {
            if (mapParent == null || caravan == null)
            {
                return;
            }

            int tile = mapParent.Tile;
            pendingRestoreTiles.Add(tile);

        }

        public override void GameComponentTick()
        {
            if (pendingRestoreTiles.Count == 0)
            {
                return;
            }

            if (Find.TickManager.TicksGame % 30 != 0)
            {
                return;
            }

            foreach (int tile in pendingRestoreTiles.ToList())
            {
                Map map = Find.Maps.FirstOrDefault(m => m.Tile == tile);
                if (map == null)
                {
                    continue;
                }

                MapParent mapParent = map.Parent;
                bool isPlayerHome = map.IsPlayerHome && mapParent is Settlement settlement && settlement.Faction == Faction.OfPlayer;
                if (!isPlayerHome)
                {
                    pendingRestoreTiles.Remove(tile);
                    continue;
                }

                List<Pawn> pawnsOnMap = map.mapPawns.AllPawnsSpawned.Where(p => p.Faction == Faction.OfPlayer).ToList();

                foreach (Pawn pawn in pawnsOnMap)
                {
                    bool stored = TryGetStoredPolicyId(pawn, out int storedPolicyId);
                    if (stored && Current.Game?.foodRestrictionDatabase != null)
                    {
                        _ = Current.Game.foodRestrictionDatabase.AllFoodRestrictions.Find(p => p.id == storedPolicyId);
                    }

                    if (pawn.IsCaravanMember())
                    {
                        continue;
                    }

                    if (pawn.foodRestriction == null)
                    {
                        continue;
                    }

                    if (TryGetAndClearPolicy(pawn, out FoodPolicy previous))
                    {
                        FoodPolicy before = pawn.foodRestriction.CurrentFoodPolicy;
                        pawn.foodRestriction.CurrentFoodPolicy = previous;
                    }
                }

                pendingRestoreTiles.Remove(tile);
            }
        }
    }
}
