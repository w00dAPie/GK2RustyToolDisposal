# Changelog

## 0.2.0

- Add `armor_0` (Rusty) and `armor_1` (Bronze) to the supported items. Both use the same rules as the tools: native Study Table completion, introduction checks, and a verified better armor worn in the toolbelt.
- Rusty armor is replaced by `armor_1`, `armor_2` or `armor_3`. Bronze armor is replaced by `armor_2` or `armor_3`. `armor_4` is deliberately not included.
- Allow the discard check when no network manager exists (single player). A manager that is co-op or not initialized still keeps items protected.
- Add a config file with `EnableDebugLogging` (section `90 - Debug`, default `false`). When enabled, the log explains why an item is protected or allowed.
- Add config migration for a legacy `[Debug]` section.
- Runtime validation: the rusty tools and `armor_0` were discarded successfully in game. `armor_1` has not been confirmed yet.

## 0.1.0

- Enable native Destroy for five verified rusty starter tools after native Study Table completion.
- Require an equipped upgrade, completed introduction checks, and safe ownership/equipment/use state.
- Recheck safety at execution; preserve native mouse/controller callbacks and UID removal.
- Fail closed on unknown game builds, unavailable state, or co-op.
- No custom save data, global item mutation, or personal mod dependencies.