# Gameplay

## Agreed foundation

First-person movement and interaction inside the prison. Daily routines, trading, favors, relationships, jobs, and cell improvements should make prison life enjoyable before escape becomes a major goal.

Player base walking speed: **3.02 m/s**, increased from 3.00 by the requested 0.02 on September 24. M5's separate NPC walking speed remains 0.52 m/s.

The experience should be approachable and easy to settle into even when its systems are complex. The game should do the tracking and bookkeeping; the player concentrates on current actions and decisions. Deadlines, suspicion, and disrupted supplies are allowed. The player must clearly understand when these apply to them. This is the agreed priority; the interface rules below are proposals for achieving it.

## Managing complexity - proposed feedback rules

- Make familiar work and trading satisfying to repeat, while supporting deeper production, social, and economic systems.
- Automatically track requests, deadlines, stock, costs, relationships, and changes that affect the player. Do not require manual bookkeeping or repeated checks of several menus.
- Keep the chosen objective visible. Give approaching deadlines and active threats a consistent place on screen, with the full list available in one notebook or overview.
- When a condition begins, changes materially, or ends, state what happened and why. Persistent conditions need persistent status; a brief notification alone is insufficient.
- Explain relevant consequences and available responses. Notify the player when a prison-imposed obligation begins as well as when they voluntarily accept one.
- Display deadlines using clearly labeled game time, with remaining time and advance reminders. Define how menus, sleep, and time skipping affect them before implementation.
- Show suspicion's source, cause, severity, and whether it is rising or falling. Distinguish one guard watching the player from a prison-wide search or alert.
- Group related effects into a clear explanation. A fight that closes a route and blocks three deliveries should update those deliveries automatically and identify the common cause.
- Use text and icons as well as color. Prioritize alerts, retain their history, and avoid repeating the same warning every few seconds.
- Helpers and production systems can automate repeat work and routine calculations. Important decisions stay with the player; tracking a problem does not mean choosing how to resolve it for them.

Example interface content, with placeholder names and values:

| Situation | What the player sees | What the game handles |
| --- | --- | --- |
| Guard suspicion | Harris is watching; restricted area; suspicion rising; leave the area to reduce it | Detect the cause, show the relevant guard, update the state, and announce when it ends |
| Delivery deadline | Deliver 3 coffees to Leon; 2/3 ready; due 18:00 game time; 2 game hours left; late delivery reduces payment | Count inventory, calculate remaining time, track the recipient, and issue useful reminders |
| Supply disruption | Laundry delivery blocked by yard closure; two orders affected; reopen time unknown | Mark affected orders and refresh stock and route information when the situation changes |

Exact meters, warning thresholds, and penalties remain open. The interface must reflect actual game state, including uncertainty. Clear communication must be tested alongside the frequency and overlap of urgent events; more visible warnings alone do not establish a comfortable pace.

## Proposed daily loop

1. Start in the cell and choose a small goal, or simply explore.
2. Use meals, work, and recreation as opportunities to meet people, with access times tracked by the game.
3. Acquire goods, perform a service, or arrange a favor.
4. Deliver on a promise, trade, or invest in a relationship.
5. Use the reward to improve living conditions, expand an operation, or prepare a future escape.
6. Read relevant changes and their urgency, then choose a response or the next activity.

Hands-on actions could include taking items from storage, carrying a delivery, arranging cell possessions, completing a short job activity, and speaking to someone face to face. The routine gives the day structure. The player should be able to tell whether there is time to wander, repeat an activity, or postpone a goal, and what a delay would mean.

## Economy and favors - proposed details

**First prototype activity agreed (September 24):** the user selected assembling and selling snack packs. This resumes earning-loop work previously deferred. After the user said "proceed" with the exchange question pending, implementation proceeded with its recommended money option, announced as a prototype assumption. This does not settle the final money/barter economy. Recipes, quantities, prices, customer demand and upgrade details remain provisional test choices.

