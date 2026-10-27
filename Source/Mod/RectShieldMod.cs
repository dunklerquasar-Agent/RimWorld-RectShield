using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace RectShield
{
    public class RectShieldSettings : ModSettings
    {
        public bool ShowPersistentOverlay = true;
        public bool BlockFriendlyPods = false;
        public bool DebugLogging = false;
        public bool ShowHitFlash = true;
        public float WattsPerCellMultiplier = 1f;
        public float EnergyPerDamageMultiplier = 1f;
        public int MaxSizeCap = 51;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ShowPersistentOverlay, "rs_showPersistentOverlay", true);
            Scribe_Values.Look(ref BlockFriendlyPods, "rs_blockFriendlyPods", false);
            Scribe_Values.Look(ref DebugLogging, "rs_debugLogging", false);
            Scribe_Values.Look(ref ShowHitFlash, "rs_showHitFlash", true);
            Scribe_Values.Look(ref WattsPerCellMultiplier, "rs_wattsPerCellMultiplier", 1f);
            Scribe_Values.Look(ref EnergyPerDamageMultiplier, "rs_energyPerDamageMultiplier", 1f);
            Scribe_Values.Look(ref MaxSizeCap, "rs_maxSizeCap", 51);
        }
    }

    public class RectShieldMod : Mod
    {
        public static RectShieldSettings Settings;

        public RectShieldMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<RectShieldSettings>();
        }

        public override string SettingsCategory() => "RectShield";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            try
            {
                var list = new Listing_Standard();
                list.Begin(inRect);
                list.Label(RectShieldStrings.SettingsHeader);
                list.Gap(8f);

                list.CheckboxLabeled(RectShieldStrings.SettingsOverlay, ref Settings.ShowPersistentOverlay);
                list.CheckboxLabeled(RectShieldStrings.SettingsFriendlyPods, ref Settings.BlockFriendlyPods);
                list.CheckboxLabeled(RectShieldStrings.SettingsDebug, ref Settings.DebugLogging);
                list.CheckboxLabeled(RectShieldStrings.SettingsHitFlash, ref Settings.ShowHitFlash);

                list.Gap(8f);
                list.Label(RectShieldStrings.SettingsPower(Settings.WattsPerCellMultiplier));
                Settings.WattsPerCellMultiplier = list.Slider(Settings.WattsPerCellMultiplier, 0.25f, 3f);

                list.Label(RectShieldStrings.SettingsEnergy(Settings.EnergyPerDamageMultiplier));
                Settings.EnergyPerDamageMultiplier = list.Slider(Settings.EnergyPerDamageMultiplier, 0.25f, 3f);

                list.Gap(8f);
                list.Label(RectShieldStrings.SettingsHint);
                list.End();
            }
            catch (Exception e)
            {
                RectShieldLog.ErrorOnce("settings", "Settings-Fehler: " + e.Message);
            }
        }
    }
}
