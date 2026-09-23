# Project
First-person prison sandbox game. Unity project setup is underway; gameplay implementation has not started. Read `docs/progress.md` for verified implementation status.

# Sources of truth (in priority order)
1. `docs/` (start at `docs/README.md`). Current and authoritative.
2. `docs/history/original-ideas.md`. My original ideas in my own wording. Some are still open (quotas, multipliers, escape pieces).
3. `docs/history/first-conversation.md`. Raw early chat log. History only; its suggestions are NOT decisions.

# Design rules
- I make design decisions. Label every idea agreed, proposed, or open, per "How to maintain these notes" in `docs/README.md`.
- Never promote a proposal to agreed, or drop one of my ideas, without asking me.
- If you think my idea is bad, say so and why. Do not quietly design around it.
- When I decide something, update its owning doc in `docs/` and its status. Do not edit `docs/history/` except to fix links.

# Communication
1. Be direct and brief.
2. Explain when needed, in plain terms for someone new to game dev and Unity (I'm an engineer). No analogies. Comparisons to other games are fine.
3. Don't agree to be agreeable. Correct me when I'm wrong and give the reason.
4. Use my vocabulary. Introduce a new term only if it's shorter, and define it once.

# Environment
Windows. Use `python`, not `python3`.

# Progress tracking - agreed workflow
- The primary development agent owns `docs/progress.md`; no always-running tracker agent is required.
- Before work, read the tracker and relevant design documents. After meaningful work, update task status, verification evidence, blockers, and the next action. Leave a clear handoff before ending a session.
- Keep task status in the tracker, long-term milestones in `docs/development-plan.md`, and design decisions in their owning documents. Link instead of duplicating details.
- Distinguish implemented, verified, and awaiting user review. Do not mark subjective approval complete on the user's behalf.
- At milestone completion, reconcile the tracker and docs against the actual project. Record failed or incomplete checks honestly.
- Temporary specialist/reviewer agents may be useful for bounded tasks when delegation is authorized; they are not required for every task. The primary agent integrates their findings and alone maintains the shared tracker during parallel work.
