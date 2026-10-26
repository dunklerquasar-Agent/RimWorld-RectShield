using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RectShield
{
    /// <summary>
    /// Der eigentliche Schild: Zustand, Energie, Geometrie, Persistenz, UI.
    /// Ein Comp, drei Stufen (Mk1 fest / Mk2+Mk3 konfigurierbar) - gesteuert ueber CompProperties.
    /// </summary>
    public class CompRectShield : ThingComp
    {
        private int width;
        private int height;
        private int offsetRight;
        private int offsetForward;
        private bool perimeterOnly;
        private bool showOverlay = true;
        private float energy;
        private int rebootTicksLeft;

        private RectShieldShape shape;
        private bool shapeDirty = true;
        private bool initialized;
        private int lastPowerUpdateTick;
        private bool autoFit;
        private int autoFitCounter;
        private int hullPadding;
        private RectShieldBounds lastHull;

        public CompProperties_RectShield Props => (CompProperties_RectShield)props;

        public RectShieldTier Tier => Props.tier;

        public RectShieldShape Shape
        {
            get
            {
                if (shapeDirty || shape == null)
                {
                    RebuildShape();
                }

                return shape;
            }
        }

        public int Width => width;
        public int Height => height;
        public int EnergyPercent => Props.maxEnergy > 0f
            ? (int)Math.Round(100.0 * energy / Props.maxEnergy)
            : 0;

        public float Energy => energy;
        public bool PerimeterOnly => perimeterOnly;
        public bool AutoFit => autoFit;
        public int HullPadding => hullPadding;
        public RectShieldBounds LastHull => lastHull;
        public bool ShowOverlay => showOverlay;
        public bool IsRebooting => rebootTicksLeft > 0;

        /// <summary>Strombedarf in Watt. Mk2/Mk3: linear mit der geschuetzten Flaeche.</summary>
        public float CurrentPowerDrawW
        {
            get
            {
                float area = Shape.Area;
                float multiplier = RectShieldMod.Settings != null
                    ? RectShieldMod.Settings.WattsPerCellMultiplier
                    : 1f;
                return Props.basePowerW + Props.wattsPerCell * area * multiplier;
            }
        }

        public bool Active
        {
            get
            {
                try
                {
                    if (parent == null || !parent.Spawned || parent.Map == null)
                    {
                        return false;
                    }

                    if (energy <= 0f || rebootTicksLeft > 0)
                    {
                        return false;
                    }

                    var flick = parent.GetComp<CompFlickable>();
                    if (flick != null && !flick.SwitchIsOn)
                    {
                        return false;
                    }

                    var power = parent.GetComp<CompPowerTrader>();
                    if (power != null && !power.PowerOn)
                    {
                        return false;
                    }

                    var breakdown = parent.GetComp<CompBreakdownable>();
                    if (breakdown != null && breakdown.BrokenDown)
                    {
                        return false;
                    }

                    return true;
                }
                catch (Exception e)
                {
                    RectShieldLog.ErrorOnce("active", "Active-Check fehlgeschlagen: " + e.Message);
                    return false;
                }
            }
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            var p = Props;
            width = p.fixedSize ? p.fixedWidth : p.defaultWidth;
            height = p.fixedSize ? p.fixedHeight : p.defaultHeight;
            width = Math.Max(p.minWidth, Math.Min(p.maxWidth, width));
            height = Math.Max(p.minHeight, Math.Min(p.maxHeight, height));
            autoFit = p.autoFitSubstructure;
            hullPadding = Math.Max(0, Math.Min(4, p.substructurePadding));
            energy = p.maxEnergy;
            shapeDirty = true;
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            EnsureInitialized();
            shapeDirty = true;

            var mapComp = parent.Map?.GetComponent<RectShieldMapComponent>();
            mapComp?.Register(this);
        }

        // RimWorld 1.6/1.7: PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            var mapComp = map?.GetComponent<RectShieldMapComponent>();
            mapComp?.Unregister(this);
            base.PostDeSpawn(map, mode);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            EnsureInitialized();

            Scribe_Values.Look(ref width, "rs_width", Props.fixedSize ? Props.fixedWidth : Props.defaultWidth);
            Scribe_Values.Look(ref height, "rs_height", Props.fixedSize ? Props.fixedHeight : Props.defaultHeight);
            Scribe_Values.Look(ref offsetRight, "rs_offsetRight", 0);
            Scribe_Values.Look(ref offsetForward, "rs_offsetForward", 0);
            Scribe_Values.Look(ref perimeterOnly, "rs_perimeterOnly", false);
            Scribe_Values.Look(ref showOverlay, "rs_showOverlay", true);
            Scribe_Values.Look(ref energy, "rs_energy", Props.maxEnergy);
            Scribe_Values.Look(ref rebootTicksLeft, "rs_rebootTicksLeft", 0);
            Scribe_Values.Look(ref autoFit, "rs_autoFit", Props.autoFitSubstructure);
            Scribe_Values.Look(ref hullPadding, "rs_hullPadding",
                Math.Max(0, Math.Min(4, Props.substructurePadding)));
            hullPadding = Math.Max(0, Math.Min(4, hullPadding));

            // Alte Saves / XML-Aenderungen: immer auf gueltige Werte clampen.
            width = Math.Max(Props.minWidth, Math.Min(Props.maxWidth, width));
            height = Math.Max(Props.minHeight, Math.Min(Props.maxHeight, height));
            if (Props.fixedSize)
            {
                width = Props.fixedWidth;
                height = Props.fixedHeight;
            }

            energy = Math.Max(0f, Math.Min(Props.maxEnergy, energy));
            shapeDirty = true;
        }

        private void RebuildShape()
        {
            EnsureInitialized();

            var p = Props;

            // Gravschiff-Modus: Feld automatisch um die Substructure-Platten legen.
            // WICHTIG: Grenzen DIREKT aus den Plattengrenzen ableiten. Ein
            // Integer-Mittelpunkt (MinX+MaxX)/2 waere bei geraden Breiten eine
            // halbe Zelle zu weit westlich -> 1-2 Kacheln Versatz (Bug v0.3.0).
            if (autoFit && p.autoFitSubstructure)
            {
                lastHull = RectShieldSubstructure.Scan(parent.Map);
                if (lastHull.Found)
                {
                    int pad = Math.Max(0, Math.Min(8, hullPadding));
                    int minX = lastHull.MinX - pad;
                    int maxX = lastHull.MaxX + pad;
                    int minZ = lastHull.MinZ - pad;
                    int maxZ = lastHull.MaxZ + pad;

                    // Symmetrisch auf min/max Feldgroesse bringen (beide Seiten
                    // gleichmaessig), damit nie eine Seite weiter ragt.
                    int wantedW = maxX - minX + 1;
                    if (wantedW > p.maxWidth)
                    {
                        int excess = wantedW - p.maxWidth;
                        minX += excess / 2;
                        maxX -= excess - excess / 2;
                    }
                    else if (wantedW < p.minWidth)
                    {
                        int missing = p.minWidth - wantedW;
                        minX -= missing / 2;
                        maxX += missing - missing / 2;
                    }

                    int wantedH = maxZ - minZ + 1;
                    if (wantedH > p.maxHeight)
                    {
                        int excess = wantedH - p.maxHeight;
                        minZ += excess / 2;
                        maxZ -= excess - excess / 2;
                    }
                    else if (wantedH < p.minHeight)
                    {
                        int missing = p.minHeight - wantedH;
                        minZ -= missing / 2;
                        maxZ += missing - missing / 2;
                    }

                    shape = RectShieldShape.BuildFromWorldBounds(minX, minZ, maxX, maxZ, parent.Map);
                    shapeDirty = false;
                    UpdatePowerDraw(force: true);
                    RectShieldLog.Debug("Rumpf-Feld: " + lastHull.Cells + " Platten, "
                                        + shape.Width + "x" + shape.Height + " Kacheln, "
                                        + CurrentPowerDrawW + " W");
                    return;
                }
                // Keine Platten gefunden -> auf das manuelle Feld zurueckfallen.
            }

            var offset = new IntVec3(offsetRight, 0, offsetForward);
            shape = RectShieldShape.Build(parent.Position, parent.Rotation, width, height, offset, parent.Map);
            shapeDirty = false;
            UpdatePowerDraw(force: true);

            RectShieldLog.Debug("Shape neu gebaut: " + shape.Width + "x" + shape.Height
                                + " (" + shape.Area + " Zellen), " + CurrentPowerDrawW + " W");
        }

        private void UpdatePowerDraw(bool force = false)
        {
            if (!force && Find.TickManager != null
                       && Find.TickManager.TicksGame - lastPowerUpdateTick < 60)
            {
                return;
            }

            lastPowerUpdateTick = Find.TickManager != null ? Find.TickManager.TicksGame : 0;

            try
            {
                var power = parent.GetComp<CompPowerTrader>();
                if (power != null)
                {
                    power.PowerOutput = -CurrentPowerDrawW;
                }
            }
            catch (Exception e)
            {
                RectShieldLog.ErrorOnce("power", "PowerOutput konnte nicht gesetzt werden: " + e.Message);
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            try
            {
                EnsureInitialized();

                // Im Rumpf-Modus periodisch auf neue/entfernte Platten reagieren.
                if (autoFit && Props.autoFitSubstructure && Props.substructureRefreshTicks > 0)
                {
                    autoFitCounter++;
                    if (autoFitCounter >= Props.substructureRefreshTicks)
                    {
                        autoFitCounter = 0;
                        shapeDirty = true;
                    }
                }

                if (shapeDirty)
                {
                    RebuildShape();
                }

                UpdatePowerDraw();

                if (rebootTicksLeft > 0)
                {
                    rebootTicksLeft--;
                    if (rebootTicksLeft == 0)
                    {
                        energy = Props.maxEnergy * 0.25f;
                        RectShieldLog.Debug("Schild nach Reboot wieder online.");
                    }

                    return;
                }

                if (!Active)
                {
                    return;
                }

                if (energy < Props.maxEnergy)
                {
                    energy = Math.Min(Props.maxEnergy, energy + Props.regenPerTick);
                }
            }
            catch (Exception e)
            {
                RectShieldLog.ErrorOnce("tick", "CompTick-Fehler: " + e.Message);
            }
        }

        /// <summary>Wird von der Abfanglogik aufgerufen, wenn ein Projektil gestoppt wurde.</summary>
        public void Notify_Impact(float damage, IntVec3 hitCell)
        {
            if (!Active)
            {
                return;
            }

            float cost = damage * Props.energyPerDamage
                         * (RectShieldMod.Settings != null
                             ? RectShieldMod.Settings.EnergyPerDamageMultiplier
                             : 1f);
            energy -= cost;

            RectShieldUtil.SpawnHitEffect(parent.Map, hitCell);

            if (energy <= 0f)
            {
                energy = 0f;
                rebootTicksLeft = Props.rebootTicks;
                RectShieldLog.Debug("Schild offline - Reboot in " + Props.rebootTicks + " Ticks.");
            }
        }

        public void Notify_SkyfallerBlocked(IntVec3 cell)
        {
            if (!Active)
            {
                return;
            }

            energy -= Props.skyfallerEnergyCost;
            RectShieldUtil.SpawnHitEffect(parent.Map, cell);

            if (energy <= 0f)
            {
                energy = 0f;
                rebootTicksLeft = Props.rebootTicks;
            }
        }

        // --- Konfiguration (Mk2 + Mk3; Mk1 ist fix) ---

        public bool CanResize => !Props.fixedSize;

        public void Resize(int deltaW, int deltaH)
        {
            if (!CanResize)
            {
                return;
            }

            var p = Props;
            width = Math.Max(p.minWidth, Math.Min(p.maxWidth, width + deltaW));
            height = Math.Max(p.minHeight, Math.Min(p.maxHeight, height + deltaH));
            shapeDirty = true;
        }

        public void SetSize(int newWidth, int newHeight)
        {
            if (!CanResize)
            {
                return;
            }

            var p = Props;
            width = Math.Max(p.minWidth, Math.Min(p.maxWidth, newWidth));
            height = Math.Max(p.minHeight, Math.Min(p.maxHeight, newHeight));
            shapeDirty = true;
        }

        public void MoveOffset(int deltaRight, int deltaForward)
        {
            if (!CanResize)
            {
                return;
            }

            offsetRight = Math.Max(-20, Math.Min(20, offsetRight + deltaRight));
            offsetForward = Math.Max(-20, Math.Min(20, offsetForward + deltaForward));
            shapeDirty = true;
        }

        public void ResetShape()
        {
            if (!CanResize)
            {
                return;
            }

            width = Props.defaultWidth;
            height = Props.defaultHeight;
            offsetRight = 0;
            offsetForward = 0;
            perimeterOnly = false;
            autoFit = Props.autoFitSubstructure;
            hullPadding = Math.Max(0, Math.Min(4, Props.substructurePadding));
            autoFitCounter = 0;
            shapeDirty = true;
        }

        public void TogglePerimeter()
        {
            perimeterOnly = !perimeterOnly;
        }

        public void ToggleOverlay()
        {
            showOverlay = !showOverlay;
        }

        public void ToggleAutoFit()
        {
            autoFit = !autoFit;
            autoFitCounter = 0;
            shapeDirty = true;
        }

        /// <summary>Rand um die aeussersten Rumpfplatten: 0..4 Kacheln, zyklisch.</summary>
        public void CycleHullPadding()
        {
            hullPadding = (hullPadding + 1) % 5;
            shapeDirty = true;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in RectShieldGizmos.Build(this))
            {
                yield return gizmo;
            }
        }

        // Heisst in Vanilla "PostDrawExtraSelectionOverlays", nicht "DrawExtraSelectionOverlays".
        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();

            try
            {
                if (!ShowOverlay || Shape.IsEmpty)
                {
                    return;
                }

                var cells = perimeterOnly ? Shape.EdgeCells() : Shape.AllCells();
                GenDraw.DrawFieldEdges(RectShieldUtil.ToList(cells),
                    RectShieldUtil.ColorFor(Tier, Active));
            }
            catch (Exception e)
            {
                RectShieldLog.ErrorOnce("draw", "Overlay-Fehler: " + e.Message);
            }
        }

        public override string CompInspectStringExtra()
        {
            EnsureInitialized();

            string state = IsRebooting
                ? RectShieldStrings.Rebooting(rebootTicksLeft)
                : (Active ? RectShieldStrings.Online : RectShieldStrings.Offline);

            string fit = autoFit && Props.autoFitSubstructure
                ? RectShieldStrings.InspectFit(lastHull.Cells, hullPadding, lastHull.Found)
                : "";

            return RectShieldStrings.Inspect(
                Shape.Width, Shape.Height, Shape.Area, (int)energy, (int)Props.maxEnergy,
                (int)CurrentPowerDrawW, perimeterOnly, Tier, state) + fit;
        }
    }
}
