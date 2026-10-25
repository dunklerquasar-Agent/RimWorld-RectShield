using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RectShield
{
    /// <summary>
    /// Alle Patches werden explizit ueber AccessTools aufgeloest (keine
    /// [HarmonyPatch]-Attribute). So fallen geaenderte Signaturen zwischen
    /// 1.6 und 1.7 sofort als Warnung auf statt still zu scheitern.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HarmonyInit
    {
        static HarmonyInit()
        {
            var harmony = new Harmony("rectshield.mod");

            // Primaerer Pfad: jedes Projektil pro Tick gegen die Schilde pruefen.
            Patch(harmony, typeof(Projectile), "Tick",
                typeof(Patch_Projectile_Tick), nameof(Patch_Projectile_Tick.Postfix), prefix: false);

            // Mk2: Drop Pods / Meteoriten / Truemmer stoppen.
            Patch(harmony, typeof(Skyfaller), "Tick",
                typeof(Patch_Skyfaller_Tick), nameof(Patch_Skyfaller_Tick.Postfix), prefix: false);

            RectShieldLog.Debug("Harmony-Patches installiert (RimWorld " + VersionInfo.GameVersion + ").");
        }

        private static void Patch(Harmony harmony, Type targetType, string methodName,
                                  Type patchType, string patchMethod, bool prefix)
        {
            try
            {
                MethodInfo original = AccessTools.Method(targetType, methodName);
                if (original == null)
                {
                    RectShieldLog.Warn("Methode nicht gefunden: " + targetType.FullName + "." + methodName
                                       + " -> docs/API_NOTES.md pruefen.");
                    return;
                }

                MethodInfo patch = AccessTools.Method(patchType, patchMethod);
                if (patch == null)
                {
                    RectShieldLog.Error("Patch-Methode fehlt: " + patchType.FullName + "." + patchMethod);
                    return;
                }

                var harmonyMethod = new HarmonyMethod(patch);
                if (prefix)
                {
                    harmony.Patch(original, prefix: harmonyMethod);
                }
                else
                {
                    harmony.Patch(original, postfix: harmonyMethod);
                }
            }
            catch (Exception e)
            {
                RectShieldLog.Error("Patch " + targetType.Name + "." + methodName
                                    + " fehlgeschlagen: " + e);
            }
        }
    }
}
