# Unity Editor scripts that build scenes and run checks

Topic: engineering-standards · Date: 2026-09-25 · Serves: RESEARCH-02
Searched: Unity CLI documentation landing page, Unity 6.3 command-line arguments page, and the live command descriptions from the installed Unity CLI (1.0.0-beta.8, Pipeline package in the project); about 4 reads plus the project's own scripts.
Assumptions: none. Project facts checked on this PC:
- `tools/unity/` holds 55 `.cs` snippets (52 from Codex, 3 from Claude on 2026-09-25), run with the CLI's `eval_file`.
- They average 116 characters per line, and 28% of their lines hold three or more statements.
- Earlier logs record repeated snippet failures: compile errors found only at run time, a shadowed local, unsupported top-level `await`, and on 2026-09-25 a 5-second timeout and a destroyed-object error that each left a half-built, unsaved scene.

## 1. What are the options for Editor automation, and which fits agents best?

**Takeaway:** the installed CLI offers three ways to run Editor code. `eval` and `eval_file` compile a loose snippet at run time with a 5-second default timeout. `run_script` compiles a normal C# file, runs a named static method, can compile without running (`dry_run`), and waits 30 seconds by default. Unity's own `-executeMethod` runs a static Editor method in batch mode and is the stable fallback, because Unity labels the CLI experimental.

**Findings:**
- "The Unity command-line interface (CLI) is experimental. The features and documentation might change in an upcoming release" ([Unity CLI docs](https://docs.unity.com/en-us/unity-cli), Official, read 2026-09-25).
- `eval`: "Evaluate C# code dynamically using Roslyn compiler"; `timeout` defaults to 5000 ms. `eval_file` evaluates C# read from a file (installed CLI command descriptions, Official, CLI 1.0.0-beta.8, read 2026-09-25).
- `run_script`, from the same command descriptions (Official, CLI 1.0.0-beta.8):
  - It is "the builder-pattern path for bulk construction".
  - It compiles a project C# file in memory, with no domain reload, and runs a named static entry point, `Namespace.Type.Method`.
  - The file "may live OUTSIDE Assets/ … so writing it never triggers an asset import or domain reload".
  - Arguments are passed as a JSON array.
  - `dry_run` compiles only and returns diagnostics without executing.
  - `pdb` maps exception stack traces to file and line.
  - `timeout_ms` defaults to 30000.
- `-executeMethod` executes a static method in an Editor folder when Unity opens the project. `-batchmode` suppresses dialogs, and `-quit` exits afterwards. A batch run fails by throwing, which gives return code 1, or by calling `EditorApplication.Exit` with a non-zero code ([Unity 6.3 manual](https://docs.unity3d.com/6000.3/Documentation/Manual/EditorCommandLineArguments.html), Official, 6.3).

**Inferences:**
- Proposed: new Editor scripts are ordinary C# classes with a namespace and one public static method each, formatted like game code. They run through `run_script`. Every run is preceded by a `dry_run` compile, so an error is found before any scene is touched. Keeping them outside `Assets` means they do not compile into the project or trigger imports.
- Proposed: every builder follows the rules the Codex scripts already applied well:
  - refuse to run while playing, compiling or with unsaved scenes
  - never overwrite an existing scene or prefab
  - restore the scene that was open
  - write evidence to `docs/evidence/`
  - also read everything it needs before closing or destroying anything, and save only after all steps succeed, so a failure leaves nothing half-built
- Proposed: keep the existing `eval_file` snippets as a history of how scenes were made. Do not convert them; they have already run once.
- Proposed: keep `-executeMethod` in mind as the fallback if the experimental CLI changes. A static method written for `run_script` can also be called from a batch-mode Editor.
- Open: whether builders should live outside `Assets` (run only through the CLI) or in an Editor-only assembly inside `Assets` (callable from Unity menus and `-executeMethod` as well).

**Gaps:** no Unity guidance was found on designing idempotent scene builders; the rules above come from this project's own failures. `run_script` was not tried in this research.
