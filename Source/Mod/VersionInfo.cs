using RimWorld;
using Verse;

namespace RectShield
{
    /// <summary>
    /// Versionsfester Zugriff auf die Spielversion. VersionControl.CurrentVersionString
    /// existiert in 1.6; falls es in 1.7 entfaellt, bleibt der Patch trotzdem lauffaehig.
    /// </summary>
    public static class VersionInfo
    {
        public static string GameVersion
        {
            get
            {
                try
                {
                    return VersionControl.CurrentVersionString ?? "unbekannt";
                }
                catch
                {
                    return "unbekannt";
                }
            }
        }
    }
}
