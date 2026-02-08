using System;
using System.Linq;
using System.Reflection;
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
            try
            {
                PatchAllSafely(HarmonyInstance);
            }
            catch (Exception e)
            {
                Log.Error("[BetterCaravans] Harmony patching failed: " + e);
            }
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

        private static void PatchAllSafely(Harmony harmony)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(HarmonyPatch), true).Any()))
            {
                try
                {
                    harmony.CreateClassProcessor(type).Patch();
                }
                catch (Exception e)
                {
                    Log.Error("[BetterCaravans] Harmony patch failed for " + type.FullName + ": " + e);
                }
            }
        }
    }
}
