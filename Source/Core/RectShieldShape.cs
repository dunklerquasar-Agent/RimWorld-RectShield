using System;
using System.Collections.Generic;
using Verse;

namespace RectShield
{
    /// <summary>
    /// Geometrie des Schildes. Zwei Modi:
    ///  - Klassisch: ein achsenparalleles Rechteck (Mk I / Mk II / manueller Modus).
    ///  - Rumpf-Modus: eine exakte Zellmenge, die jeder Gravschiff-Bodenplatte folgt.
    ///    Eine L-Form bleibt dabei eine L-Form - die Aussparung bleibt ungeschuetzt.
    /// Bewusst ohne Abhaengigkeit von Vanilla-Geometrieklassen implementiert.
    /// </summary>
    public sealed class RectShieldShape
    {
        public int MinX { get; private set; }
        public int MinZ { get; private set; }
        public int MaxX { get; private set; }
        public int MaxZ { get; private set; }
        public bool IsEmpty { get; private set; }

        private HashSet<IntVec3> customCells;
        private List<IntVec3> edgeCache;

        public bool HasCustomCells => customCells != null && customCells.Count > 0;

        public int Width => IsEmpty ? 0 : MaxX - MinX + 1;
        public int Height => IsEmpty ? 0 : MaxZ - MinZ + 1;

        /// <summary>Echte geschuetzte Zellenzahl (bei Rumpfform ohne Aussparungen).</summary>
        public int Area => HasCustomCells ? customCells.Count : (IsEmpty ? 0 : Width * Height);

        private RectShieldShape()
        {
        }

        /// <summary>
        /// Baut das klassische Rechteck um den Generator (standardmaessig zentriert).
        /// width  = Ausdehnung entlang der Rechts-Achse des Gebaeudes
        /// height = Ausdehnung entlang der Vorwaerts-Achse des Gebaeudes
        /// offset = Verschiebung in lokalen Koordinaten (rechts, vorwaerts)
        /// </summary>
        public static RectShieldShape Build(IntVec3 anchor, Rot4 rot, int width, int height,
                                            IntVec3 offset, Map map)
        {
            var result = new RectShieldShape();

            width = Math.Max(1, width);
            height = Math.Max(1, height);

            // Lokale Achsen nach Weltkoordinaten uebersetzen.
            // Rot4: 0 = Nord, 1 = Ost, 2 = Sued, 3 = West.
            int fwdX, fwdZ, rightX, rightZ;
            switch (rot.AsInt)
            {
                case 1: fwdX = 1; fwdZ = 0; rightX = 0; rightZ = -1; break; // Ost
                case 2: fwdX = 0; fwdZ = -1; rightX = -1; rightZ = 0; break; // Sued
                case 3: fwdX = -1; fwdZ = 0; rightX = 0; rightZ = 1; break; // West
                default: fwdX = 0; fwdZ = 1; rightX = 1; rightZ = 0; break; // Nord
            }

            int offX = rightX * offset.x + fwdX * offset.z;
            int offZ = rightZ * offset.x + fwdZ * offset.z;

            // Standard: Generator in der Mitte des Feldes.
            int centerX = anchor.x + offX;
            int centerZ = anchor.z + offZ;

            int halfW = width / 2;
            int halfH = height / 2;

            int minX = centerX - halfW;
            int maxX = centerX + (width - 1 - halfW);
            int minZ = centerZ - halfH;
            int maxZ = centerZ + (height - 1 - halfH);

            result.AssignBounds(minX, minZ, maxX, maxZ, map);
            return result;
        }

        /// <summary>
        /// Klassisches Rechteck direkt aus Weltkoordinaten (Fallback).
        /// </summary>
        public static RectShieldShape BuildFromWorldBounds(int minX, int minZ,
                                                           int maxX, int maxZ, Map map)
        {
            var result = new RectShieldShape();
            result.AssignBounds(minX, minZ, maxX, maxZ, map);
            return result;
        }

