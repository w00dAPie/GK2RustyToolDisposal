using System;
using LazyBearTechnology;

namespace GK2RustyToolDisposal.Helpers;

internal static class EquipmentSafetyHelper
{
    private static bool Fail(Item item, string reason)
    {
        if (Plugin.DebugEnabled)
        {
            Plugin.DebugState(
                "CanDispose:" + (item?.id ?? "null"),
                $"{item?.id ?? "null"} BLOCKED: {reason}"
            );
        }

        return false;
    }

    internal static bool SameInstance(Item left, Item right)
    {
        if (left == null || right == null)
            return false;
        if (ReferenceEquals(left, right))
            return true;
        return left.UniqueId != null
            && right.UniqueId != null
            && left.UniqueId.Guid != Guid.Empty
            && left.UniqueId.Guid == right.UniqueId.Guid;
    }

    internal static bool CanDispose(Item item, PlayerData player, PlayerController controller)
    {
        if (Plugin.DebugEnabled)
        {
            Plugin.Log?.LogInfo($"[Debug] Checking disposal eligibility for {item?.id ?? "null"}");
        }

        if (item?.UniqueId == null || item.UniqueId.Guid == Guid.Empty)
            return Fail(item, "item has no valid UniqueId");
        if (player?.Inventory?.Data == null)
            return Fail(item, "player inventory null");
        if (player.toolBeltInventory?.Data == null)
            return Fail(item, "toolbelt inventory null");

        // The vanilla action removes by UID. Verify that UID still belongs to the same displayed object.
        if (
            !player.Inventory.Data.TryGetItemInInventoryByGUID(item.UniqueId.Id, out var owned)
            || !ReferenceEquals(owned, item)
        )
            return Fail(item, "item not owned by player inventory (UID/instance mismatch)");
        if (player.toolBeltInventory.Data.TryGetItemInInventoryByGUID(item.UniqueId.Id, out _))
            return Fail(item, "item is in toolbelt");
        if (SameInstance(player.interactingItem, item))
            return Fail(item, "item is interactingItem");
        foreach (var overhead in player.OverheadItems)
            if (SameInstance(overhead, item))
                return Fail(item, "item is overhead item");

        var replacement = player.toolBeltInventory.GetItemByType(item.Definition.type);
        if (replacement == null || replacement.IsEmpty)
            return Fail(item, "no toolbelt item of type " + item.Definition.type);
        if (SameInstance(replacement, item))
            return Fail(item, "toolbelt item is the same instance");
        if (!RustyToolHelper.IsVerifiedUpgrade(item.id, replacement.id))
            return Fail(
                item,
                "toolbelt item '" + replacement.id + "' is not a verified upgrade of " + item.id
            );
        if (replacement.Definition.type != item.Definition.type)
            return Fail(item, "replacement type mismatch");

        var work = controller.PlayerWorkComponent;
        var tool = work == null ? null : work.ToolComponent;
        var attack = controller.AttackComponent;
        var fighting = LazySingleton<FightingGameController>.Instance;
        if (tool == null)
            return Fail(item, "ToolComponent null");
        if (attack == null)
            return Fail(item, "AttackComponent null");
        if (!attack.IsInitialized)
            return Fail(item, "AttackComponent not initialized");
        if (fighting == null)
            return Fail(item, "FightingGameController null");

        // StopInteraction may retain ToolInUse until the final animation callback.
        if (tool.IsActionActive)
            return Fail(item, "tool action active");
        if (tool.IsControlTakenByAnimation)
            return Fail(item, "control taken by animation");
        if (SameInstance(tool.ToolInUse, item))
            return Fail(item, "item is ToolInUse");
        if (fighting.CurrentFightState != FightState.Disabled)
            return Fail(item, "fight state = " + fighting.CurrentFightState);

        // Weapon views retain an ItemDef rather than an instance UID; block that whole ID conservatively.
        if (attack.weapon != null)
        {
            if (attack.weapon.ItemDef == null)
                return Fail(item, "weapon ItemDef null");
            if (attack.weapon.ItemDef.id == item.id)
                return Fail(item, "active weapon has same item id");
        }

        if (Plugin.DebugEnabled)
        {
            Plugin.DebugState("CanDispose:" + item.id, $"{item.id} ALLOWED");
        }

        return true;
    }
}
