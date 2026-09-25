# Project
First-person prison sandbox game. The rough room, first-person controller, and basic door/pickup/inmate interactions passed Editor checks and received user approval as a prototype. Release 1 is single-player; co-op is planned for release 2. Read `docs/progress.md` for verified implementation status and next work.

# Sources of truth (in priority order)
1. `docs/` (start at `docs/README.md`). Current and authoritative.
2. `docs/history/original-ideas.md`. My original ideas in my own wording. Some are still open (quotas, multipliers, escape pieces).
3. `docs/history/first-conversation.md`. Raw early chat log. History only; its suggestions are NOT decisions.

# Design rules
- I make design decisions. Label every idea agreed, proposed, or open, per "How to maintain these notes" in `docs/README.md`.
- Never promote a proposal to agreed, or drop one of my ideas, without asking me.
- If you think my idea is bad, say so and why. Do not quietly design around it.
- Keep release 2 co-op in mind while implementing release 1: separate game rules/state from local input, camera, and UI; pass the acting player explicitly where relevant; distinguish shared world state from per-player state. Apply this as systems are developed, without claiming multiplayer readiness or adding speculative networking infrastructure. See the release decision in `docs/development-plan.md`.
- When I decide something, update its owning doc in `docs/` and its status. Do not edit `docs/history/` except to fix links.

# Communication
1. Be direct and brief.
2. Explain when needed, in plain terms for someone new to game dev and Unity (I'm an engineer). No analogies. Comparisons to other games are fine.
3. Don't agree to be agreeable. Correct me when I'm wrong and give the reason.
4. Use my vocabulary. Introduce a new term only if it's shorter, and define it once.
   When suggesting a next step, include a recommended reasoning mode (for example Medium, High, or Extra High), with a brief reason when useful. This is a recommendation, not a claim that the setting was changed.
5. Explain what you are doing as you work. Before starting, state the immediate action and its purpose. During work, give brief plain-language updates at meaningful steps and at least about once a minute during sustained work, including findings, blockers, and changes of direction. Finish with what changed, what was verified, and what comes next. Do not leave all explanation until the final response. Pass this rule to delegated agents; the primary agent keeps user-facing updates coordinated.

# Version control
- Use the game's own repository at `C:\Users\jjoaq\vscode-python\game`.
- The user-designated private remote is `https://github.com/jjoaqu7/prisongame.git` (`origin`). Use it for this game's version control going forward; preserve its visibility and existing history.
- Keep changes scoped to this game. Do not stage unrelated projects from the parent `vscode-python` repository.

# Environment
Windows. Use `python`, not `python3`.

# Progress tracking - agreed workflow
- The primary development agent owns `docs/progress.md` and `docs/progress-log.md`; no always-running tracker agent is required.
- `docs/progress.md` holds current state only: current state, items awaiting review, one short row per task, and the handoff. Keep it under about 150 lines; replace outdated status instead of stacking new "latest" paragraphs.
- `docs/progress-log.md` holds history. Append each task's verification details, evidence links and handoff as a dated entry at the end, and link the task row to it. Read the log only when a task's history is needed; search it by task ID.
- Before work, read the tracker and relevant design documents. After meaningful work, update task status, blockers, and the next action in the tracker, and add the verification evidence to the log. Leave a clear handoff before ending a session.
- Research results go in `docs/research/`. They are references for decisions, not decisions.
- Keep task status in the tracker, long-term milestones in `docs/development-plan.md`, and design decisions in their owning documents. Link instead of duplicating details.
- Distinguish implemented, verified, and awaiting user review. Do not mark subjective approval complete on the user's behalf.
- At milestone completion, reconcile the tracker and docs against the actual project. Record failed or incomplete checks honestly.
- Temporary specialist/reviewer agents may be useful for bounded tasks when delegation is authorized; they are not required for every task. The primary agent integrates their findings and alone maintains the shared tracker during parallel work.
