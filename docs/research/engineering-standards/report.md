# Standardize with one written standard, one formatter and one commit gate

Status: updated, 2026-09-25 (RESEARCH-02). Evidence: [notes](notes/). Conventions: [research README](../README.md).

The project should standardize in three layers, strongest first. **Tools enforce what they can.** A shared `.editorconfig` holds the rules. Microsoft's `dotnet format` applies them from the command line, and editors apply the same file. The compiler's analyzers flag naming and Unity mistakes, and a Git pre-commit hook refuses commits that fail the fast checks, whichever agent or person commits. **One written standard covers the rest.** It sits in the root `AGENTS.md`, the one file both Claude Code and Codex are guaranteed to read, and links a fuller `docs/engineering-standards.md`. **Tests follow one framework for new work.** New tests use Unity's Test Framework, while Codex's executable checks stay as the regression suite until replaced. Each recommendation now rests on what C# and Unity developers actually do: Microsoft's and Google's published standards, Unity's own repositories and sample game, and the tooling that Unity builds and CI require. All proposals below were **Agreed** by the user on 2026-09-25 and 2026-09-26 ([development plan](../../development-plan.md#engineering-standards---agreed-september-25)); the rules are applied in [Engineering standards](../../engineering-standards.md).

Terms used once: a **formatter** rewrites code layout (spacing, line breaks, braces) without changing behavior. An **analyzer** is a compiler plugin that reports rule violations as warnings or errors. A **pre-commit hook** is a script Git runs before each commit; if it fails, the commit is refused.

## What differs today

