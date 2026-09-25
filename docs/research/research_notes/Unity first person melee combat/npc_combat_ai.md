# NPC melee combat AI for a Unity 6 first-person prison game

Scope: AI architectures, melee behaviors, multi-attacker management, close-combat navigation, perception and escalation, testability, and co-op considerations. This research note does not record decisions. Every recommendation below is **proposed**; none is agreed.

Source dating: Unity docs are current. Pages fetched September 2026: AI Navigation 2.0.15 (the project has 2.0.14), Behavior 1.0.16, Netcode for GameObjects 2.10, and the Unity 6.3 manual. Game AI Pro 1 is from 2013, Game AI Pro 2 from 2015, Game AI Pro 3 from 2017, the Vossen melee article from 2015, and the Doom article from 2018. The shipped-game techniques in these older sources are engine-independent. None of them is Unity-specific.

## 1. Architecture options (FSM, HFSM, behavior trees incl. Unity Behavior, utility AI, GOAP) and migration costs

### Takeaway
For a solo developer with a few fighter types, a code-first finite state machine (FSM) is the best-supported starting point, moving to a hierarchical FSM (HFSM: a state machine whose states can contain their own state machines) as behaviors are added. Doom 2016 shipped its combat AI on HFSMs. Unity's Behavior package is released and still updated, but since May 2026 it gets maintenance and stability fixes only, with no new features. That is a real risk for a multi-year project. Utility AI and GOAP trade predictability for emergent behavior, which works against deterministic automated tests.

### Cited Findings
**FSM, HFSM, BT, utility, GOAP and HTN: Game AI Pro chapter 4, "Behavior Selection Algorithms: An Overview" (Dawe, Gargolinski, Dicken, Humphreys, Mark; 2013)**
- FSMs are "the most common behavioral modeling algorithm used in game AI programming today." They are "conceptually simple and quick to code" and "intuitive and easy to visualize." The chapter's example guard FSM has Patrol, Investigate (on hearing a noise), Attack (on seeing an enemy) and Flee (if health drops too low). — [Game AI Pro ch.4](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter04_Behavior_Selection_Algorithms.pdf)
- The FSM API shape it uses: each state has `onEnter/onUpdate/onExit` and a list of transitions. Each transition has `isValid()`, `getNextState()` and `onTransition()`. Each tick, the FSM checks the active state's transitions and either switches state or calls `onUpdate`. — [Game AI Pro ch.4](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter04_Behavior_Selection_Algorithms.pdf)
- FSM weakness: once an FSM has "10, 20, or 30 existing states," adding a new state "can be extremely difficult and error-prone." Reusing a behavior such as an interruption forces duplicate states, "an explosion of states." — [Game AI Pro ch.4](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter04_Behavior_Selection_Algorithms.pdf)
- An HFSM nests state machines and uses a "history state," so an interruption such as a conversation returns to the sub-state that was active before it. This removes the duplicated states. The limit: if an FSM has "transition overload" (every state linked to every other) and an HFSM isn't helping, "other algorithms may be a better choice." — [Game AI Pro ch.4](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter04_Behavior_Selection_Algorithms.pdf)
- Behavior trees (BT): behaviors "can (and should) be written to be completely unaware of each other," so adding or removing one does not affect the rest. This fixes the FSM problem where "every state must know the transition criteria for every other state." Costs: the running time is "generally greater than that of a finite-state machine." Behaviors are stateless, so memory-dependent behavior needs extra care. The chapter's example is a fleeing character that stops fleeing once safe, is pulled back into combat by a higher-priority behavior, and loops between the two. — [Game AI Pro ch.4](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter04_Behavior_Selection_Algorithms.pdf)
- Utility systems score each candidate action from continuous inputs such as distance, health or ammo. The highest score is picked, or the scores seed a "weighted random selection." Strengths: this approach "can recover better from being disrupted" and produces "dynamic emergent behavior." Weaknesses: utility systems "are often somewhat unpredictable," and scripted moments need overrides. They "can be somewhat challenging to tune"; balancing them "is often more art than science." — [Game AI Pro ch.4](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter04_Behavior_Selection_Algorithms.pdf)
- GOAP (goal-oriented action planning) was pioneered in F.E.A.R. (2005) and later used in Just Cause 2 and Deus Ex: Human Revolution. It removes "a lot of the authorial and directorial control"; characters "can become loose cannons." HTN (hierarchical task network) planners were used in Killzone 2 and Transformers: Fall of Cybertron. Designers must author the whole task network. — [Game AI Pro ch.4](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter04_Behavior_Selection_Algorithms.pdf)

**Shipped-game example**
- Doom 2016 (id Software) used hierarchical FSMs, not behavior trees, authored in a custom editor that cross-referenced C++ headers to verify integrity. It had 16 demon archetypes, each with its own behaviors. — [Game Developer, Tommy Thompson, "Cyber Demons: The AI of DOOM (2016)", Aug 2018](https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-)

