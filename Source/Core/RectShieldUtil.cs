using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RectShield
{
    public static class RectShieldUtil
    {
        public static readonly Color ShieldColorMk1 = new Color(0.30f, 0.65f, 1.00f, 0.85f);
        public static readonly Color ShieldColorMk2 = new Color(0.35f, 1.00f, 0.85f, 0.85f);
        public static readonly Color ShieldColorDown = new Color(1.00f, 0.35f, 0.30f, 0.85f);

        public static Color ColorFor(RectShieldTier tier, bool active)
        {
            if (!active)
            {
                return ShieldColorDown;
            }

            return tier == RectShieldTier.Mk2 ? ShieldColorMk2 : ShieldColorMk1;
        }

        /// <summary>World-Position auf eine Kachel runden (ohne Unity-Hilfsmethoden).</summary>
        public static IntVec3 ToIntVec3(Vector3 pos)
        {
            return new IntVec3(
                (int)Math.Round(pos.x, MidpointRounding.AwayFromZero),
                0,
                (int)Math.Round(pos.z, MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// Indirektes Feuer (Moerser) hat in Vanilla flyOverhead = true.
        /// Mk1 laesst das durch, Mk2 nicht.
        /// </summary>
        public static bool IsIndirectFire(Projectile projectile)
        {
            try
            {
                return projectile != null
                       && projectile.def != null
                       && projectile.def.projectile != null
                       && projectile.def.projectile.flyOverhead;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>Schaetzwert fuer den Energieverbrauch eines Treffers.</summary>
        public static float EstimateDamage(Projectile projectile)
        {
            try
            {
                if (projectile != null && projectile.def != null && projectile.def.projectile != null)
                {
                    float amount = projectile.def.projectile.GetDamageAmount(1f);
                    if (amount > 0f)
                    {
                        return amount;
                    }
                }
            }
            catch (Exception)
            {
                // Faellt auf den Default zurueck.
            }

            return 25f;
        }

        public static List<IntVec3> ToList(IEnumerable<IntVec3> cells)
        {
            var result = new List<IntVec3>();
            if (cells != null)
            {
                result.AddRange(cells);
            }

            return result;
        }

        public static void SpawnHitEffect(Map map, IntVec3 cell)
        {
            // Rein kosmetisch - darf niemals die Abfanglogik brechen.
            try
            {
                if (map == null)
                {
                    return;
                }

                FleckMaker.ThrowMicroSparks(cell.ToVector3(), map);
            }
            catch (Exception)
            {
                // Absichtlich ignoriert: Mot/Fleck-Defs koennen in Modpacks fehlen.
            }
        }
    }

    /// <summary>Kleiner Log-Helfer, damit Fehler im Patch nicht den Frame sprengen.</summary>
    public static class RectShieldLog
    {
        private static readonly HashSet<string> Once = new HashSet<string>();

        public static void Error(string message)
        {
            Log.Error("[RectShield] " + message);
        }

        public static void ErrorOnce(string key, string message)
        {
            if (Once.Contains(key))
            {
                return;
            }

            Once.Add(key);
            Log.Error("[RectShield] " + message + " (weitere Meldungen dieser Art werden unterdrueckt)");
        }

        public static void Warn(string message)
        {
            Log.Warning("[RectShield] " + message);
        }

        public static void Debug(string message)
        {
            if (RectShieldMod.Settings != null && RectShieldMod.Settings.DebugLogging)
            {
                Log.Message("[RectShield] " + message);
            }
        }
    }
}
