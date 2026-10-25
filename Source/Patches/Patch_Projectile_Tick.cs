using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RectShield
{
    /// <summary>
    /// Postfix auf Verse.Projectile.Tick: vergleicht die letzte mit der aktuellen
    /// Position und prueft den Eintritt in ein Rechteck.
    /// Bewusst KEIN Prefix auf CheckForFreeInterceptBetween - dessen Signatur
    /// (__result-Typ) unterscheidet sich zwischen RimWorld-Versionen und wird von
    /// Harmony 3 streng geprueft. Der Tick-Pfad ist versionsstabil.
    /// </summary>
    public static class Patch_Projectile_Tick
    {
        public static void Postfix(Projectile __instance)
        {
            try
            {
                Projectile projectile = __instance;
                if (projectile == null || !projectile.Spawned || projectile.Destroyed)
                {
                    return;
                }

                Map map = projectile.Map;
                if (map == null)
                {
                    return;
                }

                var mapComp = map.GetComponent<RectShieldMapComponent>();
                if (mapComp == null || !mapComp.HasShields)
                {
                    return;
                }

                Vector3 now = projectile.DrawPos;
                Vector3 previous = mapComp.RememberAndGetPrevious(projectile, now);

                if (mapComp.TryIntercept(projectile, previous, now))
                {
                    projectile.Destroy(DestroyMode.Vanish);
                }
            }
            catch (Exception e)
            {
                RectShieldLog.ErrorOnce("projectilePatch",
                    "Projektil-Patchfehler: " + e.GetType().Name + ": " + e.Message);
            }
        }
    }
}
