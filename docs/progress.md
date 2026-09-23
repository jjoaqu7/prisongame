# Progress and next tasks

Last updated: September 23, 2026.

## How to use this tracker - agreed

The primary development agent maintains this file as part of each work session. There is no continuously running tracking agent. Read this file and the relevant design docs before work; update it after meaningful results and leave the next action before ending a session. Specialist/reviewer agents are optional, bounded assignments when authorized, not permanent background workers.

Task states: **To do**, **In progress**, **Blocked**, **Needs your review**, **Done**. Done means the stated checks passed; user approval of feel or appearance must come from the user. Record what was checked and distinguish untested implementation from verified behavior.

The [development plan](development-plan.md) owns milestone scope and sequencing. Design docs retain agreed/proposed/open decisions. This file owns task status; add detail to the next milestone and keep later work broad.

## Current milestone

Finish setup and make the first rough room playable. Proposed task breakdown below implements the accepted broad workflow; specific room layout and visual choices remain subject to user review.

Verified baseline: `PrisonGame/` uses Editor 6000.3.24f1 and URP. Room01_Blockout has geometry, furniture, a first-person controller, a sliding door, a carryable parcel, and a placeholder inmate. All 14 controller checks and 26 interaction checks passed in the Editor. On September 23, 2026, the user responded "All looks great!" to the completed room and interaction playtest handoff. ROOM-02/03 are accepted for this prototype; this does not approve final artwork or settle later mechanics. The Windows Development executable now passes 18 standalone checks; visible menu-button use remains a user check.

## Tasks

| ID | Task | Status | Prerequisite | Done when / evidence |
| --- | --- | --- | --- | --- |
| TRACK-01 | Establish progress tracking | Done | Accepted workflow | This tracker is linked from README; AGENTS.md specifies update and handoff responsibilities. |
| SETUP-01 | Open Unity project | Done | Editor installation | User screenshot showed PrisonGame/SampleScene in Unity 6.3 LTS; `ProjectSettings/ProjectVersion.txt` confirms 6000.3.24f1. |
| SETUP-02 | Verify CLI and MCP scene reads | Done | SETUP-01 | Both CLI `get_scene_hierarchy` and MCP `mcp__unity__get_scene_hierarchy` returned Main Camera, Directional Light, and Global Volume in this conversation. This verifies reads, not edits. |
| SETUP-03 | Establish recoverable version control checkpoint | Done | Check repository boundary | Separate `game/` repository; baseline commit `bae004a` contains 75 files. Six essential files recovered from its archive and compared successfully after line-ending normalization; generated caches excluded; `git fsck --full` passed. Visible Meta Files and text serialization confirmed. |
| SETUP-04 | Verify a reversible MCP scene edit | Done | SETUP-03 | MCP created `__MCP_Verification_Temporary`, a scene read confirmed it, MCP deleted it, and a final read confirmed the original three objects and a saved scene. Saving updated template serialization to the installed Unity version; those reviewed changes are retained. |
| SETUP-05 | Connect the user-designated private GitHub repository | Done | SETUP-03 | Configured origin as `https://github.com/jjoaqu7/prisongame.git`; pushed main with upstream tracking. `git ls-remote origin refs/heads/main` matched local checkpoint `e73bffa`. Existing baseline commits are included. |
| ROOM-01 | Build rough cell, corridor, and small common area | Done | SETUP-04 | Saved/reopened scene with connected passages and placeholder furniture; technical checks below passed. User accepted the resulting prototype on September 23. Dimensions remain provisional. |
| ROOM-02 | Add first-person movement and looking | Done | ROOM-01 | 14 Play-mode controller checks passed, with zero console errors/warnings. User accepted the resulting prototype on September 23. |
| ROOM-03 | Add basic interactions | Done | ROOM-02 | Door/controls, pickup/placement, inmate response, and prompts implemented; 26 Editor checks passed. User accepted the resulting prototype on September 23. |
| ART-01 | Assemble a small visual reference board | To do | Existing art direction | Character, cell, lighting, and interface references have source links; user reviews the proposed direction. Can overlap room work. |
| CHECK-01 | Playtest, adjust, and make a Windows build | Done | ROOM-03 | Windows x86-64 Development build succeeded; 18 executable checks passed, with rendered scene inspected. User already approved the room. Visible menu clicks and testing on another PC remain outside this automated verification. |

## ROOM-01 verification

