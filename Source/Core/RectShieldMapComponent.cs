using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RectShield
{
    /// <summary>
    /// Pro Map: Registry aller aktiven Schilde + letzte Projektpositionen.
    /// Verhindert, dass pro Projektil alle Things der Map durchsucht werden.
    /// </summary>
    public class RectShieldMapComponent : MapComponent
    {
        private readonly List<CompRectShield> shields = new List<CompRectShield>();
        private readonly Dictionary<Projectile, Vector3> lastPositions = new Dictionary<Projectile, Vector3>();
        private int cleanupCounter;

        public RectShieldMapComponent(Map map) : base(map)
        {
        }

        public bool HasShields => shields.Count > 0;

        public void Register(CompRectShield comp)
        {
            if (comp != null && !shields.Contains(comp))
            {
                shields.Add(comp);
            }
        }

        public void Unregister(CompRectShield comp)
        {
            shields.Remove(comp);
        }

        /// <summary>Merkt die aktuelle Position und liefert die vorherige zurueck.</summary>
        public Vector3 RememberAndGetPrevious(Projectile projectile, Vector3 now)
        {
            Vector3 previous;
            if (!lastPositions.TryGetValue(projectile, out previous))
            {
                previous = now;
            }

            lastPositions[projectile] = now;
            return previous;
        }

        /// <summary>
        /// Zentrale Abfangpruefung fuer Projektile. Gibt true zurueck, wenn ein Schild
        /// das Projektil gestoppt hat (Energie wurde dann bereits abgezogen).
        /// </summary>
        public bool TryIntercept(Projectile projectile, Vector3 from, Vector3 to)
        {
            if (!HasShields || projectile == null)
            {
                return false;
            }

            IntVec3 fromCell = RectShieldUtil.ToIntVec3(from);
            IntVec3 toCell = RectShieldUtil.ToIntVec3(to);

            if (fromCell == toCell)
            {
                return false;
            }

            bool indirect = RectShieldUtil.IsIndirectFire(projectile);
            float damage = RectShieldUtil.EstimateDamage(projectile);

            // Bei mehreren Schildern gewinnt der erste Treffer in der Registry.
            // Dadurch wird die Energie nur einmal abgezogen (F11).
            for (int i = 0; i < shields.Count; i++)
            {
                CompRectShield shield = shields[i];
                if (shield == null || !shield.Active)
                {
                    continue;
                }

                CompProperties_RectShield properties = shield.Props;
                if (!properties.blocksShots)
                {
                    continue;
                }

                if (indirect && !properties.blocksIndirectFire)
                {
                    continue;
                }

                if (shield.Shape.BlocksEntry(fromCell, toCell, shield.PerimeterOnly))
                {
                    shield.Notify_Impact(damage, toCell);
                    RectShieldLog.Debug("Projektil gestoppt bei " + toCell
                                        + " (" + (indirect ? "indirekt" : "direkt") + ")");
                    return true;
                }
            }

            return false;
        }

        /// <summary>Mk2: Drop Pods, Meteoriten, Truemmer ueber dem Feld stoppen.</summary>
        public bool TryBlockSkyfaller(Skyfaller skyfaller, IntVec3 cell)
        {
            if (!HasShields || skyfaller == null)
            {
                return false;
            }

            bool hostile = IsHostile(skyfaller);
            bool blockFriendly = RectShieldMod.Settings != null
                                 && RectShieldMod.Settings.BlockFriendlyPods;

            for (int i = 0; i < shields.Count; i++)
            {
                CompRectShield shield = shields[i];
                if (shield == null || !shield.Active || !shield.Props.blocksSkyfallers)
                {
                    continue;
                }

                if (!hostile && !blockFriendly)
                {
                    continue;
                }

                if (shield.Shape.Contains(cell))
                {
                    shield.Notify_SkyfallerBlocked(cell);
                    RectShieldLog.Debug("Skyfaller gestoppt bei " + cell);
                    return true;
                }
            }

            return false;
        }

        private static bool IsHostile(Thing thing)
        {
            try
            {
                Faction faction = thing.Faction;
                return faction != null
                       && Faction.OfPlayer != null
                       && faction.HostileTo(Faction.OfPlayer);
            }
            catch (Exception)
            {
                return true; // Im Zweifel lieber blocken.
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            // Alle 10 Sekunden (600 Ticks) verwaiste Projektileintraege entfernen.
            if (++cleanupCounter < 600)
            {
                return;
            }

            cleanupCounter = 0;
            try
            {
                var stale = new List<Projectile>();
                foreach (KeyValuePair<Projectile, Vector3> entry in lastPositions)
                {
                    Projectile projectile = entry.Key;
                    if (projectile == null || projectile.Destroyed || !projectile.Spawned)
                    {
                        stale.Add(projectile);
                    }
                }

                for (int i = 0; i < stale.Count; i++)
                {
                    lastPositions.Remove(stale[i]);
                }
            }
            catch (Exception e)
            {
                RectShieldLog.ErrorOnce("cleanup", "Projektil-Cleanup fehlgeschlagen: " + e.Message);
            }
        }

        public override void MapComponentDraw()
        {
            base.MapComponentDraw();

            try
            {
                if (RectShieldMod.Settings == null
                    || !RectShieldMod.Settings.ShowPersistentOverlay
                    || !HasShields)
                {
                    return;
                }

                int drawn = 0;
                for (int i = 0; i < shields.Count && drawn < 8; i++)
                {
                    CompRectShield shield = shields[i];
                    if (shield == null || !shield.Active || !shield.ShowOverlay || shield.Shape.IsEmpty)
                    {
                        continue;
                    }

                    Color color = RectShieldUtil.ColorFor(shield.Tier, true);
                    color.a = 0.25f;
                    GenDraw.DrawFieldEdges(RectShieldUtil.ToList(shield.Shape.EdgeCells()), color);
                    drawn++;
                }
            }
            catch (Exception e)
            {
                RectShieldLog.ErrorOnce("mapdraw", "MapComponentDraw-Fehler: " + e.Message);
            }
        }
    }
}
