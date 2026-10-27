using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RectShield
{
    /// <summary>
    /// Zusaetzliche Zeicheneffekte fuer den Schild:
    ///  - gefuellte, halbtransparente Zellflaeche (Treffer-Flash)
    ///  - diagonale Verbindungslinien an Ecken, wie bei verlinkten Waenden
    /// </summary>
    public static class RectShieldDraw
    {
        private static Material fillMaterial;
        private static Material lineMaterial;
        private static readonly List<Matrix4x4> Matrices = new List<Matrix4x4>();

        // Unity DrawMeshInstanced erlaubt max. 1023 Instanzen pro Aufruf.
        private const int BatchSize = 512;
        private const int RenderQueue = 2900;
        private const float FillYOffset = 0.012f;
        private const float LineWidth = 0.16f;

        private static Material MakeMaterial()
        {
            var mat = new Material(ShaderDatabase.Transparent);
            mat.mainTexture = BaseContent.WhiteTex;
            mat.enableInstancing = true;
            mat.renderQueue = RenderQueue;
            return mat;
        }

        private static Material FillMaterial => fillMaterial ?? (fillMaterial = MakeMaterial());
        private static Material LineMaterial => lineMaterial ?? (lineMaterial = MakeMaterial());

        /// <summary>
        /// Legt eine leicht durchscheinende Farbe ueber ALLE uebergebenen Zellen.
        /// Zeichnet in Batches, damit auch ein 2000-Platten-Schiff funktioniert.
        /// </summary>
        public static void DrawFilledCells(IEnumerable<IntVec3> cells, Color color)
        {
            if (cells == null || color.a <= 0.001f)
            {
                return;
            }

            try
            {
                Material mat = FillMaterial;
                mat.color = color;
                Matrices.Clear();

                foreach (IntVec3 c in cells)
                {
                    Vector3 pos = c.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays)
                                  + new Vector3(0f, FillYOffset, 0f);
                    Matrices.Add(Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one));

                    if (Matrices.Count >= BatchSize)
                    {
                        Graphics.DrawMeshInstanced(MeshPool.plane10, 0, mat, Matrices);
                        Matrices.Clear();
                    }
                }

                if (Matrices.Count > 0)
                {
                    Graphics.DrawMeshInstanced(MeshPool.plane10, 0, mat, Matrices);
                    Matrices.Clear();
                }
            }
            catch (Exception e)
            {
                RectShieldLog.ErrorOnce("flashDraw",
                    "Schild-Flaeche konnte nicht gezeichnet werden: " + e.GetType().Name + ": " + e.Message);
            }
        }

        /// <summary>
        /// Zeichnet diagonale Linien zwischen Schildzellen, die sich nur ueber eine
        /// Ecke beruehren (analog zu diagonal verbundenen Waenden). 'pairs' enthaelt
        /// paarweise die Zellmittelpunkte.
        /// </summary>
        public static void DrawDiagonalLinks(IReadOnlyList<IntVec3> pairs, Color color)
        {
            if (pairs == null || pairs.Count < 2 || color.a <= 0.001f)
            {
                return;
            }

            try
            {
                Material mat = LineMaterial;
                mat.color = color;

                for (int i = 0; i + 1 < pairs.Count; i += 2)
                {
                    Vector3 a = pairs[i].ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                    Vector3 b = pairs[i + 1].ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
                    // DrawLineBetween setzt A.y = B.y selbst.
                    GenDraw.DrawLineBetween(a, b, mat, LineWidth);
                }
            }
            catch (Exception e)
            {
                RectShieldLog.ErrorOnce("diagDraw",
                    "Diagonale Schildkante konnte nicht gezeichnet werden: " + e.GetType().Name + ": " + e.Message);
            }
        }
    }
}