- Created the scene through Unity MCP using `tools/unity/build-room01.cs`. This file is a Pipeline C# evaluation snippet outside Assets, not a gameplay script. It refuses to overwrite the existing scene and checks for unsaved scenes before creating a new one. Edit the saved scene normally; re-running is unnecessary.
- Physics checks sampled 115 points along a route from the cell, through both entries, and into the common area. A 0.6 m wide, 1.7 m high capsule with 0.05 m floor clearance found no obstructions; floor raycasts found support at every sample. Seven exterior wall probes found colliders.
- Reopened the saved scene; it contains 56 enabled, non-trigger BoxColliders, with no missing/error materials. Editor console reported zero errors/warnings and no compilation failure.
- Visually inspected the [overview capture](../PrisonGame/Assets/Screenshots/room01-overview.png). Removed temporary text labels that rendered through walls before capturing the final view.
- Initial evaluation failed to compile because Pipeline eval expects a method body rather than namespace imports. Corrected the snippet before successful execution; no gameplay scripts were compiled or introduced.
- Limits: these are Editor geometry checks, not a controller playtest or standalone build. Final room dimensions, colours, furniture, and movement feel have not been approved by the user. No gameplay or runtime automation tests were added for this static blockout.

## ROOM-02 controls and verification

Open `Assets/Scenes/Room01_Blockout.unity`, press Unity's Play button, select the Game tab, and click **Resume walking**. Use WASD to walk and the mouse to look. Escape releases the cursor and opens sensitivity settings; Resume captures it again. Leaving application focus also releases the mouse. Sensitivity is saved locally between sessions. Press Unity's Play button again to stop testing.

Current temporary settings: 3 m/s walking, 70-degree vertical field of view, 1.7 m eye offset, pitch clamped to +/-85 degrees, default sensitivity 0.12 degrees per mouse pixel (adjustable 0.03-0.4). No jump, sprint, or camera bob is implemented. ROOM-03 controls are below. Releasing the mouse stops movement input and interactions; it is not a global pause system.

`Assets/Prototype/Scripts/FirstPersonController.cs` uses the installed Input System and a CharacterController. `tools/unity/verify-room02.cs` is a repeatable Pipeline evaluation snippet: run via `eval_file` in Play mode with this scene loaded. It tests actual controller movement against the scene and synthetic keyboard events; it restores player state and temporary input settings afterward, leaving the cursor released.

Passed 14 checks: cell doorway traversal; corridor travel/turn; common entry traversal; sustained wall blocking; gravity/floor support; consistent 3 m/s at 30/60/144 simulated FPS; no diagonal speed boost; upper/lower pitch limits; fall recovery; W/A action bindings; focus-loss release; Escape release. This is an in-Editor integration check, not a standalone build or a substitute for the user's mouse/keyboard playtest.

During test development, a diagonal-speed check initially intersected a bench; its starting position was corrected. Synthetic keyboard events were also rejected while Game view was unfocused; the test now temporarily accepts background input and restores the normal focus settings in `finally`. No production focus settings were relaxed. The successful final run reported zero console errors/warnings. Visually inspected the [scaled settings panel](../PrisonGame/Assets/Screenshots/room02-controls.png).

User review received September 23: "All looks great!" Current movement and room accepted for the prototype, with no adjustments requested. Later playtests can still revise scale and feel.

## ROOM-03 controls and verification

Implemented at the user's request to proceed. The sliding mechanism, one-item carrying limit, parcel appearance, and inmate lines are temporary prototype choices, not approved final mechanics or artwork.

- Aim at the cell door or either ochre wall button and press **E** to open/close it. Buttons remain reachable when the door slides out of view. An occupied doorway refuses closing or reopens during closing.
- The parcel starts on the cell desk. Aim at it and press **E** to carry it; **Q** puts it on a clear tabletop or floor. Blocked placement keeps the parcel in your hands and explains the problem.
- Approach the placeholder inmate in the common area, aim at them, and press **E** for a short response. The response changes while carrying the parcel. There is no trading or quest system yet.
- Prompts depend on the first object hit within 2.4 m; walls block interaction. Carrying status and transient feedback appear on screen. Escape opens settings and prevents E/Q actions until walking resumes.

`tools/unity/build-room03.cs` records scene construction and refuses duplicate installation. The saved scene is authoritative; do not rerun it for normal editing. Six new runtime scripts under `Assets/Prototype/Scripts` handle targets, player interaction, the door, fixed controls, pickup, and inmate response.

`tools/unity/verify-room03.cs` passed 26 checks in Play mode: door targeting/prompt, closed-door collision, opening/traversal, controls from both sides, occupied-door protection, parcel pickup/placement/retargeting, full hands, wall and distance restrictions, inmate response, and synthetic E/Q keyboard events with settings both open and closed. Test state and temporary input settings are restored in `finally`. The first across-wall placement test had a support probe on the player's side of the wall; the fixture was corrected before the passing run.

