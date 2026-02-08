using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BetterCaravans
{
    [HarmonyPatch(typeof(Dialog_FormCaravan), "DoWindowContents")]
    public static class Dialog_FormCaravan_Patch
    {
        public static void Postfix(Dialog_FormCaravan __instance, object[] __args)
        {
            if (!BetterCaravansMod.Settings.enableInstantFormationButton || __args == null || __args.Length == 0)
            {
                return;
            }

            if (!(__args[0] is Rect inRect))
            {
                return;
            }

            const float buttonWidth = 220f;
            const float buttonHeight = 32f;
            float x = inRect.x + (inRect.width - buttonWidth) / 2f;
            float y = inRect.y + inRect.height - buttonHeight - 42f;
            Rect buttonRect = new Rect(x, y, buttonWidth, buttonHeight);

            TooltipHandler.TipRegion(buttonRect, "Instantly finalize and launch the caravan if it is safe to do so.");

            if (Widgets.ButtonText(buttonRect, "Instant Form Caravan"))
            {
                InstantCaravanFormController.TryInstantForm(__instance);
            }
        }
    }
}
