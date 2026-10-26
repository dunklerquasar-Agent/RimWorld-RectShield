namespace RectShield
{
    /// <summary>
    /// Mk1: feste Groesse, blockt nur direkte Schuesse.
    /// Mk2: frei konfigurierbar, blockt auch indirektes Feuer und Drop Pods,
    ///      Stromverbrauch skaliert mit der Flaeche.
    /// Mk3: High-Tech-Stufe, groesser, staerker und effizienter als Mk2.
    /// </summary>
    public enum RectShieldTier
    {
        Mk1 = 0,
        Mk2 = 1,
        Mk3 = 2
    }
}