Reran all 14 ROOM-02 checks successfully. That movement-only fixture temporarily disables the added door collider and restores it afterward; ROOM-03 separately tests closed/open door collision. Final Editor console: zero errors/warnings, no compilation failure. Scene saved and Editor left outside Play mode.

Visually inspected the updated [controls panel](../PrisonGame/Assets/Screenshots/room03-controls.png). Automated capture repeatedly released Game-view focus, so this screenshot verifies the settings layout, not active interaction prompt readability. The user subsequently accepted the prototype with "All looks great!"; no specific usability problems were reported. Later standalone verification is recorded under CHECK-01 below.

## CHECK-01 Windows build

Build target: Windows x86-64, Mono backend, Development build, Unity 6000.3.24f1. The only included scene is `Assets/Scenes/Room01_Blockout.unity`; the empty template scene has been removed from the build list, not deleted. Default window is resizable, 1280 x 720. Escape opens settings; the standalone settings panel adds **Quit game**.

Output: `PrisonGame/Builds/Windows/PrisonGame.exe`. Run it from its existing folder; the executable needs the accompanying `_Data` folder, UnityPlayer.dll, and runtime files. Build output is ignored by Git and is regenerated from source.

Rebuild using Unity MCP `build` with target `StandaloneWindows64`, outputPath `Builds/Windows/PrisonGame.exe`, scenes `["Assets/Scenes/Room01_Blockout.unity"]`, options `["Development", "DetailedBuildReport"]`, and confirm `true`; poll `build_status` to completion. Do not build while Play mode is running or a previous copy of the executable is still open.

Run `./tools/test-windows-build.ps1` from PowerShell for automated executable checks. It launches the build with `-prototype-smoke-test`, checks startup/physics/interactions/input, saves a camera image and results under `Builds/Windows/SmokeCheck`, and exits. `StandaloneSmokeCheck.cs` is opt-in, excluded from non-development Players, and does not create a test object during ordinary launches. Camera rendering verifies the 3D scene; it does not capture the settings or interaction UI. The script has a 45-second timeout and clears only its previous evidence files to prevent stale results.

Final build succeeded with zero errors and one warning that Pipeline automation is disabled in Players because no RuntimePipelineConfig exists. No runtime automation service was enabled to remove that warning. Initial build took about 100 seconds; final incremental build took about 12 seconds. BuildReport size: 179,328,865 bytes (about 171 MiB), excluding separate debug/evidence files. Unity refreshed URP build settings and serialized project settings during this first build; the resulting scene rendering was checked.

Passed 18 checks inside the executable: correct startup scene and objects, released startup controls, all 75 rendered materials supported, door targeting/closed collision/animation/opening/passage, parcel targeting, settings blocking E/Q, E pickup, Q placement with physics, wall occlusion, inmate targeting/response, and Escape release. The successful run exited with code 0 and recorded no Unity error/exception/assert events. [Saved report](evidence/windows-build-smoke-check.txt); [inspected camera capture](evidence/windows-build-camera.png). The native log contains a D3D12 info-queue query diagnostic; Direct3D12 rendering and all checks succeeded despite it.

Early test failures were in the verification fixture: hidden-window backbuffer capture failed, a fixed animation tick budget was too short at the Player's small frame time, and the manually advanced door needed a physics-transform sync before walking through it. Corrected those test assumptions without weakening checks or changing door gameplay. Sandbox execution also denied normal Unity per-user folders; final verification ran with normal Windows access.

An ordinary launch without the test flag stayed running for the eight-second startup check. Closing its hidden window through the OS window helper was unavailable, so that test process was explicitly stopped; do not count this as a verified Quit-button click. User can now open the executable normally, click Resume walking, try the interactions, and use Escape > Quit game. The camera capture excludes UI. This is a local prototype Development build, not a shipping/performance certification or a test on other PCs.

## Local checkpoint and recovery

The Git repository root is `C:\Users\jjoaq\vscode-python\game`. It is separate from the parent `vscode-python` repository. Run game Git commands from this root; do not stage unrelated projects in the parent repository.

Baseline commit: `bae004a` (Unity template and development docs before scene edit verification). Assets and their `.meta` files, Packages, ProjectSettings, and docs are tracked. Library, Temp, Logs, UserSettings, generated IDE projects, and build output are excluded.

Recovery verification extracted a scene, its metadata, package manifest and lockfile, Editor version, and tracker from the commit archive into a temporary folder and compared their content to Git. An initial sandbox write attempt failed; the authorized retry passed after accounting for Windows line endings. No full reimport or standalone build was tested. Temporary verification files were removed.

