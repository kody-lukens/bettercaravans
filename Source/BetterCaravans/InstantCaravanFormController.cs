using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterCaravans
{
    public static class InstantCaravanFormController
    {
        public static bool InstantLaunchInProgress { get; private set; }

        public static void TryInstantForm(object dialogInstance)
        {
            if (dialogInstance == null)
            {
                return;
            }

            Map map = TryGetMap(dialogInstance) ?? Find.CurrentMap;
            if (BetterCaravansMod.Settings.requireNoHostilesOnMap && map != null && HasHostiles(map))
            {
                Messages.Message("Cannot instantly form caravan while hostiles are present.", MessageTypeDefOf.RejectInput, false);
                Log.Warning("[BetterCaravans] Instant caravan launch blocked: hostiles present on map.");
                return;
            }

            InstantLaunchInProgress = true;
            try
            {
                InvokeInstantCaravanMethod(dialogInstance);
            }
            finally
            {
                InstantLaunchInProgress = false;
            }
        }

        private static bool InvokeInstantCaravanMethod(object dialogInstance)
        {
            Type dialogType = dialogInstance.GetType();
            MethodInfo instantMethod = dialogType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.Name.IndexOf("Instant", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                     m.GetParameters().Length == 0);
            if (instantMethod != null)
            {
                instantMethod.Invoke(dialogInstance, null);
                return true;
            }

            MethodInfo tryFormMethod = dialogType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.Name == "TryFormAndSendCaravan");
            if (tryFormMethod != null)
            {
                object[] args = BuildTryFormArgs(tryFormMethod);
                if (args == null && tryFormMethod.GetParameters().Length > 0)
                {
                    return false;
                }
                object previousInstant = TrySetDebugInstant(true);
                try
                {
                    tryFormMethod.Invoke(dialogInstance, args);
                }
                finally
                {
                    TryRestoreDebugInstant(previousInstant);
                }
                return true;
            }

            return false;
        }

        private static object[] BuildTryFormArgs(MethodInfo method)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                return null;
            }

            object[] args = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == typeof(bool))
                {
                    args[i] = true;
                }
                else if (parameters[i].HasDefaultValue)
                {
                    args[i] = parameters[i].DefaultValue;
                }
                else
                {
                    return null;
                }
            }
            return args;
        }

        private static object TrySetDebugInstant(bool value)
        {
            FieldInfo field = AccessTools.Field(typeof(DebugSettings), "instantCaravans");
            if (field == null)
            {
                return null;
            }
            object previous = field.GetValue(null);
            field.SetValue(null, value);
            return previous;
        }

        private static void TryRestoreDebugInstant(object previous)
        {
            if (previous == null)
            {
                return;
            }
            FieldInfo field = AccessTools.Field(typeof(DebugSettings), "instantCaravans");
            if (field != null)
            {
                field.SetValue(null, previous);
            }
        }

        private static Map TryGetMap(object dialogInstance)
        {
            Type dialogType = dialogInstance.GetType();
            FieldInfo field = AccessTools.Field(dialogType, "map");
            if (field != null)
            {
                return field.GetValue(dialogInstance) as Map;
            }

            PropertyInfo prop = AccessTools.Property(dialogType, "Map");
            return prop?.GetValue(dialogInstance, null) as Map;
        }

        private static bool HasHostiles(Map map)
        {
            return map.mapPawns.AllPawnsSpawned.Any(pawn =>
                pawn.Faction != null &&
                pawn.Faction.HostileTo(Faction.OfPlayer));
        }
    }
}
