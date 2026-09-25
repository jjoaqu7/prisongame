# Research formats and labels

## Naming

- Topic folders: lowercase kebab-case, no spaces, for example `combat` or `escape-routes`.
- Notes files: `notes/<subtopic>.md` in kebab-case, named for the angle, for example `notes/unity-implementation.md`.
- When research on a topic continues, add new notes files. Never overwrite or delete existing notes.

## Notes file

```markdown
# <Subtopic title>

Topic: <topic> · Date: <YYYY-MM-DD> · Serves: <decision or task ID>
Searched: <one line: kinds of sources and rough count>
Assumptions: <anything assumed because the brief was incomplete, or "none">

## 1. <Key question>

**Takeaway:** two or three sentences.

**Findings:**
- <one claim> ([source title](URL), Official, Unity 6.3 manual)

**Inferences:**
- Proposed: <recommendation and why>
- Open: <choice only the user can make>

**Gaps:** <what could not be found or read>
```

Repeat the question block for each key question in the brief.

## Report

```markdown
# <Title that states the answer>

Status: <draft | complete | updated>, <YYYY-MM-DD> (<task ID>). Evidence: [notes](notes/). Conventions: [research README](../README.md).

<Opening paragraph that answers the question directly.>

## <Sections by theme; tables where options are compared; sources cited inline>

## Open decisions for the user

| # | Decision | Options | Proposed | What waits on it |
| --- | --- | --- | --- | --- |

## Changelog

- <YYYY-MM-DD>: <new notes files>; <what changed and why>
```

Add the changelog section only when a report is updated.

**Writing style:** plain terms for an engineer new to game development and Unity. Define a term once when it first appears. No analogies; comparisons to other games are fine.

## Confidence labels, one per source

| Label | Meaning |
| --- | --- |
| Official | Vendor or platform documentation, such as Unity, Valve or Adobe |
| Primary | Developer talk, postmortem, interview or paper |
| Secondary | Journalism or analysis that cites specifics |
| Community | Forum post or tutorial |
| Fan | Fan wiki or player guide |
| Excerpt | Seen only in search results, not read in full |

Record the version and date for engine, package and platform sources. Flag sources written for older versions, and say whether they still apply.

## Status labels, one per recommendation

| Label | Use |
| --- | --- |
| Agreed | Only when an owning design doc records the user's decision. Link to it. |
| Proposed | The research's recommendation |
| Open | A choice only the user can make |
