# Common practice for decisions 2 to 7

Topic: engineering-standards · Date: 2026-09-25 · Serves: RESEARCH-02 decisions 2 to 7
Searched: Unity's Graphics repository, Microsoft's C# formatting options, GitHub's blame documentation, pre-commit.com, GameCI's builder docs and Unity's Build Automation pricing article, plus a local trial of `dotnet format`; about 9 reads.
Assumptions: none. Added at the user's request so that the remaining decisions rest on what developers do, not only on tool documentation. The local trial ran on the installed .NET SDK 10.0.401. It used read-only checks against the project, plus a formatting run on a copy of the 41 prototype scripts in a temporary folder outside the repository; no project file was changed.

## 1. Which formatter do Unity developers and C# teams use? (decision 2)

**Takeaway:** the common pattern is a shared `.editorconfig` that editors apply, with Microsoft's `dotnet format` as the command-line tool reading the same file. Unity's own graphics repository keeps a detailed `.editorconfig` and names no separate formatter. `dotnet format` ships with the .NET SDK already installed here and works on this Unity project. CSharpier is a well-made alternative, but it adds a dependency whose style editors do not share.

**Findings:**
- Unity's Graphics repository has a root `.editorconfig`. It sets LF line endings, UTF-8, 4-space indents and `csharp_new_line_before_open_brace = all` (each brace on its own line), plus spacing rules. No formatter tool or formatting check is mentioned at the root or in the README ([Unity Graphics](https://github.com/Unity-Technologies/Graphics), [its .editorconfig](https://github.com/Unity-Technologies/Graphics/blob/master/.editorconfig), Primary, read 2026-09-25).
- Microsoft's formatting options are set in `.editorconfig`. `csharp_preserve_single_line_statements = false` puts `int i = 0; string name = "John";` on separate lines, and `csharp_new_line_before_open_brace = all` gives "Allman" style. No option wraps long lines ([Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/csharp-formatting-options), Official).
- Local trial, 2026-09-25 (Primary):
  - `dotnet format whitespace Assets/PrisonGame --folder --verify-no-changes` exited 0.
  - `dotnet format style PrisonGame.Combat.csproj --verify-no-changes --no-restore` loaded Unity's generated project and exited 0.
  - On a copy of the prototype scripts, with a trial `.editorconfig` that sets `csharp_preserve_single_line_statements = false`, `dotnet format whitespace --folder` raised the line count from 3,351 to 3,779 and cut lines holding three or more statements from 4.8% to 2.9%. It spaced operators too: `if(working!=lastWork ...)` became `if (working != lastWork ...)`. The remaining dense lines are one-line blocks such as `{ current = null; dialogue = null; }`, which `csharp_preserve_single_line_blocks = false` would also split.
