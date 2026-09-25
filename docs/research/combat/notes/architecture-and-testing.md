# Architecture, Project Organization, Testing and Co-op Readiness for Unity First-Person Melee Combat

Research date: 2026-09-25. Target: Unity 6.3 LTS (6000.3.24f1), URP 17.3, Unity Test Framework 1.6, Input System 1.20, Windows/Steam, single-player release 1 then two-player host-authoritative co-op in release 2.

Source-type labels used below:
- **[Official]** Unity or Valve documentation / e-books.
- **[Shipped]** Talks or docs from studios describing a shipped game.
- **[Community]** Forum posts, community tools, third-party blogs. Opinion or secondhand.
- **[Older]** Source predates Unity 6 or is undated; still cited because the content is engine-agnostic or unchanged.

All "Inferences" are this researcher's reasoning from the cited findings. They are proposals, not agreed decisions (per project rules, the user decides).

---

## 1. Project organization: folders, naming, Assembly Definitions, namespaces, migrating a flat prototype folder, separating prototype content

### Takeaway
Unity's official guidance says there is no single correct folder layout, asks for no spaces in names, a separate folder for non-production/experimental content, and namespaces; its examples split by asset type. Assembly Definitions are the one tool that both cuts recompile time and *enforces* dependency direction, and Unity's rule that custom assemblies cannot reference `Assembly-CSharp` dictates how a flat prototype folder has to be migrated: bottom-up, starting with code that depends on nothing else.

### Cited Findings

