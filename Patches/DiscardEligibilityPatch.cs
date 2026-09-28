using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using GK2RustyToolDisposal.Helpers;
using HarmonyLib;

namespace GK2RustyToolDisposal.Patches;

[HarmonyPatch]
internal static class DiscardEligibilityPatch
{
    internal static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(
            typeof(PlayerInventoryUIItemOpHandler),
            "OnPlayerInvItemPressed2",
            new[] { typeof(UIItemCell) }
        ) ?? throw new MissingMethodException("OnPlayerInvItemPressed2");
        yield return AccessTools.Method(
            typeof(PlayerInventoryUIItemOpHandler),
            "TryDestroyItem",
            new[] { typeof(UIItemCell) }
        ) ?? throw new MissingMethodException("TryDestroyItem");
    }

    [HarmonyTranspiler]
    internal static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions
    )
    {
        var code = instructions.ToList();
        var getter = AccessTools.PropertyGetter(typeof(ItemDef), nameof(ItemDef.CanNotBeDestroyed));
        var player = AccessTools.Field(typeof(PlayerInventoryUIItemOpHandler), "playerData");
        var check = AccessTools.Method(
            typeof(RustyToolHelper),
            nameof(RustyToolHelper.CanNotBeDestroyed)
        );
        if (
            getter == null
            || player == null
            || check == null
            || code.Count(i => i.Calls(getter)) != 1
        )
            throw new InvalidOperationException(
                "Expected exactly one native destroy check; refusing an unfamiliar method."
            );

        var result = new List<CodeInstruction>();
        foreach (var instruction in code)
        {
            if (!instruction.Calls(getter))
            {
                result.Add(instruction);
                continue;
            }
            // The ItemDef is already on the stack. Add owning player and cell without changing the callback/removal.
            var first = new CodeInstruction(OpCodes.Ldarg_0);
            first.labels.AddRange(instruction.labels);
            first.blocks.AddRange(instruction.blocks);
            result.Add(first);
            result.Add(new CodeInstruction(OpCodes.Ldfld, player));
            result.Add(new CodeInstruction(OpCodes.Ldarg_1));
            result.Add(new CodeInstruction(OpCodes.Call, check));
        }
        return result;
    }
}