- CSharpier findings are in [the style notes](csharp-style-and-analysis.md#3-which-formatter-fits-a-unity-project-edited-by-ai-agents). It reprints all code, wraps at 100 columns, has almost no options, and is installed as a separate .NET tool.

**Inferences:**
- Proposed (revises the earlier CSharpier proposal): `.editorconfig` plus `dotnet format`. This matches Unity's own repository and Microsoft's tooling, adds nothing to install, and gives editors (Rider, Visual Studio, VS Code) the same rules as the command line.
- Proposed: set `csharp_preserve_single_line_statements = false` and `csharp_preserve_single_line_blocks = false`, so crammed lines are split. Long lines are not wrapped automatically. The standard sets a guideline of 120 characters, checked in review.
- Open: `dotnet format` (the standard, shared with editors, no line wrapping) or CSharpier (complete reprinting and wrapping, an extra tool, its own style).

**Gaps:** CSharpier was not run here, because research must not install tools. How many prototype files `dotnet format` would change with the final `.editorconfig` is measured only on the trial settings.

## 2. How do teams handle a one-time reformat? (decision 3)

**Takeaway:** in its own formatting-only commit, listed in `.git-blame-ignore-revs` so blame (the per-line history) keeps pointing at the real authors. GitHub applies that file automatically.

**Findings:**
- GitHub hides revisions listed in a root `.git-blame-ignore-revs` file from its blame view, one commit hash per line with `#` comments. A listed commit that was the last to touch a line can still appear ([GitHub Docs](https://docs.github.com/en/repositories/working-with-files/using-files/viewing-and-understanding-files), Official).
- Locally, `git config blame.ignoreRevsFile .git-blame-ignore-revs` applies the same file to `git blame` (Git 2.23 and later) ([Rob Allen](https://akrabat.com/ignoring-revisions-with-git-blame/), Community; consistent with the GitHub page).

**Inferences:**
- Proposed: reformat `Assets/Prototype` once. Put it in a formatting-only commit with no renames or logic changes, list it in `.git-blame-ignore-revs`, then recompile and rerun the existing Editor and executable checks.

**Gaps:** none material.

## 3. Should a failing hook block the commit? (decision 4)

**Takeaway:** yes, that is how the dominant pre-commit tooling works: a failing hook aborts the commit, and files a hook changed are left for the developer to re-stage. The stated purpose is to catch trivial style problems before review. The same tool's docs recommend running the checks in CI as well.

**Findings:**
- "The commit will be aborted if there is a nonzero exit code." Hooks exist for "identifying simple issues before submission to code review", so reviewers can "focus on the architecture of a change while not wasting time with trivial style nitpicks". The docs advise running the same checks in CI too ([pre-commit.com](https://pre-commit.com/), Official for that tool).

**Inferences:**
- Proposed: block. The hook prints the one command that fixes the problem. If CI is added later, it reruns the same check, because hooks can be skipped with `--no-verify`.

**Gaps:** none material.

## 4. Post-edit formatting hooks for agents (decision 5)

**Takeaway:** both agent vendors present formatting after each edit as a standard use of their hooks (see [agent-enforcement notes](agent-enforcement.md#2-how-can-rules-be-enforced-instead-of-merely-stated)). This is new practice with AI agents, so there is no long track record, and neither tool's hook was tested on this Windows setup.

**Inferences:**
- Proposed: unchanged. Add them after the Git hook works, since each needs one approval and a Windows test.

## 5. Where do Unity projects keep build and automation scripts? (decision 6)

**Takeaway:** as static methods in Editor code inside `Assets`, in an `Editor` folder or an Editor-only assembly. Unity's `-executeMethod` and GameCI's build step both require that, and a static method there can also run from a Unity menu or the CLI. Scripts kept outside `Assets` and run only through the experimental CLI would depart from that practice.

**Findings:**
- GameCI's `buildMethod` must be "a valid path to a `static` method", and "the class must reside in the `Assets/Editor` directory (or in an Editor Assembly)", for example `EditorNamespace.BuilderClassName.StaticBuildMethod` ([GameCI builder](https://game.ci/docs/github/builder/), Official for that tool).
- Unity's `-executeMethod` runs a static method placed in an Editor folder ([editor-script notes](editor-automation-scripts.md#1-what-are-the-options-for-editor-automation-and-which-fits-agents-best); Unity 6.3 manual, Official).
- The Unity CLI is labelled experimental (same notes).

**Inferences:**
- Proposed (revises the earlier "outside `Assets`" proposal): an Editor-only assembly, for example `Assets/PrisonGame/Scripts/EditorTools/` with `PrisonGame.EditorTools.asmdef`. It holds one static method per builder or check, compiles with the project so a broken builder shows immediately, and runs through the CLI, a Unity menu item or `-executeMethod`. The formatter, analyzers and naming rules apply to it like any other code.
- Proposed: keep the rule that a builder saves nothing until every step has succeeded.

**Gaps:** calling an in-project static method through the Pipeline CLI's `eval` or `menu` commands with a long timeout was not tried here.

## 6. Do small Unity teams run CI? (decision 7)

**Takeaway:** professional teams run CI; for one developer on one PC it is optional. Two routes exist with free allowances: GameCI on GitHub Actions (2,000 free minutes a month on a private repository, plus a Unity licence secret) and Unity's own Build Automation (200 free Windows build minutes a month and 2 concurrent builds, with no payment method needed).

**Findings:**
- Unity Build Automation's free tier since March 2026 gives 200 Windows, 100 Mac and 100 Linux build minutes a month and 2 concurrent builds, and needs no payment method ([Unity Support](https://support.unity.com/hc/en-us/articles/34748492914964-Understanding-New-Unity-DevOps-charges-starting-from-Mar-1-2026), Official, 2026-07-08).
- GameCI and GitHub Actions facts are in the [testing notes](testing-standards.md#3-coverage-performance-and-ci).

**Inferences:**
- Proposed: later. Start when release builds need to be reproducible away from this PC, a second contributor joins, or the Steam release approaches. Compare GameCI and Unity Build Automation then.

**Gaps:** whether Unity Build Automation runs Unity Test Framework tests on its free tier was not found.
