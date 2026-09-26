# Testing standards for a Unity 6.3 project

Topic: engineering-standards · Date: 2026-09-25 · Serves: RESEARCH-02
Searched: Unity 6.3/6.4 manual Test Framework pages, Unity's automated-testing guide, the Code Coverage package page, GameCI and GitHub billing docs; about 9 searches and page reads. The [combat report](../../combat/report.md#three-fast-test-layers-catch-most-combat-bugs) already covers the test layers for combat; this file covers project-wide rules.
Assumptions: none. Project facts checked on this PC:
- Test Framework 1.6.0 and the Performance Testing package are installed.
- The first NUnit tests (28 edit-mode, category `Combat`) were added on 2026-09-25 and run through the Pipeline CLI's `run_tests`.
- Codex's earlier checks are runtime classes in `Assets/Prototype/Scripts`, wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD`. In the Editor, `tools/unity/verify-*.cs` snippets run them. In a Development executable, a command-line flag starts them (for example `-prototype-smoke-test`), and `tools/test-windows-*.ps1` launches the build, waits, and reads a PASS/FAIL report file.

## 1. What does Unity recommend for structuring tests?

**Takeaway:** Arrange-Act-Assert structure, dedicated test assemblies, `[Test]` by default, and `[UnityTest]` only when a test must wait frames or time. Edit mode is for code that needs no running game; Play mode is for runtime behavior.

**Findings:**
- The Arrange section sets up objects and inputs, the Act section calls the method under test, and the Assert section checks the result. Tests live in a folder with their own `.asmdef`, which must reference the game assemblies and `UnityEngine.TestRunner` ([Unity](https://unity.com/how-to/automated-tests-unity-test-framework), Official, written against Test Framework 1.3.3; still applies to 1.6).
- "Generally, you should use the NUnit Test attribute instead of the UnityTest attribute in Edit mode, unless you need to yield special instructions, need to skip a frame, or wait for a certain amount of time in Play mode" (same source).
- For CI, the same guide splits building and running with `ITestPlayerBuildModifier` and writes XML results with `ITestRunCallback` (same source).

**Inferences:**
- Proposed: one test assembly per game assembly and mode, for example `PrisonGame.Combat.Tests.EditMode`, each test class tagged with its area category (`[Category("Combat")]`). Test names read as behavior sentences, as the combat tests already do (`AttacksDoNotHurtADownedFighter`).
- Proposed: edit-mode tests for every plain C# rule class, written with the class. Play-mode tests for physics, scene wiring and frame-driven behavior.

**Gaps:** no official Unity guidance on test doubles (mocks and fakes) was found.

## 2. Can the same tests run against the built Windows game?

**Takeaway:** yes. Play-mode tests can be built into a Player and run on Windows, reporting results back to the Editor. The project's existing flag-triggered checks do the same job in their own way, and both approaches can coexist.

**Findings:**
- The Test Runner's Player tab builds and runs Play-mode tests on the active platform, or only builds them ("Build All Tests"). Player and Editor "must be on the same network" for results to come back; if the connection fails, tests may appear to pass in the application but no XML results are produced ([Unity 6.4 manual](https://docs.unity3d.com/6000.4/Documentation/Manual/test-framework/workflow-run-playmode-test-standalone.html), Official, 6.4; the 6.3 workflow is the same page family).

**Inferences:**
- Proposed: keep Codex's flag-triggered checks as the executable regression suite until each is replaced. They already cover 106 behaviors in the real build, they are excluded from release builds, and they report through a file, which is more reliable for agents than a network callback.
- Proposed: write new checks as Test Framework tests, not as new runtime check classes. Move an old check into the Test Framework only when its system is changed substantially.
- Open: whether to eventually retire the runtime check classes entirely.

**Gaps:** building a test Player through the Pipeline CLI was not tested.

## 3. Coverage, performance and CI

**Takeaway:** Unity's Code Coverage package is released for 6.3 and could report how much of the rules code the tests exercise. CI in GitHub Actions is possible through GameCI but costs setup: a Unity licence file stored as a secret, plus the free plan's monthly minutes. For one developer on one PC, local checks before each commit give most of the benefit now.

**Findings:**
- Code Coverage package 1.3.0 is "Released" for Unity 6000.3: "Use this package to export code coverage data and reports from your automated tests" ([Unity 6.3 manual](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.testtools.codecoverage.html), Official, 6.3).
- GameCI's `unity-test-runner` runs Unity tests in GitHub Actions. A Personal licence needs a `.ulf` file created with Unity Hub, stored as the `UNITY_LICENSE` secret with `UNITY_EMAIL` and `UNITY_PASSWORD` ([GameCI activation](https://game.ci/docs/github/activation/), Community/Official project docs, v4, undated).
- GitHub Free includes 2,000 Actions minutes a month for private repositories. Windows runners cost about 1.67 times Linux per minute ([GitHub Docs](https://docs.github.com/en/billing/concepts/product-billing/github-actions), Official, undated).

**Inferences:**
- Proposed: no CI yet. One local script runs the formatter check, compiles, runs edit-mode tests, and optionally runs the executable checks. Add GameCI once the project has more than one contributor, or when release builds need to be reproducible away from this PC.
- Proposed: add Code Coverage later, when the rules code is large enough for gaps to be non-obvious. It is not needed for the current 6 classes.
- Open: whether the user wants CI in GitHub at all, given the licence-secret setup and minutes.

**Gaps:** GameCI's page does not state Unity 6 specifics or runtime limits for large Unity projects on hosted runners; how long this project's first CI import would take is unknown.
