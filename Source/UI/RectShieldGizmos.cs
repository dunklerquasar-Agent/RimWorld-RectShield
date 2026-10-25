using System.Collections.Generic;
using RimWorld;
using Verse;

namespace RectShield
{
    /// <summary>
    /// Mk1: nur ein Info-Gizmo ("feste Groesse").
    /// Mk2: Breite/Hoehe/Versatz/Presets/Randmodus/Overlay.
    /// </summary>
    public static class RectShieldGizmos
    {
        private static readonly int[,] Presets =
        {
            { 9, 7 }, { 13, 9 }, { 17, 13 }, { 23, 15 }, { 31, 21 }, { 41, 31 }
        };

        public static IEnumerable<Gizmo> Build(CompRectShield comp)
        {
            if (!comp.CanResize)
            {
                yield return InfoGizmo(comp);
                yield return OverlayGizmo(comp);
                yield break;
            }

            yield return Action(RectShieldStrings.GizmoMinus, "−", RectShieldStrings.DescResize,
                () => comp.Resize(-2, 0));
            yield return Action(RectShieldStrings.GizmoWidth(comp.Width), "W", RectShieldStrings.DescResize,
                () => comp.Resize(2, 0));
            yield return Action(RectShieldStrings.GizmoMinus, "−", RectShieldStrings.DescResize,
                () => comp.Resize(0, -2));
            yield return Action(RectShieldStrings.GizmoHeight(comp.Height), "H", RectShieldStrings.DescResize,
                () => comp.Resize(0, 2));

            yield return Action(RectShieldStrings.GizmoPresets, "▦", RectShieldStrings.DescPresets,
                () => OpenPresets(comp));
            yield return Action(RectShieldStrings.GizmoOffset, "↔", RectShieldStrings.DescOffset,
                () => OpenOffsets(comp));
            yield return Action(RectShieldStrings.GizmoMode, "◫", RectShieldStrings.DescMode,
                comp.TogglePerimeter);
            yield return Action(RectShieldStrings.GizmoReset, "↺", RectShieldStrings.DescReset,
                comp.ResetShape);
            yield return OverlayGizmo(comp);
        }

        private static Gizmo InfoGizmo(CompRectShield comp)
        {
            return new Command_Action
            {
                defaultLabel = RectShieldStrings.GizmoFixedSize(comp.Width, comp.Height),
                defaultDesc = RectShieldStrings.DescFixedSize,
                disabled = true,
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

        private static Gizmo Action(string label, string shortLabel, string desc, System.Action action)
        {
            return new Command_Action
            {
                defaultLabel = label,
                defaultDesc = desc,
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
