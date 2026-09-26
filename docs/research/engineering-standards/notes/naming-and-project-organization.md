# Naming and organizing builds, assets, scenes, versions and Git

Topic: engineering-standards · Date: 2026-09-26 · Serves: RESEARCH-02 follow-up (the user asked whether names such as the `WindowsLaundry` build follow any standard, and for other standard rules not yet applied)
Searched: Unity's project-organization guide and e-book page, Unity 6.3 manual (Build Profiles, `persistentDataPath`), GitHub docs (large files, Git LFS billing), a community Unity style guide, community Unity `.gitattributes` templates, the Semantic Versioning and Conventional Commits specifications; about 14 searches and page reads.
Assumptions: none. Project facts checked on 2026-09-26:
- **Builds:** four folders, `Builds/Windows`, `WindowsSample`, `WindowsPerf` and `WindowsLaundry`, each a full copy of about 175 MB, named after the milestone or scene they start in. No Build Profiles exist.
- **Scenes:** 17 in `Assets/Scenes`, named after task IDs (`Duty01_Laundry`, `Art03_M5_Rig`), plus Unity's template `SampleScene`. Prototype folders are also named after tasks (`Art03Rig`, `Duty01`, `Earn01`).
- **Asset names:** 14 contain spaces (`Duty01/Dex burgundy.mat`, `Earn01/Cracker packet.mat`); the rest use `Words_With_Underscores` or PascalCase.
- **Player settings:** `companyName: DefaultCompany`, `productName: PrisonGame`, `bundleVersion: 0.1.0`.
- **Unity version-control settings:** asset serialization Force Text and version-control mode Visible Meta Files are both set.
- **Git:** no Git LFS. The largest tracked file is an 11.5 MB text animation clip; 94 PNG, 27 JPG, 15 WAV and 3 MP3 files are tracked. Commit messages are imperative sentences, some prefixed with a task ID (`ENCOUNTER-01: ...`); the branch is `encounter-01`.

## 1. How are builds named and organized?

**Takeaway:** by platform, configuration and version, not by content. Unity 6 provides Build Profiles for this: a committed asset per build configuration, each with its own scene list. The version belongs in Player Settings, and Semantic Versioning's `0.y.z` range is intended for initial development.

