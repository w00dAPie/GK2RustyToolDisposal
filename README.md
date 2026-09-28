# Rusty Tool Disposal

Standalone BepInEx 5 / Harmony mod for Graveyard Keeper 2, targeting .NET Framework 4.7.2.

Version: **0.2.0**  
GUID: `de.w00dst0ckOo.gk2.rustytooldisposal`

The vanilla inventory **Destroy** action becomes available for a studied, obsolete rusty starter tool or Rusty Armor when all safety checks pass. Nothing is automatically deleted.

## Supported items

| Internal item ID | English name | Required equipped replacement |
| --- | --- | --- |
| `axe_0` | Rusty Axe | Bronze Axe (`axe_1`), Iron Axe (`axe_2`), or Steel Axe (`axe_3`) |
| `pickaxe_0` | Rusty Pickaxe | Bronze Pickaxe (`pickaxe_1`), Iron Pickaxe (`pickaxe_2`), or Steel Pickaxe (`pickaxe_3`) |
| `hammer_0` | Rusty Hammer | Bronze Hammer (`hammer_1`), Iron Hammer (`hammer_2`), or Steel Hammer (`hammer_3`) |
| `shovel_0` | Rusty Shovel | Bronze Shovel (`shovel_1`), Iron Shovel (`shovel_2`), or Steel Shovel (`shovel_3`) |
| `sword_0` | Rusty Sword | Bronze Sword (`sword_1`), Iron Sword (`sword_2`), Steel Sword (`sword_3`), or Master Sword (`sword_4`) |
| `armor_0` | Rusty Armor | Bronze Armor (`armor_1`), Iron Armor (`armor_2`), or Steel Armor (`armor_3`) |

A replacement counts only when it is worn in the toolbelt. Upgraded tools and armor, other weapons, and quest items keep their vanilla behavior. `rusty_sword` is only a localization entry in the inspected data, not a supported item ID.

## Use

1. Finish the introduction, including waking up at home.
2. Successfully study the rusty item at the Study Table. The item is kept.
3. Wear an upgrade of the same type in the toolbelt (tool, weapon, or body armor) and leave the rusty item in your inventory.
4. When idle and outside combat/tutorial mode, open the rusty item's usual secondary-action context menu with mouse or controller and choose **Destroy**.

This game build destroys immediately: vanilla has **no confirmation dialog** here. The mod keeps that behavior. Simply studying, opening the menu, or closing it never deletes anything.

Study completion uses the native per-save survey record. Studying one copy unlocks eligibility for other copies of the same item ID; it does not study different item IDs. Existing saves with recorded study completion work without studying again.

## Configuration

`BepInEx\config\de.w00dst0ckOo.gk2.rustytooldisposal.cfg` is created on first start.

| Section | Key | Default | Purpose |
| --- | --- | --- | --- |
| `90 - Debug` | `EnableDebugLogging` | `false` | Logs why a supported item is protected or allowed. Each message is written only when it changes. |

With debug logging off, the mod only writes its startup message, warnings, and errors.

## Safety and compatibility

- Exact ordinal allowlist of six verified IDs.
- Native `GameBalance.GetSurveyDefForItemOrNull` and `KnowledgeSystem.IsSurveyCompleted`, with exact ingredient/recipe/station checks.
- Rejects default-unlocked, grouped, repeatable, decomposition, missing, or uncompleted survey data.
- Requires completed quests: `1_intro_prison_find_pickaxe`, `1_intro_prison_sarchphage_pickaxe_with`, `2_intro_scout_axe`, `3_intro_defence_no_armour`, and `4_intro_base_waking_up`.
- Blocks tutorial mode and requires a verified upgrade worn in the toolbelt, preserving general work and combat capability.
- Requires the exact displayed object to belong to the local player's inventory, and rejects its UID in the toolbelt, interacting-item slot, overhead items, or retained active tool reference.
- Blocks while a work action/animation or fight is active. A weapon view still referencing the same item ID also blocks removal.
- Checks safety both while constructing the menu and when executing Destroy.
- Missing/ambiguous state or a thrown safety query leaves supported items protected.
- **Single-player only.** Co-op fails closed. If no network manager exists (normal single player), the check passes. A manager that exists but is not initialized also fails closed.
- Only the inspected game assembly, resources, and addressable catalog fingerprints are accepted. Updates require another audit; a mismatch logs a warning and installs no patches.
- Other mods that change the same methods or progression are not certified. Offline checks and fingerprints do not establish compatibility with arbitrary runtime modifications.

No custom save files, config-based study flags, global ItemDef mutation, or personal mod dependency. The mod has no delete loop and never calls inventory removal itself. Native trading, dropping, and equipment systems are unchanged.

## Build and offline verification

Requirements: .NET SDK, .NET Framework 4.7.2 targeting pack, the inspected installed game, BepInEx 5 with Harmony, and Python 3 only if repeating the asset audit.

Default game location:

```text
x:\<path to>\Graveyard Keeper 2
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

Exit the game, then copy `GK2RustyToolDisposal.dll` from `bin\Release\net472\` into your BepInEx plugins folder:

```text
<game folder>\BepInEx\plugins\
```

A subfolder such as `plugins\GK2RustyToolDisposal\` also works but is optional. Keep only one copy of the DLL under `BepInEx\plugins`, otherwise BepInEx skips one of them with a warning.

Example (PowerShell, adjust the paths to your setup):

```powershell
Copy-Item -LiteralPath '.\bin\Release\net472\GK2RustyToolDisposal.dll' -Destination '<game folder>\BepInEx\plugins\' -Force
```

Only the plugin DLL is needed. To uninstall, exit the game and remove the DLL (and optionally the `.cfg` file). There is no custom save data to remove. Items explicitly destroyed remain destroyed in the native save.