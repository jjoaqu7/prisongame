# Making Claude Code and Codex follow the same rules

Topic: engineering-standards · Date: 2026-09-25 · Serves: RESEARCH-02
Searched: agents.md, OpenAI Codex docs (AGENTS.md discovery, hooks) and one open Codex issue, Claude Code docs (memory, hooks), the Git hooks manual; about 10 searches and page reads.
Assumptions: none. Project facts checked on this PC:
- The repository has one root `AGENTS.md` and no `CLAUDE.md`.
- `.claude/` holds agents and skills and has no settings or hooks. `.codex/` holds agents.
- No Git hooks are installed and `core.hooksPath` is unset.
- Codex sessions start in the game root (their logs record `cwd` = `C:\Users\jjoaq\vscode-python\game`).

## 1. Where must shared rules live so both tools read them?

**Takeaway:** in the root `AGENTS.md`. Codex reads `AGENTS.md` files only from the Git root down to the folder it was started in, so a nested file deeper in the tree is not loaded when Codex starts at the root. Claude Code reads the root `AGENTS.md` because the project has no `CLAUDE.md`, and loads nested ones on demand. Longer detail can sit in a linked doc, with the rules that matter every session kept in `AGENTS.md` itself.

**Findings:**
- AGENTS.md is a "README for agents". Common sections are build and test commands, code style, testing instructions and PR instructions. Agents "attempt to execute relevant programmatic checks and fix failures before finishing the task" ([agents.md](https://agents.md/), Official format site).
- Codex walks from the Git root down to the current working directory, checking `AGENTS.override.md`, then `AGENTS.md`, in each directory. Subdirectories below the working directory are not scanned. The combined size is capped by `project_doc_max_bytes`, 32 KiB by default ([OpenAI Codex docs](https://learn.chatgpt.com/docs/agent-configuration/agents-md), Official, read 2026-09-25).
- A request to load nested `AGENTS.md` files dynamically as Codex moves into deeper folders is open, filed 2026-02-18 ([openai/codex#12115](https://github.com/openai/codex/issues/12115), Primary).
- Claude Code reads `AGENTS.md` when there is no `CLAUDE.md` in the working directory or above (v2.1.277 or later). A subdirectory's `AGENTS.md` loads when Claude reads a file there. Instructions are "context, not enforced configuration. To block an action regardless of what Claude decides, use a PreToolUse hook" ([Claude Code memory docs](https://code.claude.com/docs/en/memory), Official, read 2026-09-25).

**Inferences:**
- Proposed: keep one root `AGENTS.md` as the single entry point for both tools. Add a short "Code and tests" section with the non-negotiable rules and the exact commands, and link a fuller `docs/engineering-standards.md` for detail. Avoid nested `AGENTS.md` files; Codex would not see them.
- Proposed: do not add a `CLAUDE.md`. Creating one would make Claude Code stop reading `AGENTS.md` unless it imports it.

**Gaps:** none material.

## 2. How can rules be enforced instead of merely stated?

**Takeaway:** three layers, from strongest to weakest. A Git pre-commit hook blocks any commit that fails the checks, whichever agent or person commits. Each tool's post-edit hook formats files as they are written. The written rules cover what tools cannot check. Both tools now support post-edit hooks, but each needs its own configuration and a trust step, while one Git hook covers everything.

**Findings:**
- Git runs `pre-commit` before creating a commit, and a non-zero exit aborts the commit. `git commit --no-verify` bypasses it. `core.hooksPath` points Git at a hooks folder, and a relative path resolves from the repository root. Hooks are not copied by `git clone` ([Git manual](https://git-scm.com/docs/githooks), Official).
- Claude Code hooks: a `PostToolUse` hook with matcher `Edit|Write` in the project's `.claude/settings.json` can run a formatter on every file Claude edits ([Claude Code hooks guide](https://code.claude.com/docs/en/hooks-guide), Official, read 2026-09-25).
- Codex hooks are stable. They live in `<repo>/.codex/hooks.json` or `config.toml` and support `PreToolUse`, `PostToolUse`, `Stop` and more. `PostToolUse` runs after Bash, `apply_patch` and other tools. Windows commands can be given separately (`commandWindows`). Project hooks must be reviewed and trusted before they run ([OpenAI Codex hooks docs](https://learn.chatgpt.com/docs/hooks), Official, read 2026-09-25).

**Inferences:**
- Proposed: a committed `.githooks/pre-commit` that calls one PowerShell script. It runs the CSharpier check on staged C# files and fails with a readable message. `git config core.hooksPath .githooks` is run once per clone.
- Proposed: optional post-edit hooks in both tools that call the same `tools/format.ps1`, so formatting happens while working rather than at commit time. Add them only after the Git hook works, since each needs the user to approve it once.
- Proposed: keep slow checks (Unity compile, edit-mode tests, executable checks) out of the pre-commit hook. They need the Editor or a build and would make commits slow; `AGENTS.md` lists them as required before a task is marked done.
- Open: whether the user wants commits blocked by the hook, or only warned.

**Gaps:** neither tool's docs were tested live here: whether a Codex `PostToolUse` hook sees the edited file path in a form a PowerShell script can use, and whether Claude Code's `jq`-based example works on Windows without `jq` installed.