**Folder structure and naming (official)**
- [Official][Older: undated how-to] Unity: "there is no single way to organize a Unity project." Its example layouts split subfolders by asset type (textures, meshes, audio, etc.), not by feature. — [Unity: Best practices for organizing your Unity project](https://unity.com/how-to/organizing-your-project)
- [Official][Older] "Don't use spaces in file and folder names. Unity's command line tools have issues with path names that include spaces. Use CamelCase as an alternative for spaces." — [Unity: organizing your project](https://unity.com/how-to/organizing-your-project)
- [Official][Older] "Create a separate folder for nonproduction scenes and experimentation. Subfolders with usernames can divide your work area by team member." — [Unity: organizing your project](https://unity.com/how-to/organizing-your-project)
- [Official][Older] Keep your own assets separate from third-party content. — [Unity: organizing your project](https://unity.com/how-to/organizing-your-project)
- [Official][Older] "namespaces can organize your code more precisely. They allow you to separate modules inside your project and avoid conflicts with third-party assets." — [Unity: organizing your project](https://unity.com/how-to/organizing-your-project)
- [Official][Older] Recommends consistency over any particular scheme: "Pick what works for your team, and make sure that everyone is on board with it." — [Unity: organizing your project](https://unity.com/how-to/organizing-your-project)
- [Official] A Unity 6 edition e-book, "Best practices for project organization and version control (Unity 6 edition)", covers folder structure, `.meta` files and naming standards (close to 100 pages; adds Unity Asset Manager and Build Automation sections versus the 2022 edition). Its folder-structure content could not be read in this session (PDF mirror unreachable). — [Unity resource page](https://unity.com/resources/best-practices-version-control-unity-6); [Unity Blog: version control and DevOps in Unity 6](https://unity.com/blog/complete-guide-version-control-devops-unity-6)
- [Official] Unity 6 manual hosts a "Best practice guides" index. — [Unity 6 best practice guides](https://docs.unity.com/en-us/engine/6000.0/manual/best-practice-guides)

**Assembly Definitions (official, Unity 6.3 manual)**
- [Official] Assemblies help you "think clearly about the architecture of your code and about managing dependencies" and "reduce unnecessary recompilation time and make your code easier to debug." — [Unity 6.3 Manual: Assembly definitions](https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definition-files.html)
- [Official] By default "the predefined assemblies reference all other assemblies, including those created with Assembly Definitions." — [Unity 6.3 Manual: Referencing assemblies](https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definitions-referencing.html)
- [Official] Forbidden: (1) references from custom assemblies to the predefined assemblies (`Assembly-CSharp`), (2) explicit references *from* predefined assemblies, (3) cyclical references between two assemblies. — [Unity 6.3 Manual: Referencing assemblies](https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definitions-referencing.html)
- [Official] Turning off **Auto Referenced** stops predefined assemblies from referencing the assembly, and "the predefined assemblies don't recompile when you change code in the assembly." — [Unity 6.3 Manual: Referencing assemblies](https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definitions-referencing.html)
- [Official] By default "Unity recompiles all your assemblies when you update any one of the precompiled assemblies"; you can override references to list only the precompiled libraries an assembly uses. — [Unity 6.3 Manual: Referencing assemblies](https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definitions-referencing.html)
- [Official] Fix for a cycle: "refactor your code to remove the cyclical reference or put the mutually referencing classes in the same assembly." — [Unity Manual: Organizing scripts into assemblies](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html)
- [Official] Inspector and file-format references for `.asmdef` (platforms, define constraints, references). — [Unity 6.3 Assembly Definition Inspector reference](https://docs.unity3d.com/6000.3/Documentation/Manual/class-AssemblyDefinitionImporter.html); [Assembly Definition file format (6000.1)](https://docs.unity3d.com/6000.1/Documentation/Manual/assembly-definition-file-format.html)
- [Official] Edit-mode and play-mode tests "are in different assemblies." — [Unity 6.4 Manual: Edit mode and Play mode tests](https://docs.unity3d.com/6000.4/Documentation/Manual/test-framework/edit-mode-vs-play-mode-tests.html)

### Inferences
- **Hybrid layout is the practical answer.** Unity's how-to examples are type-based, but they also say no single way is right. For code, grouping by feature (for example `Combat/`, `Interaction/`, `Inventory/`, `Save/`) lines up with Assembly Definitions, since each assembly is a folder. Art and audio can stay grouped by type, or by feature as the existing `Duty01/`, `Guard01/` folders already do. This is a proposal, not Unity policy.
- **Suggested target layout (proposal):**
  - `Assets/PrisonGame/` holds production content. `Assets/Prototype/` stays as the sandbox/experiment area, matching Unity's "separate folder for nonproduction scenes and experimentation."
  - `Assets/PrisonGame/Scripts/Combat/Runtime/` (`PrisonGame.Combat.asmdef`): rules, data and interfaces.
  - `Assets/PrisonGame/Scripts/Combat/Tests/EditMode/` (`PrisonGame.Combat.Tests.EditMode.asmdef`, Editor platform only, test assembly).
  - `Assets/PrisonGame/Tests/PlayMode/` (`PrisonGame.Tests.PlayMode.asmdef`).
  - Namespaces mirror assemblies, for example `PrisonGame.Combat`.
- **Migration order is forced by the reference rules.** Custom assemblies cannot reference `Assembly-CSharp`, so any script moved into an `.asmdef` must not use a class still in the flat `Prototype/Scripts` folder. So migrate bottom-up:
  1. Move leaf types first: data structs, interfaces, plain C# rule classes, save data types.
  2. Leave the prototype MonoBehaviours in `Assembly-CSharp`. They can still use the new assemblies because Auto Referenced is on by default.
  3. When a MonoBehaviour depends only on assembly code, move it into a runtime assembly.
  4. New combat code should start inside an assembly from day one. It then cannot reach up into prototype controllers, UI or the camera, which enforces the project's "rules separate from input/camera/UI" rule at compile time.
- **Compile-time win for AI-agent iteration.** Agents trigger many recompiles. Moving combat and tests out of `Assembly-CSharp` means editing a prototype script doesn't recompile the combat assembly. Code in `Assembly-CSharp` is still recompiled whenever an assembly it references changes.
- Names: follow the "no spaces" rule for code and assets. The research-notes folder path used here contains spaces, which is fine for docs, but it would break Unity command-line tools if used under `Assets/`.
- Consider a small `PrisonGame.Core` assembly for shared IDs, time/tick abstraction and events, so `Combat`, `Save` and `AI` reference Core rather than each other. This avoids the forbidden assembly cycles.

### Gaps
- The Unity 6 edition project-organization e-book's actual folder recommendations (feature vs. type, a `_Project` root folder) could not be read. The PDF mirror was unreachable and the landing page only summarizes. Worth reading directly before finalizing the layout.
- No official Unity statement found that endorses feature-based folders for code. That recommendation here is inference.
- No sourced compile-time numbers (seconds saved) for Unity 6.3 were found.
- Did not verify the `.asmdef` "No Engine References" option or its value for pure-C# rule assemblies.

---

## 2. Combat architecture: composition, DamageInfo, data-driven weapons, events, state machines, avoiding god objects/singletons, save integration

### Takeaway
Unity's own Unity 6 e-books point to composition, ScriptableObject data, event channels in place of singletons, and the State/Command/Observer/MVP patterns. Blizzard's Overwatch shows that strictly separating data from logic kept the networked part of gameplay small. For this project, the pieces most likely to matter later are:
- plain C# combat rules that receive the acting player explicitly
- an immutable hit record (`DamageInfo`)
- ScriptableObject weapon definitions holding read-only tuning data
- presentation (sound, VFX, HUD) that only listens to events

### Cited Findings
- [Official] Unity's "Level up your code with design patterns and SOLID" (updated edition, requires Unity 6, published July 23, 2024) covers 11 patterns: Factory, Object Pooling, Singleton, Command, State, Observer, Model View Presenter, Model-View-ViewModel, Strategy, Flyweight, Dirty Flag. It also has an expanded SOLID section with a sample project. — [Unity e-book page](https://unity.com/resources/design-patterns-solid-ebook)
- [Official] Unity 6 edition of "Create modular game architecture in Unity with ScriptableObjects", with the "Paddle Ball" sample. It shows ScriptableObjects making components "testable, scalable, and designer-friendly" and covers pitfalls. — [Unity resource page](https://unity.com/resources/create-modular-game-architecture-scriptableobjects-unity-6); [Unity Discussions announcement](https://discussions.unity.com/t/unity-6-edition-ebook-and-sample-project-on-scriptable-objects/1680222)
- [Official] ScriptableObject event channels:
  - A `VoidEventChannelSO` exposes `UnityAction OnEventRaised` and `RaiseEvent()`. A `GenericEventChannelSO<T>` carries a payload.
  - Broadcasters hold a reference to the channel asset and call `RaiseEvent()`. Listeners subscribe in `OnEnable()` and unsubscribe in `OnDisable()`.
  - Stated benefit: "Every part of the application has a certain autonomy, and that makes each one easier to test."
  - Unity presents channels as an alternative to singletons: "event channels are globally available, so they can connect anything with anything."
  - Acknowledged cost: harder debugging, hence the custom Editor tools that show listener lists.

  — [Unity how-to: ScriptableObjects as event channels](https://unity.com/how-to/scriptableobjects-event-channels-game-code)
- [Shipped][Older: GDC 2017] Blizzard's Tim Ford presented Overwatch's Entity Component System: entities are IDs, components are pure data, systems hold the logic. — [GDC Vault: 'Overwatch' Gameplay Architecture and Netcode](https://www.gdcvault.com/play/1024001/-Overwatch-Gameplay-Architecture-and); [Game Developer summary](https://www.gamedeveloper.com/design/video-how-i-overwatch-s-i-gameplay-architecture-creates-variety)
- [Community/Secondary] A summary of the Overwatch talk says the ECS design reduced the gameplay netcode surface to "three systems out of hundreds." This is secondhand; verify against the talk. — [Edgegap blog: Overwatch netcode deep dive](https://edgegap.com/blog/game-backend-deep-dive-overwatch-2016-netcode-architecture-rollback)
- [Shipped][Older: GDC 2017/2019] Ubisoft's For Honor (a melee game, 8 players plus about 100 NPCs) runs a distributed peer-to-peer *deterministic* simulation. A GDC 2019 talk describes "a variation on deterministic simulation that uses time travel" so that responsiveness is kept. Relevance: For Honor's combat rules had to be deterministic functions of input. — [GDC Vault: Deterministic vs. Replicated AI (For Honor)](https://www.gdcvault.com/play/1024035/Deterministic-vs-Replicated-AI-Building); [GDC Vault: Back to the Future! Deterministic Simulation in For Honor](https://www.gdcvault.com/play/1026322/Back-to-the-Future-Working); [Game Developer write-up](https://www.gamedeveloper.com/design/video-deterministic-vs-replicated-ai-building-i-for-honor-i-s-battlefield)
- [Official] Unity's netcode guidance: the server must "validate player actions coming from clients." This implies rule code has to run on the authority, taking the requesting player as input rather than assuming "the local player". — [NGO 2.7: Dealing with latency](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.7/manual/learn/dealing-with-latency.html)

### Inferences
Proposals for the user to decide. The design values themselves (damage numbers, body-part multipliers, whether blocking exists) are open design questions, not architecture.

**Proposed components**
- **`DamageInfo` (readonly struct).** Carries:
  - `AttackerId`: stable entity ID, not a GameObject reference, so it survives saving and networking.
  - `WeaponId`: weapon definition ID, or "fists".
  - `Amount`, `HitPoint`, `Direction`, `BodyPart` (enum), `AttackKind`.
  - `Time` or `Tick`: the simulation time the hit occurred.

  Passed by `in` to `IDamageable.ApplyDamage(in DamageInfo) -> DamageResult`. `DamageResult` returns what happened: applied amount, killed, blocked. Rules and presentation both read the same record.
- **`Health` as a plain C# class** holding current/max values and death state, raising `Damaged`/`Died` C# events. A thin `HealthComponent : MonoBehaviour, IDamageable` wraps it. Tests can then create a `Health` without a scene.
- **`WeaponDefinition : ScriptableObject`** holds read-only tuning: damage, reach, wind-up/active/recovery times, stamina cost, sounds and animation references. The existing pickup/inventory items point to a `WeaponDefinition`; fists are a definition with no item.
  - Keep **runtime** state (durability, cooldown timers) out of the asset, in a per-instance object. ScriptableObject assets are shared by every user of that asset.
- **`CombatRules` as a pure function set**, for example `ResolveAttack(attackerState, targetState, weaponDef, hitQueryResult) -> HitOutcome`. No `Time.time`, `Input`, `Camera.main` or singletons inside. Time and the acting player come in as parameters. This makes the rules deterministic for a given input, the property For Honor's model depends on, and trivially unit-testable.
- **Player combat state machine** (Idle, WindUp, Active, Recovery, Blocking, Staggered, Dead) as a plain C# class advanced by an explicit `Tick(dt)`. Animation reads the state; animation does not drive the rules. Relying on Animator animation events to deal damage ties hit timing to each machine's frame rate. Chivalry 2's community server notes (section 4) describe that exact problem.
- **Presentation listens to events.** Sound, VFX, camera shake and HUD subscribe to `Damaged`/`Died`/`AttackStarted`. They don't call into rules, and rules don't call them.
  - For per-player HUD, subscribe to *that player's* `Health` instance, not a global "player health" channel. A single global channel becomes wrong the day there are two players.
  - Global ScriptableObject channels are fine for world-level events (for example "fight started in cell block" feeding the existing guard suspicion system).
- **Avoid:**
  - a `CombatManager.Instance` that holds "the player"
  - `FindObjectOfType<Player>()` in rules
  - reading input inside `Health` or `CombatRules`
  - hit detection that uses `Camera.main` directly (pass origin/direction from the acting player's view data)
  - storing mutable combat state on ScriptableObject assets
- **Save integration:**
  - NPC dead/alive and health: shared world state, keyed by a stable, serialized entity ID (for example a GUID string set once in the Editor on each NPC). Saved via the existing typed capture/restore.
  - Player health, and any per-player injury state: per-player state.
  - Bump the save data version when adding combat fields, with defaults for old saves.
  - Whether corpses persist or NPCs respawn is a design decision for the user.
- **Attacker identity matters beyond damage.** The existing guard suspicion system, and later gang brawls, will need to know *who* hit *whom*. That is another reason `DamageInfo` should carry `AttackerId` from day one.

### Gaps
- No official Unity doc found that prescribes a `DamageInfo`/`IDamageable` shape. It is common practice. Valve's Source SDK `CTakeDamageInfo` and Unreal's `TakeDamage(DamageEvent, EventInstigator, DamageCauser)` follow the same idea but were not verified this session.
- The ScriptableObject e-book's specific pitfalls section was not read, including the Editor persisting play-mode changes to ScriptableObject assets. Verify before relying on it.
- No developer write-up found on the internal combat state machines of Chivalry 2, Mordhau or For Honor.

---

## 3. Testing combat: edit-mode vs play-mode, testable structure, deterministic time/physics, fixtures, performance/regression

### Takeaway
Put most combat checks in edit-mode tests against plain C# rules (fast, no scene). Use play-mode tests only for integration: colliders, hit queries, the component wiring, the input-to-attack path. Make those deterministic by stepping physics manually with a fixed step and controlling time explicitly. Unity itself warns that PhysX is not fully deterministic even with a fixed step.

### Cited Findings
- [Official] Unity Test Framework (UTF):
  - Tests code "in both Edit mode and Play mode, and also on target platforms," using a custom NUnit (based on 3.5).
  - Unity tests "can interact with Unity-specific concepts such as frames, the application loop, and domain reload." `yield return null` skips a frame.
  - Tests run from the Test Runner window, "from the command line, or from code."

  — [Unity 6.2 Manual: Testing your code](https://docs.unity3d.com/6000.2/Documentation/Manual/test-framework/test-framework-introduction.html)
- [Official] For Unity 6.2+, the UTF user guide lives in the Unity Manual. The package page for 1.6 only hosts the scripting API. — [UTF 1.6 package docs](https://docs.unity3d.com/Packages/com.unity.test-framework@1.6/manual/index.html)
- [Official] Edit-mode vs play-mode tests:
  - Edit-mode tests run in the `EditorApplication.update` loop, can use `UnityEditor` and `UnityEngine`, and can't run coroutines. They can enter and exit Play mode.
  - Play-mode tests run as coroutines when marked `[UnityTest]`, in the Editor or in a Player.
  - The two kinds live in different assemblies.

  — [Unity 6.4 Manual: Edit mode and Play mode tests](https://docs.unity3d.com/6000.4/Documentation/Manual/test-framework/edit-mode-vs-play-mode-tests.html) (6000.4 page; applies to 6.3)
- [Official] Command line: `Unity.exe -runTests -batchmode -projectPath <path> -testResults <file.xml> -testPlatform EditMode|PlayMode`.
  - Results are NUnit XML.
  - `-testCategory` (semicolon-separated) and `-testFilter` (names or regex) select subsets.

  — [Unity 6.4 Manual: Run tests from the command line](https://docs.unity3d.com/6000.4/Documentation/Manual/test-framework/run-tests-from-command-line.html); [UTF 1.4 command-line reference](https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/reference-command-line.html)
- [Official] Tests can also be launched from code, useful for agent-driven Editor sessions over MCP. — [Unity 6.3 Manual: Running tests from code](https://docs.unity3d.com/6000.3/Documentation/Manual/test-framework/running-tests-from-code.html)
- [Official] `SimulationMode.Script` makes physics run only when you call `Physics.Simulate`. A simulate call runs collision detection, rigidbody/joint integration and contact/trigger callbacks. — [Unity Scripting API: SimulationMode.Script](https://docs.unity3d.com/ScriptReference/SimulationMode.Script.html); [Physics.Simulate](https://docs.unity3d.com/ScriptReference/Physics.Simulate.html); [SimulationMode (6000.0)](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SimulationMode.html)
- [Official] Pass a fixed step to `Physics.Simulate` every call. Steps above 0.03 are "likely to produce inaccurate results." Calling `Physics.Simulate` does **not** call `FixedUpdate`, which still runs at `Time.fixedDeltaTime`. — [Unity Scripting API: Physics.Simulate](https://docs.unity3d.com/ScriptReference/Physics.Simulate.html)
- [Official] Unity 6.3 manual: "the recommended best practice is to use a fixed time step, rather than directly passing Time.deltaTime" (suggests 0.02 s). "Using a fixed step is crucial for more reproducible physics behavior (though true determinism with PhysX has other challenges)." — [Unity 6.3 Manual: Manually set physics simulation](https://docs.unity3d.com/6000.3/Documentation/Manual/physics-optimization-cpu-manual-simulation.html)
- [Official] `PhysicsScene.Simulate` steps one specific physics scene. Useful for isolating a test scene's physics. — [Unity Scripting API: PhysicsScene.Simulate](https://docs.unity3d.com/ScriptReference/PhysicsScene.Simulate.html)
- [Official] `Time.captureDeltaTime`: when non-zero, `Time.time` advances by that fixed interval per frame "regardless of real time and the duration of a frame". It doesn't affect `Time.unscaledTime`. — [Unity Scripting API: Time.captureDeltaTime](https://docs.unity3d.com/ScriptReference/Time-captureDeltaTime.html); [Unity 6.3 Manual: Managing time and frame rate](https://docs.unity3d.com/6000.3/Documentation/Manual/managing-time-and-frame-rate.html)
- [Official] Input System `InputTestFixture` creates an isolated, blank input system per test and restores it afterwards. Real hardware input can't interfere. Helpers: `Press`, `Release`, `PressAndRelease`, `Set`, `Trigger`. — [Input System 1.14: Input testing](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Testing.html) (docs version 1.14; project has 1.20, API assumed compatible but not verified)
- [Official] Performance Testing package: `Measure.Frames()` records frame times with warmup, measurement count and a dynamic count option (2% margin, 99% confidence by default). Package docs exist up to 3.3. — [Performance testing API 3.3: Measure.Frames](https://docs.unity3d.com/Packages/com.unity.test-framework.performance@3.3/manual/measure-frames.html); [Package index 3.1](https://docs.unity3d.com/Packages/com.unity.test-framework.performance@3.1/manual/index.html)
- [Official] Unity's UTF course includes a performance-tests chapter. — [Unity 6.2 Manual: 8. Performance Tests](https://docs.unity3d.com/6000.2/Documentation/Manual/test-framework/course/LostCrypt/performance-tests.html)

### Inferences
- **Test pyramid for combat (proposal):**
  1. **Edit-mode unit tests (most tests):**
     - `Health` (damage, clamp, death once, no damage after death)
     - `CombatRules.ResolveAttack` (weapon damage, body-part handling, block outcomes once designed)
     - state machine timing with explicit `Tick(dt)` (wind-up/active/recovery windows, cancel rules)
     - `WeaponDefinition` validation (no negative times, reach > 0) over all assets found via `AssetDatabase`
     - save round-trip: capture then restore of NPC dead/alive and player health, plus loading an older save version
  2. **Play-mode integration tests:**
     - a code-built fixture (new GameObjects with CapsuleColliders, no scene dependency) or a dedicated small test scene
     - set `Physics.simulationMode = SimulationMode.Script`, place attacker and target, call the hit query, step `Physics.Simulate(0.02f)` a fixed number of times, then assert on `DamageResult` and events
     - restore the simulation mode in `TearDown`
     - NPC fight-back: step N ticks and assert the NPC enters attack state and damages the player
  3. **Input-to-intent tests** with `InputTestFixture`: pressing attack produces one `AttackIntent` for the right player. Checks the input layer without involving rules.
  4. **Performance/regression:** a `[Performance]` play-mode test with N NPCs brawling (for later gang brawls), measured with `Measure.Frames()`. Keep a baseline in CI results.
- **Avoid `Time.time`/`Time.deltaTime` inside rules.** Rules take `dt` or a tick number. Tests then need no real waiting, and the same rules can later run on a host tick.
- **Tag tests with categories** (for example `[Category("Combat")]`) so agents can run `-testCategory Combat` quickly. Run edit-mode on every change, play-mode before build checks.
- Because PhysX is not fully deterministic, assert on outcomes (hit or miss, died) with generous geometry margins, not on exact float positions after many steps.

### Gaps
- Did not verify UTF 1.6-specific new features (retry/repeat attributes, async `Task` test support details) against the 6.3 manual.
- Did not find official Unity guidance on testing `Animator`-driven combat timing deterministically. The inference is to keep timing in code, not animation.
- `Physics.SyncTransforms` / `autoSyncTransforms` behavior for raycasts after moving objects in the same frame was not checked. Relevant when tests teleport objects and immediately query.

---

## 4. Networked melee (release 2) and what to do now

### Takeaway
Shipped games split two ways:
- **Server/host-authoritative with lag compensation** (Source engine, Overwatch; Unity's recommended default). The client plays the swing animation at once, and the authority decides the hit.
- **Deterministic peer-to-peer** (For Honor).

For a two-player host-authoritative co-op, the NGO pattern is to show the swing locally at once and let the host confirm the hit. Netcode for GameObjects has **no built-in rewind**. What to do now:
- express attacks as intent objects from an explicit player
- keep rules deterministic and time-parameterized
- give entities stable IDs
- keep hit queries free of `Camera.main`
- keep presentation separable from outcome

Don't build any networking.

### Cited Findings

**Authority and latency in Unity's netcode (official)**
- [Official] "The authority is who has the right to make final gameplay decisions over objects. A server authoritative game has all its final gameplay decisions executed by the server." Client authority is more responsive but lets players forge messages. — [NGO 2.7: Tricks and patterns to deal with latency](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.7/manual/learn/dealing-with-latency.html)
- [Official] Boss Room (Unity's NGO sample) melee: "Boss Room plays an animation on Melee action client side while waiting for the server to confirm the swing. If the server doesn't confirm, worst comes to worst we've played an animation for nothing." — [NGO 2.7: Dealing with latency](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.7/manual/learn/dealing-with-latency.html)
- [Official] NGO provides `AnticipatedNetworkVariable` and `AnticipatedNetworkTransform`, which separate "authoritative" values from displayed values. These are building blocks, not a full prediction system. — [NGO 2.7: Dealing with latency](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.7/manual/learn/dealing-with-latency.html)
- [Official] "There's no server side rewind implementation right now in Netcode for GameObjects, but you can implement your own." The page describes "favor the attacker" server rewind as a security check on a client-driven feature. — [NGO 2.7: Dealing with latency](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.7/manual/learn/dealing-with-latency.html)
- [Official] "Your server must have logic to validate player actions coming from clients." — [NGO 2.7: Dealing with latency](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.7/manual/learn/dealing-with-latency.html)
- [Official] NGO has two authority models.
  - **Server authority:** one instance runs the main simulation. This is the client-server model. It enables rollback and competitive client prediction at the cost of added latency.
  - **Distributed authority:** each instance owns a subset of objects.
  - Unity suggests server authority for performance-sensitive titles such as first-person shooters.
  - `HasAuthority` is the recommended session-mode-agnostic check (`if (!HasAuthority) return;`).

  — [NGO 2.11: Authority](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.11/manual/terms-concepts/authority.html); NGO docs exist up to at least 2.13 — [NGO 2.13: Distributed authority](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/terms-concepts/distributed-authority.html)
- [Official] Ownership permission settings (`NetworkObject.OwnershipStatus`) only take effect under distributed authority. — [NGO 2.5: Understanding ownership and authority](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.5/manual/basics/ownership.html)
- [Official] Netcode for Entities (DOTS) has built-in lag compensation.
  - `PhysicsWorldHistory` stores past physics collision worlds, so the server can query what the client saw at a given tick. It is enabled via `NetCodePhysicsConfig` (EnableLagCompensation).
  - Client history size is usually 1.

  Relevant only if the project moved to ECS, which is a large change for a GameObject project. — [Netcode for Entities 1.4: Physics](https://docs.unity3d.com/Packages/com.unity.netcode@1.4/manual/physics.html); [Netcode for Entities 1.0: Physics](https://docs.unity3d.com/Packages/com.unity.netcode@1.0/manual/physics.html)

**Lag compensation (Valve and general)**
- [Official][Older: Source engine] Lag compensation is "the server using a player's latency to rewind time when processing a usercmd, in order to see what the player saw when the command was sent."
  - The server keeps a history of recent player positions for one second.
  - Command Execution Time = Current Server Time − Packet Latency − Client View Interpolation.
  - Rewind options: locations only, locations plus hitboxes (the standard), or hitboxes only when the weapon ray hits the bounds.

  (Fetched via search snippet; direct page returned HTTP 403.) — [Valve Developer Community: Lag Compensation](https://developer.valvesoftware.com/wiki/Lag_Compensation); [Source Multiplayer Networking](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking)
- [Community/Expert][Older] The client sends "the exact timestamp of your shot, and the exact aim of the weapon." The server "can reconstruct the world exactly as it looked like to any client at any point in time." The tradeoff: a target can be hit shortly "after they took cover". — [Gabriel Gambetta: Lag Compensation](https://www.gabrielgambetta.com/lag-compensation.html)

**Shipped melee games**
- [Shipped][Older: GDC 2017] Overwatch: a server-authoritative, prediction-heavy design on a strict ECS. The Q&A covers predicting slow projectiles. — [GDC Vault: Overwatch Gameplay Architecture and Netcode](https://www.gdcvault.com/play/1024001/-Overwatch-Gameplay-Architecture-and)
- [Shipped][Older: GDC 2017/2019] For Honor: distributed peer-to-peer deterministic simulation, "a non-authoritative architecture", with "time travel" (rollback) to keep responsiveness. — [GDC Vault: Back to the Future! (For Honor)](https://www.gdcvault.com/play/1026322/Back-to-the-Future-Working); [GDC Vault: Deterministic vs. Replicated AI](https://www.gdcvault.com/play/1024035/Deterministic-vs-Replicated-AI-Building)
- [Community] Chivalry 2 community server tooling says the game's netcode is "very sensitive to host and client FPS". It strongly recommends capping host FPS and matching client FPS, otherwise players at lower FPS get "delayed hits, swing-throughs, and other netcode issues." This is community documentation, not Torn Banner. — [Chiv2-Community/C2ServerAPI (GitHub)](https://github.com/Chiv2-Community/C2ServerAPI)
- [Community] Chivalry 2 players report mismatches between what they see and what registers, even under 70 ms latency. — [Steam discussion: Chivalry 2 hit detection](https://steamcommunity.com/app/1824220/discussions/0/3821910883988394300/)
- [Community] A common pattern: the client claims a hit, and the server re-tests only claimed hits, or runs heuristic plausibility checks (distance, timing, angle) instead of full re-simulation. — [GameDev.net: Server validation for client-side weapon hit detection](https://gamedev.net/forums/topic/683828-server-validation-for-client-side-weapon-hit-detection/)

### Inferences
**What to do now, in release 1** (no networking code; each item also improves single-player testability):
1. **Intent/command objects from an explicit actor.** Input produces something like `AttackIntent { ActorId, AttackKind, WeaponSlot, AimOrigin, AimDirection, Tick }`, and a single entry point, for example `CombatSystem.Submit(intent)`, consumes it.
   - In release 1 the local input layer calls it directly.
   - In release 2 a client sends the same struct to the host by RPC, and the host calls the same entry point.
   - This matches the project's "pass the acting player explicitly" rule and Unity's "server must validate player actions".
2. **Deterministic, time-parameterized rules.** Rules take `dt`/tick and state in and return outcomes. No `Time.time`, `Random` without a passed seed/stream, `Camera.main`, `Input` or singletons. This lets the host re-run rules on a remote player's intent. It also keeps the option of rollback-style approaches (For Honor) open, without committing to them.
3. **Hit queries take the acting player's view data, not the local camera.** Use origin, direction, reach and radius from the attacker's head/weapon transform or the intent. In co-op, the host must compute player 2's swing from player 2's data.
4. **Stable entity IDs** for players, NPCs and world items, serialized and independent of GameObject names or instance IDs. They serve the save system now and map to network object IDs later. `DamageInfo.AttackerId` uses them.
5. **Split each attack into "presentation" and "outcome".** Start the wind-up animation and swing sound immediately on the attacker's machine. The damage, stagger and death outcome is a separate event from the rules. This is exactly the split Boss Room uses over the network.
6. **Shared vs per-player state:**
   - Per-player: health, stamina, combat state, equipped weapon.
   - Shared world state: NPC health, dead/alive, bodies, dropped weapons.
   - The host keeps the save, so per-player state for player 2 must also be saveable by the host. Key per-player save data by player slot or ID, not "the player".
7. **Don't drive hit timing from Animator events or frame count.** Chivalry 2's community notes show frame-rate-dependent melee netcode causing "swing-throughs". Put active-frame windows in code on a fixed tick, with animation following.
8. **Keep a cheap position history later, not now.** NGO has no rewind. For two players, the likely release-2 approach is favor-the-attacker validation with a short per-character position/hitbox history on the host, in the Valve/Gambetta style. A plausibility check (distance, angle, timing) may be enough for co-op, where cheating matters less than in PvP. This is only possible cheaply if hit queries are already a function like `Query(attackerView, targetPoseAt(time))`. Don't build the history buffer in release 1.
9. **Things to explicitly avoid now:**
   - "the player" singletons
   - UI or camera code that mutates combat state
   - combat logic inside `Update()` of view scripts
   - GameObject references as saved identity
   - client-trusting designs (for example the victim's own script deciding it was hit)
   - speculative NGO packages or `NetworkBehaviour` base classes
   - premature adoption of Netcode for Entities (built-in lag compensation, but it requires moving to ECS)
10. **NGO host mode = server authority with the host also playing.** The host player has zero latency to the authority and player 2 does not, so the player-2 experience (anticipated swings, favor-the-attacker validation) is the release-2 design problem. Worth a design note, but no code now.

### Gaps
- No primary Torn Banner (Chivalry 2) or Triternion (Mordhau) developer write-up on melee hit detection or netcode architecture was found. Claims about those games rest on community sources only.
- The Overwatch and For Honor talks are on GDC Vault (video, some possibly members-only). This session relied on abstracts and summaries, not the talks themselves.
- Valve pages returned HTTP 403, so content came from search snippets of the official page. The one-second history and the formula match the widely known text, but were not re-read in full.
- Did not research the Multiplayer Center package's recommendations for a 2-player co-op game, or Unity Relay/Lobby/Steam transport options. Out of scope for "don't build networking now", but relevant to release 2.
- No sourced guidance found specifically on *melee* (swept volume over time) lag compensation versus hitscan. The inference is that the same history-rewind idea applies per active-frame sweep, but no shipped-game source confirms how melee titles do it.
