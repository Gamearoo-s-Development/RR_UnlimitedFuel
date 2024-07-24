using HarmonyLib;
using System;
using System.Reflection;
using UnityModManagerNet;

namespace UnlimitedFuel
{
    static class Main
    {
        private static bool enabled;
        private static UnityModManager.ModEntry myModEntry;
        private static Harmony myHarmony;

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            try
            {
                myModEntry = modEntry;
                modEntry.OnToggle = OnToggle;
                modEntry.OnUnload = OnUnload;

                myHarmony = new Harmony(myModEntry.Info.Id);
                myHarmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                myModEntry.Logger.LogException($"Failed to load {myModEntry.Info.DisplayName}:", ex);
                myHarmony?.UnpatchAll(myModEntry.Info.Id);
                return false;
            }

            modEntry.Logger.Log("Loaded");

            return true;
        }

        private static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
            myHarmony?.UnpatchAll(myModEntry.Info.Id);
            return true;
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (value == enabled) return true; // No change in state

            enabled = value;

            if (enabled)
            {
                // Reapply patches when enabled
                myHarmony.PatchAll(Assembly.GetExecutingAssembly());
                modEntry.Logger.Log("Hello!");
            }
            else
            {
                // Remove patches when disabled
                myHarmony.UnpatchAll(myModEntry.Info.Id);
                modEntry.Logger.Log("Goodbye!");
            }

            return true;
        }
    }
}
