using Exiled.API.Features;
using Exiled.Events.EventArgs;
using HarmonyLib;
using LightContainmentZoneDecontamination;
using NorthwoodLib.Pools;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static HarmonyLib.AccessTools;

namespace FacilityManagement.Patches.IntercomText
{
    #pragma warning disable IDE0060 // Supprimer le paramètre inutilisé

    // Modifica il setter del testo dell'interfono
    [HarmonyPatch(typeof(Exiled.API.Features.Intercom), nameof(Exiled.API.Features.Intercom.DisplayText), MethodType.Setter)]
    public static class CommandIntercomTextSetterFix
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent();

            newInstructions.AddRange(new CodeInstruction[]
            {
                new(OpCodes.Ldsfld, Field(typeof(FacilityManagement),nameof(FacilityManagement.Singleton))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldind_Ref),
                new(OpCodes.Stfld, Field(typeof(FacilityManagement),nameof(FacilityManagement.Singleton.CustomText))),
                new(OpCodes.Ret),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }

    // Modifica il getter del testo dell'interfono per aggiungere anche il messaggio del "Owner"
    [HarmonyPatch(typeof(Exiled.API.Features.Intercom), nameof(Exiled.API.Features.Intercom.DisplayText), MethodType.Getter)]
    public static class CommandIntercomTextGetterFix
    {
        public static bool Prefix(ref string __result)
        {
            // Conta i giocatori vivi per ciascun ruolo
            int classD = Player.List.Count(p => p.Role.Type == RoleTypeId.ClassD && p.IsAlive);
            int mtf = Player.List.Count(p => p.Role.Team == Team.FoundationForces && p.Role.Type.ToString().Contains("Cadet") && p.IsAlive);
            int ntf = Player.List.Count(p => p.Role.Team == Team.FoundationForces && p.Role.Type.ToString().Contains("NTF") && p.IsAlive);
            int chaos = Player.List.Count(p => p.Role.Team == Team.ChaosInsurgency && p.IsAlive);
            int guards = Player.List.Count(p => p.Role.Type == RoleTypeId.FacilityGuard && p.IsAlive);

            // Crea il testo da visualizzare sull'interfono
            __result =
                "-----INFORMAZIONI-----\n" +
                $"Class-D: {classD} vivi\n" +
                $"MTF: {mtf} vivi\n" +
                $"NTF: {ntf} vivi\n" +
                $"Chaos: {chaos} vivi\n" +
                $"Guardie: {guards} vivi\n";

            // Impedisce che il metodo originale venga eseguito
            return false;
        }
    }
}
