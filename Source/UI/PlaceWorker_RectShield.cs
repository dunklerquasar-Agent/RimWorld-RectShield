using Verse;

namespace RectShield
{
    /// <summary>
    /// Platzierungs-Pruefung. Die Geometrie wird erst nach dem Bau relevant,
    /// deshalb wird hier nur geprueft, dass das Feld nicht komplett ausserhalb
    /// der Karte liegt. Die visuelle Vorschau laeuft ueber DrawExtraSelectionOverlays.
    /// HINWEIS: Signatur von AllowsPlacing in docs/API_NOTES.md gegen die eigene
    /// Assembly-CSharp.dll pruefen (1.6 vs. 1.7).
    /// </summary>
    public class PlaceWorker_RectShield : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc,
            Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            return true;
        }
    }
}
