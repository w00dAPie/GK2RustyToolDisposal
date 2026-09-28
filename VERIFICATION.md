# Verification

- Target: net472; BepInEx 5; installed Harmony 2.9.0.0 (HarmonyX).
- Final required commands: `dotnet tool run csharpier format .`, then `dotnet build -c Release`.
- Final Release build: **0 warnings, 0 errors**.
- Offline harness: **135 checks passed** against installed native assemblies.
- Checks cover study-state predicates, quest states, allowlists, replacements, instance UID identity, actual patch-target IL, preservation of native instructions/removal, label/exception metadata, unexpected IL rejection, and inspected-file fingerprints.
- Full Unity work/combat/UI execution and actual Harmony detour installation are not exercised by the offline harness.
- Reproducible asset audit passed the exact GameBalance object boundary check (41,479,596).
- icon.png: original 256 x 256 RGB PNG, visually inspected.
- Release output contains only this plugin DLL and its PDB, with no copied game dependencies.
- Game files, saves, and installed plugin directory were not modified.
- No in-game run was performed. Follow the manual checklist in README.md.

The standalone .NET Framework harness loads trusted local assembly bytes through its resolver because downloaded BepInEx files may retain Windows origin metadata. Inert Items are used only for offline UID tests: constructing a real Item consults Unity's GameBalance. Engine-dependent state checks require the game runtime.


