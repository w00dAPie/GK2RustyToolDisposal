# Changelog

## 0.2.1

- Remove strict game build hash validation that could disable the mod after unrelated game updates.
- Fix Harmony patch registration for the inventory destroy checks.
- Restore disposal functionality after the latest Graveyard Keeper 2 update.
- Keep vanilla protection behavior when the mod's safety checks fail.

## 0.2.0

- Add armor support using the same safety rules as the existing tools: native Study Table completion, completed introduction progression, and a verified better replacement currently equipped.
- Support the following armor upgrade paths:
  - `armor_0` can be replaced by `armor_1`, `armor_2`, or `armor_3`.
  - `armor_1` can be replaced by `armor_2` or `armor_3`.
  - `armor_4` is deliberately excluded.
- Allow discard checks when no network manager exists, which is the expected singleplayer case. If a network manager exists but is not initialized or indicates co-op, supported items remain protected.
- Add the `EnableDebugLogging` configuration option under the `90 - Debug` section. It defaults to `false`.
- When debug logging is enabled, log the reason why a supported item is either protected or allowed to be destroyed.
- Add migration support for the legacy `[Debug]` configuration section.
- Runtime validation:
  - All five rusty starter tools were successfully discarded in game.
  - `armor_0` was successfully discarded in game.
  - `armor_1` support is implemented but has not yet been confirmed through an in-game discard test.

## 0.1.0

- Enable the native Destroy action for five verified rusty starter tools after native Study Table completion.
- Require an equipped upgrade, completed introduction checks, and a safe ownership, equipment, and usage state.
- Recheck all safety conditions when the Destroy action is executed.
- Preserve the game's native mouse and controller callbacks as well as UID-based inventory removal.
- Fail closed on unsupported game builds, unavailable game state, or co-op sessions.
- Add no custom save data, global item mutations, or personal mod dependencies.