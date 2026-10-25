using System;
using System.Collections.Generic;
using Verse;

namespace RectShield
{
    /// <summary>
    /// Reine Geometrie des Schildes. Achsenparalleles Rechteck in Welt-Koordinaten.
    /// Bewusst ohne Abhaengigkeit von Vanilla-Methoden implementiert (nur IntVec3/Rot4),
    /// damit Clipping und Rotation exakt kontrollierbar und unit-testbar sind.
    /// </summary>
    public sealed class RectShieldShape
    {
        public int MinX { get; private set; }
        public int MinZ { get; private set; }
        public int MaxX { get; private set; }
        public int MaxZ { get; private set; }
        public bool IsEmpty { get; private set; }

        public int Width => IsEmpty ? 0 : MaxX - MinX + 1;
        public int Height => IsEmpty ? 0 : MaxZ - MinZ + 1;
        public int Area => Width * Height;

        private RectShieldShape()
        {
        }

        /// <summary>
        /// Baut das Rechteck um den Generator (standardmaessig zentriert).
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

            // An den Kartenrand clippen (eigene Logik statt CellRect.ClipInsideMap,
            // dessen Signatur sich zwischen Versionen unterscheidet).
            if (map != null)
            {
                minX = Math.Max(minX, 0);
                minZ = Math.Max(minZ, 0);
                maxX = Math.Min(maxX, map.Size.x - 1);
                maxZ = Math.Min(maxZ, map.Size.z - 1);
            }

            result.MinX = minX;
            result.MinZ = minZ;
            result.MaxX = maxX;
            result.MaxZ = maxZ;
            result.IsEmpty = minX > maxX || minZ > maxZ;
            return result;
        }

        public bool Contains(IntVec3 cell)
        {
            return !IsEmpty
                   && cell.x >= MinX && cell.x <= MaxX
                   && cell.z >= MinZ && cell.z <= MaxZ;
        }

        public bool IsEdge(IntVec3 cell)
        {
            return Contains(cell)
                   && (cell.x == MinX || cell.x == MaxX || cell.z == MinZ || cell.z == MaxZ);
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