The user designated `https://github.com/jjoaqu7/prisongame.git` as this game's private remote. Origin is connected and main has upstream tracking; SETUP-05 records the initial successful push. Later publication status is recorded in the handoff below. Local commits, uncommitted changes, and ignored files are not necessarily backed up by GitHub. When recovering work, inspect the target commit and affected files first; avoid overwriting newer user changes.

## Decisions and later work

- Agreed September 23: release 1 single-player, release 2 co-op. Keep later co-op in mind as systems grow; boundaries and open hosting/progression choices are in the development plan. No networking implementation is authorized by this note alone.
- Earning-loop work is deferred at the user's request. The snack-pack example remains proposed; resume selection and implementation later.
- Once room dimensions work, begin one representative art sample alongside the small earning loop. Finished characters, rigging, animation, and a full art set are later work.
- Add an asset register when collecting production assets: source, usage terms, editable source file, Unity location, and temporary/finished status.
- Use independent review when a substantial feature or milestone warrants it. No additional agents are running or scheduled by this document.
- Keep future gameplay milestones in the development plan; move their concrete tasks here when approaching them.

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

For each evaluation, record whether to adopt, defer, or decline the role and why. A decision to defer or decline completes the evaluation without implying an agent was installed or run. Additional roles can be added when a concrete need appears.

Patterns to consider borrowing: Unity's [inspect/change/run/verify workflow](https://unity.com/blog/meet-the-unity-cli), and the community [gamedev-ai-agents workflow](https://github.com/ilezhnin/gamedev-ai-agents) for playable milestones, placeholder assets, and test evidence. The community kit has not been tested here; its inclusion is a research reference, not a decision to install it.

## First-stage estimate - provisional

For this estimate, first stage means the remaining setup, rough connected room, movement, basic interactions, small reference board, user playtest, and an initial Windows build. This spans the remainder of development-plan milestone 1 and milestone 2, plus early visual references. It does not mean the larger earning loop or polished playable sample is finished.

Assume a solo placeholder prototype, existing Unity/CLI/MCP installation, agent-assisted implementation, and the user available for review. These are project-specific planning ranges, not measured industry averages or a delivery promise.

| Work | Focused working hours |
| --- | --- |
| Remaining setup and recoverable checkpoint | 1-3 |
| Rough room geometry and collision | 2-4 |
| Movement and camera | 2-4 |
| Door, pickup, placeholder inmate, and prompts | 3-6 |
| Playtest adjustments and Windows build | 2-4 |
| Small visual reference board | 2-3 |
| Total | 12-24 |

Working hours include implementation, checks, troubleshooting, and active review, not just user typing time. At roughly two focused hours per working day, allow 6-12 working days. Waiting for feedback, extended Unity learning, unexpected setup failures, or scope changes adds calendar time. Custom finished models, multiplayer, economy, saving, and extensive animation are outside this range. Re-estimate after ROOM-02 using actual elapsed work; record the reason when the range changes.

Re-estimate after ROOM-02 implementation: the original 12-24 hour range above is retained as an initial forecast, not a current remaining-work estimate. The rough room and controller each took minutes of agent execution rather than the originally allocated hours; the controller implementation/checking pass was roughly 15 minutes, excluding user review. A provisional remaining allowance is 3-8 focused hours for interaction work (1-3), references (1-2), and playtest adjustments/build troubleshooting (1-3). This assumes no major scope change; human feedback and asset decisions remain unmeasured. Revisit after the user's first movement playtest.

## Session handoff

- Latest update: CHECK-01 built and verified. Windows executable is under `PrisonGame/Builds/Windows`; Editor is outside Play mode with a saved scene. Added standalone Quit button and opt-in executable verification. Recorded first release single-player / second release co-op, with coding boundaries in AGENTS.md and development-plan.md.
- Publication: local commits contain the prototype and approval. The last confirmed push attempt could not obtain GitHub credentials; the last remote verification reported `150742e`. Agents are authorized to commit and push scoped game work when authentication works. The built executable is ignored by Git; source and test evidence are versioned.
- Next action: user opens the Windows executable normally for visible menu/use checks. ART-01 references remain the next design task; the user asked what a reference board means and has not selected its specific visual proposals. Earning-loop work is deferred at the user's request.
- Current blockers: no build or gameplay blocker found in automated checks. Last publication attempt was blocked on GitHub authentication. Co-op implementation is deferred to release 2, not blocking the solo prototype.
- User decisions/reviews upcoming: standalone visible menu use and art references; return to the earning-loop activity later. Release 2 hosting/player count/shared progression remain open.
