using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BetterCaravans
{
    [HarmonyPatch(typeof(Dialog_FormCaravan), "DoBottomButtons", new[] { typeof(Rect) })]
    public static class Dialog_FormCaravan_Patch
    {
        private const float DefaultButtonWidth = 160f;
        private const float DefaultButtonHeight = 35f;
        private const float ButtonGap = 10f;

        public static void Postfix(Dialog_FormCaravan __instance, Rect rect)
        {
            if (!BetterCaravansMod.Settings.enableInstantFormationButton)
            {
                return;
            }

            if (BetterCaravansMod.Settings.autoSwitchFoodRestriction)
            {
                CaravanFoodRestrictionController.EnsureCaravanPolicy();
            }

            Rect sendRect;
            if (!FormCaravanSendButtonTracker.TryGetSendButtonRect(out sendRect))
            {
                float buttonHeight = Mathf.Min(DefaultButtonHeight, rect.height);
                float buttonY = rect.yMax - buttonHeight;
                sendRect = new Rect(rect.xMax - DefaultButtonWidth, buttonY, DefaultButtonWidth, buttonHeight);
            }

            const string label = "Send Instantly";
            float desiredWidth = Mathf.Max(DefaultButtonWidth, Text.CalcSize(label).x + 20f);
            float availableWidth = sendRect.x - ButtonGap - rect.x;
            if (availableWidth <= 0f)
            {
                return;
            }
            float buttonWidth = Mathf.Min(desiredWidth, availableWidth);

            Rect buttonRect = new Rect(sendRect.x - ButtonGap - buttonWidth, sendRect.y, buttonWidth, sendRect.height);
            TooltipHandler.TipRegion(buttonRect, "Instantly finalize and launch the caravan if it is safe to do so.");

            if (Widgets.ButtonText(buttonRect, label))
            {
                if (InstantCaravanFormController.TryInstantForm(__instance))
                {
                    __instance.Close(true);
                }
            }
        }
    }
}
