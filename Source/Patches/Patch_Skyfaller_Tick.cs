using System;
using RimWorld;
using Verse;

namespace RectShield
{
    /// <summary>
    /// Postfix auf RimWorld.Skyfaller.Tick. Greift nur bei Stufen mit
    /// blocksSkyfallers = true (Mk2/Mk3). Mk1 laesst Drop Pods absichtlich durch.
    /// </summary>
    public static class Patch_Skyfaller_Tick
    {
        public static void Postfix(Skyfaller __instance)
        {
            try
            {
                Skyfaller skyfaller = __instance;
                if (skyfaller == null || !skyfaller.Spawned || skyfaller.Destroyed)
                {
                    return;
                }

                Map map = skyfaller.Map;
                if (map == null)
                {
                    return;
                }

                var mapComp = map.GetComponent<RectShieldMapComponent>();
                if (mapComp == null || !mapComp.HasShields)
                {
                    return;
                }

                if (mapComp.TryBlockSkyfaller(skyfaller, skyfaller.Position))
                {
                    skyfaller.Destroy(DestroyMode.Vanish);
                }
            }
            catch (Exception e)
            {
                RectShieldLog.ErrorOnce("skyfallerPatch",
                    "Skyfaller-Patchfehler: " + e.GetType().Name + ": " + e.Message);
            }
        }
    }
}
