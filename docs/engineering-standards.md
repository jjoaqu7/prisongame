# Engineering standards

Agreed September 25-26, 2026 (STD-01; project names, builds, versions and Git added in STD-02). These rules apply to every agent and to manual work. The evidence and the options considered are in the [engineering-standards research](research/engineering-standards/report.md); the decisions are recorded in the [development plan](development-plan.md#engineering-standards---agreed-september-25). Where tools enforce a rule, the tool wins; where this page and a tool disagree, fix the tool configuration and this page together.

## Enforcement and commands

| What | How it is enforced | Command |
| --- | --- | --- |
| Layout (indents, spacing, braces on their own line, one statement per line) | `PrisonGame/.editorconfig`, applied by `dotnet format` and by editors | Check: `tools/check.ps1`. Fix: `tools/format.ps1` |
| Naming and braces in new code (`Assets/PrisonGame`) | `.editorconfig` naming rules and `IDE0011`, checked by `dotnet format style` | Same. Braces are fixed automatically; names are fixed by hand |
| Every commit | Git pre-commit hook `.githooks/pre-commit` runs `tools/check.ps1 -Staged` and blocks the commit on failure | Enable once per clone: `git config core.hooksPath .githooks` |
| Unity best practices in new code | Microsoft.Unity.Analyzers, raised to warnings by a ruleset beside each new assembly | `tools/check.ps1` checks the rulesets; `-Unity` fails on any compiler or analyzer warning in new code |
| Compiles, edit-mode and play-mode tests pass | Required before a task is marked done | `tools/check.ps1 -Unity` (needs the Unity Editor open on the project) |
| No spaces in asset names | Checked in every `tools/check.ps1` run | Fix: `PrisonGame.EditorTools.AssetNaming.RemoveSpaces` in Unity |
| Existing prototype behavior | Runtime checks in Development builds and the Editor | See [Tests](#tests) |

- Do not bypass the hook (`git commit --no-verify`) unless the user asks.
- `tools/format.ps1 -Staged` formats only staged files; stage them again afterwards.
- The checks need the .NET SDK (installed: 10.0.401). Naming and brace checks also need the `.csproj` files Unity generates when the project is opened; without them they are skipped with a warning.

## Layout

- Microsoft .NET layout: 4-space indents, each brace on its own line, one statement per line, always use braces, even around one statement. `dotnet format` applies most of it.
- The formatter does not split a one-line block such as `void Reset() { a = 1; b = 2; }`, because that setting would also stretch every `{ get; set; }` property over four lines. Write such methods over several lines yourself.
- Keep lines under about 120 characters. The formatter does not wrap lines; this is checked in review.
- C# files use LF line endings (`.gitattributes` and `.editorconfig` agree).

## Naming

New code, meaning everything under `Assets/PrisonGame`, follows Microsoft's .NET convention:

| Element | Style | Example |
| --- | --- | --- |
| Types, methods, properties, events, public fields, namespaces | PascalCase | `FighterStateMachine`, `TryStartAttack`, `HitWindowOpen` |
| Interfaces | `I` + PascalCase | `IDamageable` |
| Private and internal fields, including `[SerializeField]` ones | `_camelCase` | `_maxHealth` |
| Private static fields, including `static readonly` | `s_camelCase` | `s_player` |
| Constants, any visibility | PascalCase | `Capacity` |
| Parameters and locals | camelCase | `deltaSeconds` |
| Booleans | Read as a question | `IsAlive`, `HitWindowOpen` |
| Events | Verb phrase; past tense for after the fact | `Damaged`, `WentDown` |

- Unity shows serialized fields in the Inspector without the `_` prefix, so `_maxHealth` still reads "Max Health".
- Renaming a serialized field loses its saved values in scenes and prefabs unless the field keeps `[FormerlySerializedAs("oldName")]`. Keep the attribute after renaming.
- Prototype code (`Assets/Prototype`, `Assets/Editor`) keeps its current names until a task rewrites a file. Then rename it to this convention, with `[FormerlySerializedAs]` on its serialized fields, and recheck the scenes that use it.

## Folders, assemblies and C# version

- New code goes in `PrisonGame/Assets/PrisonGame/Scripts/<Area>/Runtime/`, with one assembly definition per area (`PrisonGame.<Area>`). Tests go in `<Area>/Tests/EditMode/` or `<Area>/Tests/PlayMode/`, and Editor tools in `EditorTools/` (`PrisonGame.EditorTools`, Editor-only). Namespaces match assembly names. Every new assembly also gets its ruleset copy (see [Analyzers](#analyzers)).
- Dependencies point one way. `Core` depends on nothing. Game areas depend on `Core` and on each other only where needed, never in a cycle. The prototype (`Assembly-CSharp`) may use new code; new code must not use the prototype.
- Unity 6.3 compiles C# 9.0. Block-scoped `namespace X { }` only, because file-scoped namespaces are C# 10. No `init` setters, and no records in serialized types.

## Analyzers

- Microsoft.Unity.Analyzers 1.27.0 (MIT, [licence](../PrisonGame/Assets/PrisonGame/Scripts/Core/Runtime/Analyzers/LICENSE.txt)) sits in `Assets/PrisonGame/Scripts/Core/Runtime/Analyzers/`. It is labelled `RoslynAnalyzer` and disabled for every platform, so it never ships. It checks Unity-specific mistakes such as null propagation on Unity objects (`UNT0008`) and wrong message signatures (`UNT0006`).
- Its rules are suggestions by default, which the Unity Console does not show. New code raises them to warnings with a ruleset: every assembly under `Assets/PrisonGame` has `<AssemblyName>.ruleset` beside its `.asmdef`, an identical copy of the master `Assets/PrisonGame/Scripts/PrisonGame.ruleset`. Unity ignores a ruleset with any other name, does not follow ruleset includes, and does not read `.editorconfig` for compiler severities. So a new assembly needs its own copy; `tools/check.ps1` fails when one is missing or differs.
- `UNT0021` (make Unity messages protected) stays off, because private Unity messages are the common convention.
- The prototype also compiles with the analyzer but keeps default severities, so its findings stay hidden until a task rewrites a file.

## Game-rule code and co-op boundaries

These make release 2 co-op possible and let tests run without a scene (see the [release decision](development-plan.md#single-player-first-release-co-op-second-release---agreed)):

- Game rules are plain C# classes in their area's `Runtime` assembly. They take time and input as parameters and never read `Time`, `Input`, `Keyboard`, `Camera.main`, the HUD or `UnityEngine.Random`. The assembly boundaries make most of this a compile error.
- Input is turned into intents by one local adapter per player; the acting player or fighter is passed explicitly.
- Per-player state (health, held item, inventory) stays separate from shared world state (NPCs, doors, dropped items).
- Anything saved or referenced across sessions uses `StableId`, never GameObject names, instance IDs or `UnityEngine.EntityId`.
- Randomness in rules uses a seeded `System.Random` owned by that system.
- Presentation (animation, sound, effects, HUD) reacts to rule events and never changes rule state.
- No networking code until release 2 work starts.

## Project names and layout

- **No spaces** in file or folder names under `Assets`, because Unity's command-line tools mishandle them. Join the words in PascalCase instead (`Dex burgundy` became `DexBurgundy`). Rename through Unity, not the file system, so references survive; `AssetNaming.RemoveSpaces` does this in bulk.
- **Asset names:** PascalCase, with underscores only between the parts of a name (`Inmate_Burgundy`). No type prefixes; the Project window shows each asset's type.
- **New content** goes under `Assets/PrisonGame/`, in Unity's type folders, created when the first file needs them: `Art/Materials`, `Art/Models`, `Art/Textures`, `Audio`, `Prefabs`, `Scenes`, `Scripts`, `Settings`. Test and experiment scenes go in `Scenes/Sandbox/`, for example `Scenes/Sandbox/CombatSandbox.unity`.
- **Third-party content** stays in its own folder, never mixed into our folders, and each item is recorded in the [asset register](asset-register.md).
- **The prototype's task-named folders and scenes** (`Assets/Prototype/Duty01`, `Assets/Scenes/Duty01_Laundry.unity`) are a frozen record: docs, logs and build scripts refer to them. New work does not add to them.

## Builds and versions

- **Build Profiles:** `Windows Development` and later `Windows Release`, set up at ENCOUNTER-01 step 7. Output goes to `Builds/Windows-Development/` and `Builds/Windows-Release/`, replacing the old one-folder-per-milestone builds. The development profile holds every scene the checks need; the command-line check flags pick what runs.
- **Version:** Player Settings holds a Semantic Version, `0.MINOR.PATCH`, during development. MINOR goes up when a milestone is finished (0.2.0 when ENCOUNTER-01 closes the prototype loop), and PATCH for fixes. Tag the commit `v0.2.0`.
- **Archives:** a build given to anyone else is kept as `PrisonGame-<version>-windows-<development|release>.zip`.
- **Identity:** company name `Niuq`; product name `PrisonGame`, a working title. Together they form the Windows save folder `AppData/LocalLow/Niuq/PrisonGame`. Changing either later moves where saves are looked for, so settle the shipped title before the Steam release and plan a save migration if it changes.

## Git

- **Commit messages:** `<TASK-ID>: <imperative summary>`, for example `STD-02: rename spaced assets`. Add a body explaining why when it isn't obvious.
- **Branches:** `<task-id>` or `<task-id>-<topic>`, lowercase, for example `encounter-01`.
- **Large binaries:** new binary art and audio go through Git LFS, which `.gitattributes` handles automatically. This covers PSD, TGA, TIFF, EXR, HDR, FBX, OBJ, BLEND, OGG, FLAC, AIFF, MP4, MOV and WebM everywhere, and PNG, JPG, WAV and MP3 under `Assets/PrisonGame/`. Older PNG, JPG, WAV and MP3 files stay ordinary Git files. The LFS hooks are committed in `.githooks`. GitHub Free includes 10 GiB of LFS storage and 10 GiB of bandwidth a month.
- **Evidence files:** `docs/evidence/<taskid>-<what>-<where>.<ext>`, for example `std01-laundry-editor-checks.txt`.

## Comments

- Explain why, not what. Give each class a short comment on its role and what it must not do when that is not obvious.
- Record decisions and their sources in the docs, not in code comments.

## Tests

- New tests use Unity's Test Framework (NUnit):
  - One test assembly per game assembly and mode.
  - Each class carries `[Category("<Area>")]`.
  - Test names read as behavior: `AttacksDoNotHurtADownedFighter`.
  - Arrange-Act-Assert structure.
  - `[Test]` by default. `[UnityTest]` only when a test must wait frames.
- Every plain C# rules class gets edit-mode tests in the same task that adds it. Play-mode tests cover physics, scene wiring and frame-driven behavior.
- An unfocused Editor does not advance Play-mode frames. A check that needs real frames sets `Application.runInBackground = true` for its duration and restores it afterwards.
- Input tests must not depend on window focus or on someone using the computer. Set `InputSystem.settings.backgroundBehavior` to `IgnoreFocus` (otherwise simulated devices are disabled whenever Unity is not the focused app), and disable the real mouse and keyboard for the test (`InputIsolation` in the play-mode tests), so a real device cannot become `Mouse.current` or `Keyboard.current`. Restore both afterwards.
- A test that loads a scene unloads it in a `[UnityTearDown]`, so a failure cannot leak the scene into later tests.
- In the Editor, keyboards reach the game only while the Game view has focus. A check that simulates key presses must route input to the game for its run. To run a prototype runtime check in Editor Play mode, use `PrisonGame.EditorTools.PrototypeChecks.Start("<CheckClass>")`, which does this, then `PrototypeChecks.RestoreInput()` when its results file appears.
- Run edit-mode and play-mode tests with `tools/check.ps1 -Unity`, or in Unity's Test Runner window. Play-mode tests restart scripting, so the script starts them in the background and polls `test_status`. The check refuses to start while the Editor is in, entering or leaving Play mode, because a recompile then reloads scripts inside Play mode and every component loses its runtime state. It also accepts a test result only if the Unity console shows that run's own "Run finished" line, since a test command can return an earlier run's stored results (both added September 27 after exactly that happened).
- The prototype's runtime checks stay as the regression suite until replaced. They compile only into the Editor and Development builds and start with a command-line flag or from the Editor:
  - `LaundryStandaloneCheck`: `-laundry-check` in the WindowsLaundry build. It writes `LaundryCheck/results.txt` beside the build, or under `PrisonGame/` in the Editor.
  - `tools/test-windows-build.ps1` and `tools/test-windows-sample.ps1` for the other builds.
  - Replace one only when its system is substantially changed.
- Save a check's results under `docs/evidence/` and link them from the progress log.

## Unity Editor scripts

- New scene builders and Editor checks are static methods in the Editor-only `PrisonGame.EditorTools` assembly. They are callable from the Unity CLI, a menu item or `-executeMethod`, and follow this standard like any other code.
- A builder or check must:
  - refuse to run while the Editor is playing or compiling, or has unsaved scenes
  - never replace an existing scene or prefab; a builder that adds to an existing scene only adds, and refuses to run a second time
  - read everything it needs before closing or destroying anything, and save only after every step has succeeded
  - restore the scene that was open
  - write its results to `docs/evidence/`
- Loose `eval_file` snippets are for inspection and small one-off edits of a few API calls (for example setting the company name). Anything longer or reusable is a static method in `PrisonGame.EditorTools`. Always pass a timeout, because the CLI's default is 5 s: `eval_file <file> <milliseconds>`.
- The existing `tools/unity/*.cs` snippets are the record of how each scene was built. They are not formatted or converted.

## Formatting-only commits

- A formatting-only commit that only reformats code contains no renames or logic changes. Its hash goes into `.git-blame-ignore-revs`, so GitHub's line history skips it. Locally, run `git config blame.ignoreRevsFile .git-blame-ignore-revs` once.

## Before marking a task done

1. `tools/check.ps1 -Unity` passes: layout, naming, rulesets, compile without warnings in new code, and edit-mode and play-mode tests.
2. New rules code has edit-mode tests; changed behavior has updated checks.
3. Any affected existing runtime checks were rerun, with results in `docs/evidence/`.
4. The tracker and progress log record what was verified and what was not.

## Later

Agreed as later steps:
- Formatting hooks in Claude Code (`.claude/settings.json`) and Codex (`.codex/hooks.json`) that call `tools/format.ps1` after each edit, once tested on Windows.
- Unity's Code Coverage package.
- CI with GameCI or Unity Build Automation.
