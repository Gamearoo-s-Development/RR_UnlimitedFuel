using HarmonyLib;
using UnityEngine;
using Model;
using Game.State;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace UnlimitedFuel
{
    [HarmonyPatch(typeof(DieselLocomotive))]
    [HarmonyPatch("PeriodicUpdate")]
    class FuelPatchDiesel
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var codes = new List<CodeInstruction>(instructions);
            var methodToCall = typeof(DieselLocomotive).BaseType.GetMethod("PeriodicUpdate", BindingFlags.Instance | BindingFlags.NonPublic);

            if (methodToCall == null)
            {
                Debug.LogError("Base PeriodicUpdate method not found.");
                return instructions;
            }

            var newInstructions = new List<CodeInstruction>();
            var labelOriginalCode = il.DefineLabel();
            var labelEndOfMethod = il.DefineLabel();

            // Add instructions to call CheckCondition at the start of the method
            newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0)); // Load "this"
            newInstructions.Add(new CodeInstruction(OpCodes.Call, typeof(FuelPatchSteam).GetMethod(nameof(CheckCondition), BindingFlags.Static | BindingFlags.NonPublic))); // Call CheckCondition
            newInstructions.Add(new CodeInstruction(OpCodes.Brfalse, labelOriginalCode)); // If false, jump to original code

            // Insert additional behavior if CheckCondition is true
            newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0)); // Load "this"
            newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_1)); // Load "dt"
            newInstructions.Add(new CodeInstruction(OpCodes.Call, methodToCall)); // Call base method
            newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0)); // Load "this"
            newInstructions.Add(new CodeInstruction(OpCodes.Call, typeof(FuelPatchSteam).GetMethod(nameof(AdditionalBehavior), BindingFlags.Static | BindingFlags.NonPublic))); // Call AdditionalBehavior
            newInstructions.Add(new CodeInstruction(OpCodes.Br, labelEndOfMethod)); // Jump to the end of the method

            // Mark the label for original code
            newInstructions.Add(new CodeInstruction(OpCodes.Nop) { labels = new List<Label> { labelOriginalCode } });

            // Add the rest of the original instructions
            newInstructions.AddRange(codes);

            // Mark the end of the method
            newInstructions.Add(new CodeInstruction(OpCodes.Nop) { labels = new List<Label> { labelEndOfMethod } });

            return newInstructions.AsEnumerable();
        }

        static bool CheckCondition(DieselLocomotive __instance)
        {
            PlayersManager playersManager = GameObject.FindObjectOfType<PlayersManager>();
            if (playersManager == null)
            {
                Debug.LogError("PlayersManager instance not found.");
                return false;
            }

            string crewName = playersManager.NameForTrainCrewId(__instance.trainCrewId);
            Debug.Log($"Crew Name: {crewName}");

            return !crewName.StartsWith("FUEL");
        }

        static void AdditionalBehavior(DieselLocomotive __instance)
        {
            Debug.Log("Executing additional behavior for DieselLocomotive.");
            // Add your custom behavior here
        }
    }

}