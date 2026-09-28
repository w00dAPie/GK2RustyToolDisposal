# Rusty Tool Disposal

Standalone BepInEx 5 / Harmony mod for Graveyard Keeper 2, targeting .NET Framework 4.7.2.

Version: **0.1.0**  
GUID: `de.w00dst0ckOo.gk2.rustytooldisposal`

The vanilla inventory **Destroy** action becomes available for a studied, obsolete rusty starter tool when all safety checks pass. Nothing is automatically deleted.

## Supported tools

| Internal item ID / localization key | English name | Required equipped replacement |
| --- | --- | --- |
| `axe_0` | Rusty Axe | `axe_1`, `axe_2`, or `axe_3` |
| `pickaxe_0` | Rusty Pickaxe | `pickaxe_1`, `pickaxe_2`, or `pickaxe_3` |
| `hammer_0` | Rusty Hammer | `hammer_1`, `hammer_2`, or `hammer_3` |
| `shovel_0` | Rusty Shovel | `shovel_1`, `shovel_2`, or `shovel_3` |
| `sword_0` | Rusty Sword | `sword_1`, `sword_2`, `sword_3`, or `sword_4` |

Rusty armor, upgraded tools, other weapons, and quest items keep their vanilla behavior. `rusty_sword` is only a localization entry in the inspected data, not a supported item ID.

## Use

1. Finish the introduction, including waking up at home.
2. Successfully study the rusty tool at the Study Table. All five verified recipes require 2 Science and retain the tool.
3. Equip an upgrade of the same type and leave the rusty tool in your inventory.
4. When idle and outside combat/tutorial mode, open the rusty tool's usual secondary-action context menu with mouse or controller and choose **Destroy**.

This game build destroys immediately: vanilla has **no confirmation dialog** here. The mod keeps that behavior. Simply studying, opening the menu, or closing it never deletes anything.

Study completion uses the native per-save survey record. Studying one copy unlocks eligibility for other copies of the same item ID; it does not study different tool IDs. Existing saves with recorded study completion work without studying again.

## Safety and compatibility

- Exact ordinal allowlist of five verified IDs.
- Native `GameBalance.GetSurveyDefForItemOrNull` and `KnowledgeSystem.IsSurveyCompleted`, with exact ingredient/recipe/station checks.
- Rejects default-unlocked, grouped, repeatable, decomposition, missing, or uncompleted survey data.
- Requires completed quests: `1_intro_prison_find_pickaxe`, `1_intro_prison_sarchphage_pickaxe_with`, `2_intro_scout_axe`, `3_intro_defence_no_armour`, and `4_intro_base_waking_up`.
- Blocks tutorial mode and requires an equipped verified upgrade, preserving general work capability.
- Requires the exact displayed object to belong to the local player's inventory, and rejects its UID in the toolbelt, interacting-item slot, overhead items, or retained active tool reference.
- Blocks while a work action/animation or fight is active. A weapon view still referencing that rusty weapon ID also blocks removal.
- Checks safety both while constructing the menu and when executing Destroy.
- Missing/ambiguous state or a thrown safety query leaves supported tools protected.
- **Single-player only.** Co-op and unavailable network state fail closed.
- Only the inspected game assembly, resources, and addressable catalog fingerprints are accepted. Updates require another audit; a mismatch logs a warning and installs no patches.
- Other mods that change the same methods or progression are not certified. Offline checks and fingerprints do not establish compatibility with arbitrary runtime modifications.

No custom save files, config-based study flags, global ItemDef mutation, or personal mod dependency. The mod has no delete loop and never calls inventory removal itself. Native trading, dropping, and equipment systems are unchanged.

See [INVESTIGATION.md](INVESTIGATION.md) for evidence and limits.

## Build and offline verification

Requirements: .NET SDK, .NET Framework 4.7.2 targeting pack, the inspected installed game, BepInEx 5 with Harmony, and Python 3 only if repeating the asset audit.

Default game location:

```text
G:\SteamLibrary\steamapps\common\Graveyard Keeper 2
```

From this project directory:

```powershell
dotnet tool restore
dotnet tool run csharpier format .
dotnet build -c Release
dotnet build .\Verification\GK2RustyToolDisposal.Verification.csproj -c Release
& .\Verification\bin\Release\net472\GK2RustyToolDisposal.Verification.exe
```

The tool manifest pins CSharpier 1.3.0. Local game/BepInEx references have `Private=false`; proprietary game assemblies are not packaged. No files from another personal mod are referenced.

To build against another installation path, pass `-p:GameDir="X:\path\to\Graveyard Keeper 2"`. This changes reference paths only; it does not bypass the verified-build check.

Offline verification exercises the actual native survey helper, quest status predicates, exact ID/upgrade scope, UID identity, actual target method IL, retained native instructions, and rejection of unfamiliar patch shapes. It does **not** execute Unity work/combat state or the in-game menu.

## Install

Exit the game, then run:

```powershell
New-Item -ItemType Directory -Force -Path 'G:\SteamLibrary\steamapps\common\Graveyard Keeper 2\BepInEx\plugins\GK2RustyToolDisposal' | Out-Null
Copy-Item -LiteralPath 'D:\Programming\GK2RustyToolDisposal\bin\Release\net472\GK2RustyToolDisposal.dll' -Destination 'G:\SteamLibrary\steamapps\common\Graveyard Keeper 2\BepInEx\plugins\GK2RustyToolDisposal\GK2RustyToolDisposal.dll' -Force
```

Only the plugin DLL is needed. To uninstall, exit the game and remove that DLL. There is no custom save data to remove. Items explicitly destroyed remain destroyed in the native save.

## Manual in-game smoke test

Use a disposable copy of a save. These tests have **not been run in game**.

1. Launch the inspected game with BepInEx and this DLL. Check `BepInEx\LogOutput.log` for the 0.1.0 loaded message and no Harmony errors.
2. For **each of the five IDs**, before study, equip the matching upgrade, open the rusty tool's secondary menu, and verify Destroy is disabled. Verify regular items retain their original Destroy behavior.
3. Start study and cancel before completion. Verify Destroy remains disabled.
4. Complete that exact tool's Study Table recipe. Verify the tool remains in inventory and native study completion is shown. Other unstudied rusty tool IDs must remain protected.
5. Leave the studied rusty tool equipped: its equipment slot must retain vanilla unequip behavior, without a Destroy action.
6. Unequip it without equipping an upgrade: Destroy must remain disabled. Equip the matching upgrade: Destroy becomes enabled once all introduction checks are complete.
7. With two copies, study one; both unequipped copies should become eligible. Destroy one explicitly and verify only that selected UID disappears, with the other copy and equipped upgrade intact.
8. Open and close the enabled menu without selecting Destroy; verify nothing is deleted. Then select Destroy and verify immediate native removal with inventory refresh and no stale selection/equipment state.
9. Repeat the menu/open/close/delete flow using a controller's secondary action. Verify primary action still equips the tool and toolbelt action still unequips it.
10. Test a tool in a tool bag through its normal inventory context menu. Verify native bag-transfer mode remains unchanged and no unrelated item is removed.
11. During work, a finishing tool animation, or combat, try opening the relevant menu if vanilla permits it. Destroy must remain blocked. After all active-use references clear, eligibility should return. Exercise a menu opened before a state change to verify the execution check still protects the tool.
12. In an early-introduction or tutorial-mode test save with study completion already present, verify disposal stays blocked. Complete the five prerequisite quests normally and leave tutorial mode; eligibility should return with the upgrade equipped.
13. Save after studying but before destroying, quit, reload, and verify eligibility persists. Destroy, save, quit, reload, and verify the selected tool stays removed without affecting other items. Switch to a save with no study completion and verify protection returns.
14. Check `armor_0`, upgraded tools, bows, pikes, normal equipment, and quest items: their behavior must match vanilla.
15. In a co-op session, verify the mod does not enable rusty-tool destruction.
16. On an isolated copy of a changed/unrecognized build, verify the startup warning and no enabled disposal. Do not modify your production installation for this test.

Build and offline checks are recorded in [VERIFICATION.md](VERIFICATION.md). Runtime success is not claimed.

