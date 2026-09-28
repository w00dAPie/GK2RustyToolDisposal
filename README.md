# Rusty Tool Disposal

A small quality-of-life mod for **Graveyard Keeper 2** that allows obsolete rusty starter equipment to be safely destroyed through the game's existing **Destroy** action.

Nothing is deleted automatically, and the mod does not globally disable item protection.

## Features

Supported equipment:

- Rusty Axe
- Rusty Pickaxe
- Rusty Hammer
- Rusty Shovel
- Rusty Sword
- Rusty Armor

A supported rusty item can only be destroyed when:

- it has been successfully studied at the Study Table
- the relevant introduction/tutorial progression has been completed
- an upgraded replacement of the same type is equipped
- the rusty item itself is not equipped or currently in use
- the player is not performing a work action
- the player is not in combat
- the game is running in singleplayer

The mod uses the game's existing inventory menu and native Destroy handling.

It does not automatically remove items, modify global item protection, or add custom save data.

## Safety

The mod is intentionally conservative.

If the required game state cannot be verified, the rusty item remains protected.

Safety checks are performed both when the inventory menu is opened and again when **Destroy** is executed.

The mod also verifies the supported game build before installing its patches. Unsupported game versions remain unchanged.

## Installation

Install **BepInEx 5**, then place:

`GK2RustyToolDisposal.dll`

inside:

`BepInEx/plugins/GK2RustyToolDisposal/`

Placing the DLL directly inside `BepInEx/plugins/` also works.

Alternatively, install the mod through **Thunderstore / r2modman**.

## Configuration

A configuration file is created automatically:

`BepInEx/config/de.w00dst0ckOo.gk2.rustytooldisposal.cfg`

Available option:

- `EnableDebugLogging = false`

Enable it if you want the log to explain why a supported item is currently protected or allowed to be destroyed.

## Compatibility

- Graveyard Keeper 2
- BepInEx 5
- Singleplayer

**Co-op is intentionally blocked** until multiplayer safety has been verified.

## Uninstalling

Remove `GK2RustyToolDisposal.dll`.

The configuration file can also be removed if desired.

The mod does not create custom save data.

**Items that were manually destroyed through the normal game inventory remain destroyed in the save.** 