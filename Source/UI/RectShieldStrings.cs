using Verse;

namespace RectShield
{
    /// <summary>Alle UI-Texte laufen ueber Keyed-XML (DE/EN), nie hartcodiert.</summary>
    public static class RectShieldStrings
    {
        private static string T(string key) => key.Translate().ToString();

        public static string Online => T("RS_Online");
        public static string Offline => T("RS_Offline");
        public static string Rebooting(int ticks) => string.Format(T("RS_Rebooting"), ticks);

        public static string Inspect(int width, int height, int area, int energy, int maxEnergy,
                                     int power, bool perimeterOnly, RectShieldTier tier, string state)
        {
            string mode = perimeterOnly ? T("RS_ModePerimeter") : T("RS_ModeVolume");
            string tierName = tier == RectShieldTier.Mk3 ? T("RS_TierMk3")
                           : tier == RectShieldTier.Mk2 ? T("RS_TierMk2")
                           : T("RS_TierMk1");

            return string.Format(T("RS_Inspect"),
                tierName, width, height, area, energy, maxEnergy, power, mode, state);
        }

        // --- Gizmos ---
        public static string GizmoFixedSize(int w, int h) => string.Format(T("RS_GizmoFixedSize"), w, h);
        public static string GizmoWidth(int value) => string.Format(T("RS_GizmoWidth"), value);
        public static string GizmoHeight(int value) => string.Format(T("RS_GizmoHeight"), value);
        public static string GizmoPresets => T("RS_GizmoPresets");
        public static string GizmoOffset => T("RS_GizmoOffset");
        public static string GizmoMode => T("RS_GizmoMode");
        public static string GizmoReset => T("RS_GizmoReset");
        public static string GizmoOverlay => T("RS_GizmoOverlay");
        public static string GizmoFit(bool on) => T(on ? "RS_GizmoFitOn" : "RS_GizmoFitOff");
        public static string InspectFit(int cells, bool found) => found
            ? string.Format(T("RS_InspectFit"), cells)
            : T("RS_FitNoPlates");
        public static string GizmoPlus => "+";
        public static string GizmoMinus => "-";
        public static string DescResize => T("RS_DescResize");
        public static string DescPresets => T("RS_DescPresets");
        public static string DescOffset => T("RS_DescOffset");
        public static string DescMode => T("RS_DescMode");
        public static string DescReset => T("RS_DescReset");
        public static string DescOverlay => T("RS_DescOverlay");
        public static string DescFixedSize => T("RS_DescFixedSize");
        public static string DescFit => T("RS_DescFit");
        public static string PresetLabel(int w, int h) => string.Format(T("RS_Preset"), w, h);

        // --- Settings ---
        public static string SettingsHeader => T("RS_SettingsHeader");
        public static string SettingsOverlay => T("RS_SettingsOverlay");
        public static string SettingsFriendlyPods => T("RS_SettingsFriendlyPods");
        public static string SettingsDebug => T("RS_SettingsDebug");
        public static string SettingsHint => T("RS_SettingsHint");
        public static string SettingsPower(float value) => string.Format(T("RS_SettingsPower"), value.ToString("0.00"));
        public static string SettingsEnergy(float value) => string.Format(T("RS_SettingsEnergy"), value.ToString("0.00"));
    }
}
