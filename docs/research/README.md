# Research

Sourced research that informs the user's decisions. Findings are references, not decisions: the user decides, and each decision is recorded in its owning design doc (see [docs README](../README.md)), which overrides a report where they differ.

## Topics

| Topic | Report | Status | Answers |
| --- | --- | --- | --- |
| combat | [combat/report.md](combat/report.md) | Complete, September 25, 2026 (RESEARCH-01) | How to design, implement, organize, test and produce real first-person melee; 13 open decisions, of which 1, 4 and 5 are now decided |

## Layout

```
docs/research/
  README.md                 this file: topic index and how to read research
  <topic>/                  lowercase kebab-case, no spaces
    report.md               entry point: the answer, open decisions, changelog
    notes/<subtopic>.md     one file per researcher run; the evidence behind the report
```

## Reading research

- Start with a topic's `report.md`. Open a notes file only when you need the evidence behind a claim. Reports are long, so read the sections the task needs.
- Each source carries a confidence label (Official, Primary, Secondary, Community, Fan, Excerpt), and each recommendation a status (Agreed, Proposed, Open). They are defined in [formats.md](../../.agents/skills/game-research/references/formats.md).

## Running research

The workflow is the `game-research` skill in [.agents/skills/game-research/](../../.agents/skills/game-research/SKILL.md), written once in the open Agent Skills format so that any tool can use it.

| Tool | Finds the workflow through | Parallel researchers |
| --- | --- | --- |
| Codex | `.agents/skills/` (loaded natively; invoke with `$game-research`) | `.codex/agents/game-researcher.toml` and `research-writer.toml`; ask Codex to spawn them |
| Claude Code | `.claude/skills/game-research/` pointer (invoke with `/game-research`) | `.claude/agents/game-researcher.md` and `research-writer.md` |
| Any other agent | `AGENTS.md`, which points to the skill | Runs the roles one after another |

To request research, say for example "Research how prison games structure escape routes. Topic: escape." To continue a topic: "Continue the combat research on weapon durability and injuries."
