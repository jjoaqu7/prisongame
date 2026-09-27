# Progress and next tasks

Last updated: September 27, 2026.

## How to use this tracker - agreed

The primary development agent maintains this file as part of each work session. There is no continuously running tracking agent. Read this file and the relevant design docs before work; update it after meaningful results and leave the next action before ending a session. Specialist/reviewer agents are optional, bounded assignments when authorized, not permanent background workers.

Task states: **To do**, **In progress**, **Blocked**, **Needs your review**, **Done**. Done means the stated checks passed; user approval of feel or appearance must come from the user. Record what was checked and distinguish untested implementation from verified behavior.

The [development plan](development-plan.md) owns milestone scope and sequencing. Design docs retain agreed/proposed/open decisions. This file owns task status; add detail to the next milestone and keep later work broad.

**Two files - agreed September 25.** This file holds current state only: current state, items awaiting review, one short row per task, and the handoff. Keep it under about 150 lines, and replace outdated status instead of adding new "latest" paragraphs. Write each task's verification details, evidence links and handoff notes as a dated entry at the end of the [progress log](progress-log.md), and link the task row to that entry. Read the log only when a task's history is needed; search it by task ID.

## Current state

- **Milestone:** closing the prototype chain with ENCOUNTER-01, the neighboring-cell fight. Its [fight rules](gameplay.md#neighboring-cell-fight-rules---agreed-september-25) and [build plan](development-plan.md#encounter-01-build-plan---september-25) were agreed September 25. Release 1's broad scope is agreed: [Release 1 scope](development-plan.md#release-1-scope---agreed-september-25).
- **Next action:** the user tries the step 4 fight in the sandbox (see Awaiting your review). Then continue ENCOUNTER-01: step 5, the book and rod as weapons (**Medium**), or steps 6 and 6b (ragdoll, kill, grab and throw; **High**) if the user wants the ragdoll sooner; all their rules are now agreed. After ENCOUNTER-01, REL1-01 (**High**).
- **Blockers:** none. Working on `encounter-01`; see Publication in the handoff.
- **Latest builds** (ignored by Git): `PrisonGame/Builds/WindowsLaundry/PrisonGame.exe` has laundry duty, HUD, inventory and the neighboring cell (scene `Duty01_Laundry`). `PrisonGame/Builds/WindowsSample/PrisonGame.exe` has the combined earning sample with sound (scene `Save01_Progress`).

## Awaiting your review

Implemented and technically checked; only the user can judge feel and appearance. List each item here with what to try and what to judge, and move it to Done when the user approves it.

ENCOUNTER-01 step 4, the inmate fights back. Open `Assets/PrisonGame/Scenes/Sandbox/CombatAttacksSandbox.unity`, press Play, click **Resume walking** and punch or shove the orange dummy to start the fight:
- **Try:**
  - Let him hit you: 12 damage, a red edge flash and a view jolt.
  - Hold RMB well before his punch: you take 3.
  - Raise RMB just before his punch lands, when his fist is pulled back: no damage, "Timed block", and he staggers.
  - Kick (V) to stagger him, even through his guard, or punch four times quickly.
  - Get him below a third of his health: he hunches and winds up slower.
  - Lose the fight: it fades to black and puts you both back where the fight started.
  - Down him: he lies on his side, and your health refills.
- **Judge:**
  - whether his wind-up (the fist pulled back and up, a lean back, 0.7 s) reads in time to block
  - whether the timed block is learnable
  - whether 2 to 3 s between his punches and his 2 m/s approach feel fair
  - whether the flash and jolt read without being too much
- **Adjust:** camera shake, including the jolt when hit, can be turned off in the Escape menu. Exit and re-enter Play to reset. He has no wind-up sound yet (the sound pack has none). The capsule and fist are placeholders until step 6.
- [Step-4 log](progress-log.md#encounter-01-step-4-the-inmate-fights-back---september-27), [verification rerun](progress-log.md#encounter-01-step-4-verification-rerun---september-27), [wind-up capture](evidence/encounter01-step4-windup.png).

## Tasks

Details for each row are in the [progress log](progress-log.md). Rows without their own log entry are detailed in the [pre-split task table](progress-log.md#task-table-before-the-split).

| ID | Task | Status | Evidence / notes |
| --- | --- | --- | --- |
| TRACK-01 | Establish progress tracking | Done | Linked from README; AGENTS.md sets update and handoff duties. |
| TRACK-02 | Split the tracker into current state and a log | Done | History moved unchanged to the log in date order; word count matched; links repointed. [Log](progress-log.md#track-02-tracker-split-and-release-1-scope---september-25) |
| SETUP-01 | Open Unity project | Done | Editor 6000.3.24f1 confirmed. |
| SETUP-02 | Verify CLI and MCP scene reads | Done | Both returned the scene hierarchy. |
| SETUP-03 | Recoverable version-control checkpoint | Done | Separate game repository; baseline `bae004a`; recovery compared. [Log](progress-log.md#local-checkpoint-and-recovery) |
| SETUP-04 | Reversible MCP scene edit | Done | Create, read and delete verified. |
| SETUP-05 | Connect the private GitHub repository | Done | `origin` is `jjoaqu7/prisongame`; initial push verified. Later pushes: see Current state. |
| ROOM-01 | Rough cell, corridor and common area | Done | User accepted September 23. [Log](progress-log.md#room-01-verification) |
| ROOM-02 | First-person movement and looking | Done | 14 checks; accepted. [Log](progress-log.md#room-02-controls-and-verification) |
| ROOM-03 | Basic interactions | Done | 26 checks; accepted. [Log](progress-log.md#room-03-controls-and-verification) |
| CHECK-01 | First Windows build | Done | 18 executable checks. [Log](progress-log.md#check-01-windows-build) |
| ART-01 | Visual reference board | Done | Direction selected. [Board](art-reference-board.md) |
| ART-02 | Lighting and materials test | Done | User approved for the prototype September 25. Movement and interaction checks pass in all variants. [Log](progress-log.md#art-02-atmosphere-comparison--september-24) |
| ART-03 | Original inmate design (M5) | Done | Accepted prototype; 20 behavior checks at 0.52 m/s. [Log](progress-log.md#m5-review-faster-walking-and-earn-01-assembly--september-24) |
| EARN-01 | Snack packs and cell shelf | Done | 27 loop checks; combined prototype accepted ("it all works"). [Log](progress-log.md#earn-01-complete-earning-prototype--september-24) |
| EARN-02 | One tracked customer request | Done | 23 checks. [Log](progress-log.md#earn-02-tracked-customer-request--september-24) |
| EARN-03 | Prison clock and request deadline | Done | 33 checks. [Log](progress-log.md#earn-03-timed-request-and-player-speed--september-24) |
| GUARD-01 | One guard's suspicion and resolution | Done | 32 checks; further enforcement open. [Log](progress-log.md#guard-01-observed-trespass-and-suspicion--september-24) |
| SUPPLY-01 | Supply interruption and affected request | Done | 31 + 9 checks; trigger and duration provisional. [Log](progress-log.md#supply-01-controlled-stock-inspection--september-24) |
| SAVE-01 | Save and restore the combined sample | Done | 32 checks plus fresh-session reload. [Log](progress-log.md#save-01-manual-sample-persistence---september-24) |
| CHECK-02 | Combined Windows sample | Done | 35 executable checks; other-PC testing open. [Log](progress-log.md#check-02-combined-windows-sample---september-24) |
| AUDIO-01 | First cozy sound palette | Done | User approved for the prototype September 25. 37 audio, 32 save and 37 executable checks. [Log](progress-log.md#audio-01-cozy-sound-palette) |
| PERF-01 | Performance baseline | Done | [Log](progress-log.md#perf-01-windows-performance-baseline---september-25); [report](evidence/performance-summary.md) |
| PERF-02 | Frame hitches and frame limit | Done | Presentation waiting identified; 60 FPS default. [Log](progress-log.md#perf-02-visible-profiling-and-frame-limit---september-25) |
| PERF-03 | Longer performance session | To do | Watch the isolated 226 ms pause, paused-menu outliers and memory; other-PC testing open. **High** for profiling. |
| DUTY-01 | Supervised laundry duty | Done | User approved for the prototype September 25. 36 rule and 25 executable checks. [Log](progress-log.md#duty-01-verification-and-review-handoff---september-25) |
| HUD-01 | Minimal HUD, notifications, Tab inventory view | Done | User approved for the prototype September 25. 36 duty and 32 executable checks. [Log](progress-log.md#hud-01-and-first-inventory-view---implemented-awaiting-review) |
| INV-01 | Six-slot inventory and Dex-to-Harris key | Done | User approved for the prototype September 25. 41 rule/save, 79 Editor and 79 executable checks. [Log](progress-log.md#inv-01---functional-inventory-and-officer-secret) |
| CELL-01 | Immediate dialogue, Harris's secret, neighboring cell | Done | User approved for the prototype September 25. 22 new, 106 Editor and 106 executable checks. [Log](progress-log.md#cell-01---dialogue-correction-and-harriss-neighboring-cell-key) |
| PLAYTEST-01 | Uncoached new-player test | To do | Milestone 6 exit test: a new player tries the sample without coaching; note confusion, repetition and interest. **Medium**. |
| RESEARCH-01 | Combat research | Done | [Report](research/combat/report.md) with a proposed approach, build order and 13 open decisions; findings are not decisions. Updated September 27 with the [common conventions](research/combat/notes/controls-and-feedback-conventions.md) for grab, kill and throw keys, timed blocks and hit feedback. [Log](progress-log.md#research-01-combat-research---september-25) |
| RESEARCH-02 | Coding, scripting and testing standards | Done | [Report](research/engineering-standards/report.md). Decisions 1 to 13 agreed and applied in STD-01 and STD-02. [Log](progress-log.md#research-02-engineering-standards---september-25) |
| STD-01 | Adopt the engineering standards | Done | [Standard](engineering-standards.md) in place September 26, enforced by `.editorconfig` + `dotnet format`, `tools/check.ps1` / `tools/format.ps1`, a blocking pre-commit hook (proven to block), and Microsoft.Unity.Analyzers with per-assembly rulesets (proven to warn). Prototype reformatted whitespace-only (`a704ac6`, in `.git-blame-ignore-revs`); 28 tests, 106 laundry and 18 smoke checks pass in the Editor. Windows builds were not rebuilt. Later: agent post-edit hooks, coverage, CI. [Log](progress-log.md#std-01-engineering-standards-adopted---september-26) |
| STD-02 | Names, builds, versions and Git | Done | Company name `Niuq`, with saves and settings copied to the new location. 14 spaced materials renamed. `CombatSandbox` moved to `Assets/PrisonGame/Scenes/Sandbox/`; template scene deleted. Git LFS for new binaries, commit and branch rules, `PrisonGame.EditorTools` (`AssetNaming`, `PrototypeChecks`). Check passes; 106 laundry, 18 smoke and 18 sandbox checks pass. Build Profiles come at ENCOUNTER-01 step 7. [Log](progress-log.md#std-02-names-builds-versions-and-git---september-26) |
| ENCOUNTER-01 | Neighboring-cell fight | In progress | Rules and 8-step plan agreed September 25. **Steps 1-3 done:** the user approved the controls and hit feel September 27. **Step 4 implemented and technically verified** in `CombatAttacksSandbox`: the inmate approaches, telegraphs and throws his punch, blocks, staggers (meter; the kick always staggers), gets hurt, goes down; the player's block, timed block, fight-only health bar and hit flash; losing restores the fight-start checkpoint. 100 edit-mode and 31 play-mode tests pass; the 18 step 1 checks pass. **Needs your review:** step 4 feel. All grab and throw rules for step 6b are now agreed. Next: step 5. [Log](progress-log.md#encounter-01-step-4-the-inmate-fights-back---september-27). Done when the fight, death, saved outcome and completion work through the full Rue/Dex/Harris chain. [Plan](development-plan.md#encounter-01-build-plan---september-25) |
| SHOVE-01 | Reactions to shoving by character type | To do | Agreed September 25: shove anyone at any time on F; reactions depend on who (a prisoner starts a fight). ENCOUNTER-01 step 2 builds the shove itself; this task adds reactions for guards, gangs and contacts when those are developed. [Gameplay](gameplay.md#shoving---agreed-direction-and-key-reactions-open) |
| REL1-01 | Release 1 milestone list | To do | Derive from the agreed [scope](development-plan.md#release-1-scope---agreed-september-25): detailed for the next milestones, broad later, with proposed quantities (wings, gangs, escape routes) for the user to decide. **High**. |

## Later tasks: evaluate additional agent roles

Agreed: retain these as future evaluation tasks. Adding or running the agents is deferred; the roles below remain candidates. Begin with gameplay testing and independent review when the first playable room warrants them. The primary agent owns the shared tracker and integrates findings; the user judges gameplay feel and visual direction.

| ID | Candidate role | Status | When to consider it | Evaluation outcome |
| --- | --- | --- | --- | --- |
| AGENT-01 | Gameplay tester | To do | ROOM-03 is playable | Define a repeatable test of doors, pickups, collisions, and restarting; assess whether a separate agent produces useful reproducible findings. |
| AGENT-02 | Independent code reviewer | To do | First substantial feature or milestone review | Trial a bounded review against requirements and record actionable issues, verification, and whether to reuse the role. |
| AGENT-03 | Art and asset specialist | To do | First representative assets are imported | Assess scale, materials, collisions, and consistency with the user-approved art direction. |
| AGENT-04 | Interface reviewer | To do | Orders, deadlines, or suspicion indicators exist | Check that cause, timing, progress, and consequences are understandable; retain user playtesting. |
| AGENT-05 | Performance specialist | To do | A representative prison scene can be measured | Define performance targets, collect measurements, and assess whether specialist investigation is useful. |
| AGENT-06 | Networking specialist | To do | Planning the release 2 co-op feasibility test | Review shared item ownership, simultaneous interactions, and state boundaries; evaluation deferred, no networking agent running. |
| AGENT-07 | Research specialist | Adopted September 25 | Research recurs (HUD, prison rules, combat; later escape, gang AI, Steam) | User chose a reusable research agent that works in any tool. One `game-research` skill in the open Agent Skills format (`.agents/skills/game-research/`), with thin pointers for Claude Code (`.claude/skills/`, `.claude/agents/`) and Codex (`.codex/agents/`). See the [research README](research/README.md). Not yet used for a new topic; Codex agent files are checked against the docs but not run. |

For each evaluation, record whether to adopt, defer, or decline the role and why. A decision to defer or decline completes the evaluation without implying an agent was installed or run. Additional roles can be added when a concrete need appears.

Patterns to consider borrowing: Unity's [inspect/change/run/verify workflow](https://unity.com/blog/meet-the-unity-cli), and the community [gamedev-ai-agents workflow](https://github.com/ilezhnin/gamedev-ai-agents) for playable milestones, placeholder assets, and test evidence. The community kit has not been tested here; its inclusion is a research reference, not a decision to install it.

## Session handoff

- **September 25:** user agreed release 1 scope, real combat, and the combat decisions for ENCOUNTER-01 (weapon adds damage, downed then separate kill, reload on loss), plus shoving as a general action; recorded in Gameplay, Factions/events and the development plan. User approved the six review items. Tracker split (TRACK-02). RESEARCH-01 finished; research moved to `docs/research/<topic>/`; a tool-independent research skill with Claude Code and Codex agents (AGENT-07). No game code, scenes or builds changed.
- **September 25, later:** planned ENCOUNTER-01; the user agreed the fight rules (changing four defaults: add a ragdoll, no critical hits, no breaking, LMB hit / RMB block) and code organization. Recorded in Gameplay, the development plan and Factions/events.
- **September 25, step 1:** built the greybox room and combat foundations (see the ENCOUNTER-01 row). Combat foundations are committed; the standards checkpoint is covered below.
- **September 25-26, standards:** researched (RESEARCH-02); the user accepted all proposals; STD-01 set them up and verified them. Every agent now follows [Engineering standards](engineering-standards.md); run `git config core.hooksPath .githooks` once in any new clone.
- **September 26, resumed:** STD-02 independently rechecked; 28 edit-mode, 106 laundry, 18 smoke and 18 combat checks pass. [Resume log](progress-log.md#std-02-resumed-verification-and-checkpoint---september-26).
- **September 26, step 2:** player attacks implemented and verified; `CombatAttacksSandbox` is ready for an early controls review. [Log](progress-log.md#encounter-01-step-2-player-attacks---september-26). Windows builds are unchanged.
- **September 27, step 3:** hit feedback added to `CombatAttacksSandbox`, and `tools/check.ps1 -Unity` now also runs play-mode tests. The sandbox is ready for your feel review. [Log](progress-log.md#encounter-01-step-3-hit-feedback---september-27).
- **September 27, kick:** the user replaced the charged punch with a kick on V; built, tested and recorded in Gameplay. Play-mode input tests were made independent of window focus and real mouse use. [Log](progress-log.md#encounter-01-kick-replaces-the-charged-punch---september-27).
- **September 27, grab and throw (docs only, no code):** user chose simple physics dragging with no slowdown, one throw action for handheld objects and downed or dead bodies, standard keys (E grab, LMB throw), and hiding bodies from guards as a later use. Recorded in Gameplay, Factions/events and the plan (step 6b). [Log](progress-log.md#grab-drag-and-throw-decided---september-27).
- **September 27, step 4:** the user approved the step 2-3 feel and asked for the most common option on every open question; the conventions were researched and recorded as agreed. Step 4 built in `CombatAttacksSandbox`. After an Editor restart, it passed verification, including a layer fix, and `check.ps1` gained guards against Play-mode and stale results. [Rerun log](progress-log.md#encounter-01-step-4-verification-rerun---september-27). [Log](progress-log.md#encounter-01-step-4-the-inmate-fights-back---september-27).
- **Start of the next chat:** (1) ask for the user's step 4 review (see Awaiting your review) and tune from it; (2) continue with step 5, the book and rod (**Medium**), or steps 6 and 6b (**High**) if the user prefers.
- **Open user decisions:** step 4 feel approval; the shipped product name (working title `PrisonGame`, which sets the save folder); release 1 quantities (wings, gangs, escape routes). The user will merge `encounter-01` into `main` after testing it. The encounter has no story in this prototype (agreed).
- **Publication:** `encounter-01` pushed to `origin` September 27 with step 4 and its verification.
