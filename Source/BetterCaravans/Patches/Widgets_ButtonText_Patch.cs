using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace BetterCaravans
{
    [HarmonyPatch]
    public static class Patch_Widgets_ButtonText_Send
    {
        private static bool IsFormCaravanWindow()
        {
            WindowStack stack = Find.WindowStack;
            if (stack == null)
            {
                return false;
            }

            FieldInfo field = AccessTools.Field(typeof(WindowStack), "currentlyDrawnWindow");
            Window current = field?.GetValue(stack) as Window;
            return current is RimWorld.Dialog_FormCaravan;
        }

        static MethodBase TargetMethod()
        {
            return typeof(Widgets).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .First(m =>
                {
                    ParameterInfo[] p = m.GetParameters();
                    return m.Name == "ButtonText" &&
                           p.Length == 6 &&
                           p[0].ParameterType == typeof(Rect) &&
                           p[1].ParameterType == typeof(string);
                });
        }

        public static void Postfix(Rect rect, string label)
        {
            if (label != "Send")
            {
                return;
            }

            if (!IsFormCaravanWindow())
            {
                return;
            }

            FormCaravanSendButtonTracker.RecordSendButtonRect(rect);
        }
    }

    [HarmonyPatch]
    public static class Patch_Widgets_ButtonText_Send_WithColor
    {
        static MethodBase TargetMethod()
        {
            return typeof(Widgets).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .First(m =>
                {
                    ParameterInfo[] p = m.GetParameters();
                    return m.Name == "ButtonText" &&
                           p.Length == 7 &&
                           p[0].ParameterType == typeof(Rect) &&
                           p[1].ParameterType == typeof(string);
                });
        }

        public static void Postfix(Rect rect, string label)
        {
            if (label != "Send")
            {
                return;
            }

            WindowStack stack = Find.WindowStack;
            if (stack == null)
            {
                return;
            }

            FieldInfo field = AccessTools.Field(typeof(WindowStack), "currentlyDrawnWindow");
            Window current = field?.GetValue(stack) as Window;
            if (current is RimWorld.Dialog_FormCaravan)
            {
                FormCaravanSendButtonTracker.RecordSendButtonRect(rect);
            }
        }
    }
}