### Snack-pack earning test - implemented, awaiting review

`Earn01_SnackPacks` now contains the complete small earning loop. These are provisional mechanics for playtesting:

- Collect free starter supplies for two packs from the common-table box, once per play session.
- At the tray, press E to add crackers, add dried fruit, then wrap. Each step consumes its ingredient. The finished physical pack goes into the player's hand; Q puts it down. The tray retains unfinished work for its acting player.
- Carry a finished pack to M5 and press E to sell for $3. The pack is consumed once and the money updates immediately.
- Return to the supply box for a complete refill costing $1, up to six of each ingredient. If money and all ingredients are exhausted and no unsold pack from this player's supplies remains in the scene, one free recovery batch keeps the loop playable. Putting a pack down does not qualify for free supplies.
- Earn $8 and use the cell's shelf plaque to install a shelf. Two starter sales, one paid refill and a third sale reach the goal. The purchased shelf accepts two carried items with E; stored items can be picked up normally.
- An on-screen panel tracks money, ingredients, sales and the next action, including completion of the cell improvement.

Money and ingredient counts are personal; the tray, physical packs and installed shelf are world state. Interactions identify the acting player. This separation is not multiplayer support. The test has one buyer with unlimited demand, fixed prices and placeholder props. There is no saving, timed request, suspicion or supply disruption yet; stopping Play resets progress. [Progress](progress.md) owns verification. Actual gameplay captures: [common table and guidance](images/earn01-table-hud.png), [purchased shelf and completion](images/earn01-shelf-hud.png).

### Tracked snack request - implemented, awaiting review

The user authorized continuing with a tracked customer request on September 24 ("OK good, proceed"). `Earn02_Requests` extends the earning prototype; `Earn01_SnackPacks` remains available. The following are provisional test rules, not final request design:

- M5 offers a request for three snack packs. The prompt and persistent panel state the quantity, $3 per delivery ($9 total) and no deadline before acceptance.
- Press E on M5 to accept. Acceptance does not take a carried pack or pay money; use E again with a pack to deliver it.
- Each valid delivery to M5 consumes one physical pack, pays the existing $3 and advances personal request progress. The HUD shows delivered, remaining and ready packs from the player's supplies, including packs left on a table or shelf.
- On the third delivery, completion remains visible and guidance returns to earning/installing the shelf. There is no extra completion bonus. Normal sales continue afterward; the completed request cannot restart or pay twice.
- Ingredient counts, payment and the existing restocking/recovery rules remain unchanged. The three request deliveries can fund the $8 shelf after one $1 refill.

This earlier scene retains its untimed request for comparison. The subsequent timed test and its selected time rules are described below. There is one request for one customer, no request list, cancellation, repeat-request generation or saving. Request state is personal and identifies the recipient; this is not co-op support. Actual captures: [active request](images/earn02-active.png), [completion](images/earn02-complete.png). Verification lives in [Progress](progress.md).

### Timed request and prison clock - agreed prototype rules, awaiting play review

On September 24 the user selected **1 game minute = 5 real seconds**, **1 game hour (5 real minutes)** to deliver three packs after acceptance, pausing in settings or on focus loss, and leaving sleep/time-skipping for later. The user also selected **remaining deliveries pay $2 instead of $3 after the deadline**. These selections govern the current prototype; final day pacing remains subject to playtesting.

`Earn03_Deadline` implements those rules:

- Prison time starts at Day 1, 08:00:00, a provisional test starting point. A persistent clock labels game time and its speed. It starts paused in settings; Resume starts play. Escape and focus loss pause the clock and world simulation. Returning focus leaves settings open until Resume.
- Before acceptance, the offer states the quantity, time allowed and late-payment consequence. Acceptance fixes the due time one game hour ahead. Repeated talking does not extend it.
- The request panel tracks due time, game time remaining, delivered/ready packs and payment rules. At ten game minutes remaining, a one-time reminder and persistent "due soon" heading warn the player; this warning threshold is provisional.
- At the deadline, the request remains deliverable but its remaining packs pay $2 each. The panel and interaction prompt update, and a notification explains the change. Already earned money is untouched; no debt or retrospective deduction is applied.
- Completion reports the actual total paid and whether it finished late. Passing the deadline after an on-time completion does not change its result. Normal $3 sales resume after completion, so the shelf remains earnable even after late deliveries.