**Unity Behavior package: status and maturity**
- The Unity 6.3 manual lists Behavior 1.0.16 as "Released" and describes it as "a graph-based tool to create and run behavior trees," with real-time debugging and an event-driven design. — [Unity 6.3 Manual: Behavior](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.behavior.html)
- Timeline: 1.0.0 was released 2024-09-17. Later releases were 1.0.13 (2025-10-20), 1.0.14 (2025-12-18), 1.0.15 (2026-02-02) and 1.0.16 (2026-05-26). 1.0.16 migrated from InstanceID to EntityId "to support engine version 6.5 and newer." — [Behavior CHANGELOG](https://docs.unity3d.com/Packages/com.unity.behavior@1.0/changelog/CHANGELOG.html)
- On 2025-02-10 the Behavior team lead wrote: "our team is included in the latest round of layoffs and we will not be able to support you any longer." Open-sourcing was requested with "no guarantee." — [Unity Discussions: An update on Behavior](https://discussions.unity.com/t/an-update-on-behavior/1598451)
- On 2026-05-01 a Unity Senior Engineering Manager wrote: "our approach to Behavior will shift toward maintenance and stability rather than new feature development," while committing to "address the most impactful issues." Two named staff would have "much lower interaction" on the forums. One reply worried the package may "become completely stale." — [Unity Discussions: Update on Behavior Package Support and Team Presence](https://discussions.unity.com/t/update-on-behavior-package-support-and-team-presence/1718517)
- Debug features: node breakpoints "can now pause playmode without attaching the IDE debugger" (1.0.15); an "Allow Disabled Agent Debugging" setting (1.0.10); JSON serialization of graphs, blackboard and node values (1.0.0-pre.1). — [Behavior CHANGELOG](https://docs.unity3d.com/Packages/com.unity.behavior@1.0/changelog/CHANGELOG.html)
- A forum thread titled "Are other people going to continue using the behavior package or switch to a third-party asset?" exists, showing community doubt after the layoffs. I saw only the title, not the contents. — [Unity Discussions thread](https://discussions.unity.com/t/are-other-people-going-to-continue-using-the-behavior-package-or-switch-to-a-third-party-asset/1599273)

### Inferences
- **Proposed starting architecture:** a plain C# FSM. It could use the Game AI Pro state and transition shape, or a simpler switch on an enum.
  - The first opponent needs about 8 to 12 states: Idle, Approach, Circle/Strafe, WindUp, Attack, Recover, Block, Stagger, Flee/Surrender, Downed, Dead. That is below the 10 to 30 states where the chapter says FSMs get painful.
  - Once reusable interruptions appear (Stagger or Surrender from any combat state, a guard interrupting any activity), group the states into super-states: an HFSM with a history state.
  - Coding agents can read, write and diff C#. Graph assets are harder for them to author and review. This is a practical reason to prefer code-first AI in this project.
- **Migration cost if starting simple:** the expensive part to migrate is the transition logic. Actions are cheap to move if written as self-contained classes that read a shared context object: MoveTo, FaceTarget, PlayAttack, Block and similar. Those classes can later become behavior tree leaf nodes or utility-scored options almost unchanged. Keep the decision logic (which state or action next) separate from execution (animation and movement) from day one.
- **Unity Behavior verdict:** usable, but a risk for a multi-year project. It is released and was still being patched in May 2026, but it is maintenance-only with fewer staff. Graph assets are harder for coding agents to edit, and the changelog mentions no determinism or seeding support. Recommend not adopting it now. Reconsider it only if an HFSM runs into transition overload, and compare it against writing a small C# behavior tree.
- Utility AI fits narrow sub-decisions later: which target to pick in a brawl, whether to join a fight, or which side to take. It fits poorly as the top-level brain for the first fight, because it is hard to tune and less predictable. Seeded weighted random keeps it testable.
- GOAP and HTN are overkill for a small number of fighter types, and they reduce designer control. Not recommended for release 1.

### Gaps
- No primary source found on which third-party Unity behavior tree assets are maintained as of 2026, or on how well they work in Unity 6.3.
- The Behavior package's minimum supported editor version and any 6.3-specific known issues were not stated in the changelog pages read.

## 2. Melee-specific techniques: telegraphing, spacing and circling, blocks, armed vs unarmed, stagger and hit reactions, retreat, surrender, knockout, low health

### Takeaway
Shipped melee games give attacks an explicit wind-up tell (a preparation pose plus visual and audio cues), space enemy attacks about 2 to 3 seconds apart, keep non-attackers moving (circling, taunting), and use graded hit reactions that interrupt the AI. The first-person melee game Condemned is described as having enemies that feint to bait a block and that flee and hide. No primary source was found for AI reactions to the player being armed vs unarmed, or for surrender behavior.

### Cited Findings
**Tells, attack pacing and spacing: Bart Vossen, Game Developer, 2015**
- Tells: in DmC (Rahni Tucker), an attacking enemy "gets into his preparation pose, then we would usually [have] a glint on the weapon and a sound effect." Batman: Arkham City uses obvious icon flashes above attackers; Ninja Gaiden 2 relies on subtle movement cues. — [Game Developer, Bart Vossen, "Enemy design and enemy AI for melee combat systems", May 2015](https://www.gamedeveloper.com/design/enemy-design-and-enemy-ai-for-melee-combat-systems)
- Pacing: most games analyzed space enemy attacks 2 to 3 seconds apart. God of War 3 (Alex Sulman) regulated attacks "to prevent that chaos of just everyone attacking you at once." Enemies typically avoid attacking while the player is in a stagger animation from a previous hit. — [Vossen 2015](https://www.gamedeveloper.com/design/enemy-design-and-enemy-ai-for-melee-combat-systems)
- Non-attackers circle the player, taunt, cheer allies, or move between groups, so they look engaged rather than waiting. — [Vossen 2015](https://www.gamedeveloper.com/design/enemy-design-and-enemy-ai-for-melee-combat-systems)
- Spacing: DmC keeps an exclusion zone around the player: "when they are that close they crowd you and you can't see what you are doing." Enemies are split into near and far groups. Only the near group attacks; the far group advances gradually. — [Vossen 2015](https://www.gamedeveloper.com/design/enemy-design-and-enemy-ai-for-melee-combat-systems)
- Aztez allows simultaneous attackers but never lets two enemies use attacks from the same category at once, to keep them readable. — [Vossen 2015](https://www.gamedeveloper.com/design/enemy-design-and-enemy-ai-for-melee-combat-systems)
- Enemy roles: Emphasizers, Enforcers, Smashers and Challengers. — [Vossen 2015](https://www.gamedeveloper.com/design/enemy-design-and-enemy-ai-for-melee-combat-systems)

**Hit reactions and stagger: Doom 2016**
- Pain reactions are graded, from "mild twitching" (affects aim) up to "falters and push back" (interrupts the current behavior). Specific weapons trigger specific reactions. A stagger state opens the glory-kill finisher window. During a glory kill the player is invincible and the AI does not start new attacks. Difficulty changes token counts. — [Game Developer, Thompson 2018, Doom AI](https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-)

**Attack variety: Kingdoms of Amalur**
- Kingdoms of Amalur added cooldowns "on individual, creature-wide, and global bases." This prevents too many of the same attack within a time window, even when difficulty raises the attack capacity. — [Game AI Pro ch.28, Dawe, "Beyond the Kung-Fu Circle"](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter28_Beyond_the_Kung-Fu_Circle_A_Flexible_System_for_Managing_NPC_Attacks.pdf)

**First-person melee precedent: Condemned**
- Condemned: Criminal Origins, a first-person melee game: "enemies are able to flee and hide effectively… and can also effectively feint to trick the player into blocking at an inopportune time." — [Wikipedia: Condemned: Criminal Origins](https://en.wikipedia.org/wiki/Condemned:_Criminal_Origins). This is secondary, and I found no developer postmortem.

**Improvised weapons**
- Batman: Arkham Asylum enemies charge and attack with "whatever weapon it has equipped, whether bare hands or a picked-up object like a metal pipe." — [Game Developer, "Artificial Intelligence in Game Design"](https://www.gamedeveloper.com/design/artificial-intelligence-in-game-design). This is a search snippet only; I did not read the full article.

**Low health and flee**
- The textbook FSM uses Attack to Flee on "Health Low". The behavior tree flee-loop caveat above applies: a character that stops fleeing once safe can be pulled straight back into combat. — [Game AI Pro ch.4](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter04_Behavior_Selection_Algorithms.pdf)

### Inferences
- **Proposed per-attack data:** wind-up duration, active window, recovery duration, reach, damage, stagger power, attack weight and cooldown. The FSM steps through WindUp, Attack and Recover using this data. Automated tests can then check that the tell time is at least X ms before damage.
- **Reacting to player blocks:** keep a per-NPC count of recently blocked hits. After N blocks, raise the chance of a feint (a wind-up that cancels) or a grab or shove. This matches the Condemned description, but it is a design proposal, not a documented implementation.
- **Armed vs unarmed player:** treat the player's held item as an input to the decision. A metal rod could increase the NPC's preferred spacing, bait-and-punish behavior, or flee or surrender chance. Kingdoms of Amalur's weights and Splinter Cell's "same event, different meaning by context" (section 5) support encoding weapon threat as data rather than as new states.
- **Stagger:** use a poise or stagger meter that fills from hits scaled by weapon. Stagger is an interrupt super-state, usable from any combat state, with graded levels like Doom's. Knockout or Downed is a terminal combat state. Surrender or Flee is triggered by low health or morale, with hysteresis (a gap between the enter and exit thresholds) so the NPC does not oscillate between fleeing and fighting.

### Gaps
- No primary source found on: AI reacting to armed vs unarmed players; surrender or knockout AI behavior; and Sleeping Dogs' melee AI specifics.
- No developer source was found for Condemned's AI; the Wikipedia description is unsourced there.

## 3. Managing multiple attackers: token and kung-fu circle systems, how many engage, readability in first person

### Takeaway
The best-documented shipped systems are Kingdoms of Amalur's "Belgian AI" and Doom 2016's tokens. Amalur used a central stage manager with slot and weight budgets per target plus approach and attack circles. Doom used per-attack-type tokens that demons request, release and steal. Both let difficulty scale by changing a few budget numbers. God of War 3 designers avoided off-screen attacks, which matters more in first person, where the field of view is narrow.

### Cited Findings
**The kung-fu circle**
- Letting opponents attack "one at a time" is the "Kung-Fu Circle." It is simple and suits single-opponent focus, but may be "too strict" for fast combat. — [Game AI Pro ch.28, Dawe](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter28_Beyond_the_Kung-Fu_Circle_A_Flexible_System_for_Managing_NPC_Attacks.pdf)

**Kingdoms of Amalur: Reckoning ("Belgian AI"), Game AI Pro ch.28, Dawe**
- **The grid:**
  - Every creature carries a grid, though in practice the player's grid matters most. The grid is world-aligned, centered on the character, and has 8 slots.
  - It stores **grid capacity**, which limits how many creatures attack, and **attack capacity**, which limits the number and type of attacks.
  - Each creature has a **grid weight** and each attack has an **attack weight**.
  - Worked example: player grid capacity 12. A soldier (weight 4) leaves 8; a troll (weight 8) leaves 0. A second soldier must "wait outside the grid area."
  - With attack capacity 10: the troll picks a charge (weight 6), leaving 4. The soldier cannot use his lunge (5) and must use his sword swing (3).
  - [Game AI Pro ch.28](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter28_Beyond_the_Kung-Fu_Circle_A_Flexible_System_for_Managing_NPC_Attacks.pdf)
- **Stage manager:** all spatial reasoning was taken out of the individual creatures and centralized in a "stage manager." Creatures request a spot and wait for assignment. They "never remember their grid slot assignments" and check with the stage manager every frame. — [Game AI Pro ch.28](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter28_Beyond_the_Kung-Fu_Circle_A_Flexible_System_for_Managing_NPC_Attacks.pdf)
- **Circles:**
  - The "attack" circle radius is set by the minimum and maximum melee distances. The "approach" circle lies outside it.
  - Permitted creatures stand in the approach circle and step into the attack circle only to attack. Unassigned creatures wait outside, which "helps the player to determine which creatures are immediate melee threats."
  - Creatures inside the outer circle without permission "had to leave the circle as quickly as possible."
  - [Game AI Pro ch.28](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter28_Beyond_the_Kung-Fu_Circle_A_Flexible_System_for_Managing_NPC_Attacks.pdf)
- **Rotation and reassignment:**
  - Creatures give up their slot "immediately after launching an attack," so attackers rotate and "no single creature could monopolize attack opportunities."
  - The manager may "steal" or reassign slots, for example when the player moves toward a waiting creature.
  - A creature mid-attack "locks" its slot.
  - Pseudocode for closest-slot assignment is given.
  - [Game AI Pro ch.28](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter28_Beyond_the_Kung-Fu_Circle_A_Flexible_System_for_Managing_NPC_Attacks.pdf)
- **Difficulty:** raising grid and attack capacity lets more creatures surround the player and use stronger attacks, without re-tuning each creature. — [Game AI Pro ch.28](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter28_Beyond_the_Kung-Fu_Circle_A_Flexible_System_for_Managing_NPC_Attacks.pdf)

**Doom 2016: tokens**
- Each attack type has a limited number of tokens. Demons request a token to attack and release it afterwards. Demons can "steal tokens from one another if they feel they're better suited to use them," so frontline enemies stay aggressive. Difficulty changes token counts. — [Game Developer, Thompson 2018](https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-). The original talk is "Embracing Push Forward Combat in DOOM" (Kurt Loudy and Jake Campbell, GDC 2018). — [GDC Vault](https://www.gdcvault.com/play/1024940/Embracing-Push-Forward-Combat-in)
- A third-party open-source Unreal plugin re-implements the Doom-style token system. It frames the purpose as rationing the player's limited attention. — [GitHub: Lim-Young/UnrealAITokenSystem](https://github.com/Lim-Young/UnrealAITokenSystem). This is a community implementation, not id Software's.

**God of War**
- God of War 3: "We always try to make it that enemies almost never attack you off screen, which is always really unsatisfying because it's really hard to read." — [Vossen 2015](https://www.gamedeveloper.com/design/enemy-design-and-enemy-ai-for-melee-combat-systems)
- God of War 2018 moved to a close over-the-shoulder camera. The game shows colored arrows at the screen edges: white for an enemy behind you, red for an imminent melee attack from that direction. Atreus also calls out threats. — [Digital Trends combat guide](https://www.digitaltrends.com/gaming/god-of-war-combat-guide/). This is a player guide, not a developer source. The developer talk is "Evolving Combat in 'God of War' for a New Perspective" (Mihir Sheth, GDC 2019). — [GDC Vault](https://www.gdcvault.com/play/1026423/Evolving-Combat-in-God-of). The slides PDF returned 403, so the talk contents are unverified.

**Assassin's Creed**
- Early games had enemies take turns. In Brotherhood the AI "was made more aggressive and enemies can attack simultaneously," because counter-focused play made fights slow. — [Wikipedia: Assassin's Creed: Brotherhood](https://en.wikipedia.org/wiki/Assassin%27s_Creed:_Brotherhood). This comes from a search snippet and is secondary.

### Inferences
- **Proposed design for multiple attackers:** a per-target attack coordinator, modeled on Amalur's stage manager.
  - Every fighter, whether player or NPC, owns a small budget: grid capacity plus attack capacity. NPCs request, release and lock through it.
  - Because the budget is per target, the same code handles inmate-vs-inmate brawls and, later, several co-op players.
  - In the first single-opponent fight the coordinator trivially grants the only request, so it can be added later without changing the NPC's states. The NPC only needs a "may I attack?" check from the start.
- **First-person attacker count:** first person gives a narrower view than third-person games. Following God of War 3's rule, a reasonable proposal is one active attacker at a time, plus at most one more in wind-up, and only from within the player's view cone.
  - Waiting fighters circle at the approach radius or taunt.
  - Enforce "no damage from outside the camera view" as a rule in the coordinator, using the player's facing on the game side rather than the render camera.
  - For co-op, "outside the view" should be computed from each player's facing, not a local camera. See section 7.
- Amalur-style cooldowns (per NPC, per archetype, global) prevent a gang all using the same heavy move.

### Gaps
- No primary developer source was found for Sleeping Dogs or Assassin's Creed attacker management, nor for Batman Arkham's exact attacker limits.
  - A search snippet claimed Arkham Asylum enemies "only ever attack one at a time." I could not find that text in the Game Developer articles I fetched ([Game Design Review](https://www.gamedeveloper.com/design/game-design-review-batman-arkham-asylum), [Arkham Design Analysis](https://www.gamedeveloper.com/design/batman-arkham-design-analysis-part-1-)), so it is unverified.
  - That claim may also conflict with Arkham City showing warning icons over several attackers ([Vossen 2015](https://www.gamedeveloper.com/design/enemy-design-and-enemy-ai-for-melee-combat-systems)).
- The God of War 2018 GDC talk content on enemy coordination could not be accessed (403).
- No source gave a researched number of simultaneous melee attackers specifically for first-person games.

## 4. Navigation for close combat: NavMeshAgent vs root motion, jitter at melee range, local avoidance, tight spaces

### Takeaway
Unity documents two valid couplings: the animation follows the agent (velocity drives the animator; simple, some foot sliding), or the agent follows the animation (`updatePosition`/`updateRotation` off, positions reconciled in `OnAnimatorMove`). Unity's built-in local avoidance (RVO, reciprocal velocity obstacles) is local and short-horizon. At melee range, the fighter should stop path-following and switch to scripted facing and spacing, and the slot and circle system should decide the positions.

### Cited Findings
**Coupling animation and navigation (AI Navigation 2.0.15 docs)**
- NavMeshAgent and root motion "create race conditions"; the flow of information must go one way. — [AI Navigation 2.0: Use NavMesh Agent with Other Components](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/MixingComponents.html)
- Option A: the animation follows the agent. `NavMeshAgent.velocity` is fed to the animator; this is simple but produces foot sliding. — [AI Navigation 2.0: MixingComponents](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/MixingComponents.html)
- Option B: the agent follows the animation. Disable `updatePosition` and `updateRotation`, then drive the animation from the difference between `agent.nextPosition` and the animator root. — [AI Navigation 2.0: MixingComponents](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/MixingComponents.html)
- **Unity's sample:**
  - Set `agent.updatePosition = false`.
  - Convert the agent's velocity to local space into `velx`/`vely` for a 2D Simple Directional blend tree, with low-pass smoothing and a `move` bool.
  - In `OnAnimatorMove()`, either set `transform.position = agent.nextPosition` (the agent wins) or use root motion.
  - Correct drift when `worldDeltaPosition.magnitude > agent.radius` by pulling the agent back toward the animation root.
  - Make the head look at the target through `OnAnimatorIK`.
  - [AI Navigation 2.0: Couple Animation and Navigation](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/CouplingAnimationAndNavigation.html)

**Physics and obstacles**
- Do not enable an agent and an obstacle on the same object: "Enabling both will make the agent trying to avoid itself." For a dead or stationary character, disable the agent and enable a NavMeshObstacle. A Rigidbody used alongside an agent must be kinematic. For the player, a low avoidance priority number (high priority) lets them brush through crowds. — [AI Navigation 2.0: MixingComponents](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/MixingComponents.html)
- Local avoidance uses RVO against nearby agents and NavMesh edges. "Since the algorithm is local, it only considers the next immediate collisions, and cannot steer around traps or handle cases where an obstacle blocks a path." For stationary blockers, use NavMesh carving. — [AI Navigation 2.0: Inner Workings](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavInnerWorkings.html)

**Positions and tight spaces in shipped games**
- Kingdoms of Amalur: the attack circle radius equals the melee min/max range. The stage manager assigns world-space slot positions and reassigns them to reduce "total travel time." — [Game AI Pro ch.28](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter28_Beyond_the_Kung-Fu_Circle_A_Flexible_System_for_Managing_NPC_Attacks.pdf)
- Splinter Cell (Conviction/Blacklist): with 12 active NPCs in "very tight areas," NPCs often had no useful position. Ubisoft built TEAS (Tactical Environment Awareness System), which splits the NavMesh into areas (rooms) connected by choke nodes (doors, windows). Each area had metadata including "the number of NPCs that should enter that area for search or combat." — [Game AI Pro 2 ch.28, Walsh](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter28_Modeling_Perception_and_Awareness_in_Tom_Clancy's_Splinter_Cell_Blacklist.pdf)
- DmC: a no-go zone right around the player to prevent crowding the view. — [Vossen 2015](https://www.gamedeveloper.com/design/enemy-design-and-enemy-ai-for-melee-combat-systems)

### Inferences
- **Proposed approach for the project's single stylized rig and scripted motion:** Option A (animation follows the agent) with in-place clips. It is the simplest and most robust, and foot sliding is less visible on a stylized rig. Move to Option B only if attack lunges need root motion. A common hybrid is agent-driven locomotion plus short root-motion attack moves, with the agent's `nextPosition` synced afterwards.
- **Avoiding jitter at melee range:**
  - Once inside the attack circle, stop the agent (`isStopped` or `ResetPath`) and rotate manually toward the target.
  - Use separate enter and exit distances (for example, enter the attack range at 1.6 m and leave it at 2.0 m) so the NPC does not flip between Approach and Attack.
  - Give each NPC a slot position rather than the target's position as its destination, so agents do not converge on one point and push each other.
  - Circle and strafe by moving to slot positions offset around the target, not by continuously re-pathing to the target.
- **Cells and corridors:**
  - A cell door is a choke point, so cap the number of fighters allowed in a cell, as with TEAS area metadata. Excess NPCs wait at the door or in the corridor.
  - Corridors are too narrow for circling, so reduce the slot count there.
  - Because RVO cannot solve blockages, bodies and knocked-out NPCs should switch to a carving NavMeshObstacle.

### Gaps
- No official Unity source specifically addresses NavMeshAgent jitter at melee range; the fixes above are inferences.
- No Unity 6 source compared the AI Navigation agent with custom steering for crowds in tight spaces.

## 5. Perception and escalation: noticing fights, joining, breaking up, calling for help, factions choosing sides

### Takeaway
Splinter Cell: Blacklist's published model is the most detailed shipped reference. Sound events have a radius and a priority. Sound distance is measured along room-to-room paths, not straight lines. Hearing is reduced for NPCs the player can't see, for fairness. A group-behavior system gets first chance at events so several NPCs react together. For taking sides, Bethesda's Creation Kit documents a small faction relation table (Enemy, Neutral, Friend, Ally) plus a per-NPC setting for whether it helps friends and allies.

### Cited Findings
**Splinter Cell: Blacklist, Game AI Pro 2 ch.28 (Martin Walsh, Ubisoft Toronto; GDC 2014 talk)**
- Four perception types were modeled: visual, auditory, environmental, and social/contextual. The goals were fairness, consistency, good feedback and intelligence. — [Game AI Pro 2 ch.28](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter28_Modeling_Perception_and_Awareness_in_Tom_Clancy's_Splinter_Cell_Blacklist.pdf); talk: [GDC Vault](https://www.gdcvault.com/play/1020195/Modeling-AI-Perception-and-Awareness)
- Hearing: "every audio event has a radius and priority; if an NPC is in range of the event, he will hear it and react differently based on the event, who else is in range, and his current state." — [Game AI Pro 2 ch.28](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter28_Modeling_Perception_and_Awareness_in_Tom_Clancy's_Splinter_Cell_Blacklist.pdf)
- Straight-line sound distance "would mean the player would be heard through walls." Using the sound engine was expensive and caused "false detection bugs, which made testing difficult." Ubisoft instead summed distances along the room-to-room path through choke points. — [Game AI Pro 2 ch.28](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter28_Modeling_Perception_and_Awareness_in_Tom_Clancy's_Splinter_Cell_Blacklist.pdf)
- Fairness: "it's only important what's plausible from the player's point of view." NPCs that are off-screen and far enough away have their hearing "reduced by ½" for certain events. The same rule was applied to indirect visual events such as "seeing an NPC get shot." — [Game AI Pro 2 ch.28](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter28_Modeling_Perception_and_Awareness_in_Tom_Clancy's_Splinter_Cell_Blacklist.pdf)
- Feedback: barks (short voice lines) come in three tiers. Specific lines ("I think I heard footsteps") play first, then more generic ones, so the player learns what caused detection without hearing lines repeat. — [Game AI Pro 2 ch.28](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter28_Modeling_Perception_and_Awareness_in_Tom_Clancy's_Splinter_Cell_Blacklist.pdf)
- Changed objects, such as a door left open, create an event with a lifetime. "The first NPC to witness that event would claim it." — [Game AI Pro 2 ch.28](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter28_Modeling_Perception_and_Awareness_in_Tom_Clancy's_Splinter_Cell_Blacklist.pdf)
- Group behaviors: the system "takes control of all NPCs involved and gets to be first to handle any event received by any of those NPCs." Example: a conversation pauses for an investigation. If one NPC dies, the group behavior sends the survivor to investigate ("Hey, are you ok over there?"). — [Game AI Pro 2 ch.28](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter28_Modeling_Perception_and_Awareness_in_Tom_Clancy's_Splinter_Cell_Blacklist.pdf)
- Context: the same event means different things in different contexts. A dead body found during a search triggers a call for help; in a war zone it is ignored. — [Game AI Pro 2 ch.28](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter28_Modeling_Perception_and_Awareness_in_Tom_Clancy's_Splinter_Cell_Blacklist.pdf)

**Factions: Bethesda Creation Kit (Skyrim and Fallout 4)**
- Faction relations use a Group Combat Reaction of Neutral, Enemy, Ally or Friend. Enemies are attacked on sight by Aggressive, Very Aggressive and Frenzied actors. Friends and Allies are attacked only by Frenzied actors and are assisted by actors set to "Helps Friends and Allies" (and "Helps Allies" for Allies). Neutral is the default between factions. — [Fallout Wiki: Creation Kit/Faction](https://fallout.wiki/wiki/Resource:Creation_Kit/Faction). This is a search snippet; the page itself was not fetched.
- The Skyrim script function `GetFactionRelation` returns 0 for Neutral, 1 for Enemy, 2 for Ally and 3 for Friend. — [UESP CK Wiki: GetFactionRelation](https://ck.uesp.net/wiki/GetFactionRelation). This is also a search snippet; the main Faction page returned 403.

### Inferences
- **Proposed fight noise model:**
  - Combat hits, shouts and falling bodies post stimulus events with a type, position, radius, priority and instigator.
  - The project already has a guard sight check (range, facing, occlusion raycasts). Add a hearing check that measures distance along cell, corridor and wing connectivity rather than straight lines, following Splinter Cell.
  - A room graph fits prison layouts well. Splinter Cell notes its model fits indoor spaces with clear choke points best.
- **Proposed escalation using existing vocabulary:**
  - Hearing or seeing a fight raises a guard's suspicion. Past a threshold, the guard enters a Respond super-state: approach, order the fight to stop, then subdue whoever doesn't stop.
  - A "call for help" is a group event posted to nearby guards, like Splinter Cell's group behaviors.
  - Following the "first to witness claims it" rule, only one guard claims the initial response, so every guard doesn't run to the same fight.
- **Proposed faction model:**
  - A relation table between gangs (and guards), in the Creation Kit style of Enemy, Neutral, Friend and Ally.
  - Each NPC has an aggression setting and an assistance setting.
  - A bystander who notices a fight looks up its relation to each fighter and joins the side it is allied with. This can later be refined by utility scoring (distance, own health, relationship strength) without changing the table.
- Fairness rule to adopt early: an NPC must not react to anything the player couldn't plausibly expect it to notice. Pair this with barks or on-screen feedback when an NPC notices.

### Gaps
- No primary source found on how shipped prison games (for example The Escapists, or Riddick: Escape from Butcher Bay) or crime-simulation games implement guards breaking up fights. Skyrim's crime and assault reporting system was not fetched because the page returned 403.
- The Splinter Cell visual-perception section (detection shapes and timings) was not read in full. Only the pages on environmental awareness, hearing and social awareness were read.

## 6. Testability: deterministic, debuggable AI for automated play-mode checks

### Takeaway
Unity's global `UnityEngine.Random` is shared state, and Unity itself recommends `System.Random` instances for independent streams. Give each NPC brain its own seeded generator. Shipped studios rely on AI recorders, replay and log visualization. The testability lesson from Splinter Cell: deterministic, graph-based perception math removed "false detection bugs" that made testing difficult.

### Cited Findings
- "UnityEngine.Random is a static class, and so its state is globally shared." It offers `InitState(seed)` and a gettable and settable `state`. For "multiple independent random number generators," Unity advises managing instances of `System.Random`. — [Unity 6.3 Scripting API: Random](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Random.html)
- Game AI Pro 3 ch.6 (David Young, Treyarch) describes an in-game recorder. It captures the minimal data needed to reproduce each object's visual state every frame, and on pause restores it for instant scrubbing. — [Game AI Pro 3 ch.6](http://www.gameaipro.com/GameAIPro3/GameAIPro3_Chapter06_Debugging_AI_with_Instant_In-Game_Scrubbing.pdf). This comes from a search summary; the full PDF was not read.
- Game AI Pro 3 ch.3 is "Logging Visualization in FINAL FANTASY XV" (Square Enix). — [Game AI Pro](https://www.gameaipro.com/). This is a search-result listing; the contents were not read.
- Splinter Cell replaced sound-engine distance queries, which caused "false detection bugs, which made testing difficult," with a deterministic room-path calculation. — [Game AI Pro 2 ch.28](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter28_Modeling_Perception_and_Awareness_in_Tom_Clancy's_Splinter_Cell_Blacklist.pdf)
- Kingdoms of Amalur centralized all attack positioning in one stage manager. — [Game AI Pro ch.28](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter28_Beyond_the_Kung-Fu_Circle_A_Flexible_System_for_Managing_NPC_Attacks.pdf)
- Doom's custom state editor cross-checked its states against C++ headers for integrity. — [Game Developer, Thompson 2018](https://www.gamedeveloper.com/design/cyber-demons-the-ai-of-doom-2016-)
- Unity Behavior's debugging includes node breakpoints that pause play mode (1.0.15), live graph debugging, and JSON serialization of graph and blackboard values. — [Behavior CHANGELOG](https://docs.unity3d.com/Packages/com.unity.behavior@1.0/changelog/CHANGELOG.html); [Unity 6.3 Manual: Behavior](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.behavior.html)
- FSMs are "intuitive and easy to visualize." — [Game AI Pro ch.4](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter04_Behavior_Selection_Algorithms.pdf)

### Inferences
- **Proposed design for testability:**
  - Write the combat brain as a plain C# class, not a MonoBehaviour, with a `Tick(dt, perceptionSnapshot)` method.
  - Inject the random generator (`System.Random(seed)` per NPC) and the clock.
  - Have the brain output intents (move-to, face, start-attack X, block), which a separate MonoBehaviour executes.
  - Most decision logic can then be covered by Edit-mode unit tests without physics or animation. Play-mode tests cover the integration.
- Log every state transition, token or slot grant, and stimulus received as structured data: time, NPC id, from/to state, reason. Play-mode tests can then assert sequences, for example "inmate entered WindUp at least 300 ms before first damage event" or "at most 1 attacker granted at any time." The same log can drive an on-screen debug overlay and Gizmos showing circles, slots, hearing radii and sight rays.
- Avoid `UnityEngine.Random` inside AI decisions. Other systems such as audio and VFX would share and shift its state, making results depend on unrelated code.
- Physics and NavMesh are not guaranteed bit-exact across machines. Assert on outcomes with tolerances (states reached, distances within a range), not exact positions.

### Gaps
- The full Treyarch and Final Fantasy XV debugging chapters were not read. No source was found on NavMesh or physics determinism guarantees in Unity 6.3.

## 7. Co-op considerations for a later host-authoritative version

### Takeaway
In Netcode for GameObjects' default client-server topology, the server owns all NetworkObjects, so NPC AI runs on the host. Per-target attack budgets (Amalur) and camera-independent fairness checks carry over to several players naturally. No primary source was found on threat and target selection for co-op melee NPCs.

### Cited Findings
- "By default, Netcode for GameObjects assumes a client-server topology, in which the server owns all NetworkObjects." Clients can request ownership, but "the server has the final say." — [NGO 2.10: Ownership](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.10/manual/terms-concepts/ownership.html)
- Unity Behavior 0.9.0: "BehaviorGraphAgent is now public instead of internal when Netcode For GameObjects is included." An earlier version (0.5.10) had errors when NGO was included. The changelog mentions nothing else about networking or determinism. — [Behavior CHANGELOG](https://docs.unity3d.com/Packages/com.unity.behavior@1.0/changelog/CHANGELOG.html)
- Kingdoms of Amalur: "every NPC had a grid for itself," though the player was the main case. — [Game AI Pro ch.28](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter28_Beyond_the_Kung-Fu_Circle_A_Flexible_System_for_Managing_NPC_Attacks.pdf)
- Splinter Cell's fairness rules depend on what the player can see, such as halved hearing for off-screen NPCs. — [Game AI Pro 2 ch.28](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter28_Modeling_Perception_and_Awareness_in_Tom_Clancy's_Splinter_Cell_Blacklist.pdf)

### Inferences
- Design now, without building networking:
  - The AI brain takes an explicit target entity (the acting player or an NPC), never "the local player" or `Camera.main`.
  - Perception works from world state: player transforms, facing and held items. It does not read local input or camera data.
  - Each player-controlled fighter owns its own attack budget, so two co-op players each get their own attacker limit.
  - The "no off-screen attacks" and off-screen hearing rules should use each player's facing vector, which the host has, and not a render camera.
- **Threat selection (proposed, unsourced):** score candidate targets with a small utility function: distance, who hit me last, current attackers on that target (from the coordinator), and whether the target is armed. Add hysteresis so NPCs don't switch targets every frame. Record the chosen target in the transition log for tests.
- Shared world state vs per-player state:
  - Shared: faction relations, a guard's alert level for an area, and fight events. Guard suspicion that is per player is per-player state.
  - This matches the project's existing "personal suspicion" design, which is already per-player-shaped.

### Gaps
- No primary source found on threat or aggro selection for co-op melee NPCs, for example MMO threat tables or co-op brawler postmortems.
- No source found on how much NPC animation and hit-reaction state a host-authoritative melee game must replicate for readable client-side combat. That topic belongs to the netcode research rather than AI.
