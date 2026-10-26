using System;
using Verse;

namespace RectShield
{
    /// <summary>Bounding Box der gefundenen Gravschiff-Bodenplatten.</summary>
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

        public bool SameAs(RectShieldBounds other)
        {
            return MinX == other.MinX && MinZ == other.MinZ
                   && MaxX == other.MaxX && MaxZ == other.MaxZ
                   && Cells == other.Cells;
        }
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

        public static RectShieldBounds Scan(Map map)
        {
            var result = new RectShieldBounds();
            if (map == null)
            {
                return result;
            }

            try
            {
                int minX = int.MaxValue;
                int minZ = int.MaxValue;
                int maxX = int.MinValue;
                int maxZ = int.MinValue;
                int count = 0;

                foreach (IntVec3 cell in map.AllCells)
                {
                    TerrainDef foundation = map.terrainGrid.FoundationAt(cell);
                    if (foundation == null || !foundation.HasTag(SubstructureTag))
                    {
                        continue;
                    }

                    if (cell.x < minX) minX = cell.x;
                    if (cell.z < minZ) minZ = cell.z;
                    if (cell.x > maxX) maxX = cell.x;
                    if (cell.z > maxZ) maxZ = cell.z;
                    count++;
                }

                if (count == 0)
                {
                    return result;
                }

                result.MinX = minX;
                result.MinZ = minZ;
                result.MaxX = maxX;
                result.MaxZ = maxZ;
                result.Cells = count;
            }
            catch (Exception e)
            {
                // Z.B. wenn eine Version FoundationAt nicht mehr anbietet.
                // Dann einfach keine Platten melden statt das Spiel zu crashen.
                RectShieldLog.ErrorOnce("substructure", 
                    "Gravschiff-Platten-Scan fehlgeschlagen: " + e.GetType().Name + ": " + e.Message);
                return new RectShieldBounds();
            }

            return result;
        }
    }
}
