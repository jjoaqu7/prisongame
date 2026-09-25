# Unity 6 (6000.3) / URP 17 implementation techniques for first-person melee combat

Scope: engine-side techniques for a first-person melee system in Unity 6.3 LTS with URP 17. Research date: 2026-09-25. Source labels: **[Official]** = Unity Manual, Scripting API, package docs, or Unity-Technologies GitHub. **[Community]** = forums, blogs, and designer write-ups (opinion or practice, not engine guarantees). Where a page was only seen as a search-result summary and not fetched in full, it is marked "(search summary)". Treat those as lower confidence.

---

## 1. Hit detection: physics queries vs weapon triggers vs sweeps, active windows, one hit per swing

### Takeaway
No official Unity page recommends a melee hit-detection method. Unity gives you the building blocks: garbage-free `...NonAlloc` overlap and cast queries that are still supported in 6.3, plus CCD that cannot catch rotational tunneling when sweep-based. Community practice and engine limits both point to **script-driven queries (overlap or sweep casts) run only during an animation-timed active window, with a per-swing hit set**, instead of relying on `OnTrigger` callbacks from a swinging weapon collider.

### Cited Findings
- **[Official]** `Physics.OverlapSphereNonAlloc` is not deprecated in 6.3. It "Computes and stores colliders touching or inside the sphere into the provided buffer" and "generates no garbage". It "Does not attempt to grow the buffer if it runs out of space", and "The length of the buffer is returned when the buffer is full". It takes a `layerMask` and a `QueryTriggerInteraction` (default `UseGlobal`). The doc example keeps the results array as a reused field. The doc says nothing about result ordering. — [Physics.OverlapSphereNonAlloc (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Physics.OverlapSphereNonAlloc.html)
- **[Official]** The 6.3 `Physics` class lists `OverlapBox/Capsule/Sphere` and their `NonAlloc` variants, `BoxCast/CapsuleCast/SphereCast/Raycast` with `All` and `NonAlloc` variants, `Linecast`, `CheckBox/Capsule/Sphere`, `ClosestPoint` ("Returns a point on the given collider that is closest to the specified location") and `ComputePenetration` ("Compute the minimal translation required to separate the given colliders"). None are marked obsolete on the class page. — [Physics class (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Physics.html)
- **[Official]** CCD calculates "collisions that happen between physics timesteps" at extra cost. It supports "Box, Sphere and Capsule colliders". — [Continuous collision detection (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/ContinuousCollisionDetection.html)
- **[Official]** Sweep-based CCD (the Continuous and Continuous Dynamic modes) only handles linear movement: "it cannot predict collisions that might happen if the physics body rotates". The doc's example is a pinball flipper, which "only has angular motion". It adds: "If you also need to account for an object's rotation, use speculative CCD." — [Sweep-based CCD (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/sweep-based-ccd.html)
- **[Official]** `Physics.SyncTransforms` flushes Transform changes to the physics engine. Unity "already syncs transforms automatically before each physics step". Call it after changing Transforms in `Update`/`LateUpdate` if you then run immediate physics queries. It is "expensive if called every frame unnecessarily". — [Physics.SyncTransforms (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Physics.SyncTransforms.html)
- **[Official]** In 6.0, `Physics.autoSyncTransforms` exists and is not obsolete. When false, "synchronization only occurs prior to the physics simulation step during the Fixed Update". Unity positions it as a backwards-compatibility option for projects made before 2017.2 and recommends leaving it off. — [Physics.autoSyncTransforms (6000.0)](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics-autoSyncTransforms.html). In the **6.3** Scripting API the same page returns 404 and the property is not listed on the Physics class page. — [6000.3 URL (404)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Physics-autoSyncTransforms.html); [Physics class (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Physics.html)
- **[Community]** A Unity Discussions thread weighs the options. A raycast is precise but "clunky" and needs player accuracy. `OverlapSphere` is more forgiving but has close-range issues, and `OverlapCapsule` works better than a sphere up close. A weapon collider with `OnCollisionEnter` needs a per-attack list, checked with `Contains`, so one swing cannot hit the same target twice. No Unity staff posted. — [Unity Discussions: Raycast or OverlapSphere for melee weapons](https://discussions.unity.com/t/raycast-or-overlapsphere-for-melee-weapons/927490)
- **[Community, Unreal product page, concept only]** Colliders and simple traces are framerate-dependent and can miss hits when a weapon moves further in one frame than its own size. The fix is to "sweep" the trace over the distance covered each frame and interpolate size and rotation between start and end. — [Enhanced Melee Trace for UE5 (itch.io)](https://pantheraonline.itch.io/enhanced-melee-trace-for-ue5) (search summary)

### Inferences
- **Recommended for this project:** drive hit detection from a script, not from trigger callbacks.
  1. The attack's timing data (from the weapon definition, section 7) or an animation event opens and closes an **active window**.
  2. Each tick while the window is open, sample 2 to 3 points along the striking part (fist, or rod from grip to tip). Query the volume swept from last tick's positions to this tick's: `CapsuleCastNonAlloc`/`SphereCastNonAlloc` from each point's previous to current position, or an `OverlapCapsuleNonAlloc` between them. Use a `Hurtbox` layer mask and an explicit `QueryTriggerInteraction`.
  3. Resolve each hit collider to its owning combatant and skip any combatant already in the swing's `HashSet`. Clear the set when the swing starts.

  This avoids the rotational-tunneling gap that sweep-based CCD cannot cover. It also makes hit timing independent of `OnTrigger` callbacks, which only fire on physics steps.
- **Player attacks should not use the viewmodel's bone positions for queries.** The viewmodel (section 3) is typically drawn with an overridden FOV and position offset, so its on-screen position does not match a world position. Build the player's query volume from the gameplay camera or body transform instead: for example, a capsule from the camera forward out to the weapon's reach, swept across the swing arc during the active window. NPC attacks can use their real (world-space) hand or weapon bones.
- If NPC hurtboxes sit on animated bones and the query runs in `Update`/`LateUpdate`, call `Physics.SyncTransforms()` once before the batch of queries, or run the queries in `FixedUpdate`. Without that, the colliders may lag the rendered pose by a physics step (per the SyncTransforms doc).
- `ClosestPoint` on the hit collider gives a contact position for effects and knockback direction. `ComputePenetration` is an alternative for the hit normal.
- **Co-op separation:** keep the query and damage resolution in a combat rules component that takes the attacking combatant as a parameter (`TryResolveSwing(attacker, weaponDef, swingState)`). Local input only requests the swing. That lets a release-2 authority re-run the same function.

### Gaps
- Result ordering of `Overlap*NonAlloc` is undocumented. Do not rely on it.
- Why `Physics.autoSyncTransforms` is absent from the 6.3 docs (removed, moved, or a docs gap) is unconfirmed. Check in the Editor (e.g. `Physics.autoSyncTransforms` in a test script) before depending on either behavior.
- No official Unity guidance or GDC talk on melee hit detection was found in this pass. The sweep argument rests on the CCD documentation plus community sources.
- Whether triggers get CCD was not documented on the pages fetched.

---

## 2. Hitboxes/hurtboxes on skinned NPCs, layers and the collision matrix, knockback with a CharacterController

### Takeaway
Unity's documented pieces are: colliders and Rigidbodies generated per bone by the Ragdoll Wizard, a layer collision matrix for contacts, `layerMask` on queries, and a CharacterController that is "not affected by forces". The standard pattern is to reuse the ragdoll's per-bone colliders as hurtboxes while alive, and to implement knockback as scripted displacement through `CharacterController.Move`.

### Cited Findings
- **[Official]** The Ragdoll Wizard (GameObject > 3D Object > Ragdoll…) generates "all colliders, rigidbody components, and joints that make up a ragdoll from your skinned mesh character". You map limbs from the Hierarchy. Importing with "Generate Colliders" off is optional. — [Ragdoll Wizard (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/wizard-RagdollWizard.html)
- **[Official]** The Layer Collision Matrix (Project Settings > Physics) "defines which GameObjects can collide with which Layers". The page does not say whether the matrix affects raycasts or overlaps. — [Layer-based collision detection (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/LayerBasedCollision.html)
- **[Official]** Queries filter with their own `layerMask` parameter, "which layers of colliders to include in the query", and a trigger-inclusion parameter. — [Physics.OverlapSphereNonAlloc (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Physics.OverlapSphereNonAlloc.html)
- **[Official]** CharacterController is "not affected by forces and will only move when you call the Move function". It gives "movement constrained by collisions without having to deal with a rigidbody". `OnControllerColliderHit` "is called when the controller hits a collider while performing a Move". `detectCollisions` controls "whether other rigidbodies or character controllers collide with this character controller (by default this is always enabled)". — [CharacterController (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/CharacterController.html)
- **[Official]** Ragdoll stability advice includes: avoid "direct Transform access on Kinematic Rigidbodies connected to other Rigidbodies" (the page cites `Rigidbody2D.MovePosition`/`MoveRotation`, which looks like a docs slip for 3D), and keep Transform scale at 1 on objects with Rigidbodies or Joints. — [Joint and Ragdoll stability (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/RagdollStability.html)
- **[Official]** Mecanim performance advice: "Always optimize animations by setting the animator Culling Mode to Cull Completely, and disable the skinned mesh renderer's Update When Offscreen property." — [Mecanim performance and optimization (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/MecanimPeformanceandOptimization.html)

### Inferences
- **Hurtboxes:** run the Ragdoll Wizard on the NPC rig once. Keep the generated Rigidbodies **kinematic** while alive and put their colliders on a dedicated `Hurtbox` layer. Add a small `HurtboxPart` component to each bone collider holding a body-part enum and damage multiplier (head/torso/limbs), plus a reference to the owning combatant. The same colliders then become the death ragdoll (section 5), so there is only one collider set to maintain.
- **Layers (suggested):** `Player` (CharacterController), `NPCBody` (NPC CharacterController or NavMesh capsule), `Hurtbox` (bone colliders), `HeldItem` (pickups while held), `Viewmodel` (render-only, no colliders). In the matrix, disable `Hurtbox` vs `Player`/`NPCBody`/`Hurtbox` so bone colliders never push capsules or each other, and disable `HeldItem` vs `Player`. Attack queries use a mask containing only `Hurtbox` (plus world geometry if blocking by walls is wanted).
- **The Cull Completely advice conflicts with melee NPCs.** With culling on, an NPC attacking from behind the player (offscreen) may stop animating, which freezes its bone hurtboxes and any animation-event timing. Use Always Animate for NPCs currently in combat and Cull Completely for the rest.
- **Knockback without a Rigidbody:** keep a per-character `externalVelocity` vector in the movement component. A hit adds `direction * impulse`. Each frame, add it to the `CharacterController.Move` delta, then decay it toward zero (exponentially, or with a fixed deceleration). This matches the documented contract that the controller only moves via `Move`. For co-op, the hit resolver should write knockback into shared character state, and each character's movement applies it.
- Keep `detectCollisions` on for the player so NPC capsules block it. Bone hurtboxes are excluded by the matrix, not by `detectCollisions`.

### Gaps
- Whether the Layer Collision Matrix also filters queries was not stated on the page fetched. Assume it does not, and always pass an explicit `layerMask`. Verify in Editor.
- How NavMeshAgent-driven NPCs (AI Navigation 2.0) should take knockback (agent `Move`, temporarily disabling the agent, or off-mesh warp) was not researched.
- Whether per-bone kinematic Rigidbodies driven by the Animator have a measurable cost for a handful of NPCs was not measured or found.

---

## 3. First-person arms/viewmodel in URP 17: overlay camera vs Render Objects vs single camera

### Takeaway
Unity's own URP example ("FPS Demo") renders first-person objects on a **dedicated layer with Render Objects renderer features**, using a 40° FOV override and stencil tricks, instead of a second camera. Camera stacking also works but adds a camera. Its only documented performance advice is to trim culling masks. Because URP Compatibility Mode is removed in 6.3, only the built-in Render Objects feature or Render Graph-based custom passes are options.

### Cited Findings
- **[Official, Unity-Technologies GitHub]** The URP FPS Demo puts weapons on a "First Person Objects" layer that is excluded from the renderer's default layer mask, so the weapon is drawn only by three Render Objects features:
  - **GunOpaques**: event *Before Rendering Opaques*, writes stencil 1 (compare Always), FOV override 40°.
  - **GunTransparents**: *After Rendering Transparents*, stencil Equal 1 (transparent parts that overlap the gun's opaque parts), 40°.
  - **GunTransparentsOverlay**: *After Rendering Transparents*, depth test Always, stencil Equal 0 (transparent parts outside the gun), 40°.

  The wiki gives no Unity/URP version. — [UniversalRenderingExamples wiki: FPS Demo](https://github.com/Unity-Technologies/UniversalRenderingExamples/wiki/FPS-Demo)
- **[Official]** The Render Objects feature (URP in 6.3) has Event, Filters (Queue, Layer Mask), Pass Names, and Overrides: material, shader, depth (Write Depth, Depth Test), stencil, and Camera. The camera override has **Field of View** ("the Renderer Feature uses this Field of View instead of the value specified on the Camera"), **Position Offset** ("moves them by this offset"), and **Restore** ("restores the original Camera matrices after executing the render passes"). — [Render Objects Renderer Feature reference (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/renderer-features/renderer-feature-render-objects.html)
- **[Official]** Unity's own Render Objects walkthrough in 6.3 is a "draw character behind walls" effect, not a viewmodel. It notes that overlapping parts of complex objects can wrongly render as hidden because of relative depth. — [Render Objects example (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/renderer-features/how-to-custom-effect-render-objects.html)
- **[Official]** Camera stacking layers a Base Camera and one or more Overlay Cameras onto the same render target. You "must assign any GameObjects the Overlay Cameras need to render to a layer, then set the Culling Mask of each camera to match the layer". The only performance statement: "cameras in URP render all layers by default, so rendering is faster if you remove layers that contain unneeded GameObjects." — [Camera stacking (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/camera-stacking.html)
- **[Official]** URP Compatibility Mode "was deprecated in Unity 6.0 and is now removed" in 6.3. `RenderGraphSettings.enableRenderCompatibilityMode` is read-only false. A `URP_COMPATIBILITY_MODE` define can restore it for conversion only and stops working in 6.4. — [Upgrade to Unity 6.3](https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity63.html)
- **[Community]** In a URP thread about the official tutorial, users report the environment still drawing over the gun. Suggested fixes:
  - Render the gun after other opaques with depth testing disabled.
  - A ZWrite-mask setup that "works great when your environment is mostly solid geometry" but looks odd with foliage.
  - An overlay camera with a much lower FOV, which needs weapons modeled for that FOV.

  No Unity staff replied. — [Unity Discussions: URP FPS-object camera clipping into walls](https://discussions.unity.com/t/urp-fps-object-camera-clipping-into-walls-official-srp-tutorial/835118)
- **[Community]** The Render Objects approach "doesn't require setting up another camera" and weapons "can still receive lighting and shadows from the scene". Camera stacking also lets the item camera have its own FOV but "has many drawbacks". — [Medium: How to prevent weapon clipping in 4 quick steps with Unity URP](https://medium.com/@SniperED007/how-to-prevent-weapon-clipping-in-4-quick-steps-with-unity-urp-4bde9ddb95f6) (search summary)

### Inferences
- **Recommended:** follow the FPS Demo pattern. Use a `Viewmodel` layer removed from the URP Renderer's Opaque and Transparent Layer Masks, and a Render Objects feature for opaque viewmodel parts at *Before Rendering Opaques* (or *After Rendering Opaques* with depth test Always) with a Camera FOV override. Add the transparent features only if the arms or held items have transparent parts. This is one camera, one culling pass, and a feature Unity maintains, so there is no custom Render Graph code.
- Drawing the viewmodel first with its own projection writes near depth values, so walls then fail the depth test where the arms are. That is how it avoids clipping without shrinking the model. Verify in-Editor that SSAO, depth-of-field, and other effects reading the depth texture do not misbehave near the arms. The sources did not cover this.
- **Held pickups (book, rod):** when equipped, move the world item's renderer onto the `Viewmodel` layer and parent it to the viewmodel hand socket, or spawn a separate viewmodel prefab. Turn off its world colliders while held, and restore layer and colliders on drop. Keep the gameplay item (its `ItemDefinition`/weapon data) separate from its viewmodel presentation.
- **Co-op:** the viewmodel is purely local presentation. In release 2, other players see the third-person NPC-style body, so combat state must not live on viewmodel objects.
- Because the Render Objects FOV and position overrides change only rendering, gameplay code must never read viewmodel transforms as world positions (see section 1).

### Gaps
- No measured performance comparison between an overlay camera and Render Objects in URP 17 was found. Unity's docs give only the culling-mask advice.
- Whether the built-in Render Objects feature interacts correctly with SSAO, depth priming, and the depth texture for viewmodels in URP 17 Render Graph was not found in official docs.
- Unity's older FPS Sample (HDRP, 2018) and Starter Assets First Person were not reviewed. Both predate URP 17 and have no melee.

---

## 4. Animation: attack chains, event timing, override controllers, Playables, hit reactions, root motion, Humanoid vs Generic

### Takeaway
The Animator provides every piece needed for this scope:
- **StateMachineBehaviours** for state-entry and exit logic.
- **Animation events** for authored timing.
- **Override Controllers** for per-weapon clip sets over one state machine.
- **Additive layers with Avatar Masks** for hit reactions.

Playables suit runtime-assembled graphs but are more code. Humanoid is required only if clips must be retargeted between rigs.

### Cited Findings
- **[Official]** `StateMachineBehaviour` exposes `OnStateEnter`, `OnStateUpdate` ("each frame except first and last"), `OnStateExit`, `OnStateMove` (root motion), `OnStateIK`, and `OnStateMachineEnter/Exit`. A state machine "can have up to three different active states at the same time: the current state, the next state, and the interrupted state". Each Animator gets its own behaviour instances unless `SharedBetweenAnimatorsAttribute` is used. — [StateMachineBehaviour (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/StateMachineBehaviour.html)
- **[Official]** Animation events call a function "in any script attached to the GameObject" that accepts a single parameter: float, int, string, object reference, or `AnimationEvent`. — [Animation events (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/script-AnimationWindowEvent.html)
- **[Official]** `AnimationEvent` carries `animatorClipInfo`, `animatorStateInfo`, `isFiredByAnimator`, `time`, `intParameter`/`floatParameter`/`stringParameter`/`objectReferenceParameter`, and `messageOptions`. — [AnimationEvent (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AnimationEvent.html)
- **[Official]** An Animator Override Controller overrides "the animation clips in an Animator Controller while retaining the structure, parameters, and logic of its state machine". Use normalized exit times: "When in seconds, the exit time might be ignored if you specify an override clip shorter than the transition exit time." — [Animator Override Controller (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/AnimatorOverrideController.html)
- **[Official]** Animation layers support Override and **Additive** blending ("Add the animation on this layer on top of the animation from previous layers"), Avatar Masks to restrict body parts (e.g. upper-body only), and Synced layers. — [Animation layers (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/AnimationLayers.html)
- **[Official]** Performance:
  - "When the weight of the layer is zero, Unity skips the layer update."
  - "Use hashes instead of strings to query the Animator."
  - For Generic rigs, "using root motion is more expensive than not using it. If your animations don't use root motion, make sure that you have not specified a root bone."
  - For Humanoid, "use an Avatar Mask to remove IK Goals or finger animation if you don't need them."
  - Animating scale curves costs more than translation and rotation curves.

  — [Mecanim performance and optimization (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/MecanimPeformanceandOptimization.html)
- **[Official]** "Retargeting is only possible for humanoid models with a configured Avatar." — [Retargeting of Humanoid animations (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/Retargeting.html)
- **[Official]** Playables let you "Dynamically add or adjust playable nodes at runtime instead of creating a complex static graph that accounts for all possible outcomes" and play clips without an Animator Controller. — [Playables API (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/Playables.html)

### Inferences
- **Recommended timing source: data first, events as markers.** Store wind-up, active, and recovery durations in the weapon definition (section 7). The combat rules then run the same timeline with or without an Animator, which matters for automated play-mode tests and for a future co-op authority. Animation events (`AttackActiveStart`/`AttackActiveEnd` with an int attack ID) or a StateMachineBehaviour checking `normalizedTime` against windows can be used for NPCs where the clip is the ground truth. In either case the event should only call into the rules component, never apply damage directly.
- During a crossfade, events from the outgoing clip may still fire. Filtering on `AnimationEvent.animatorClipInfo.weight` (for example, ignore when < 0.5) is a common approach. It is not stated in the docs fetched, so verify.
- **Combos:** one Animator sub-state machine per attack set (`Light1 → Light2 → Light3`, `Heavy`), advanced by a trigger parameter only inside a cancel/combo window. Use int hash IDs. Put a StateMachineBehaviour on each attack state that reports `OnStateEnter`/`OnStateExit` to the combatant so rules and animation stay in sync.
- **Per-weapon sets:** one base "MeleeUpperBody" controller, with an `AnimatorOverrideController` per weapon class (fists, short blunt like a book, long blunt like the rod). The weapon definition references its override controller, and equipping assigns it to the viewmodel Animator (player) or the NPC Animator. Author transitions with normalized exit times (per the doc warning).
- **Hit reactions:** add an Additive layer with an upper-body Avatar Mask and short flinch clips, triggered by hit direction. Use a full-body Override state only for staggers and knockdowns. Zero-weight layers cost nothing.
- **Root motion:** player attacks should use scripted movement through the CharacterController (the camera must not be driven by clip motion). For NPCs, lunges could use root motion fed into their mover via `OnAnimatorMove` or `OnStateMove`. Scripted movement is simpler and deterministic for a small cell fight, and it keeps movement in rules code for co-op.
- **Humanoid vs Generic for the custom rig:** stay Generic unless the team wants to reuse Humanoid clips (store or mocap) on the custom rig. Retargeting needs a configured Humanoid Avatar. The player viewmodel arms are a separate small rig and should be Generic.
- Playables are only worth it if weapon move-sets become data-assembled at runtime. For this scope, Override Controllers are enough.

### Gaps
- Official statements on (a) animation-event behavior during transitions and (b) CPU cost of Humanoid vs Generic were not on the pages fetched.
- The minimum bone requirements for mapping the project's custom rig to a Humanoid Avatar were not checked.
- `Animator.keepAnimatorStateOnDisable` and `Animator.fireEvents`-style controls for hit-stop and ragdoll transitions were not researched.

---

## 5. Death: ragdolls in Unity 6, switching and blending, stability, alternatives

### Takeaway
The Ragdoll Wizard is still the documented way to generate per-bone Rigidbodies, colliders, and CharacterJoints in 6.3, and Unity publishes concrete stability rules. Switching to a ragdoll (disable the Animator, make the Rigidbodies non-kinematic, add an impulse) and blending back (pose capture plus get-up clips) are community techniques, not official ones.

### Cited Findings
- **[Official]** Ragdoll Wizard: GameObject > 3D Object > Ragdoll…, map limbs, and it creates Rigidbodies, colliders (boxes shown), and Character Joints. The joint axes are Twist, Swing 1, and Swing 2. — [Ragdoll Wizard (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/wizard-RagdollWizard.html)
- **[Official]** Joint and ragdoll stability rules:
  - Avoid small angular Y/Z limits; keep them at 5–15° minimum or zero to lock the axis.
  - Disable "Enable Preprocessing" to avoid erratic separation.
  - Enable projection (`CharacterJoint.enableProjection`).
  - Raise Default Solver Iterations to 10–20 if joints jitter, and Default Solver Velocity Iterations to 10–20 for bounces.
  - Keep mass ratios under 10x.
  - Keep Transform scale at 1.
  - Lower `Rigidbody.maxDepenetrationVelocity` for overlapping bodies.

  — [Joint and Ragdoll stability (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/RagdollStability.html)
- **[Official]** "Setting the linear velocity of a kinematic rigidbody is not allowed and will have no effect." Unity also warns "Do not set the linear velocity of an object every physics step" and prefers `AddForce`. — [Rigidbody.linearVelocity (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rigidbody-linearVelocity.html)
- **[Official]** A Unity 6 physics change: the torque from `Rigidbody.AddForceAtPosition` and `AddExplosionForce` with `ForceMode.VelocityChange` or `Acceleration` is computed differently. The 2022 LTS behavior can be replicated by multiplying by mass and using `Impulse`/`Force`. — [Upgrade to Unity 6 (6000.3 manual)](https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity6.html)
- **[Community]** Get-up blending, as commonly described:
  1. Detect front or back from the root bone's up vector, which needs at least "get up from back" and "get up from front" clips.
  2. Snap or teleport the animated skeleton root to the ragdoll.
  3. Blend each bone from its ragdoll pose to its animated pose over roughly 0.2–0.5 s while the get-up clip plays, with zero-blend-time transitions into the get-up states.

  — [MoCap Online: Ragdoll physics in games](https://mocaponline.com/blogs/mocap-news/ragdoll-physics-animation-guide) (search summary); [Unity Discussions: From animation to ragdoll and back](https://discussions.unity.com/t/from-animation-to-ragdoll-and-back/162233) (search summary); [nbzeman RagdollHelper.cs (GitHub)](https://github.com/nbzeman/Ragdoll/blob/master/Assets/Scripts/RagdollHelper.cs) (search summary)

### Inferences
- **Recommended for the first kill:** when health reaches 0:
  1. The rules component marks the inmate `Dead` (shared world state).
  2. The presentation reacts: disable the Animator, set all ragdoll Rigidbodies `isKinematic = false`, set the bone colliders' layer from `Hurtbox` to `Ragdoll` (collides with world, not with the player capsule), disable the NPC CharacterController or NavMesh agent, and apply `AddForceAtPosition` with `ForceMode.Impulse` at the hit bone in the hit direction.
  3. Order matters. Make bodies non-kinematic *before* applying velocity or force, because a kinematic body ignores velocity (per the doc).
- Apply Unity's stability rules at setup time. In a small cell, lower `maxDepenetrationVelocity` matters because bodies spawn close to walls and bunks.
- Blending back is not needed for death, and knockdown and get-up is not in the agreed first case. Keep the pose-capture approach noted for later gang fights.
- **Alternative:** a death animation with no ragdoll is cheaper and fully deterministic, which helps automated checks and later co-op. Ragdoll motion is not naturally identical across machines. A hybrid is common: a short death clip, then ragdoll on impact. Deciding which approach to use is a design question to put to the user.
- For automated verification, assert on rules state (`Dead`, Animator disabled, Rigidbodies non-kinematic), not on the final ragdoll pose.

### Gaps
- No official Unity 6 page on animation-to-ragdoll switching or blending back was found. The only fetched write-up (ModDB, Unspottable) returned HTTP 403.
- Behavior of `SkinnedMeshRenderer` bounds and culling while ragdolled was not verified.
- No source on ragdoll determinism, or on syncing ragdolls for co-op, was researched.

---

## 6. Input System 1.20: attack/block/heavy actions (tap vs hold) and input buffering

### Takeaway
Input System interactions (Tap, Hold, SlowTap, Press, MultiTap) give tap-vs-hold on one button. The documented pattern `"tap,slowTap"` evaluates interactions in binding order. Input buffering is not an engine feature. It is a small queue in combat code, and shipped games range from generous (God of War) to none on normal attacks (Sekiro).

### Cited Findings
- **[Official]** Interactions and their phases:
  - **Hold**: `duration`, `pressPoint`. *Started* on press, *performed* after the duration, *canceled* if released early.
  - **Tap**: *performed* if released within `duration`, *canceled* if held too long.
  - **SlowTap**: *performed* on release after holding at least the duration, *canceled* if released too early.
  - **MultiTap**: `tapTime`, `tapDelay`, `tapCount`.
  - **Press**: `pressPoint` and `behavior`.

  "The Input System checks the Interactions in the order they are present on the Binding." When an earlier interaction times out it cancels and the next takes over. The documented example is `WithInteractions("tap,slowTap")` on a fire action. (Page version 1.14. The project has 1.20.) — [Input System Interactions (1.14)](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/Interactions.html)
- **[Official]** Project-wide actions make one Action Asset globally available: `InputSystem.actions.FindAction("Move")`. The default asset comes "pre-configured with some default Actions such as 'Move', 'Jump', and more". — [Project-wide actions (1.14)](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/ProjectWideActions.html)
- **[Community, designer]** Jason de Heras (Respawn, Sony Santa Monica, Heavy Iron) says: "In God of War, there is GENEROUS input buffering", with block and evade cancels allowed "at ANYTIME during the attack (except during hit frames)". "In Sekiro, there is ZERO input buffering on normal attacks", and cancels are limited to "a small window at the start". — [The Art of Melee Combat Design](https://www.jasondeheras.com/gamedesign/2021/2/11/meleecombatdesign)
- **[Community, low confidence]** Suggested buffer sizes: 5–15 frames (~80–250 ms) in general, or attacks 6–8 frames and dodges 3–4. These come from an unverified blog and a forum post. — [salivity.github.io: Input buffering](https://salivity.github.io/game-development/article/input-buffering-for-better-combat-responsiveness) (search summary); [Moonjump forum thread](https://moonjump.com/forum/game-dev/input-buffering-in-action-games-how-precise-is-precise-enough-and-what-s-your-actual-window-dbe216) (search summary)

### Inferences
- **Suggested actions:**
  - `Attack` (LMB): `tap,slowTap` or `tap,hold`. Tap performs a light attack. Hold starts a heavy-attack wind-up on *started* or *performed* and releases on *canceled* or release.
  - `Block` (RMB): Button/Press with a hold-to-block state, reading *started* and *canceled*.
  - Add these to the project's existing action asset alongside the current "E" interaction.
- A separate `HeavyAttack` binding avoids tap-vs-hold latency: with `tap,hold`, a light attack cannot fire until release or until the tap duration passes. That delay is a real feel cost, which is why many first-person melee games put heavy on a separate button or use hold-to-charge on release. This is a design choice for the user.
- **Buffer:** the local input layer converts actions into timestamped intents (`LightAttack`, `HeavyStart`, `HeavyRelease`, `BlockStart`, `BlockEnd`). It enqueues them to the player's combatant as a single-slot "latest intent" with an expiry (e.g. ~150 ms as a starting value to tune). The combatant consumes the intent when its state allows (recovery ended or combo window open). This keeps input separate from rules for co-op, where a remote player's intents would arrive through the same entry point.
- Use `InputAction.WasPressedThisFrame()` or callbacks only in the local input adapter. Combat rules should never read `InputSystem` directly, which also makes play-mode tests drive intents without faking devices.

### Gaps
- The Input System 1.20 manual itself was not fetched; the 1.14 manual was used. Check the 1.20 changelog for interaction changes.
- No authoritative source for buffer window lengths was found. The numbers above are forum-level.

---

## 7. Data: ScriptableObject weapon definitions, including improvised weapons from pickups

### Takeaway
Unity documents ScriptableObjects as shared, read-only-at-runtime data containers. That fits weapon definitions exactly: damage, reach, wind-up/active/recovery timings, stamina cost, animation set, and viewmodel prefab. Improvised weapons are ordinary pickups that reference a weapon definition.

### Cited Findings
- **[Official]** "A common use for ScriptableObjects is as a container for shared data used by multiple objects at runtime, which can reduce a project's memory usage by avoiding copies of values." "In a standalone Player at runtime, you can only read saved data from the ScriptableObject assets." In the Editor, changes to SO assets made in Play mode persist, and script edits need `EditorUtility.SetDirty()`. — [ScriptableObject (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/class-ScriptableObject.html)
- **[Official]** Override Controllers swap clips per variant over one state machine (see section 4). — [Animator Override Controller (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/AnimatorOverrideController.html)

### Inferences
- **Suggested `MeleeWeaponDefinition : ScriptableObject` fields:**
  - `id`, `displayName`
  - `baseDamage`, `bodyPartMultipliers` (or a global table), `knockbackImpulse`
  - `reach`, `hitRadius`, `sweepPoints` (local offsets along the weapon)
  - `windUp`, `active`, `recovery` (seconds, per attack in an array for combos), `comboWindow`
  - `staminaCost` (light/heavy), `heavyChargeTime`
  - `canBlock`, `blockDamageReduction`, `durability` (optional)
  - `AnimatorOverrideController viewmodelAnimations`, `AnimatorOverrideController npcAnimations`
  - `GameObject viewmodelPrefab`, `hitSfx`, `hitVfx`
- **Fists:** a default `MeleeWeaponDefinition` asset used when nothing is equipped.
- **Improvised weapons:** add an optional `MeleeWeaponDefinition` reference to the existing pickup item's definition (or a `MeleeCapable` component on the pickup prefab). Equipping from the six-slot inventory or the hand then reads it. Book and rod are just two assets. This keeps a single item identity for inventory, pickup, and combat.
- **Runtime-mutable state goes in plain C# instances, never in the SO:** current durability, cooldowns, the per-swing hit set, and who is holding it. The doc confirms SO data is read-only in builds, and Editor play-mode edits persist, which is a common source of "values changed after testing" bugs.
- **Co-op:** SO definitions are shared static data and safe to reference by ID across clients. Per-player equipment state belongs in the player's combatant state.

### Gaps
- No official Unity sample of an SO-based melee weapon system was found. The Unity e-book on ScriptableObject architecture was not fetched this session.

---

## 8. Unity 6-specific API and workflow changes relevant to this system

### Takeaway
The relevant changes:
- Physics renames: `velocity → linearVelocity`, drag to `linearDamping`/`angularDamping`, `PhysicMaterial → PhysicsMaterial`.
- A torque behavior change for `AddForceAtPosition` and `AddExplosionForce`.
- URP Compatibility Mode removed in 6.3 (Render Graph only; custom passes use `RecordRenderGraph`).
- Input System project-wide actions.
- `AnimatorUpdateMode` values Normal/Fixed/UnscaledTime.

The 6.3 upgrade guide lists no physics, animation, or Input changes.

### Cited Findings
- **[Official]** `Rigidbody.linearVelocity`: "The linear velocity vector of the rigidbody." It is world-space, is not settable on kinematic bodies, and should not be set every physics step. — [Rigidbody.linearVelocity (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rigidbody-linearVelocity.html)
- **[Official]** The `linearDamping` API page exists for Rigidbody in 6000.x. — [Rigidbody.linearDamping (6000.6)](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/Rigidbody-linearDamping.html) (search summary). The rename set (`velocity→linearVelocity`, `drag/angularDrag→linearDamping/angularDamping`, `PhysicMaterial→PhysicsMaterial`) is described in secondary sources. — [UhiyamaLab Rigidbody guide](https://uhiyama-lab.com/en/notes/unity/unity-rigidbody-guide/) (search summary)
- **[Official]** Unity 6 changed torque for `AddForceAtPosition`/`AddExplosionForce` with `VelocityChange`/`Acceleration`. — [Upgrade to Unity 6 (6000.3 manual)](https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity6.html)
- **[Official]** Compatibility Mode is removed in 6.3, with a conversion-only define that ends in 6.4. — [Upgrade to Unity 6.3](https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity63.html). The 6.3 upgrade guide lists no physics, animation, or Input System changes. — same source.
- **[Official]** Render Graph passes: implement `RecordRenderGraph`, declare a `PassData` class, set inputs and outputs via `IRasterRenderGraphBuilder`, and draw in a function set with `SetRenderFunc`. "In the RecordRenderGraph method you declare render pass inputs and outputs, but do not add commands to command buffers." — [Write a render pass using the render graph system (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/render-graph-write-render-pass.html)
- **[Official]** `AnimatorUpdateMode` in 6.3 lists **Normal**, **Fixed** ("updates in the FixedUpdate loop"), and **UnscaledTime** ("updates independently of Time.timeScale"). — [AnimatorUpdateMode (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AnimatorUpdateMode.html)
- **[Official]** `Physics.autoSyncTransforms` is documented in 6.0 but absent from the 6.3 Scripting API (see section 1). — [6000.0 page](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics-autoSyncTransforms.html); [6000.3 Physics class](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Physics.html)
- **[Official]** Project-wide actions via `InputSystem.actions`. — [Project-wide actions](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/ProjectWideActions.html)

### Inferences
- Implementing agents trained on pre-Unity-6 code will often write `rb.velocity`, `rb.drag`, `PhysicMaterial`, `ScriptableRenderPass.Execute`, or `AnimatorUpdateMode.AnimatePhysics`. Add a note to the implementation brief to use the Unity 6 names and a Render Graph-only URP.
- Prefer built-in Render Objects over custom passes for the viewmodel, so there is no Render Graph code at all for this milestone.

### Gaps
- An official list of the Unity 6 physics renames on one page was not retrieved. The 6000.3 "Upgrade to Unity 6" page fetched did not show them, so they were confirmed only via API pages and secondary sources.
- Whether `AnimatorUpdateMode.Fixed` is a rename of `AnimatePhysics` was not confirmed.
- The status of `Physics.autoSyncTransforms` in 6.3 is unconfirmed.

---

## 9. Game feel: hit-stop, camera shake, hit flashes in URP

### Takeaway
Global `Time.timeScale` hit-stop is easy but has documented side effects: FixedUpdate stops at 0, changes apply next frame, and physics timing shifts. It also cannot work in co-op without freezing everyone. Per-character hit-stop via `Animator.speed` or a local time scale in the combat code is the safer pattern. Cinemachine 3's External Impulse Listener can shake a plain camera. For hit flashes in URP, avoid `MaterialPropertyBlock`, which breaks SRP Batcher compatibility, and use a material instance or a shader property on a shared-variant material.

### Cited Findings
- **[Official]** `Time.timeScale`:
  - "FixedUpdate functions and suspended Coroutines with WaitForSeconds are not called when timeScale is set to zero."
  - Changes take effect on the following frame.
  - Multiply `fixedDeltaTime` by the new scale to keep physics consistent with real time.

  — [Time.timeScale (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Time-timeScale.html)
- **[Official]** `AnimatorUpdateMode.UnscaledTime`: "Animator updates independently of Time.timeScale." — [AnimatorUpdateMode (6000.3)](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AnimatorUpdateMode.html). `Animator.speed` is the Animator's playback speed, where 1 is normal. — [Animator.speed](https://docs.unity3d.com/ScriptReference/Animator-speed.html) (search summary)
- **[Community]** Hit-stop is commonly done by setting `Time.timeScale` to 0 briefly. There are threads on per-object hit-stop. — [Unity Discussions: HitStop for one single GameObject](https://discussions.unity.com/t/hitstop-for-one-single-gameobject/1572871) (search summary)
- **[Official]** Cinemachine Impulse: Impulse Sources emit a signal that "propagates outwards, much like a sound wave", and Listeners react by shaking. — [Cinemachine Impulse (3.1)](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineImpulse.html). `CinemachineExternalImpulseListener` "can be attached to any object to make it shake in response to Impulses", including the main Camera. — [CinemachineExternalImpulseListener API (3.1)](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/api/Unity.Cinemachine.CinemachineExternalImpulseListener.html) (search summary)
- **[Official]** SRP Batcher: objects "mustn't use MaterialPropertyBlocks" to stay compatible. — [SRP Batcher materials compatibility (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/SRPBatcher-Materials.html). The Batcher works on "materials that use the same shader variant", and "You can still use as many different materials with the same shader as you want." — [SRP Batcher (6000.3)](https://docs.unity3d.com/6000.3/Documentation/Manual/SRPBatcher.html)

### Inferences
- **Hit-stop (recommended):** a short freeze (starting value ~50–100 ms, to tune) applied to the **attacker's and victim's** Animators (`speed = 0`, restored after the duration using unscaled time) and to the combat timeline of those two combatants only. Do not change `Time.timeScale`. This avoids the FixedUpdate and next-frame side effects, leaves the CharacterController and other NPCs unaffected, and works unchanged in co-op. If global hit-stop is still wanted in single-player for a kill, isolate it in a local presentation service, restore `fixedDeltaTime`, and exclude UI via unscaled time.
- **Camera shake:** Cinemachine is not in the installed package list. Adding it for shake alone (External Impulse Listener on a pivot above the FP camera) is an option. A custom shake is also cheap: an additive local position and rotation offset on a child pivot of the camera, driven by decaying noise. Either must stay a local presentation effect triggered by combat events, never written into the gameplay camera transform that the rules read for aim or queries.
- **Hit flash:** use a per-renderer material instance (`renderer.material` once, cached) with a `_FlashAmount` property in the NPC's URP shader or Shader Graph. Instances of the same shader variant still batch under the SRP Batcher. Avoid `MaterialPropertyBlock`.
- A damage vignette for the player could use URP's Full Screen Pass Renderer Feature or a Volume override. That keeps it within built-in features and avoids custom Render Graph code.
- Drive all feel effects from a combat event stream (`OnHitResolved(attacker, victim, part, damage)`). The rules raise it, and local presentation listens. This is the rules/presentation split that co-op needs.

### Gaps
- No official Unity page or GDC talk on hit-stop or melee game feel was retrieved. Duration values above are starting points, not sourced.
- Cinemachine 3's `CinemachineImpulseSource.GenerateImpulse` scripting details were not fetched.
- The URP Full Screen Pass Renderer Feature docs for 6.3 were not fetched.
