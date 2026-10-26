using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RectShield
{
    /// <summary>
    /// Mk I: nur ein Info-Gizmo ("feste Groesse").
    /// Mk II / Gravship: Breite/Hoehe mit +/- Icons, Versatz/Presets/Randmodus/
    /// Overlay und zusaetzlich der Auto-Fit-Schalter fuer Gravschiff-Platten.
    /// </summary>
    public static class RectShieldGizmos
    {
        private static readonly int[,] Presets =
        {
            { 9, 9 }, { 15, 15 }, { 21, 21 }, { 31, 31 }, { 41, 41 }, { 51, 51 }
        };

        private static Texture2D iconPlus;
        private static Texture2D iconMinus;
        private static Texture2D iconFit;
        private static bool iconsLoaded;

        private static void EnsureIcons()
        {
            if (iconsLoaded)
            {
                return;
            }

            iconsLoaded = true;
            iconPlus = Load("UI/Gizmos/RectShield_Plus");
            iconMinus = Load("UI/Gizmos/RectShield_Minus");
            iconFit = Load("UI/Gizmos/RectShield_FitHull");
        }

        private static Texture2D Load(string path)
        {
            try
            {
                return ContentFinder<Texture2D>.Get(path, false);
            }
            catch (Exception)
            {
                // Fehlende Textur ist kein Fehler: RimWorld zeigt dann nur das Label.
                return null;
            }
        }

        public static IEnumerable<Gizmo> Build(CompRectShield comp)
        {
            EnsureIcons();

            if (comp.Props.autoFitSubstructure)
            {
                yield return Action(
                    RectShieldStrings.GizmoFit(comp.AutoFit),
                    RectShieldStrings.DescFit,
                    comp.ToggleAutoFit,
                    iconFit);

                // Rand um die aeussersten Platten: 0 = buendig, 1..4 = Abstand.
                yield return Action(
                    RectShieldStrings.GizmoPadding(comp.HullPadding),
                    RectShieldStrings.DescPadding,
                    comp.CycleHullPadding,
                    iconPlus);
            }

            if (!comp.CanResize)
            {
                yield return InfoGizmo(comp);
                yield return OverlayGizmo(comp);
                yield break;
            }

            // Vier Groessen-Gizmos mit + / - Symbolgrafiken.
            yield return Action(RectShieldStrings.GizmoMinus, RectShieldStrings.DescResize,
                () => comp.Resize(-2, 0), iconMinus);
            yield return Action(RectShieldStrings.GizmoWidth(comp.Width), RectShieldStrings.DescResize,
                () => comp.Resize(2, 0), iconPlus);
            yield return Action(RectShieldStrings.GizmoMinus, RectShieldStrings.DescResize,
                () => comp.Resize(0, -2), iconMinus);
            yield return Action(RectShieldStrings.GizmoHeight(comp.Height), RectShieldStrings.DescResize,
                () => comp.Resize(0, 2), iconPlus);

            yield return Action(RectShieldStrings.GizmoPresets, RectShieldStrings.DescPresets,
                () => OpenPresets(comp), null);
            yield return Action(RectShieldStrings.GizmoOffset, RectShieldStrings.DescOffset,
                () => OpenOffsets(comp), null);
            yield return Action(RectShieldStrings.GizmoMode, RectShieldStrings.DescMode,
                comp.TogglePerimeter, null);
            yield return Action(RectShieldStrings.GizmoReset, RectShieldStrings.DescReset,
                comp.ResetShape, null);
            yield return OverlayGizmo(comp);
        }

        private static Gizmo InfoGizmo(CompRectShield comp)
        {
            return new Command_Action
            {
                defaultLabel = RectShieldStrings.GizmoFixedSize(comp.Width, comp.Height),
                defaultDesc = RectShieldStrings.DescFixedSize,
                // Gizmo.disabled ist protected; das oeffentliche Property heisst "Disabled".
                Disabled = true,
                action = () => { }
            };
        }

        private static Gizmo OverlayGizmo(CompRectShield comp)
        {
            return new Command_Action
            {
                defaultLabel = RectShieldStrings.GizmoOverlay,
                defaultDesc = RectShieldStrings.DescOverlay,
                action = comp.ToggleOverlay
            };
        }

        private static Gizmo Action(string label, string desc, System.Action action,
                                    Texture2D icon = null)
        {
            return new Command_Action
            {
                defaultLabel = label,
                defaultDesc = desc,
                icon = icon,
                action = action
            };
        }

        private static void OpenPresets(CompRectShield comp)
        {
            var options = new List<FloatMenuOption>();
            for (int i = 0; i < Presets.GetLength(0); i++)
            {
                int w = Presets[i, 0];
                int h = Presets[i, 1];
                // Nur Vorlagen anbieten, die der Generator auch abdecken kann.
                if (w > comp.Props.maxWidth || h > comp.Props.maxHeight)
                {
                    continue;
                }

                options.Add(new FloatMenuOption(
                    RectShieldStrings.PresetLabel(w, h),
                    () => comp.SetSize(w, h)));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void OpenOffsets(CompRectShield comp)
        {
            var options = new List<FloatMenuOption>
            {
                new FloatMenuOption("RS_OffsetNorth".Translate().ToString(), () => comp.MoveOffset(0, 2)),
                new FloatMenuOption("RS_OffsetSouth".Translate().ToString(), () => comp.MoveOffset(0, -2)),
                new FloatMenuOption("RS_OffsetEast".Translate().ToString(), () => comp.MoveOffset(2, 0)),
                new FloatMenuOption("RS_OffsetWest".Translate().ToString(), () => comp.MoveOffset(-2, 0))
            };

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}
