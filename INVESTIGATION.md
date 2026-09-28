# Investigation — installed build, 2026-09-28

Completed before project files were created. Read-only analysis used ILSpy 11.1, Mono.Cecil metadata, and an in-memory Unity serialized-data reader. Existing decompilation was used only after its Assembly-CSharp SHA-256 matched the installed assembly. Neither that analysis directory nor any personal mod is a project dependency.

## Build fingerprints

| File | SHA-256 |
| --- | --- |
| Managed/Assembly-CSharp.dll | `7ACB243A08897D8CC7B67EF17AED50EEA17857494AEF4278F4F3B00D324823E5` |
| resources.assets | `FD64048DF834EC19DA725481A4CE703C79ABB98AE6808107182D82A78B87F1CC` |
| StreamingAssets/aa/catalog.bin | `47426C3BECBF16E51C07042E415A570CC8A79991CBEECC6D20A3D4940CF54475` |

Unity version in resources.assets: `6000.3.9f1`. Assembly-CSharp size: 4,039,680 bytes. The resources GameBalance object is path ID 2459, offset 37,160,416, length 4,319,180. Parsing all balance fields ended exactly at its object boundary, 41,479,596.

## Verified item data

| ID / display key | Name | ItemType | isTool / isWeapon | iconId | quality | talentBonus / branch | ItemDef offset | SurveyDef offset |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| axe_0 | Rusty Axe | Axe (1) | true / false | i_axe_0 | 0 | 1 / talent_orange | 37252768 | 39936888 |
| pickaxe_0 | Rusty Pickaxe | Pickaxe (3) | true / false | i_pickaxe_0 | 0 | 1 / talent_orange | 37254496 | 39939780 |
| hammer_0 | Rusty Hammer | Hammer (4) | true / false | i_hammer_0 | 0 | 1 / talent_red | 37256272 | 39942720 |
| shovel_0 | Rusty Shovel | Shovel (2) | true / false | i_shovel_0 | 0 | 1 / talent_green | 37259348 | 39947932 |
| sword_0 | Rusty Sword | Sword (11) | false / true | i_sword_t1 | 1 | 0 / none | 37266300 | 39959804 |

All offsets are byte offsets in resources.assets. All five have stackCount=1, itemSize=1, qualityType=None, canBeUsed unset/false, canNotBeDestroyed expression `1` (pure boolean true), isQuestItem=false, hasDurability=false, durDecreaseOnUse=0, canBeUsedInAlchemy=false, isLinkedToWgo=false, isProduct=true, basePrice=10, isStaticCost=true. On-use/drop/sell/buy expression lists are empty. Groups are `u_bag, tool` for the four tools and `u_bag, weapon, melee` for the sword. ItemDef.CanItemBeEquipped() returns true for all five.

The sword's stamina expression is `20*(1-$buff_berserk)`; damage is `4+2*$perk_swordmaster+3*$buff_damage+2*$buff_combat`. These are unrelated to disposal.

All 814 ItemDefs were decoded. Rusty Armor (`armor_0`) exists but is outside tool scope. `rusty_sword` occurs in localization keys, not in the decoded ItemDefs. No rusty fishing rod/pike or additional rusty starter tool was verified; none is added speculatively.

## Native restriction and removal flow

1. CharMainPageWidgetData binds player inventory secondary presses to PlayerInventoryUIItemOpHandler.OnPlayerInvItemPressed2.
2. UIItemCell.OnPress2 invokes OnItemCellPress2 for right mouse press or active gamepad. OnGamepadPress2 delegates to OnPress2.
3. OnPlayerInvItemPressed2 appends the existing localized `ui_destroy` option with enabled = !item.Definition.CanNotBeDestroyed. Its callback invokes TryDestroyItem(cell), then closes the native context menu.
4. TryDestroyItem rechecks !cell.DisplayingItem.Definition.CanNotBeDestroyed.
5. It calls playerData.Inventory.RemoveItemFromInventoryByUID(cell.DisplayingItem), using the default count=-1.
6. Inventory delegates to Item.RemoveItemFromInventoryByUID(Guid, int), then emits the native removal notification. The item method removes the selected UID, including native bag handling.

No confirmation window, equipment check, study check, or quest-flag check occurs in this native Destroy action. The constant ItemDef expression is the actual block. Removing from a toolbelt directly would not automatically clear active component references; the mod never does so.

The global getter also affects DropView collision behavior, which is one reason not to patch it globally. VendorTierData.IsBuyingProduct/IsSellingProduct use separate vendor lists. The blacksmith lists the rusty IDs among products and notSelling, not notBuying. Therefore these items are protected from native inventory destruction, not categorically from every possible inventory transaction. Native trade remains unchanged.

## Study Table state

