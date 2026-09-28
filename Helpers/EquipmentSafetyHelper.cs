using System;
using LazyBearTechnology;

namespace GK2RustyToolDisposal.Helpers;

internal static class EquipmentSafetyHelper
{
    private static bool Fail(string reason)
    {
        if (Plugin.DebugEnabled)
            Plugin.DebugState("CanDispose", "CanDispose FAIL: " + reason);
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
        if (item?.UniqueId == null || item.UniqueId.Guid == Guid.Empty)
            return Fail("item has no valid UniqueId");
        if (player?.Inventory?.Data == null)
            return Fail("player inventory null");
        if (player.toolBeltInventory?.Data == null)
            return Fail("toolbelt inventory null");

        // The vanilla action removes by UID. Verify that UID still belongs to the same displayed object.
        if (
            !player.Inventory.Data.TryGetItemInInventoryByGUID(item.UniqueId.Id, out var owned)
            || !ReferenceEquals(owned, item)
        )
            return Fail("item not owned by player inventory (UID/instance mismatch)");
        if (player.toolBeltInventory.Data.TryGetItemInInventoryByGUID(item.UniqueId.Id, out _))
            return Fail("item is in toolbelt");
        if (SameInstance(player.interactingItem, item))
            return Fail("item is interactingItem");
        foreach (var overhead in player.OverheadItems)
            if (SameInstance(overhead, item))
                return Fail("item is overhead item");

        var replacement = player.toolBeltInventory.GetItemByType(item.Definition.type);
        if (replacement == null || replacement.IsEmpty)
            return Fail("no toolbelt item of type " + item.Definition.type);
        if (SameInstance(replacement, item))
            return Fail("toolbelt item is the same instance");
        if (!RustyToolHelper.IsVerifiedUpgrade(item.id, replacement.id))
            return Fail(
                "toolbelt item '" + replacement.id + "' is not a verified upgrade of " + item.id
            );
        if (replacement.Definition.type != item.Definition.type)
            return Fail("replacement type mismatch");

        var work = controller.PlayerWorkComponent;
        var tool = work == null ? null : work.ToolComponent;
        var attack = controller.AttackComponent;
        var fighting = LazySingleton<FightingGameController>.Instance;
        if (tool == null)
            return Fail("ToolComponent null");
        if (attack == null)
            return Fail("AttackComponent null");
        if (!attack.IsInitialized)
            return Fail("AttackComponent not initialized");
        if (fighting == null)
            return Fail("FightingGameController null");

        // StopInteraction may retain ToolInUse until the final animation callback.
        if (tool.IsActionActive)
            return Fail("tool action active");
        if (tool.IsControlTakenByAnimation)
            return Fail("control taken by animation");
        if (SameInstance(tool.ToolInUse, item))
            return Fail("item is ToolInUse");
        if (fighting.CurrentFightState != FightState.Disabled)
            return Fail("fight state = " + fighting.CurrentFightState);

        // Weapon views retain an ItemDef rather than an instance UID; block that whole ID conservatively.
        if (attack.weapon != null)
        {
            if (attack.weapon.ItemDef == null)
                return Fail("weapon ItemDef null");
            if (attack.weapon.ItemDef.id == item.id)
                return Fail("active weapon has same item id");
        }
        return true;
    }
}