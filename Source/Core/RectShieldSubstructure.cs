using System;
using System.Collections.Generic;
using Verse;

namespace RectShield
{
    /// <summary>Bounding Box der gefundenen Gravschiff-Bodenplatten (nur Info/Clipping).</summary>
    public struct RectShieldBounds
    {
        public int MinX;
        public int MinZ;
        public int MaxX;
        public int MaxZ;
        public int Cells;

        public bool Found => Cells > 0;
        public int Width => Found ? MaxX - MinX + 1 : 0;
        public int Height => Found ? MaxZ - MinZ + 1 : 0;
    }

    /// <summary>
    /// Alle tatsaechlich gefundenen Rumpfzellen. Eine L-Form bleibt damit eine
    /// L-Form - es wird NICHT die leere Aussparung der Bounding Box aufgefuellt.
    /// </summary>
    public sealed class RectShieldHull
    {
        public RectShieldBounds Bounds;
        public readonly HashSet<IntVec3> Cells = new HashSet<IntVec3>();

        public int Count => Cells.Count;
        public bool Found => Cells.Count > 0;
    }

    /// <summary>
    /// Findet die Gravschiff-Substructure (Odyssey Foundation-Layer) auf einer Map.
    ///
    /// Bewusst nur mit stabilen Vanilla-APIs gearbeitet:
    ///   map.terrainGrid.FoundationAt(IntVec3) -> TerrainDef
    ///   Def.HasTag("Substructure")            -> seit jeher vorhanden
    ///
    /// Es wird KEIN Odyssey-spezifischer Typ hart referenziert (kein
    /// Building_GravEngine, ModsConfig.OdysseyActive oder
    /// TerrainDef.IsSubstructure). Das haelt den Code 1.6/1.7-kompatibel und
    /// funktioniert auch ohne DLC (dann werden einfach keine Platten gefunden).
    /// </summary>
    public static class RectShieldSubstructure
    {
        public const string SubstructureTag = "Substructure";

        // Defs werden einmalig geladen. Statt pro Zelle HasTag (String-Vergleich
        // ueber die Tag-Liste) cachen wir die passenden TerrainDef-Referenzen.
        // Der Zaehler faengt ein spaeteres Nachladen von Defs ab.
        private static HashSet<TerrainDef> substructureDefs;
        private static int cachedDefCount = -1;

        private static HashSet<TerrainDef> GetSubstructureDefs()
        {
            int count = 0;
            foreach (TerrainDef def in DefDatabase<TerrainDef>.AllDefs)
            {
                count++;
            }

            if (substructureDefs == null || cachedDefCount != count)
            {
                substructureDefs = new HashSet<TerrainDef>();
                foreach (TerrainDef def in DefDatabase<TerrainDef>.AllDefs)
                {
                    if (def.HasTag(SubstructureTag))
                    {
                        substructureDefs.Add(def);
                    }
                }

                cachedDefCount = count;
            }

            return substructureDefs;
        }

        /// <summary>Vergleicht zwei Rumpfmengen, ohne unnoetig neu zu dilatieren.</summary>
        public static bool SameCells(RectShieldHull a, RectShieldHull b)
        {
            if (a == null || b == null)
            {
                return ReferenceEquals(a, b);
            }

            return a.Count == b.Count && a.Cells.SetEquals(b.Cells);
        }

        public static RectShieldHull Scan(Map map)
        {
            var hull = new RectShieldHull();
            if (map == null)
            {
                return hull;
            }

            HashSet<TerrainDef> defs = GetSubstructureDefs();
            if (defs.Count == 0)
            {
                // Kein Odyssey-DLC / keine Substructure-Defs vorhanden.
                return hull;
            }

            try
            {
                int minX = int.MaxValue;
                int minZ = int.MaxValue;
                int maxX = int.MinValue;
                int maxZ = int.MinValue;

                foreach (IntVec3 cell in map.AllCells)
                {
                    TerrainDef foundation = map.terrainGrid.FoundationAt(cell);
                    if (foundation == null || !defs.Contains(foundation))
                    {
                        continue;
                    }

                    var c = new IntVec3(cell.x, 0, cell.z);
                    hull.Cells.Add(c);

                    if (c.x < minX) minX = c.x;
                    if (c.z < minZ) minZ = c.z;
                    if (c.x > maxX) maxX = c.x;
                    if (c.z > maxZ) maxZ = c.z;
                }

                if (hull.Count == 0)
                {
                    return hull;
                }

                hull.Bounds = new RectShieldBounds
                {
                    MinX = minX,
                    MinZ = minZ,
                    MaxX = maxX,
                    MaxZ = maxZ,
                    Cells = hull.Count
                };
            }
            catch (Exception e)
            {
                // Z.B. wenn eine Version FoundationAt nicht mehr anbietet.
                // Dann einfach keine Platten melden statt das Spiel zu crashen.
                RectShieldLog.ErrorOnce("substructure",
                    "Gravschiff-Platten-Scan fehlgeschlagen: " + e.GetType().Name + ": " + e.Message);
                return new RectShieldHull();
            }

            return hull;
        }

        /// <summary>
        /// Erweitert die Rumpfform um 'pad' Kacheln in alle Richtungen
        /// (Chebyshev-Dilatation, 8er-Nachbarschaft). Die Form bleibt erhalten:
        /// eine L-Form wird zu einer groesseren L-Form, nicht zu einem Quadrat.
        /// </summary>
        public static HashSet<IntVec3> Dilate(HashSet<IntVec3> source, int pad)
        {
            var result = new HashSet<IntVec3>();
            if (source == null)
            {
                return result;
            }

            if (pad <= 0)
            {
                result.UnionWith(source);
                return result;
            }

            foreach (IntVec3 c in source)
            {
                for (int dx = -pad; dx <= pad; dx++)
                {
                    for (int dz = -pad; dz <= pad; dz++)
                    {
                        result.Add(new IntVec3(c.x + dx, 0, c.z + dz));
                    }
                }
            }

            return result;
        }
    }
}