The clock is world state; request progress and earnings are personal. Pausing uses a solo-only driver and is not a release 2 co-op pause design. There is no sleep, time skipping, save/load, guard suspicion or supply disruption in Earn03. The subsequent Guard01_Suspicion scene combines this timed request with [one guard-suspicion test](factions-and-events.md#guard-suspicion-test---implemented-provisional-rules). [Progress](progress.md) owns verification and the remaining scenarios.

### Request feedback during supply interruption - implemented, provisional

`Supply01_Inspection` combines the timed request, guard suspicion and one controlled supply closure. Its provisional trigger and duration live in [Factions and events](factions-and-events.md#supply-interruption-test---implemented-provisional-rules); the agreed clock and late-payment rules above are unchanged.

The HUD explains the inspection, reopening time and continuing deadline. It counts ingredients, finishable work on the tray and existing carried/stored packs before marking any remaining quantity dependent on restocking. Guidance continues to offer assembly or delivery with available stock; once that stock is exhausted, it gives the reopening time. Reopening clears the supply block while preserving any overdue status and actual earnings.

The complete combined loop is technically checked and awaits user review for clarity, pacing and repetition. There is no saving; stopping Play resets this scene too.

### Saving the sample - implemented, provisional controls

On September 24 the user authorized proceeding to save/load. `Save01_Progress` extends Supply01; earlier scenes remain available and do not have save controls. This continuation does not approve the combined loop's feel or pacing.

- Escape/settings offers **Save progress** and **Load save**. One manual slot; saving replaces its prior contents. Loading replaces current progress and stays paused until Resume walking. There is no autosave or automatic loading.
- Saves preserve personal money, ingredients, sales, request progress/payment/deadline, suspicion, player position and view. Shared state includes prison time, unfinished tray work, the installed shelf, physical packs and parcel (including carrying), door position/direction, and inspection timing. Restored packs retain their supplying player.
- Time does not advance while the game is closed. Loading an overdue request retains its reduced payment; loading a closure retains its remaining game time.
- Missing, malformed, incompatible or invalid saves show a failure message without replacing current progress. File replacement retains the previous save as a `.bak` file; automatic backup recovery is not implemented.
- Windows storage: `%USERPROFILE%/AppData/LocalLow/DefaultCompany/PrisonGame/sample-save-v1.json`. This is local sample progress, not cloud saving. Tests use separate project Temp fixtures and do not overwrite this slot.

Controls and single-slot behavior are provisional prototype choices. This format supports this scene/version and one player; migrations, multiple slots, ambient NPC animation/walking phase, transient messages, automatic saving and release 2 save compatibility remain outside this implementation. Mouse sensitivity continues using the existing separate setting. Verification and remaining user review live in [Progress](progress.md).

### Broader economy - proposed

Start with a few ordinary goods such as coffee, snacks, and toiletries. Different inmates have different needs and limited supplies. Services and information can become additional opportunities later.

Favors are promises to specific people. Helping someone might secure an introduction or a future service. Some requests may have deadlines or consequences for delay; show those when the request begins and track them automatically. A simple notebook should show requirements, progress, timing, and what each person owes. The balance between timed and untimed favors remains open.

The main game's choice of money, barter, or a mixture remains open; the prototype's temporary cash prices do not decide it. Begin with understandable stock and demand rules; the economy's complexity should serve player decisions.

## Relationships, jobs, and the cell - proposed details

- NPCs can distinguish trust, fear, and suspicion. Present the states that affect the player's current decisions through simple indicators, dialogue, and behavior. Avoid making the player infer an important active penalty from subtle dialogue alone. The exact model remains open.
- Let jobs create different contacts and access: kitchen, laundry, and library work are candidates.
- Make cell improvements visible and useful through storage, comfort, and workspace.
- Give the cellmate habits and preferences that affect everyday life. Clearly communicate any resulting obligation or risk to the player's belongings. The extent of upkeep and loss remains open.
- Later, trusted inmates could handle parts of an operation, with costs and limits.

### Duty direction - user priority, proposed mechanics (September 25)

**Agreed direction:** duties create access to characters, connections, tools and opportunities. A detailed hands-on work minigame is not required. The user explicitly agreed that free looking, nearby conversation and useful observation should remain available while work progresses. Dialogue during work must therefore allow the duty simulation to continue; this does not change the existing Escape/focus-loss pause rule. The earlier assistant recommendation to choose a job enjoyable as a minigame is no longer a requirement. Existing snack crafting was not requested for removal.

**Agreed first prototype:** the user said "ok lets try this" following the laundry proposal: prison-assigned laundry duty, proximity-based work progress, a supervising guard, an inmate contact and a useful tool in an adjacent supply room. Leaving the work position creates exploration opportunities when the guard is not watching. This authorizes developing the test; the selected prototype rules and remaining details are below. The playable implementation is `Duty01_Laundry`; verification and user review are tracked in [Progress](progress.md).

**Proposed behavior:** guard turns and movements should be readable, with some timing variation and a chance to react. Different duty locations can later use different guard positions/routines and distinct opportunities. Exact variation and detection thresholds remain provisional; the agreed warning/failure rule is below.

**Agreed prototype choices (user follow-up, September 25):** a two-real-minute shift with one cumulative real minute of nearby work (shortened from four/two minutes after user play feedback); leaving pauses work progress without resetting it. The player can look, talk nearby and observe during work. First detection of being away gives a warning and an opportunity to return. Sneaking away supports an inmate favor: obtain the requested tool from the adjacent supply room to earn an introduction to a second useful contact. The user subsequently selected: **ignore the warning or get caught away again ? fail this shift; keep unrelated progress**.

**Open/provisional details:** assignment scheduling; warning grace/detection thresholds; retry handling; specific tool and contact benefit; tool ownership on failure. The two/one-minute values are agreed for this test, not final day balance. Country, security level and historical period of the fictional prison have not been selected.

**Realism research (September 25):** rules differ by jurisdiction, institution, security and activity. Pennsylvania's handbook permits low-voiced conversation with adjacent inmates in its housing rules; this establishes that prison is not universally silent, not blanket permission to socialize during every work detail. Canada's movement policy requires authorized movement and institution-specific times, passes and out-of-bounds rules. Canadian law treats leaving work without reasonable excuse and unauthorized presence in prohibited areas as disciplinary offences. Canada's discipline directive calls for reasonable steps toward informal resolution where possible, supporting a warning for a minor lapse without guaranteeing one for every offence. Sources: [PA inmate handbook](https://www.pa.gov/content/dam/copapwp-pagov/en/cor/documents/about-us/doc-policies/inmate-handbook.pdf), [CSC movement policy](https://www.canada.ca/en/correctional-service/corporate/acts-regulations-policy/commissioners-directives/566-3.html), [CCRA section 40](https://laws-lois.justice.gc.ca/eng/acts/c-44.6/section-40.html), [CSC discipline policy](https://www.canada.ca/en/correctional-service/corporate/acts-regulations-policy/commissioners-directives/580.html). These are comparative references, not a selected jurisdiction or claims about universal day-to-day enforcement.

**Research-informed design recommendation:** quiet nearby conversation and task-related movement can be normal within the assigned room; exploration into the supply room is unauthorized. Proximity progress represents doing the work while looking/talking, not merely standing idle. Distinguish a minor absence from taking an unauthorized tool; the same warning should not imply real prisons treat those as equivalent. This recommendation preserves the selected game rules and does not add a new punishment system.

**Proposed implementation choices:** use rough modular rooms and placeholder character variants first; exact furniture, names and appearance can be revised. Keep personal assignment/progress separate from shared guard/world state and local input/UI. Do not add speculative networking infrastructure.

### Laundry implementation - provisional details awaiting play review

`Duty01_Laundry` extends the saved snack sample with a connected laundry/supply module. The reusable `LaundryModule` prefab contains the room geometry, work/supply areas, guard and observation markers. Player assignment and favor state live separately. The module can be moved/rotated during authoring, but connections must be rebuilt and existing world-position saves are not automatically migrated.

- Report at the entrance board with E; the two-minute shift ends only when its timer expires. Complete 60 cumulative seconds inside the blue work line. Looking and E conversations keep work running; Escape/focus loss pauses the solo simulation.
- Officer Vale visibly alternates between the machines and desk, using a repeating 8/11/9/13-second schedule and gradual turns. These are varied, deterministic timings for this test, not randomized patrols. Detection requires one continuous second of unobstructed visibility, within 11 metres and a 100-degree horizontal field of view.
- First observed absence gives eight seconds to return. Returning clears the order, retains earned work and uses up the warning. Ignoring it or a second observed absence fails only the current shift. Missing the work quota also fails at shift end. E on the board retries; money, snack progress and favor progress remain. Retry and tool retention are provisional, with no confiscation system.
- Rue is reachable while working. Accept his favor, pocket the valve key from the supply shelf with E, then return it to him. This unlocks an introduction to Dex near the entrance. Dex now reveals the poker-money secret described below; other services remain open. The key is a tracked pocket item with hands left free. Names, appearances and exact reward are placeholders.
- The HUD tracks shift/work, warning countdown and favor. Existing snack/suspicion panels reappear outside the laundry when no shift is active; their underlying rules continue.
- Manual saving includes duty progress, warning/grace, favor/key/introduction and guard attention/rotation. This scene uses `laundry-save-v1.json` in the same local Unity save directory, separate from `sample-save-v1.json`. There is no autosave or import of the older scene's slot.

**User play feedback:** the task feels too simple and the HUD crowded/messy. **Proposed next work:** simplify the HUD now to a compact work bar, one current objective and contextual warnings; defer final styling. **Declined:** the user explicitly said not to add a competing choice for Rue's key. Keep the existing favor. Guard-route variation remains an unselected idea. HUD cleanup remains the recommended next step, not yet implemented or explicitly selected. Older duty saves remain loadable: work and remaining shift time are capped at the new 60/120-second limits without clearing favor or unrelated progress.

**Proposed contextual HUD behavior (research follow-up):** briefly show the assignment when accepted; show a compact work bar while working; on leaving, briefly confirm that progress is paused, then collapse to a small active-shift timer. Show the favor update when accepted/changed and allow details to be recalled on demand. Hide completed task panels after a short confirmation. Keep an active return warning and countdown visible until resolved; retain a small warning-used marker for the rest of the shift. Near-deadline alerts must remain available even away from the duty room. Exact durations, reveal control and placement are unselected. The user requested low obstruction; these implementation details are recommendations awaiting review.

The test has no daily assignment scheduler, punishment escalation beyond shift failure, tool searches, new NPC trading or duty work animation. Quiet conversation in the work strip is allowed; leaving it while supervised and entering the supply room create the intended risk. Proximity represents ongoing work, with placeholder visuals pending the user's judgment of this loop.

## Escape and earlier ideas

Escape may become a later major goal. The earlier concept of combining collected pieces into different routes remains relevant. Physical objects, information, and cooperation are possible requirements; specific routes are unchosen.

Labor quotas, earnings multipliers, and global upgrades came from the earlier concept. Their role in this sandbox is open. Quotas and deadlines can fit if requirements, progress, remaining time, and consequences are clear and automatically tracked. Their difficulty and frequency require playtesting.

## Design checks

- Can the player explain what they want to achieve today?
- Does a delivery create a choice beyond walking between two points?
- Do improvements open useful options?
- Is another prison day appealing even when the player makes no escape progress?
- Can the player tell which tasks can wait and what happens if they postpone one?
- Does repeating the core activity feel pleasant, and do improvements reduce chores?
- Can the player understand the next action without inspecting several status screens?
- When suspicion, a deadline, or a supply problem applies, can the player identify its cause, urgency, consequences, and possible response without being told by the developer?


## Inventory and pickup feedback - proposed next scope

The user asked when inventory and pickup notifications should be added. Current implementation has a single held physical item, snack ingredient counters and a separate pocketed favor-key flag, not a unified inventory.

**Proposed sequence:** implement selected HUD 01 with a shared short-notification presentation, then a small inventory covering existing items before expanding the cast or rooms. Inventory should distinguish pocket contents from the item physically held, support quantities and appropriate select/hold/drop/give actions, and preserve existing save progress. Capacity, stacking, pocket eligibility and quick-slot controls remain open design choices. Do not silently make every prop pocketable or change shelf/physical-pack gameplay.

**Proposed notification behavior:** show a short item-and-quantity message only after a successful transfer; combine repeated pickups; distinguish holding an object from adding it to pockets. Use the same presentation for deliveries, payments and duty completion. Guard orders keep a separate persistent, higher-priority warning; ordinary messages must not replace an active return countdown. Final position, duration and limits are unselected.


### First inventory view and contextual HUD - provisional implementation

Duty01 now uses selected HUD 01 and B-style warm notifications. Hold Tab while walking to view a right-side inventory panel and recall the current favor; release to close. It is a read-only view of the existing pocketed key, snack ingredient counts and separately held physical object. The world continues while viewing; Escape remains the solo pause. E/Q world interactions are blocked while Tab is held. There are no new inventory capacity, weight, storage, item-transfer or drag/drop rules. No save format change is required for the view.

During laundry, work progress shows near machines; the favor line appears briefly on changes or while Tab is held. Leaving work briefly shows paused work before collapsing to the shift timer. The used-warning marker persists; active return orders stay visible and suppress ordinary message display. Notifications use a warm charcoal panel, preserve longer dialogue durations, briefly fade at their end and queue up to three pending messages. Identical current messages refresh their duration. Notification timers pause with controls/settings or during inventory/guard warning display. This first pass does not combine different quantity messages into summed pickups.

The inventory layout is provisional and can be replaced independently of item state. It is a view of existing systems, not a completed unified inventory backend. Earlier snack-room HUD panels remain outside the laundry-focused display; broader HUD consistency is later work.


### Six-item inventory and Dex's secret - September 25

**Agreed:** a six-item limit. Dex reveals that the first officer is skimming poker-game money; the player can blackmail him to obtain a key. This extends Rue's existing favor without adding a competing choice.

**Provisional interpretation pending clarification:** six carried slots, including the item in hand. Each ingredient type stacks in one slot; each parcel, finished snack pack and key occupies one slot. Stack interpretation remains provisional. **Subsequent explicit decision:** the secret belongs to Officer Harris, not Vale, and his key opens a new cell next to the player's.

**Implemented controls, awaiting review:** hold Tab to view six numbered slots, press 1-6 to select. F holds a selected pocketed parcel/pack; R pockets the held parcel/pack, including outside the panel; Q places the selected physical item on a clear nearby surface. Release Tab to close. One object can be held. The world keeps running while viewing. Ingredients are used directly at the assembly tray; Rue's valve key is given directly with E. Quest keys stay in pockets. Other props are not made pocketable. The warm-panel layout remains provisional.

New pickups and key rewards cannot exceed capacity. Supply batches are checked before charging money or consuming the starter allocation. Moving an object from hand to pockets does not create another slot. A blocked drop retains the item. Finished packs in pockets continue counting as available stock; hold one before delivering to M5. Pocket ownership is saved alongside existing held/world item states; old saves default physical objects to not pocketed. Malformed over-capacity saves are rejected without replacing current progress.

**Implemented progression:** give Rue the valve key, talk to Dex, then approach Harris in the common room and use the explicitly labelled blackmail interaction. Dex reveals that Harris pockets extra poker proceeds before splitting the take. Harris gives one key, once; if inventory is full, the reward remains available. Secret and key survive saving, loading, duty failure and retry. Existing saves with Dex introduced can learn the secret by talking to him again. No added retaliation or punishment system.

**Agreed follow-up:** Harris's key opens a newly added neighboring cell. Use E on its door/control to unlock and open it. **Provisional implementation:** the key is retained, and the door stays unlocked; unlock and slide position are shared world state included in manual saves. The previous generic officer-key save flags now represent Harris's key, preserving earned progress. Old saves without room state start the new door closed and locked.

**Agreed subsequent purpose:** another inmate occupies the neighboring cell, and the player kills him to close this prototype's progression loop. See the encounter scope below. The current build still contains only furnishings. **Open:** the inmate's identity, the player's motivation, combat details (real combat is agreed; see below), consequences of killing/blackmail, and confirmation of slot/stack interpretation.


### Immediate centered dialogue - user-requested correction

**Agreed:** conversations should respond promptly and appear nearer the center for easier reading. The previous notification queue delayed spoken lines behind earlier messages.

**Implemented, awaiting review:** conversations now use a separate centered warm panel below the crosshair. The complete line appears immediately; another conversation replaces it immediately. Item/payment notices remain separate and resume after dialogue. Guard return warnings remain visible above. Walking, looking and duty work continue during dialogue; Escape still pauses. Dialogue clears on load and has a reading timer, with no typewriter delay or forced wait before talking again. Applies to Rue, Dex, Vale, Harris and M5 in Duty01. Exact panel placement/read durations remain tunable.

### Neighboring-cell inmate encounter - agreed purpose and real combat, details open

**Agreed:** the user selected another inmate in the neighboring cell whom the player kills, closing the current progression loop: laundry access -> Rue's favor -> Dex's information -> blackmail Harris -> obtain key -> enter neighboring cell -> kill the inmate. This replaces the earlier open-ended suggestion of a discovery, contact or storage reward in that room.

**Agreed September 25 - real combat:** the encounter uses real combat; the user declined a staged interaction. The player fist-fights the inmate and the inmate fights back. The player can pick up nearby objects, such as a book or a rod, and use them as weapons. Real combat is also agreed for release 1 as a whole; see [Release 1 scope](development-plan.md#release-1-scope---agreed-september-25).

**Open - what a weapon changes:** the user's wording was "if I am fist fighting then they would be able to fight back unless I pick up something like a weapon like a book or rod or something nearby." Whether an armed player stops the inmate from fighting back, or only gains a strong advantage, is not yet confirmed.

**Superseded proposal:** the earlier recommended first implementation (one deliberate labelled interaction and a simple death response) was a staged approach and is no longer planned. A saved alive/dead state and an explicit completion outcome are still needed. The concern it addressed, that a fight should not start from an accidental talk or door press, remains relevant to combat input.

**Open decisions:** combat controls and rules (attacks, blocking, health, stamina, knockdown); which objects can be weapons and what each changes; how the objective/motivation is introduced; the inmate's identity; presentation; and what follows the killing, including any witness/guard response, punishment or reward. Selecting this encounter does not imply that killing is consequence-free in the final game. [Combat research](research/reports/Unity%20first%20person%20melee%20combat.md) collects design and implementation references for these choices (in progress September 25; findings are not decisions). No combat, death or encounter completion is implemented in the current build.