Measured in the repository on 2026-09-25 ([measurements](notes/csharp-style-and-analysis.md#4-how-far-apart-are-the-two-agents-code-today)):

| Area | Prototype (Codex) | Combat (Claude) | Standard needed |
| --- | --- | --- | --- |
| Layout | Clean early files; later files put several statements on a line (in `LaundryHud`, 12% of lines hold three or more) | One statement per line | Formatter decides, not the agent |
| Naming | PascalCase public, camelCase private, no prefixes | Same | Microsoft .NET convention for new code (agreed) |
| Structure | One namespace; components find each other at runtime; the HUD reads keys and moves inventory items | Assemblies; the compiler blocks input and UI access from rules | Folder and assembly rules, co-op boundaries |
| Tests | 106+ runtime checks run in the Editor and in the executable | 28 NUnit edit-mode tests | One framework for new tests; keep the old checks |
| Editor scripts | 52 dense `eval_file` snippets | 3, copying that style | Compiled static methods in an Editor assembly |

## Put the rules where both tools read them

Codex loads `AGENTS.md` files only from the Git root down to the folder it was started in, and stops at a 32 KiB total by default. A nested `AGENTS.md` deeper in the tree is not read when Codex starts at the root, and a request to change that is still open ([agent-enforcement notes](notes/agent-enforcement.md#1-where-must-shared-rules-live-so-both-tools-read-them)). Claude Code reads the root `AGENTS.md` because the project has no `CLAUDE.md`, and its docs say instructions are "context, not enforced configuration" (same notes).

**Proposed:**
- Keep one root `AGENTS.md`. Add a short "Code and tests" section: the handful of rules that matter every session, the exact check and test commands, and what "done" requires.
- Put the full standard in `docs/engineering-standards.md`, linked from `AGENTS.md`. It covers naming, folders and assemblies, the co-op boundaries already agreed in the [development plan](../../development-plan.md#encounter-01-build-plan---september-25), comments, tests, Editor scripts, evidence files and commits.
- Do not add a `CLAUDE.md` or nested `AGENTS.md` files. The first would stop Claude Code reading `AGENTS.md`; the second would be invisible to Codex.

## Let tools enforce layout and common mistakes

**Rules file and formatter: `.editorconfig` with `dotnet format`** ([practice notes](notes/practice-for-remaining-decisions.md#1-which-formatter-do-unity-developers-and-c-teams-use-decision-2)). Unity's own graphics repository keeps a detailed root `.editorconfig`: LF line endings, 4-space indents, each brace on its own line. It names no separate formatter, so editors apply that file. Microsoft's `dotnet format` is the command-line tool that reads the same file. It ships with the .NET SDK already installed here, and a trial on this project worked. It checked the new code without changes, loaded Unity's generated project file, and on a copy of the prototype split crammed statements and spaced operators (`if(working!=lastWork ...)` became `if (working != lastWork ...)`). `csharp_preserve_single_line_statements = false` and `csharp_preserve_single_line_blocks = false` give one statement per line. It does not wrap long lines, so the standard sets a 120-character guideline checked in review. CSharpier ([style notes](notes/csharp-style-and-analysis.md#3-which-formatter-fits-a-unity-project-edited-by-ai-agents)) reprints everything and wraps at 100 columns, but it is an extra tool whose style editors do not share. It is the alternative, not the proposal. The earlier draft proposed it on documentation alone.

**Analyzers: Microsoft.Unity.Analyzers, scoped to the new code** ([trial notes](notes/analyzer-setup-trial.md)). Version 1.27.0 is built against Roslyn 3.11, and Unity 6.3 compiles with Roslyn 4.3, so it loads. Placed in the `Core` folder, it reaches every assembly that references `Core`, including the prototype. Its rules are suggestions by default, which the Unity Console never shows. A test in this project corrected two points in the earlier notes. Unity did not pass `.editorconfig` to its compiler, and it applied a ruleset only when the file was named after its assembly (`PrisonGame.Combat.ruleset`), with includes ignored. So each new-code assembly carries an identical `<AssemblyName>.ruleset` copy that raises the rules to warnings, and the prototype stays at default severity.

**Naming: Microsoft's .NET convention, Agreed** ([naming notes](notes/naming-in-practice.md); recorded in the [development plan](../../development-plan.md#engineering-standards---agreed-september-25)). The rules are `_camelCase` private fields, `s_camelCase` private statics, and PascalCase constants and public members, with each brace on its own line and one statement per line. Microsoft's .NET teams and Google publish it; Unity's own code uses `m_PascalCase` instead, and Unity's style guide allows both. Either prefix is invisible in the Inspector, because Unity strips `m_` and `_` from field labels. Renaming a serialized field loses its saved Inspector values unless it carries `[FormerlySerializedAs("oldName")]`. So the prototype's 43 serialized fields, stored in 17 scenes and 7 prefabs, are renamed only when a task rewrites their file. The 4 new combat fields are renamed now, while only the sandbox scene uses them. The `.editorconfig` naming rules apply to `Assets/PrisonGame/**`.

## Gate commits with Git; format while editing with tool hooks

([agent-enforcement notes](notes/agent-enforcement.md#2-how-can-rules-be-enforced-instead-of-merely-stated); [practice notes](notes/practice-for-remaining-decisions.md#3-should-a-failing-hook-block-the-commit-decision-4))

- **Git pre-commit hook (Proposed first).** A committed `.githooks/pre-commit` calls `tools/check.ps1 -Staged`; it is enabled once with `git config core.hooksPath .githooks`. The script runs `dotnet format --verify-no-changes` on staged C# files and prints the one command that fixes a failure (`tools/format.ps1`). It runs for Claude, Codex and manual commits alike. Blocking is how the dominant hook tooling works: "the commit will be aborted if there is a nonzero exit code". The stated purpose is to catch trivial style issues before review. Keep it fast: Unity compilation and tests need the Editor or a build, so they are required before a task is marked done, not on every commit. `git commit --no-verify` bypasses the hook, and the Claude Code harness already forbids skipping hooks unless asked.
- **Tool post-edit hooks (Proposed later).** Both tools can run a command after each file edit: Claude Code through a `PostToolUse` hook in `.claude/settings.json`, and Codex through a stable `PostToolUse` hook in `.codex/hooks.json`, which also fires after `apply_patch`. Both vendors present formatting after edits as a standard use. Both would call the same `tools/format.ps1`. Each needs the user to approve it once, and neither was tested on this Windows setup, so add them after the Git hook works.
- **One command for "is this done".** `tools/check.ps1` runs the formatter check, asks the open Editor to recompile, and runs the edit-mode tests through the Pipeline CLI. A `-Build` switch adds the Windows build and executable checks. `AGENTS.md` names this command, which matches how agents use AGENTS.md: they run the checks it lists before finishing.

## Tests: one framework for new work, keep the executable checks

([testing notes](notes/testing-standards.md))

- New tests use Unity's Test Framework:
  - one test assembly per game assembly and mode
  - test classes tagged with an area category (`[Category("Combat")]`)
  - names that read as behavior (`AttacksDoNotHurtADownedFighter`)
  - Arrange-Act-Assert structure
  - `[Test]` by default, and `[UnityTest]` only when a test must wait frames, as Unity advises
- Every plain C# rules class gets edit-mode tests written with it. Play-mode tests cover physics, scene wiring and frame-driven behavior. Tests needing real frames in the unfocused Editor set `Application.runInBackground` for their duration only.
- Keep Codex's runtime checks. They cover 106 behaviors in the real executable, are excluded from release builds (`#if UNITY_EDITOR || DEVELOPMENT_BUILD`), start only with a command-line flag, and report through a file, which agents can read reliably. Unity can also run Play-mode tests inside a built Player, but results return over a network connection to the Editor. Move an old check into the Test Framework only when its system is substantially changed.
- Add Unity's Code Coverage package (1.3.0, released for 6.3) later, when the rules code is large enough for untested parts to be hard to spot.
- **CI later** ([practice notes](notes/practice-for-remaining-decisions.md#6-do-small-unity-teams-run-ci-decision-7)). Professional teams run CI; for one developer on one PC it is optional. Two routes have free allowances:
  - GameCI on GitHub Actions: 2,000 minutes a month on a private repository, plus a Unity licence file stored as a secret.
  - Unity Build Automation: 200 Windows build minutes a month, 2 concurrent builds, no payment method needed.

  Compare them when release builds must be reproducible away from this PC, a second contributor joins, or the Steam release approaches.

## Editor scripts: static methods in an Editor assembly

([editor-script notes](notes/editor-automation-scripts.md); [practice notes](notes/practice-for-remaining-decisions.md#5-where-do-unity-projects-keep-build-and-automation-scripts-decision-6))

Unity projects keep build and automation code as static methods in Editor code inside `Assets`. Unity's `-executeMethod` batch mode requires that, and GameCI's build step requires the class to be "in the `Assets/Editor` directory (or in an Editor Assembly)". The Unity CLI used so far is labelled experimental. Its `eval_file` path compiles loose snippets only at run time and defaults to a 5-second timeout, which caused two of today's failures, each leaving a half-built scene.

**Proposed** (revises the earlier "outside `Assets`" proposal, which ran against this practice):
- New builders and checks are static methods in an Editor-only assembly, for example `Assets/PrisonGame/Scripts/EditorTools/` with `PrisonGame.EditorTools.asmdef`. They compile with the project, so a broken builder shows at once. They follow the same formatting and naming rules, and run through the CLI, a Unity menu item or `-executeMethod`.
- They keep the Codex scripts' good rules: refuse to run while playing, compiling or with unsaved scenes; never overwrite; restore the open scene; write evidence to `docs/evidence/`. Two more rules come from today's failures: read everything needed before closing or destroying anything, and save only after every step succeeds.
- The existing `tools/unity` snippets stay as the record of how each scene was built; they are not converted.

## What to change once, and one refactor to schedule

- **Prototype layout, Proposed: reformat once** ([practice notes](notes/practice-for-remaining-decisions.md#2-how-do-teams-handle-a-one-time-reformat-decision-3)). Reformat `Assets/Prototype` with `dotnet format` in one commit that contains only formatting: no renames, no logic. List the commit in a root `.git-blame-ignore-revs` file, which GitHub applies automatically so each line's history still shows its real author. Then recompile and rerun the existing Editor and executable checks to prove nothing changed.
- **Prototype structure.** `LaundryHud` reads keys and moves inventory items, and `PlayerInteraction` polls the keyboard. That is input and UI code performing game actions, which the agreed co-op rule in `AGENTS.md` asks to separate. This is not a formatting job. **Proposed:** fix it when ENCOUNTER-01 step 2 adds the player input adapter, by routing the existing keys through the same adapter.

## Rollout

| Step | Work | Mode |
| --- | --- | --- |
| 1 | User decides the open items below | — |
| 2 | Write `docs/engineering-standards.md` and the `AGENTS.md` "Code and tests" section | Medium |
| 3 | Add `.editorconfig`; add `tools/format.ps1` and `tools/check.ps1`; format the new `PrisonGame` code and rename its 4 serialized fields | Medium |
| 4 | Add the Git pre-commit hook and prove it blocks an unformatted file | Medium |
| 5 | Reformat the prototype once, record it in `.git-blame-ignore-revs`, rerun its checks | Medium |
| 6 | Microsoft.Unity.Analyzers for the new assemblies; fix or accept what it reports | Medium |
| Later | Tool post-edit hooks, Code Coverage, CI | Medium |

Steps 2 to 5 fit in one session before ENCOUNTER-01 step 2, so the fight's input code is the first to follow the standard.

## Open decisions for the user

None. The user agreed decision 1 (naming) and then decisions 2 to 7 as proposed on 2026-09-25; they are recorded in the [development plan](../../development-plan.md#engineering-standards---agreed-september-25) and applied in [Engineering standards](../../engineering-standards.md).

## Changelog

- 2026-09-25: added [naming-in-practice notes](notes/naming-in-practice.md) after the user pointed out that decision 1's first recommendation ("keep camelCase") rested on avoiding rename work, not on practice. The naming section now compares Microsoft's, Google's and Unity's own conventions and adds the serialized-field rename cost. The user then agreed Microsoft's convention.
- 2026-09-25: added [practice notes for decisions 2 to 7](notes/practice-for-remaining-decisions.md). The formatter proposal changed from CSharpier to `.editorconfig` with `dotnet format`: Unity's own repository uses `.editorconfig`, the tool is already installed, and a local trial worked. The Editor-script proposal changed from outside `Assets` to an Editor-only assembly, because `-executeMethod` and GameCI require that. The one-time reformat now uses `.git-blame-ignore-revs`. CI options now include Unity Build Automation.
- 2026-09-26: the user accepted decisions 2 to 7; the open-decisions table is now empty.
- 2026-09-26: added [analyzer trial notes](notes/analyzer-setup-trial.md). In this project Unity ignored `.editorconfig` for compiler severities and required rulesets named after their assembly, so the analyzer paragraph now describes rulesets instead of `.editorconfig` severities.