- UIResourceBasedCraftWindowData builds an allowed-item list from SurveyDef.GetSurveyedItemDefs(), excluding completed one-time surveys.
- OnResourcePickerPressed/CreateCraftElement selects the concrete item ID; OnStartSurvey calls CraftComponent.TryStartCraft.
- Each supported item has its own `surv:<item ID>`, craftsIn=[survey_wgo], ingredient groupType=None, ingredient count=1, science fuel count=2, isOneTimeCraft=true, isScienceFuelCraft=false, surveyedAtStart=false, isHidden=false.
- The recipes have no Faith ingredient. Duration is `3-$perk_science`, energy per tick `2-$perk_science`. Reward: 10 red technology points, researcher inspiration, plus the recipe's conditional story output.
- CraftElementSurvey.RemoveCraftRequirements skips the first ingredient for ordinary study, retaining the studied tool.
- CraftElementBase.Finish calls knowledgeSystem.CompleteOneTimeCraft(Def) only if the craft was started. Cancel clears isStarted without recording completion.
- KnowledgeSystem.CompleteOneTimeCraft adds the survey ID to `oneTimeCompletedCrafts`.
- KnowledgeSystem.IsSurveyCompleted checks that list for one-time surveys, but also returns true for surveyedAtStart. The mod explicitly rejects surveyedAtStart so default knowledge cannot substitute for studying.
- CraftElementBase's eligibility checks and the study picker prevent another completed one-time survey.
- GameSave.knowledgeSystem is native serializable state. SaveSystem uses OdinBinaryFileSerializer for GameSave; the public completion list survives serialization and loading. No custom data is needed.

Completion is per survey ID in the save, shared across duplicate instances of each supported ID. It is not per UID, item category, Science balance, or Faith balance. Completion hides the study option later; missing resources/tool/access can also prevent starting study without implying completion.

## Equipment and active use

PlayerData.toolBeltInventory contains equipped instances. TryEquipItem/TryUnEquipItem transfer items by UID between inventory and toolbelt. The toolbelt UI binds both presses to unequip, not Destroy.

PlayerWorkComponent obtains work tools from the toolbelt. ToolComponent retains ToolInUse and exposes IsActionActive and IsControlTakenByAnimation. StopInteraction can finish asynchronously with the last animation loop, so merely checking the equipment slot is insufficient.

AttackComponent.weapon retains a Weapon whose ItemDef describes its weapon, rather than a selected inventory UID. The implementation conservatively blocks that ID when still referenced and blocks all fights. It also checks interacting and overhead item references, inventory ownership by UID and reference identity, and an equipped upgrade of the same type.

The safety query must belong to MainGame.PlayerData, GameSave.playerData, and the local PlayerController. Co-op is rejected because these local checks do not prove remote-player use.

## Quest/tutorial audit

Decoded all 538 QuestDefs, dialogue object DialogData, balance progression/tech/craft/achievement data, and searched current game code. Scanned serialized content in all 25,261 addressable bundles using UnityFS block decompression: 91 matching bundles, zero decode errors. Texture/audio streaming payloads were excluded. Exact-token matches avoid false positives such as the stone workbench `mf_hammer_0`.

Relevant authored quest references:

| Quest | Reference |
| --- | --- |
| 1_intro_prison_find_pickaxe | Drops pickaxe_0 on completion |
| 1_intro_prison_sarchphage_pickaxe_with | Requires HasPlayerItemEquip("pickaxe_0", 1) |
| 2_intro_scout_axe | Drops axe_0 on completion |
| 3_intro_defence_no_armour | Checks sword_0 and armor_0 in inventory/equipment |

The prison's `1_intro_prison_sarchphage_removed` completion explicitly completes the pickaxe check. `3_intro_defence_battle` explicitly completes the armor/sword check. `4_intro_base_waking_up` records later introductory progression and unlocks general systems.

The implementation requires all four listed quests and waking-up completion, rejects tutorial mode, and requires an equipped replacement. Requiring actual Completed status, rather than just absence of an active quest, protects missing/uninitialized and awaiting states.

No later exact-ID requirement appeared in the inspected data. Bundle matches include default sword view references, starter inventory/chest data, Test_RunEvents tool grants, and Test_TownFights sword grants. No exact-ID removal or requirement appeared in those matched test graphs. A test corpse balance definition references axe_0 in handsId. General tool-type work checks remain relevant after study, hence the upgrade requirement.

This is a static audit of this installed build, not proof about arbitrary external mods or a runtime playthrough.

## Patch design and project

DiscardEligibilityPatch rewrites exactly one ItemDef.CanNotBeDestroyed call in each of:

- PlayerInventoryUIItemOpHandler.OnPlayerInvItemPressed2(UIItemCell)
- PlayerInventoryUIItemOpHandler.TryDestroyItem(UIItemCell)

It supplies the original ItemDef, owning handler's private playerData field, and current cell to a shared safety predicate. Branch labels/exception metadata are preserved. The rest of each method is retained, including menu delegates, mouse/controller flow, and native UID removal.

Unsupported items evaluate the original getter. Supported items require every guard; errors return protected. Unfamiliar IL aborts installation and rolls back this mod's patches. Startup checks assembly/resources/catalog SHA-256 and fails closed on mismatch.

Files follow the requested Plugin/Helpers/Patches layout. GameBuildHelper adds compatibility protection. Verification contains an offline harness and reproducible read-only audit utilities. Metadata, MIT license, icon, formatter manifest, and documentation are included. The plan is implemented without copying game source into the project or depending on another personal mod.

Runtime UX deliberately uses the existing enabled/disabled menu option. UIContextMenuWindowWidgetData exposes name, callback, and enabled, but no disabled-reason field was found; no custom tooltip/confirmation/deletion system is added.