        /// <summary>
        /// Baut das Schild aus einer exakten Zellmenge (Gravschiff-Rumpfform).
        /// Zellen ausserhalb der Karte werden verworfen; die Bounds dienen nur
        /// noch Inspect-Anzeige und Kamera-Clipping.
        /// </summary>
        public static RectShieldShape BuildFromCells(HashSet<IntVec3> source, Map map)
        {
            var result = new RectShieldShape();
            var set = new HashSet<IntVec3>();

            if (source != null)
            {
                foreach (IntVec3 raw in source)
                {
                    var c = new IntVec3(raw.x, 0, raw.z);
                    if (map != null &&
                        (c.x < 0 || c.z < 0 || c.x >= map.Size.x || c.z >= map.Size.z))
                    {
                        continue;
                    }

                    set.Add(c);
                }
            }

            result.customCells = set;
            if (set.Count == 0)
            {
                result.IsEmpty = true;
                result.edgeCache = new List<IntVec3>();
                return result;
            }

            int minX = int.MaxValue;
            int minZ = int.MaxValue;
            int maxX = int.MinValue;
            int maxZ = int.MinValue;
            var edges = new List<IntVec3>();

            foreach (IntVec3 c in set)
            {
                if (c.x < minX) minX = c.x;
                if (c.z < minZ) minZ = c.z;
                if (c.x > maxX) maxX = c.x;
                if (c.z > maxZ) maxZ = c.z;

                // Randzelle = mindestens ein 4er-Nachbar fehlt.
                if (!set.Contains(new IntVec3(c.x + 1, 0, c.z)) ||
                    !set.Contains(new IntVec3(c.x - 1, 0, c.z)) ||
                    !set.Contains(new IntVec3(c.x, 0, c.z + 1)) ||
                    !set.Contains(new IntVec3(c.x, 0, c.z - 1)))
                {
                    edges.Add(c);
                }
            }

            result.MinX = minX;
            result.MinZ = minZ;
            result.MaxX = maxX;
            result.MaxZ = maxZ;
            result.IsEmpty = false;
            result.edgeCache = edges;
            return result;
        }

        private void AssignBounds(int minX, int minZ, int maxX, int maxZ, Map map)
        {
            if (map != null)
            {
                minX = Math.Max(minX, 0);
                minZ = Math.Max(minZ, 0);
                maxX = Math.Min(maxX, map.Size.x - 1);
                maxZ = Math.Min(maxZ, map.Size.z - 1);
            }

            MinX = minX;
            MinZ = minZ;
            MaxX = maxX;
            MaxZ = maxZ;
            IsEmpty = minX > maxX || minZ > maxZ;
        }

        public bool Contains(IntVec3 cell)
        {
            if (!IsEmpty && HasCustomCells)
            {
                return customCells.Contains(new IntVec3(cell.x, 0, cell.z));
            }

            return !IsEmpty
                   && cell.x >= MinX && cell.x <= MaxX
                   && cell.z >= MinZ && cell.z <= MaxZ;
        }

        public bool IsEdge(IntVec3 cell)
        {
            if (!Contains(cell))
            {
                return false;
            }

            if (HasCustomCells)
            {
                return !Contains(new IntVec3(cell.x + 1, 0, cell.z))
                       || !Contains(new IntVec3(cell.x - 1, 0, cell.z))
                       || !Contains(new IntVec3(cell.x, 0, cell.z + 1))
                       || !Contains(new IntVec3(cell.x, 0, cell.z - 1));
            }

            return cell.x == MinX || cell.x == MaxX || cell.z == MinZ || cell.z == MaxZ;
        }

        /// <summary>
        /// Kernregel F4: nur beim EINTRITT blockieren.
        /// Schuesse von innen nach aussen bleiben erlaubt.
        /// </summary>
        public bool BlocksEntry(IntVec3 from, IntVec3 to, bool perimeterOnly)
        {
            if (!Contains(to))
            {
                return false;
            }

            if (Contains(from))
            {
                return false;
            }

            return !perimeterOnly || IsEdge(to);
        }

        /// <summary>Alle geschuetzten Zellen (fuer DrawFieldEdges im Volumen-Modus).</summary>
        public IEnumerable<IntVec3> AllCells()
        {
            if (HasCustomCells)
            {
                foreach (IntVec3 c in customCells)
                {
                    yield return c;
                }

                yield break;
            }

            if (IsEmpty)
            {
                yield break;
            }

            for (int x = MinX; x <= MaxX; x++)
            {
                for (int z = MinZ; z <= MaxZ; z++)
                {
                    yield return new IntVec3(x, 0, z);
                }
            }
        }

        /// <summary>Nur die Randzellen (fuer Randmodus und das persistente Overlay).</summary>
        public IEnumerable<IntVec3> EdgeCells()
        {
            if (HasCustomCells)
            {
                if (edgeCache != null)
                {
                    foreach (IntVec3 c in edgeCache)
                    {
                        yield return c;
                    }
                }

                yield break;
            }

            if (IsEmpty)
            {
                yield break;
            }

            for (int x = MinX; x <= MaxX; x++)
            {
                for (int z = MinZ; z <= MaxZ; z++)
                {
                    if (x == MinX || x == MaxX || z == MinZ || z == MaxZ)
                    {
                        yield return new IntVec3(x, 0, z);
                    }
                }
            }
        }
    }
}
