using UnityEngine;
using Verse;

namespace BetterCaravans
{
    public class BetterCaravansSettings : ModSettings
    {
        public bool enableInstantFormationButton = true;
        public bool requireNoHostilesOnMap = true;
        public bool autoSwitchFoodRestriction = true;
        public bool onlySwitchOnInstant = true;
        public bool restorePreviousRestriction = false;
        public float caravanSpeedMultiplier = 1.0f;
        public bool affectAllCaravans = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableInstantFormationButton, "enableInstantFormationButton", true);
            Scribe_Values.Look(ref requireNoHostilesOnMap, "requireNoHostilesOnMap", true);
            Scribe_Values.Look(ref autoSwitchFoodRestriction, "autoSwitchFoodRestriction", true);
            Scribe_Values.Look(ref onlySwitchOnInstant, "onlySwitchOnInstant", true);
            Scribe_Values.Look(ref restorePreviousRestriction, "restorePreviousRestriction", false);
            Scribe_Values.Look(ref caravanSpeedMultiplier, "caravanSpeedMultiplier", 1.0f);
            Scribe_Values.Look(ref affectAllCaravans, "affectAllCaravans", false);
            base.ExposeData();
        }

        public void ResetToDefaults()
        {
            enableInstantFormationButton = true;
            requireNoHostilesOnMap = true;
            autoSwitchFoodRestriction = true;
            onlySwitchOnInstant = true;
            restorePreviousRestriction = false;
            caravanSpeedMultiplier = 1.0f;
            affectAllCaravans = false;
        }

        public void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            DrawCheckbox(listing, "Enable instant formation button", ref enableInstantFormationButton,
                "Shows a button on the caravan formation screen to instantly launch the caravan.");
            DrawCheckbox(listing, "Require no hostiles on map", ref requireNoHostilesOnMap,
                "Blocks instant launch if any spawned pawn on the forming map is hostile to you.");
            DrawCheckbox(listing, "Auto-switch food restriction", ref autoSwitchFoodRestriction,
                "Sets participating pawns' food restriction to the plan named \"Caravan\" when the caravan launches.");
            DrawCheckbox(listing, "Only switch on instant formation", ref onlySwitchOnInstant,
                "If enabled, auto-switch applies only when using the instant formation button.");
            DrawCheckbox(listing, "Restore previous restriction after launch", ref restorePreviousRestriction,
                "Restores each pawn's prior food restriction after the caravan is created.");

            listing.GapLine();

            Rect sliderRect = listing.GetRect(Text.LineHeight);
            float newMultiplier = Widgets.HorizontalSlider(sliderRect, caravanSpeedMultiplier, 0.5f, 3.0f, true,
                $"Caravan speed multiplier: {caravanSpeedMultiplier:0.00}", "0.5x", "3.0x");
            if (!Mathf.Approximately(newMultiplier, caravanSpeedMultiplier))
            {
                caravanSpeedMultiplier = newMultiplier;
            }
            TooltipHandler.TipRegion(sliderRect,
                "Multiplies world caravan movement speed. Higher is faster; affects ticks-per-tile only.");

            DrawCheckbox(listing, "Affect all caravans", ref affectAllCaravans,
                "If enabled, the speed multiplier applies to all caravans, not just the player's.");

            listing.Gap();

            Rect resetRect = listing.GetRect(30f);
            if (Widgets.ButtonText(resetRect, "Reset to defaults"))
            {
                ResetToDefaults();
            }

            listing.End();
        }

        private static void DrawCheckbox(Listing_Standard listing, string label, ref bool value, string tooltip)
        {
            Rect rowRect = listing.GetRect(Text.LineHeight);
            Widgets.CheckboxLabeled(rowRect, label, ref value);
            TooltipHandler.TipRegion(rowRect, tooltip);
        }
    }
}
