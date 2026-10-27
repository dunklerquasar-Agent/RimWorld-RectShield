using Verse;

namespace RectShield
{
    /// <summary>
    /// XML-Parameter des Schild-Comps. Alle Werte sind in den Defs ueberschreibbar.
    /// </summary>
    public class CompProperties_RectShield : CompProperties
    {
        public RectShieldTier tier = RectShieldTier.Mk1;

        // --- Geometrie ---
        public bool fixedSize = true;          // Mk1 = true, Mk2/Mk3 = false
        public int fixedWidth = 9;
        public int fixedHeight = 7;
        public int defaultWidth = 13;
        public int defaultHeight = 9;
        public int minWidth = 5;
        public int minHeight = 5;
        public int maxWidth = 41;
        public int maxHeight = 41;

        // --- Energie ---
        public float maxEnergy = 800f;
        public float energyPerDamage = 1f;
        public float regenPerTick = 0.6f;
        public float skyfallerEnergyCost = 180f;
        public int rebootTicks = 900;

        // --- Strom (Mk2/Mk3 skalieren mit der Flaeche) ---
        public float basePowerW = 150f;
        public float wattsPerCell = 0f;        // Mk1: 0 (fix), Mk2: 3.0, Mk3: 2.0

        // --- Visuelle Effekte ---
        // Bei einem Treffer leuchtet das GANZE Feld kurz leicht durchscheinend auf.
        public int flashTicks = 18;            // Dauer des Aufleuchtens in Ticks
        public float flashAlpha = 0.30f;       // maximale Deckkraft (0..1)
        // Diagonale Ecken der Rumpfform wie verlinkte Waende verbinden.
        public bool diagonalLinks = true;

        // --- Gravschiff-Modus (nur Stufe 3) ---
        // Passt das Feld automatisch an die Odyssey-Substructure-Platten an.
        public bool autoFitSubstructure = false;
        public int substructurePadding = 2;       // Kacheln Abstand um die Platten
        public int substructureRefreshTicks = 180; // 3 Sekunden

        // --- Abfangverhalten ---
        public bool blocksShots = true;        // direkte Projektile
        public bool blocksIndirectFire = false;// Moerser / flyOverhead (Mk2/Mk3)
        public bool blocksSkyfallers = false;  // Drop Pods, Meteoriten (Mk2/Mk3)
        public bool blocksExplosions = false;  // fuer v0.2 reserviert

        public CompProperties_RectShield()
        {
            compClass = typeof(CompRectShield);
        }
    }
}
