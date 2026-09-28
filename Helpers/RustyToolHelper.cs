using System;
using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;

namespace GK2RustyToolDisposal.Helpers;

internal static class RustyToolHelper
{
    private static readonly HashSet<string> SupportedIds = new HashSet<string>(
        StringComparer.Ordinal
    )
    {
        "axe_0",
        "pickaxe_0",
        "hammer_0",
        "shovel_0",
        "sword_0",
        "armor_0",
        "armor_1",
    };

    private static readonly string[] RequiredQuests =
    {
        "1_intro_prison_find_pickaxe",
        "1_intro_prison_sarchphage_pickaxe_with",
        "2_intro_scout_axe",
        "3_intro_defence_no_armour",
        "4_intro_base_waking_up",
    };

    private static bool loggedFailure;

    private static bool Block(string id, string reason)
    {
        if (Plugin.DebugEnabled)
            Plugin.DebugState(id, id + " BLOCKED: " + reason);
        return true;
    }

    private static string JoinAny(IEnumerable values)
    {
        if (values == null)
            return "null";
        var parts = new List<string>();
        foreach (var v in values)
            parts.Add(v == null ? "null" : v.ToString());
        return string.Join(",", parts.ToArray());
    }

    internal static bool IsSupported(string id) => id != null && SupportedIds.Contains(id);

    internal static bool IsVerifiedUpgrade(string rustyId, string upgradeId)
    {
        switch (rustyId)
        {
            case "armor_0":
                return upgradeId == "armor_1" || upgradeId == "armor_2" || upgradeId == "armor_3";
            case "armor_1":
                return upgradeId == "armor_2" || upgradeId == "armor_3";
            case "axe_0":
                return upgradeId == "axe_1" || upgradeId == "axe_2" || upgradeId == "axe_3";
            case "pickaxe_0":
                return upgradeId == "pickaxe_1"
                    || upgradeId == "pickaxe_2"
                    || upgradeId == "pickaxe_3";
            case "hammer_0":
                return upgradeId == "hammer_1"
                    || upgradeId == "hammer_2"
                    || upgradeId == "hammer_3";
            case "shovel_0":
                return upgradeId == "shovel_1"
                    || upgradeId == "shovel_2"
                    || upgradeId == "shovel_3";
            case "sword_0":
                return upgradeId == "sword_1"
                    || upgradeId == "sword_2"
                    || upgradeId == "sword_3"
                    || upgradeId == "sword_4";
            default:
                return false;
        }
    }

    internal static bool HasCompletedRequiredQuests(QuestSystemData quests, bool tutorialActive)
    {
        if (tutorialActive || quests?.questCollection?.questsCache == null)
            return false;
        foreach (string id in RequiredQuests)
            if (!quests.IsQuestInStatus(id, QuestStatus.Completed))
                return false;
        return true;
    }

    // This replaces only the two inventory UI checks, never the global ItemDef getter.
    internal static bool CanNotBeDestroyed(ItemDef definition, PlayerData player, UIItemCell cell)
    {
        if (definition == null)
            return true;
        if (!IsSupported(definition.id))
            return definition.CanNotBeDestroyed;

        var id = definition.id;
        try
        {
            var item = cell == null ? null : cell.DisplayingItem;
            var game = MainGame.Instance;
            var save = game == null ? null : game.GameSave;
            var controller = MainGame.PlayerController;

            if (!Plugin.Ready)
                return Block(id, "Plugin.Ready=false");
            if (item == null || item.IsEmpty)
                return Block(id, "item null/empty");
            if (item.Count != 1)
                return Block(id, "Count=" + item.Count);
            if (!ReferenceEquals(item.Definition, definition))
                return Block(id, "Definition mismatch");
            if (definition.isQuestItem)
                return Block(id, "isQuestItem");
            if (definition.isLinkedToWgo)
                return Block(id, "isLinkedToWgo");
            if (!definition.CanItemBeEquipped())
                return Block(id, "CanItemBeEquipped=false");
            if (player == null || save == null || controller == null)
                return Block(id, "player/save/controller null");
            if (
                !ReferenceEquals(player, save.playerData)
                || !ReferenceEquals(player, MainGame.PlayerData)
                || !ReferenceEquals(player, controller.PlayerData)
            )
                return Block(id, "PlayerData reference mismatch");

            // No manager means no network session exists. A manager that is co-op or not
            // initialized is treated as unsafe.
            var nm = LazyNetwork.NetworkManager;
            if (Plugin.DebugEnabled)
                Plugin.DebugState(
                    "net",
                    "net: IsInitialized="
                        + LazyNetwork.IsInitialized
                        + " manager="
                        + (nm == null ? "null" : "set")
                        + " IsCoop="
                        + (nm == null ? "n/a" : nm.IsCoopGame.ToString())
                );
            if (nm != null && nm.IsCoopGame)
                return Block(id, "IsCoopGame");
            if (!LazyNetwork.IsInitialized && nm != null)
                return Block(id, "network manager exists but is not initialized");

            if (player.isInTutorialMode)
                return Block(id, "tutorial mode");
            if (save.questSystemData?.questCollection?.questsCache == null)
                return Block(id, "questsCache null");
            foreach (var q in RequiredQuests)
                if (!save.questSystemData.IsQuestInStatus(q, QuestStatus.Completed))
                    return Block(id, "quest not Completed: " + q);

            var survey = GameBalance.GetSurveyDefForItemOrNull(id);
            if (survey == null)
                return Block(id, "no survey for item");
            var knowledge = save.knowledgeSystem;
            if (Plugin.DebugEnabled)
                Plugin.DebugState(
                    id + ":survey",
                    id
                        + " survey: id="
                        + survey.id
                        + " oneTime="
                        + survey.isOneTimeCraft
                        + " sciFuel="
                        + survey.isScienceFuelCraft
                        + " atStart="
                        + survey.surveyedAtStart
                        + " craftsIn="
                        + JoinAny(survey.craftsIn)
                        + " surveyed="
                        + (survey.SurveyedItem == null ? "null" : survey.SurveyedItem.id)
                        + " completedCrafts="
                        + (
                            knowledge?.oneTimeCompletedCrafts == null
                                ? "null"
                                : knowledge.oneTimeCompletedCrafts.Contains(survey.id).ToString()
                        )
                        + " IsSurveyCompleted="
                        + (
                            knowledge == null
                                ? "null"
                                : knowledge.IsSurveyCompleted(survey).ToString()
                        )
                );
            if (!StudyStateHelper.HasBeenStudied(id, survey, knowledge))
                return Block(id, "not studied at the Study Table");

            if (!EquipmentSafetyHelper.CanDispose(item, player, controller))
                return Block(id, "equipment safety check failed (see CanDispose)");

            if (Plugin.DebugEnabled)
                Plugin.DebugState(id, id + " ALLOWED");
            return false;
        }
        catch (Exception ex)
        {
            if (!loggedFailure)
            {
                loggedFailure = true;
                Plugin.Log?.LogWarning(
                    "Could not prove discard safety; keeping the item protected. " + ex
                );
            }
            return true;
        }
    }
}
