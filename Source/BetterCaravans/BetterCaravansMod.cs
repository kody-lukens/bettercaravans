using HarmonyLib;
using UnityEngine;
using Verse;

namespace BetterCaravans
{
    public class BetterCaravansMod : Mod
    {
        private const string HarmonyId = "kodylukens.bettercaravans";
        public static Harmony HarmonyInstance { get; private set; }
        public static BetterCaravansSettings Settings { get; private set; }

        public BetterCaravansMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<BetterCaravansSettings>();
            HarmonyInstance = new Harmony(HarmonyId);
            HarmonyInstance.PatchAll();
            Log.Message("[BetterCaravans] Loaded.");
        }

        public override string SettingsCategory()
        {
            return "Better Caravans";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoWindowContents(inRect);
            base.DoSettingsWindowContents(inRect);
        }
    }
}