**Findings:**
- A build profile is "a set of configuration settings you can use to build your application on a particular platform". You "can assign specific scenes to each build profile". Profiles are "saved as an asset file that is ready for use with version control", and the manual suggests "saving different configurations for release and development builds" ([Unity 6.3 manual](https://docs.unity3d.com/6000.3/Documentation/Manual/build-profiles.html), Official, 6.3).
- Semantic Versioning: MAJOR.MINOR.PATCH, where "Major version zero (0.y.z) is for initial development. Anything MAY change at any time". It recommends starting at `0.1.0` and incrementing the minor version for each release. Build metadata follows a `+`, as in `1.0.0-beta+exp.sha.5114f85`, and does not affect ordering ([semver.org](https://semver.org/), Official specification).

**Inferences:**
- Proposed: two Build Profiles, `Windows Development` and later `Windows Release`, with output folders `Builds/Windows-Development/` and `Builds/Windows-Release/`. The development profile includes every scene the checks need, and the existing command-line check flags choose what runs. This replaces the four content-named folders.
- Proposed: the version in Player Settings follows `0.MINOR.PATCH`. MINOR goes up when a milestone is finished (for example 0.2.0 when ENCOUNTER-01 closes the prototype loop), and PATCH for fixes. The matching Git commit is tagged `v0.2.0`. Builds given to anyone else are archived as `PrisonGame-0.2.0-windows-development.zip`.

**Gaps:** Steam's own build and branch naming was not researched; it matters only at store setup.

## 2. What does the company name change?

**Takeaway:** the Windows save location is built from the company and product names, so `DefaultCompany` ends up in every player's save path. Changing either later moves where the game looks for saves.

**Findings:**
- On Windows, `persistentDataPath` "usually points to `%userprofile%\AppData\LocalLow\<companyname>\<productname>`" ([Unity 6.3 scripting reference](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-persistentDataPath.html), Official, 6.3). The project's saves currently live in `.../LocalLow/DefaultCompany/PrisonGame/` ([Gameplay](../../../gameplay.md#saving-the-sample---implemented-provisional-controls)).

**Inferences:**
- Proposed: set the real company or studio name, and decide whether "PrisonGame" is the product name to ship, before the first outside playtester (PLAYTEST-01) creates saves. Changing either later strands existing saves unless a migration is written.
- Open: the company name, and the product name to ship.

**Gaps:** none material.

## 3. How should assets, folders and scenes be named?

**Takeaway:** Unity's official rules are no spaces, a documented convention, your own content in one root folder, third-party content kept separate, and non-production scenes in their own folder. For folders, Unity's guide organizes by asset type, while the most-cited community guide, adapted from Unreal's, organizes by feature and puts type prefixes on names. Both insist on consistency.

**Findings:**
- Unity: "Don't use spaces in file and folder names. Unity's command line tools have issues with path names that include spaces. Use CamelCase as an alternative." "Be consistent with your naming conventions." "Document your naming conventions and folder structure." Store content in `Assets` and do not add root-level folders unless necessary. Keep your own assets separate from third-party assets. "Create a separate folder for nonproduction scenes and experimentation." Its examples split subfolders by asset type ([Unity](https://unity.com/how-to/organizing-your-project), Official, undated). The fuller Unity 6 e-book on project organization and version control (October 2024) covers the same ground ([Unity](https://unity.com/resources/best-practices-version-control-unity-6), Official; not read in full, behind a form).
- Community style guide: names follow `Prefix_BaseAssetName_Variant_Suffix` in PascalCase, "Never Use Spaces". Prefixes include `M_` material, `T_` texture, `SM_` static mesh, `SK_` skeletal mesh, `AC_` animator controller and `VFX_` effects; prefabs have no prefix. All project content goes under `Assets/<ProjectName>/`, organized by feature, because type folders are "redundant" once names carry a prefix. It credits Allar's Unreal Engine style guide ([Unity-Style-Guide](https://github.com/justinwasilenko/Unity-Style-Guide), Community).

**Inferences:**
- Proposed: no spaces anywhere. Rename the 14 spaced materials with Unity's rename, which keeps each asset's GUID so scene references survive.
- Proposed: new content goes under `Assets/PrisonGame/`, which already holds the new code, with Unity's type folders: `Art/Materials`, `Art/Models`, `Art/Textures`, `Audio`, `Prefabs`, `Scenes`, `Scripts`, `Settings`. Test scenes go in `Scenes/Sandbox/`. Names are PascalCase, with underscores only between parts (`Inmate_Burgundy`). No type prefixes, because Unity's own guidance does not use them and the Project window shows each asset's type. The prototype's task-named folders and scenes stay as a frozen record, since docs, logs and build scripts refer to them. Delete the template `SampleScene`.
- Open: Unity's type folders without prefixes, or the community's feature folders with type prefixes.

**Gaps:** no survey of what studios use in practice was found; both conventions are widely published.

## 4. Git conventions and large files

**Takeaway:** GitHub warns at 50 MiB, blocks files over 100 MiB, and recommends repositories under 1 GB. Unity projects conventionally store binary art and audio through Git LFS. Git LFS (Large File Storage) keeps large files outside normal history, and GitHub Free includes 10 GiB of LFS storage and 10 GiB of bandwidth. Conventional Commits is a widely used commit-message standard, whose main payoff is automated changelogs and version bumps.

**Findings:**
- GitHub warns above 50 MiB, blocks above 100 MiB, and recommends repositories "ideally less than 1 GB, and less than 5 GB is strongly recommended" ([GitHub Docs](https://docs.github.com/en/repositories/working-with-files/managing-large-files/about-large-files-on-github), Official).
- GitHub Free includes 10 GiB of Git LFS storage and 10 GiB of bandwidth a month. Beyond the storage quota without a payment method, pushes of new LFS files stop; beyond the bandwidth quota, LFS is disabled until the next month ([GitHub Docs](https://docs.github.com/en/billing/concepts/product-billing/git-lfs), Official).
- Community Unity `.gitattributes` templates route images (`png`, `psd`, `jpg`), audio (`wav`, `mp3`, `ogg`), models (`fbx`) and video through LFS. They mark Unity's text assets (`.unity`, `.prefab`, `.mat`, `.asset`, `.meta`, `.controller`, `.anim`) with `eol=lf` and Unity's own merge tool ([Hextant Studios](https://hextantstudios.com/unity-gitattributes/), [community gist](https://gist.github.com/nemotoo/b8a1c3a0f1225bb9231979f389fd4f3f), Community).
- Conventional Commits: `<type>[optional scope]: <description>`. `feat` and `fix` are the core types, and `docs`, `refactor`, `test` and `chore` are common. The standard enables "automated changelog generation" and version bumps, where `fix` maps to PATCH and `feat` to MINOR ([conventionalcommits.org](https://www.conventionalcommits.org/en/v1.0.0/), Official specification).

**Inferences:**
- Proposed: turn on Git LFS now for new binary art and audio, before production assets arrive. Tracking new files is cheap; moving existing history into LFS later means rewriting history. Existing small binaries stay as they are.
- Proposed: keep the current commit style and make it a rule: `<TASK-ID>: <imperative summary>`. It links every commit to the tracker, which matters more here than automated changelogs. Branches are named `<task-id>` or `<task-id>-<topic>` in lowercase, as `encounter-01` already is. Conventional Commits is the alternative if automated changelogs are wanted later.
- Proposed: evidence files are named `<taskid>-<what>-<where>.<ext>` (for example `std01-laundry-editor-checks.txt`), as most already are.
- Open: whether to adopt LFS now or when art production starts, and whether to prefer Conventional Commits over task-ID prefixes.

**Gaps:** Unity's Smart Merge needs a merge-tool setting on each machine. It matters when two people edit the same scene, which does not apply to one developer, so it was not researched further.
