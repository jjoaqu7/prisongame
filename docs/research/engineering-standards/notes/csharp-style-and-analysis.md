# C# style, formatting and automatic analysis in Unity 6

Topic: engineering-standards · Date: 2026-09-25 · Serves: RESEARCH-02
Searched: Unity manual (6.3) and Unity style-guide pages, Microsoft Learn, Microsoft.Unity.Analyzers and CSharpier documentation; about 12 searches and page reads.
Assumptions: the project keeps Unity 6.3 LTS (6000.3.24f1). Project facts checked on this PC: .NET SDK 10.0.401 is installed; Unity generates `.csproj` files, which Git ignores; there is no `.editorconfig`. Existing code uses PascalCase public members, camelCase private fields with no prefix, and braces on their own lines. Later prototype files put several statements on one line.

## 1. What naming and layout conventions does Unity recommend?

**Takeaway:** Unity publishes a C# style guide (Unity 6 edition e-book, plus web articles) built on Microsoft's .NET conventions. Its own examples mix private-field styles (`_currentHealth` and `m_` prefixes), and its author says consistency matters more than any single rule, so the project can keep its existing field style.

**Findings:**
- Unity offers the e-book "Use a C# style guide for clean and scalable game code (Unity 6 edition)" ([Unity](https://unity.com/resources/c-sharp-style-guide-unity-6), Official, Unity 6).
- The companion article recommends PascalCase noun class names, `I`-prefixed interfaces, verb method names, and questions for bool methods (`IsGameOver`). It recommends one variable declaration per line, no abbreviations, and PascalCase enums. It shows private fields as `_currentHealth`, and elsewhere suggests `m_`, `k_` and `s_` prefixes ([Unity](https://unity.com/how-to/naming-and-code-style-tips-c-scripting-unity), Official, undated web article).
- The recommendations are "essentially all a subset of the Microsoft Framework Design guidelines," and "the specifics of an individual rule are less important than having everyone agree to follow it consistently" ([Unity-Code-Style-Guide README, by the e-book's project lead](https://github.com/thomasjacobsen-unity/Unity-Code-Style-Guide), Primary).

**Inferences:**
- Proposed: keep the style the prototype already uses (PascalCase public, camelCase private without prefix, braces on their own line), write it down once, and let a formatter enforce layout. Renaming every field to `_x` or `m_x` would churn 41 working files for no behavior gain.
- Open: whether private fields get a prefix. It is a taste choice with no functional effect.

**Gaps:** the e-book PDF itself was not read (it sits behind a download form); only the web articles and the companion repository were.

## 2. Can Unity 6.3 enforce rules automatically at compile time?

**Takeaway:** yes. Unity 6.3 loads Roslyn analyzers (compiler plugins that report rule violations) as DLLs labelled `RoslynAnalyzer`, applies `.editorconfig` severities, and shows the results in the Unity Console. Analyzer scope follows assembly definitions, so rules can be strict for the new `PrisonGame.*` code and left loose for the prototype.

**Findings:**
- Analyzers and source generators are supported as managed plugins. "Roslyn analyzers are only compatible with the IDEs that Unity supports" ([Unity 6.3 manual](https://docs.unity3d.com/6000.3/Documentation/Manual/roslyn-analyzers.html), Official, 6.3).
- To install one: extract the NuGet package's `analyzers/dotnet/cs` DLL into Assets, turn off Any Platform, Editor and Standalone, and add the asset label `RoslynAnalyzer`, which is case-sensitive ([Unity 6.3 manual](https://docs.unity3d.com/6000.3/Documentation/Manual/install-existing-analyzer.html), Official, 6.3).
- Scope: "analyzers in the root of the `Assets` folder apply to all the predefined assemblies"; an analyzer inside an assembly definition's folder "only applies to that assembly, and to any other assembly that references it." `Default.ruleset` in Assets applies to all assemblies; a ruleset beside an `.asmdef` overrides it for that assembly. An `.editorconfig` in the project root can change a rule's severity, for example from warning to error. Diagnostics "appear in the Console window" ([Unity 6.3 manual](https://docs.unity3d.com/6000.3/Documentation/Manual/analyzer-scope-and-diagnostics.html), Official, 6.3).
- Microsoft.Unity.Analyzers adds Unity-specific diagnostics (`UNT####`) and suppresses general C# warnings that are wrong for Unity (`USP####`). It ships with Visual Studio's Unity workload and the VS Code Unity extension, is on NuGet for other uses, and can run inside Unity with the `RoslynAnalyzer` label. MIT licence; current builds require .NET 10 and Visual Studio 2026 or VS Code 1.92+ ([GitHub](https://github.com/microsoft/Microsoft.Unity.Analyzers), Official, read 2026-09-25).

**Inferences:**
- Proposed: put a root `.editorconfig` in the repository with naming and formatting rules. Install Microsoft.Unity.Analyzers inside the `PrisonGame` code folder so its checks run in the Unity Console for the new assemblies only. Raise a few rules to errors once the code is clean.
- Proposed: do not force analyzers onto `Assets/Prototype` yet. It would flood the Console with warnings on code that already works.

**Gaps:** no source gave the Microsoft.CodeAnalysis version range Unity 6.3's compiler accepts; the analyzer DLL version must be checked by compiling after import.

## 3. Which formatter fits a Unity project edited by AI agents?

**Takeaway:** CSharpier formats plain `.cs` files without needing a project, has almost no options, and can check without changing anything, so it suits a Unity project whose `.csproj` files are generated and not committed. `dotnet format` is the Microsoft alternative, but its style and analyzer passes need a project or solution, and only its whitespace pass works on a bare folder.

**Findings:**
- CSharpier is an opinionated formatter ported from Prettier's printer. It limits options on purpose: "Option requests are out of scope for CSharpier." It installs with `dotnet tool install csharpier` and runs with `dotnet csharpier format .` ([CSharpier](https://csharpier.com/docs/About), Official).
- Options: `printWidth` (default 100), `useTabs`, `indentSize` (default 4 for C#) and `endOfLine`. It reads `.csharpierrc` or `.editorconfig` ([CSharpier configuration](https://csharpier.com/docs/Configuration), Official).
- It works on plain files and folders with no project file. `check` returns exit code 1 when any file is unformatted. It respects `.csharpierignore` and skips generated files by default ([CSharpier CLI](https://csharpier.com/docs/CLI), Official).
- Documented pre-commit setups: Husky.Net (a .NET Git-hook tool installed through a tool manifest), the Python `pre-commit` framework, or MegaLinter ([CSharpier pre-commit](https://csharpier.com/docs/Pre-commit), Official).
- `dotnet format` applies `.editorconfig` preferences and analyzer fixes to a project or solution. `--verify-no-changes` exits non-zero when files would change. Only the `whitespace` subcommand has `--folder`, which treats the argument as a plain folder of code files ([Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format), Official, updated 2026-09-18, .NET 6 SDK and later).

**Inferences:**
- Proposed: CSharpier as the one formatter, pinned in a committed .NET tool manifest (`.config/dotnet-tools.json`) so every agent and machine runs the same version. `check` runs before commits, and `format` fixes files. The 100-column default replaces the prototype's 150+ character lines.
- Proposed: run it only on new code at first, through `.csharpierignore` excluding `Assets/Prototype` and `tools/unity`. Formatting the prototype is a separate, behavior-free commit, if wanted.
- Open: whether to reformat the existing prototype code once (one large whitespace-only commit) or leave it as it is.

**Gaps:** no source tested CSharpier's output against Unity serialization or code generation. Formatting cannot change serialized field names, so no risk is expected, but it is unverified.

## 4. How far apart are the two agents' code today?

**Takeaway:** measurably apart in layout, and the Unity build scripts are the densest code in the project. Session logs on this PC show Codex wrote the prototype (September 21 to 25) and Claude Code wrote the combat code (September 25).

**Findings** (project repository, measured by the coordinator on 2026-09-25 by counting non-blank lines; Primary):

| Files | Count | Average line length | Lines with 3+ statements | Comment lines |
| --- | --- | --- | --- | --- |
| Codex runtime scripts, `Assets/Prototype/Scripts` without checks | 35 | 54 | 2.3% | 1.9% |
| Codex runtime check scripts | 6 | 75 | 11.0% | 1.3% |
| Codex `tools/unity` snippets | 52 | 116 | 27.9% | 3.0% |
| Claude combat runtime | 6 | 41 | 0.6% | 7.7% |
| Claude combat tests | 3 | 39 | 0.3% | 0.3% |
| Claude `tools/unity` snippets, written to match Codex's | 3 | 102 | 10.7% | 11.6% |

- Codex's earliest files are conventionally formatted (`FirstPersonController`: 0% of lines hold 3+ statements). Later ones are denser (`LaundryHud`: 11.9%; `LaundryDuty`: 8.0%).
- `PlayerInteraction` looks up the HUD component six times. `LaundryHud` reads the keyboard and calls inventory actions. Four prototype scripts read `Keyboard.current` directly.

**Inferences:**
- Proposed: let the formatter settle layout, so neither agent's habits matter; see question 3.

**Gaps:** differences are partly caused by the tasks, not the tools. The prototype was built fast in small edits, while the combat code followed a plan the research had already laid out.
