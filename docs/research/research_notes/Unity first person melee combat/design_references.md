# First-person melee combat design references (fists and improvised weapons)

Research date: 2026-09-25. Status of everything below: **research input only, not a design decision.** Any "option" in the Inferences sections is a proposal for the designer to accept or reject.

Source-confidence tags used on each finding:
- **[fetched]**: I read the page itself.
- **[excerpt]**: taken from search-result text for that URL; I did not open the page. Treat as likely but unverified.
- **[fan]**: fan-maintained wiki, player guide, or forum. Mechanics details may be outdated or wrong.
- Fandom wikis returned HTTP 402 to direct fetches, so every Fandom-sourced item is [excerpt][fan].

## 1. How do the reference games structure unarmed and improvised-weapon melee (verbs, stamina, stagger, hit reactions, knockdown)?

### Takeaway
Almost every successful reference uses the same small core: **attack (often with a hold-to-charge heavy), block (with a stronger timed "perfect block"), and a shove/kick or grab that creates space or opens a finisher**, with stamina limiting spam. Deep directional systems (Kingdom Come, Chivalry 2, Mordhau) exist but target players who want to master duels. First-person-specific games (Condemned, Riddick, Zeno Clash, Dark Messiah) keep verbs few and make timing of the block the main skill.

### Cited Findings

**Condemned: Criminal Origins (Monolith, 2005): the closest tone/verb reference for improvised weapons in first person**
- Players spend "a good 85% of the time swinging melee weapons"; weapons include pipes pulled off walls, boards with nails, and the back end of a shotgun; firearms have "extremely limited ammo" — [Manveer Heir, Game Developer, "Design Lesson 101 - Condemned", 2008](https://www.gamedeveloper.com/design/design-lesson-101---i-condemned-criminal-origins-i-) [fetched]
- "There is very little chance an enemy will miss in combat in Condemned if they swing at you and you are standing still", so "the player must then learn to block and counter attacks reliably" — [Game Developer](https://www.gamedeveloper.com/design/design-lesson-101---i-condemned-criminal-origins-i-) [fetched]
- Close-quarters combat plus tight corridors limits field of view and lets enemies flank, producing tension even though the game is "not that difficult" — [Game Developer](https://www.gamedeveloper.com/design/design-lesson-101---i-condemned-criminal-origins-i-) [fetched]
- Each melee weapon has stats for damage, speed, block, and reach — [GameFAQs weapon/enemy guide by Joylock](https://gamefaqs.gamespot.com/pc/926310-condemned-criminal-origins/faqs/49952) [excerpt][fan]
- Blocking is timing-based; "just holding the block button won't help you" — [Gaming Pastime review](https://gamingpastime.com/condemned-criminal-origins-review/) [excerpt]

**Condemned 2: Bloodshot (2008): dedicated fist fighting**
- Left and right triggers control the left and right fist; unarmed has less damage and reach than weapons but is fast and chains into combos — [Wikipedia](https://en.wikipedia.org/wiki/Condemned_2:_Bloodshot) [excerpt]
- A full "chain meter" allows a chain attack via quick-time event, up to four attacks; taking damage mid-combo resets the combo; context-sensitive environmental kills (head into a TV, spiked wall, off a balcony) — [Wikipedia](https://en.wikipedia.org/wiki/Condemned_2:_Bloodshot) [excerpt]

**The Chronicles of Riddick: Escape from Butcher Bay (Starbreeze, 2004): first-person prison game with fist fighting**
- First-person; mostly improvised melee with fists, shivs and clubs — [Giant Bomb](https://giantbomb.com/wiki/Games/The_Chronicles_of_Riddick_Escape_from_Butcher_Bay) [excerpt]
- Attack and block; a late or early block still reduces damage, a perfectly timed block minimizes it; four directional punches chosen by stick direction; blocks and punches combine into counters; against an armed enemy a well-timed press grabs the weapon and turns it on him — [Wikipedia](https://en.wikipedia.org/wiki/The_Chronicles_of_Riddick:_Escape_from_Butcher_Bay); [GameSpot review](https://www.gamespot.com/reviews/the-chronicles-of-riddick-escape-from-butcher-bay-/1900-6099643/) [excerpt; the search summary blended these two pages, so exact attribution between them is unverified]
- Critics called it among the best hand-to-hand combat in a first-person shooter — [PC Review](https://www.pcreview.co.uk/articles/the-chronicles-of-riddick-escape-from-butcher-bay.133/) [excerpt]

**Zeno Clash (ACE Team, 2009): first-person brawler**
- Lock on by looking at a target and pressing a button; hold block (blocking costs stamina, no health damage); block + move = dodge; dodge a punch then counter; walking up to a stunned enemy auto-grabs him; while holding, attack = knee bash, direction + attack = throw; enemies can be disarmed and their weapons used — [GameFAQs guide](https://gamefaqs.gamespot.com/xbox360/975437-zeno-clash-ultimate-edition/faqs/59819); [Altered Gamer combat guide](https://www.alteredgamer.com/shooters/34566-zeno-clash-combat-guide-put-em-up/) [excerpt][fan]
- ACE Team: "The first combat mechanics were very restrictive, because we were convinced that we had to take control from the player and let the combat system handle certain events"; references studied were Dark Messiah, Condemned and Breakdown — [bit-tech interview](https://bit-tech.net/reviews/gaming/pc/zeno-clash-interview-into-the-unknown/3/) [fetched]
- The first lock-on was so restrictive it made players play to the enemies' rhythm rather than their own; the team added versus screens and HUD health bars "like in classic Final Fight" to emphasize fighting; they attended Arkane's GDC session on Dark Messiah — [Game Developer, Road to the IGF: Zeno Clash](https://www.gamedeveloper.com/game-platforms/road-to-the-igf-ace-team-s-i-zeno-clash-i-) [excerpt]

**Dark Messiah of Might and Magic (Arkane, 2006)**
- The kick knocks enemies back, and "spike racks, open fires, and pitfalls" are in almost every combat area, which "can often end a fight more efficiently than using only weapon attacks" — [Wikipedia](https://en.wikipedia.org/wiki/Dark_Messiah_of_Might_and_Magic) [fetched]
- Arkane's goal: "We didn't want Half Life 2 with a sword instead of the crowbar" — [Game Developer, GDC 2007 report](https://www.gamedeveloper.com/game-platforms/gdc-arkane-s-colantonio-takes-on-first-person-melee) [excerpt]
- Later Arkane games: the kick was well liked, and Deathloop's kick has its own design story — [PlayStation Blog, "The birth of Deathloop's powerful kick"](https://blog.playstation.com/2021/10/20/the-birth-of-deathloops-powerful-kick/) [excerpt, not opened]

**Kingdom Come: Deliverance 1 and 2 (Warhorse)**
- Five attack zones chosen by mouse/stick; every strike costs stamina; blocking costs stamina scaled to the opponent's strength; at zero stamina, hits go to health; a "perfect block" timed to a green shield icon costs no stamina and interrupts the attacker's chain; KCD1 master strike is a counter from a correctly directed perfect block with "no defence"; combos are timed inputs on contact; the kick button near an enemy starts a clinch resolved by button mashing; feints switch direction mid-windup; blunt weapons beat armor; wounds cause bleeding that can kill — [KCD wiki.gg, Combat](https://kingdomcomedeliverance.wiki.gg/wiki/Combat) [fetched][fan]
- Unarmed: the basic strike is a left or right hook to the head; the fast strike is a jab to the face — [KCD Fandom codex, Basic Combat](https://kingdom-come-deliverance.fandom.com/wiki/Combat) [excerpt][fan]
- KCD2: master strike is no longer tied to blocking; you must strike from the opposite side at the same moment as the opponent — [memoryPC blog guide](https://www.memorypc.eu/blog/gaming-news/kingdom-come-deliverance-2-how-to-use-the-master-strike-correctly/) [excerpt][fan]; master strike works only with swords — [Game Rant](https://gamerant.com/kingdom-come-deliverance-2-kcd2-how-master-strikes-explained/) [excerpt]
- Warhorse made "iterative changes to the combat, with some aiming to make it more accessible to people for whom it was too complex in the first game" and others making it deeper — [GOG interview with Warhorse](https://www.gog.com/en/news/read_our_exclusive_interview_with_warhorse_studios_about_kingdom_come_deliverance_ii) [excerpt]

**Chivalry 2 and Mordhau (multiplayer first-person duel systems)**
- Chivalry 2: holding block does not stop everything; block must line up with the swing direction; riposte is a faster attack in a short window after a successful parry and costs no extra stamina; feints switch attacks; a "counter" (same attack as the opponent, started before blocking) acts as a perfect parry that avoids the stamina loss and knockback of a normal block — [Chivalry 2 Fandom wiki, Combat](https://chivalry2.fandom.com/wiki/Combat); [GINX combat guide](https://www.ginx.tv/en/chivalry-2-combat-guide-how-to-parry-riposte-counter-initiative-mechanic) [excerpt][fan]
- Mordhau: feint (cancel windup) costs 10 stamina; morph (switch attack type mid-windup) costs 7; parry is a brief window that drains both players' stamina; chamber (mirror the opponent's attack just before it lands) costs more but hard-counters feints; kick costs 10 stamina on miss and drains 10 from the target on hit — [Mordhau Fandom wiki](https://mordhau.fandom.com/wiki/Combat_techniques) [excerpt][fan]

**Dying Light / Dying Light 2 (Techland)**
- Hit reactions depend on "reaction type * weapon type * weapon direction * body part * hit phase (enemy stamina) * player direction in relation to the enemy", producing about 1,000 animations for one common enemy type; "not all hit reactions result in a ragdoll effect. In such cases, we need to rely on animations" — Animation Director Dawid Lubryka, [3DVF interview](https://3dvf.com/en/redaction/dying-light-2-stay-human-our-interview-with-techland-about-the-gut-feeling-update-focused-on-combat-animations-and-physics/) [fetched]
- In the "Gut Feeling" update Techland found "each attack felt excessively powerful, which didn't align with the variety of contexts" and rebalanced — [3DVF](https://3dvf.com/en/redaction/dying-light-2-stay-human-our-interview-with-techland-about-the-gut-feeling-update-focused-on-combat-animations-and-physics/) [fetched]
- A perfect block (block at the last moment) staggers the enemy and opens him up — [Steam discussion](https://steamcommunity.com/app/534380/discussions/0/3186862118584720995/) [excerpt][fan]

**Sandbox/prison games with simple melee**
- The Escapists 2: left click attacks once, hold left click to charge extra damage; right click blocks, taking no damage but moving slower — [Escapists Fandom wiki, Combat System (TE2)](https://theescapists.fandom.com/wiki/Combat_System_(The_Escapists_2)) [excerpt][fan]
- Schedule I: with nothing equipped the player punches; punches are charged attacks, and holding increases "damage, impact force, stamina cost, and attack cooldown"; melee weapons include a baseball bat, frying pan and machete (50 damage per swing) — [Schedule I Fandom wiki, Weapons](https://schedule-1.fandom.com/wiki/Weapons); [TheGamer](https://www.thegamer.com/schedule-1-all-weapons-baseball-bat-revolver-fight-npcs-how-to-guide/) [excerpt][fan]
- Bully: fights continue until one fighter is knocked out or runs away; low-health opponents can be humiliated (e.g., spit finisher after a tackle) — [Bully Fandom wiki, Fighting](https://bully.fandom.com/wiki/Fighting) [excerpt][fan]
- Sleeping Dogs: close to Batman: Arkham's free-flow system; a "Face" meter fills faster with varied attacks and environmental attacks — [Wikipedia](https://en.wikipedia.org/wiki/Sleeping_Dogs_(video_game)) [excerpt]
- Lethal Company (first-person, cute-but-eerie co-op): shovel is held to wind up and released to swing, has about a 1-second cooldown, and briefly stuns what it hits — [Lethal Company Fandom wiki, Shovel](https://lethal-company.fandom.com/wiki/Shovel); [Miraheze Lethal Company wiki](https://lethal.miraheze.org/wiki/Shovel) [excerpt][fan]
- Party Animals (cute physics brawler): punch, toss, jump, kick, headbutt; three styles of engagement: spam punches up close, run in for a heavy punch, or wait and counter — [TheGamer](https://www.thegamer.com/party-animals-how-to-play-guide/) [excerpt][fan]
- Sifu: player and enemies both have a "structure" gauge filled by blocked attacks; when full, guard breaks and the target is open to finishers; parrying prevents your own structure from breaking — [SVG](https://www.svg.com/760718/how-sifus-structure-system-actually-works/) [excerpt]

### Inferences
- The shared minimum across first-person references is: fast attack, hold-to-charge heavy, block with a timed perfect-block reward, and a space-making move (kick/shove) or grab. Schedule I, The Escapists 2 and Lethal Company show that "hold to charge" alone gives meaningful choice with very few inputs, and matches the project's Schedule I reference point.
- A "stagger/structure" meter (Sifu, KCD stamina-to-health, Dying Light "hit phase") gives a readable intermediate state between "hit" and "down," which suits a one-on-one first case where the player must learn the fight.
- Directional-attack systems (KCD, Chivalry 2, Mordhau) are the deepest but also the least accessible; Warhorse itself moved KCD2 toward accessibility. For a first combat case these look like scope and learning-curve risk, not a starting point. (Proposed, not decided.)
- Techland's ~1,000 hit-reaction animations per enemy is out of reach for a small team; a smaller set of directional hit reactions plus physics (ragdoll only on knockdown) is the practical approximation. (Inference; Unity implementation was out of scope for this research.)
- Zeno Clash's lesson (do not take control away from the player; restrictive lock-on made players follow enemy rhythm) argues against auto-lock or scripted counters as the default.

### Gaps
- No primary developer source (talk or postmortem) found for Condemned's melee system; Monolith's own design commentary was not found.
- No Starbreeze commentary found on Riddick's melee design.
- Chivalry 2, Mordhau, The Escapists 2, Schedule I, Bully, Lethal Company and Party Animals mechanics come from fan wikis and guides only; numbers could be patch-dependent.
- Half-Life 2 physics objects as improvised weapons were not researched (tool budget).
- Schedule I NPC behavior when punched (fight back vs flee) was not found.

## 2. How do these games make incoming attacks readable in first person with a limited field of view?

### Takeaway
Readability comes from four layers used together: **(1) long, exaggerated, centered wind-up animations; (2) a distinct cue at the parry/counter moment (color flash, icon, sound); (3) off-screen direction indicators or call-outs when multiple enemies exist; (4) limiting how many enemies attack at once.** First-person makes (1) and (3) harder, which is why Arkane redesigned animations and why Condemned's tension relied partly on limited view.

### Cited Findings

**First-person-specific problems (Arkane GDC 2007, Raphael Colantonio, "The Challenges of Designing First-Person Melee Combat")**
- Talk listing: [GDC Vault](https://gdcvault.com/play/558/The-Challenges-of-Designing-First) [fetched; talk content paywalled]
- Problems: an aiming system for "close range moving targets", strong damage feedback, and gauging distance; distance was "never fully solved" and was worked around by enlarging the player's hit zone so near-misses counted — [Game Developer report](https://www.gamedeveloper.com/game-platforms/gdc-arkane-s-colantonio-takes-on-first-person-melee) [fetched]
- With a full-body player model, attack animations often happened below the field of view; Arkane exaggerated attacks so they looked distorted in third person but natural in first person and stayed centered in view; the body was split into three pieces driven by camera pitch — [Game Developer](https://www.gamedeveloper.com/game-platforms/gdc-arkane-s-colantonio-takes-on-first-person-melee) [fetched]
- After combat worked they widened the FOV, which forced every character animation to be redone from scratch; levels were built before combat was final, which Colantonio regretted — [Game Developer](https://www.gamedeveloper.com/game-platforms/gdc-arkane-s-colantonio-takes-on-first-person-melee) [fetched]

**Attack anatomy and telegraph rules (general design sources)**
- Every attack should be split into Anticipation, Attack, Recovery; each anticipation must be "drastically different" from others and "hint at its follow-up attack"; anticipation time = player reaction time (~0.25 s) + time to trigger the answer + a difficulty buffer, typically 18+ frames at 30 fps; attacks themselves should be fast, unambiguous, constant in direction and speed; weapon trails highlight the dangerous area; recovery is the player's window of opportunity — [Nicolas Kraj, GDKeys "Anatomy of an Attack", 2020](https://gdkeys.com/keys-to-combat-design-1-anatomy-of-an-attack/) [fetched]
- Player input lag under ~100 ms feels "tight"; above ~160 ms feels sluggish; animation dead frames are a common source — [GDKeys](https://gdkeys.com/keys-to-combat-design-1-anatomy-of-an-attack/) [fetched]
- "If players don't understand the questions they are being asked, they actually can not play your game"; enemies need a noticeable pre-attack delay; layer animation, sound, VFX and voice; each attack type needs a distinct, consistent telegraph — [Mike Stout, Game Developer, "Enemy Attacks and Telegraphing", 2015](https://www.gamedeveloper.com/design/enemy-attacks-and-telegraphing) [fetched]
- Readability splits into telegraphing (before) and expectations (after) and "arguably has the largest impact on frustration" — [Game Developer, "Designing for Difficulty: Readability in ARPGs"](https://www.gamedeveloper.com/game-platforms/designing-for-difficulty-readability-in-arpgs) [excerpt]

**Explicit cue systems in shipped games**
- Kingdom Come: a green shield icon marks the perfect-block moment — [KCD wiki.gg](https://kingdomcomedeliverance.wiki.gg/wiki/Combat) [fetched][fan]
- Mirror's Edge (first person): an enemy's weapon turns red during his melee swing; pressing the disarm button at that moment takes the weapon — [Mirror's Edge Fandom wiki, Disarming](https://mirrorsedge.fandom.com/wiki/Disarming) [excerpt][fan]
- Critique of that cue: Tom Francis called the red-flash disarm "preposterous"—"a terrible challenge, relying either on using slow-mo so slow that the wait becomes boring, or learning the animations by rote"; he proposed that disarms always succeed but take longer against a firing enemy and are quick against a staggered one — [Tom Francis, pentadact.com, 2009](https://www.pentadact.com/2009-01-25-the-combat-in-mirrors-edge-and-why-it-fucking-sucks/) [fetched]
- Sleeping Dogs: an enemy flashes red when a counter is possible; counters work even mid-punch — [Giant Bomb forum tips](https://www.giantbomb.com/sleeping-dogs/3030-29441/forums/combat-help-tips-and-tricks-572789/); [GamersHeroes guide](https://www.gamersheroes.com/game-guides/sleeping-dogs-basic-combat-guide/) [excerpt][fan]
- Dying Light 2: a red marker around a human enemy signals a power attack that cannot be parried, so the player must dodge — [Steam discussion](https://steamcommunity.com/app/534380/discussions/0/3186862118584720995/); [GameFAQs board](https://gamefaqs.gamespot.com/boards/288759-dying-light-2-stay-human/79889466) [excerpt][fan]
- God of War (2018, close over-the-shoulder camera): an indicator around Kratos points toward incoming attacks; yellow = blockable, red = unblockable; Atreus calls out attacks — [Steam discussion "Off Screen attacks"](https://steamcommunity.com/app/1593500/discussions/0/3202622816429803429/); [PlayStation accessibility page (Ragnarök)](https://www.playstation.com/en-us/games/god-of-war-ragnarok/accessibility/) [excerpt]
- Accessibility guideline: "Ensure that all important supplementary information (eg. the direction you are being shot from) conveyed by audio is replicated in text / visuals" (Intermediate) — [Game Accessibility Guidelines, full list](https://gameaccessibilityguidelines.com/full-list/) [fetched]
- A separate low-vision review of God of War Ragnarök discusses combat cue visibility — [Game Accessibility Nexus](https://www.gameaccessibilitynexus.com/blog/2022/11/03/low-vision-game-review-god-of-war-ragnarok/) [not opened]

**Limiting simultaneous attackers**
- Kingdoms of Amalur: Reckoning's "Kung-Fu Circle" gives each attacker a weighted cost against a capacity around the player, so only a limited number attack at once and others wait in approach positions; the stated problem: encounters "felt overwhelmingly chaotic when too many enemies attacked from all angles simultaneously" — [Game AI Pro, ch. 28 "Beyond the Kung-Fu Circle" (PDF)](http://www.gameaipro.com/GameAIPro/GameAIPro_Chapter28_Beyond_the_Kung-Fu_Circle_A_Flexible_System_for_Managing_NPC_Attacks.pdf) [fetched; author not shown in fetched text — commonly attributed to Kevin Dill, unverified here]
- DOOM (2016) uses attack tokens: an AI must obtain a token before attacking; token count caps simultaneous attackers; AI can steal tokens — [Sam Bloomberg, "Realtime turn-based AI"](https://xbloom.io/2021/03/19/designing-a-combat-system/) [excerpt]

**Enemy behavior that tests readability**
- Condemned enemies feint (start an attack, stop, then strike after the player blocks early), retreat into darkness and ambush, and attack in groups — [TV Tropes](https://tvtropes.org/pmwiki/pmwiki.php/VideoGame/CondemnedCriminalOrigins); [Wikipedia](https://en.wikipedia.org/wiki/Condemned:_Criminal_Origins) [excerpt; blended search summary, exact attribution unverified]

### Inferences
- In first person, the player's own arms and weapon hide the lower screen, so enemy wind-ups should be high and wide (overhead, big side swing) and stay near screen center, following Arkane's exaggeration approach.
- A color flash or icon at the perfect-block moment is common (KCD, Mirror's Edge, Sleeping Dogs, Dying Light 2), but Tom Francis's critique shows a too-short window reads as unfair. A generous window plus a sound cue (grunt or whoosh before the swing) likely fits the project's "player always understands" rule better than a frame-tight window. (Proposed.)
- For the first case (one inmate in a cell), off-screen indicators are unnecessary; they become important when gang fights arrive. An attack-token cap is the standard answer for group fights and is cheap to design in early.
- Enemy feints (Condemned) add depth but directly reduce readability; for a first fight they may be better held back. (Proposed.)
- Arkane's regret suggests fixing the FOV and building a combat test room before building levels around combat.

### Gaps
- No published hitbox/wind-up timings found for any first-person melee game specifically.
- Sifu's readability design (GDC talk or developer interview) was not found.
- God of War indicator details come from player discussions, not from Santa Monica Studio.

## 3. How is armed vs unarmed asymmetry handled (does a weapon make fights one-sided?), plus durability and throwing?

### Takeaway
The references keep armed fights two-sided mainly through **enemy access to weapons** (Condemned enemies grab pipes; Riddick/Zeno Clash/Mirror's Edge let either side disarm the other), **weapon trade-offs** (Condemned damage/speed/block/reach stats; Condemned 2 fists are faster but weaker), and **durability** (Dying Light, Escapists-style breakage). A weapon usually wins fights faster, but the defender still blocks and hits back.

### Cited Findings
- Condemned: every weapon trades damage, speed, block and reach — [GameFAQs guide](https://gamefaqs.gamespot.com/pc/926310-condemned-criminal-origins/faqs/49952) [excerpt][fan]
- Condemned: unarmed enemies look around "frantically" for a weapon, rip pipes from walls or pick items off the ground — [Giant Bomb](https://giantbomb.com/wiki/Games/Condemned_Criminal_Origins); [Wikipedia](https://en.wikipedia.org/wiki/Condemned:_Criminal_Origins) [excerpt]
- Condemned 2: fists have less damage and reach but are faster and chain combos — [Wikipedia](https://en.wikipedia.org/wiki/Condemned_2:_Bloodshot) [excerpt]
- Riddick: a timed input against an armed enemy grabs his weapon and turns it on him — [Wikipedia](https://en.wikipedia.org/wiki/The_Chronicles_of_Riddick:_Escape_from_Butcher_Bay) [excerpt]
- Zeno Clash: weapons are obtained partly by disarming enemies; stunned enemies can be grabbed and thrown — [GameFAQs guide](https://gamefaqs.gamespot.com/xbox360/975437-zeno-clash-ultimate-edition/faqs/59819) [excerpt][fan]
- Mirror's Edge: disarm grabs the weapon; a second press throws it away — [Mirror's Edge Fandom wiki](https://mirrorsedge.fandom.com/wiki/Disarming) [excerpt][fan]
- Dying Light: each hit degrades a weapon; each weapon can be repaired only a set number of times — [Dying Light Fandom wiki, Weapons](https://dyinglight.fandom.com/wiki/Weapons_(Dying_Light)) [excerpt][fan]; Dying Light 2 weapons degrade and are meant to be discarded and replaced; each mod restores ~50 durability — [GamesRadar+](https://www.gamesradar.com/dying-light-2-repair-weapons/) [excerpt]; Dying Light: The Beast common weapons can be repaired three times — [games.gg guide](https://games.gg/dying-light-the-beast/guides/dying-light-the-beast-weapon-durability/) [excerpt][fan]
- Dying Light 2's developers once shipped a patch and reverted it about three hours later after backlash (the article is in the durability search results; the specific change was not verified) — [TechRadar](https://www.techradar.com/news/dying-light-2-devs-launch-update-backtrack-three-hours-later) [excerpt, not opened]
- Schedule I: melee weapons do not attract police attention as easily as guns — [TheGamer](https://www.thegamer.com/schedule-1-all-weapons-baseball-bat-revolver-fight-npcs-how-to-guide/) [excerpt][fan]
- Party Animals: pick-up weapons like a two-handed shovel "smacks other players away with speed and has a great chance of landing knockouts" — [TheLoadout](https://www.theloadout.com/party-animals/weapons-best) [excerpt][fan]
- Kingdom Come: blunt weapons beat armor; slashing and stabbing beat light armor — [KCD wiki.gg](https://kingdomcomedeliverance.wiki.gg/wiki/Combat) [fetched][fan]

### Inferences
- The cheapest way to keep the neighbor-cell fight two-sided when the player grabs the metal rod is to let the inmate grab an object too (Condemned) or attempt a disarm (Riddick). This also makes the inmate feel like a person rather than a punching bag.
- A book and a metal rod naturally map to Condemned-style stats: book = fast, low damage, breaks quickly, maybe better blocking; rod = slower, more damage and reach, durable. Durability could be a small hit count rather than a repair economy for release 1. (Proposed.)
- Throwing an object as a one-time stun or distraction (the book) is a plausible cheap verb; the references support throwing people (Zeno Clash) and discarding weapons (Mirror's Edge) more than a thrown-object combat system.
- Schedule I's "melee draws less police attention than guns" pattern could map to guard response tiers later (fists < improvised weapon < shiv). (Proposed; ties into question 4.)

### Gaps
- No primary developer rationale found for durability in Dying Light or Condemned; findings are mechanics only.
- The Escapists weapon durability and breakage numbers were not verified.
- Thrown-object damage/stun rules in first-person games (e.g., Half-Life 2, Prey) were not researched.

## 4. Lethal vs non-lethal outcomes and consequences (knockouts, surrender, fleeing, finishers, death; guards, reputation, injuries, lockdown)

### Takeaway
Prison and sandbox references mostly default to **knockout, not death**, then attach consequences through a **visible suspicion meter** (Escapists "heat", Bully "trouble meter") and **witnesses** (Kingdom Come). Games that allow killing make it a **distinct deliberate act** (finishers, environmental kills, refusing surrender) and track it in world state (Dishonored chaos).

### Cited Findings
**Knockout and recovery**
- The Escapists 2: at 0 health the player is knocked out, taken to the infirmary, and loses carried contraband — [Escapists Fandom wiki, Combat System](https://theescapists.fandom.com/wiki/Combat_System_(The_Escapists_2)) [excerpt][fan]
- Party Animals: after enough damage a player is temporarily knocked out and recovers unless thrown off the map or into hazards; mashing buttons shortens wake-up — [Twinfinite](https://twinfinite.net/guides/how-to-wake-up-faster-party-animals/); [TheGamer](https://www.thegamer.com/party-animals-how-to-play-guide/) [excerpt][fan]
- Thief: the blackjack knocks out only unaware targets; Garrett raises the blackjack overhead as a ready state before the knockout — [Thief Fandom wiki, Knockout](https://thief.fandom.com/wiki/Knockout) [excerpt][fan]
- Schedule I: weapons can knock out police during a chase so they cannot cuff you — [TheGamer](https://www.thegamer.com/schedule-1-all-weapons-baseball-bat-revolver-fight-npcs-how-to-guide/) [excerpt][fan]
- Bully: fights end when one fighter is knocked out or runs away — [Bully Fandom wiki](https://bully.fandom.com/wiki/Fighting) [excerpt][fan]

**Surrender and fleeing**
- Kingdom Come: opponents can yield; the player can let them go, refuse and finish the fight, or accept surrender for money or equipment; the player can also surrender within a few metres, but not to bandits or Cumans — [KCD wiki.gg](https://kingdomcomedeliverance.wiki.gg/wiki/Combat) [fetched][fan]; [KCD Fandom archive, Surrender](https://kingdomcomedeliverance-archive.fandom.com/wiki/Surrender) [excerpt][fan]
- Condemned: enemies may run into darkness and ambush later — [TV Tropes](https://tvtropes.org/pmwiki/pmwiki.php/VideoGame/CondemnedCriminalOrigins) [excerpt][fan]

**Finishers and killing**
- Condemned 2: finishers are quick-time events; environmental finishers always kill — [Wikipedia](https://en.wikipedia.org/wiki/Condemned_2:_Bloodshot) [excerpt]
- Sleeping Dogs: grab an enemy, drag him to an object glowing red, press the prompt for an environmental attack that eliminates him regardless of health — [beforeiplay wiki](https://beforeiplay.com/wiki/Sleeping_Dogs); [GamersHeroes](https://www.gamersheroes.com/game-guides/sleeping-dogs-basic-combat-guide/) [excerpt][fan]
- Kingdom Come: wounds cause bleeding that can kill — [KCD wiki.gg](https://kingdomcomedeliverance.wiki.gg/wiki/Combat) [fetched][fan]

**Consequences in prison/sandbox games**
- The Escapists 2: knocking out a guard sets heat to 99 almost always (and -30 relationship with that guard); at high heat, guards attack on sight and snipers shoot outdoors — [Escapists Fandom wiki, Stats/Heat](https://theescapists.fandom.com/wiki/Stats#Heat); [Steam discussion](https://steamcommunity.com/app/641990/discussions/0/1639787494964523499/) [excerpt][fan]
- Bully: fighting ordinary male students raises the trouble meter gradually based on fight duration; attacking little kids, girls, or adults maxes it; authority figures grab Jimmy, who can escape by mashing within 10 seconds unless the meter is red; humiliating a low-health opponent is its own "Bullying" violation — [Bully Fandom wiki, Fighting](https://bully.fandom.com/wiki/Fighting); [List of rule violations](https://bully.fandom.com/wiki/List_of_rule_violations); [Busted](https://bully.fandom.com/wiki/Busted) [excerpt][fan]
- Kingdom Come: assault is a crime; if caught, pay a fine, go to jail, or flee; criminals lose reputation; release from jail gives a debuff lowering Strength, Agility, Vitality; witnesses remember you — [KCD wiki.gg, Crime](https://kingdomcomedeliverance.wiki.gg/wiki/Crime) [excerpt][fan]; KCD2 witnesses can extort the player for silence — [Game Rant](https://gamerant.com/kingdom-come-deliverance-2-crime-punishment-reputation-system/) [excerpt]
- Dishonored: the whole game can be finished without killing; killing raises "Chaos," which makes dialogue, cutscenes, and security darker and more dangerous; Dishonored 2 has three chaos states — [Wikipedia, Dishonored](https://en.wikipedia.org/wiki/Dishonored); [Wikipedia, Dishonored 2](https://en.wikipedia.org/wiki/Dishonored_2) [excerpt]
- Colantonio on Chaos: "there are still consequences to the state of the world... otherwise why does it matter? ... because it is harder to be good. That's the point." — [DualShockers interview](https://www.dualshockers.com/making-of-dishonored-harvey-smith-raph-colantonio/) [excerpt]
- Dishonored non-lethal options are chokes and sleep darts, not open-combat takedowns (player report) — [Steam discussion](https://steamcommunity.com/app/205100/discussions/0/523890046872346484/) [excerpt][fan]

### Inferences
- A two-stage outcome (health 0 = downed/knocked out; killing requires a separate deliberate action on a downed opponent) fits both the agreed "player kills the neighbor" first case and the rule that the player always understands what is happening. It also keeps non-lethal outcomes available later without redesign. (Proposed, not decided.)
- A single visible suspicion value with clear thresholds (Escapists heat, Bully trouble meter) is the most readable consequence model found; witnesses (KCD) make it spatial and fairer. Per the project's co-op rule, suspicion is likely per-player while lockdown is shared world state. (Proposed.)
- Letting a beaten inmate yield or flee (KCD, Condemned) makes the kill a choice and gives the fight a readable ending state.

### Gaps
- No prison-specific game found that handles lethal inmate-on-inmate killing with consequences in first person (Riddick's consequences were not researched).
- No data on how players respond to lethal vs non-lethal defaults in cute/stylized games.

## 5. Game-feel techniques for melee impact, and why first-person melee feels floaty

### Takeaway
Impact comes from **hitstop (brief freeze on contact, scaled by strength), camera kick/shake, distinct target hit reactions and knockback, layered impact sound, and short input latency**. First-person melee feels floaty when damage is only a number: swings pass through targets, every hit gets the same generic reaction, and distance is hard to judge.

### Cited Findings
**Hitstop**
- Sakurai (Smash Bros.): "When you strike the opponent, both parties momentarily freeze, emphasizing the power of impact"; "The more damage an attack inflicts, the longer the hitstop period," with per-attack modifiers (Marth's sword tip gets extra); the attacker also vibrates slightly; hurtboxes stay in place while the model shakes; grounded characters shake side to side; there is a hard cap — [Source Gaming translation of Sakurai's Famitsu column vol. 490](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/) [fetched]
- Sakurai: in multiplayer, "when you and the opponent are frozen in hitstop, that creates a chance for a third player to move in and strike," which limits how long hitstop can be — [Source Gaming](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/) [fetched]
- Pausing the arc at the collision point "helps sell that the collision actually happened, gives the eyes a few frames to register and confirm it" — [Celia Wagar, CritPoints](https://critpoints.net/2017/05/17/hitstophitfreezehitlaghitpausehitshit/) [excerpt]
- Hit stop is "maybe the best researched phenomenon in the area of impact feedback visualisations" — [Designing Game Feel: A Survey (arXiv)](https://arxiv.org/pdf/2011.09201) [excerpt]

**Screenshake, kick, knockback**
- Jan Willem Nijman (Vlambeer), "The Art of Screenshake" (INDIGO 2013, 30 tips): "sleep" (pause 1–2 frames when enemies die, the player is hit, things explode), screen shake of "a pixel or two", recoil/kickback on the attacker, knockback on both player and enemies when hit — [Blue Tengu write-up](https://www.bluetengu.com/2014/12/12/art-of-screenshake-experiments/) [fetched]; video: [YouTube](https://www.youtube.com/watch?v=AJdEqssNZ-U) [not watched]
- Jonasson and Purho, "Juice It or Lose It" (GDC Europe 2012): live demo adding cascading feedback to a plain Breakout clone — [GDC Vault](https://www.gdcvault.com/play/1016487/juice-it-or-lose); [Rob Miller summary](https://roblog.co.uk/2024/03/juicy-games/) [excerpt]
- Counterpoint: Folmer Kelly (GDC Europe Independent Games Summit 2014): "There has been such a tremendous focus on putting eye candy in our games... that the context doesn't get enough consideration" — [Game Developer](https://www.gamedeveloper.com/design/video-indies-resist-the-urge-to-juice-it-or-lose-it-) [fetched]

**Hit reactions and physicality**
- Dying Light 2 builds reactions from weapon, direction, body part, enemy stamina and player direction; ragdoll is used only for some reactions — [3DVF interview](https://3dvf.com/en/redaction/dying-light-2-stay-human-our-interview-with-techland-about-the-gut-feeling-update-focused-on-combat-animations-and-physics/) [fetched]
- Tom Francis on Mirror's Edge: "Nothing is physical, everything is the result of abstract rules"; every attack triggers the same fake stagger regardless of context, undermining flying kicks; kicks should knock enemies down according to momentum — [pentadact.com](https://www.pentadact.com/2009-01-25-the-combat-in-mirrors-edge-and-why-it-fucking-sucks/) [fetched]
- Dark Messiah: damage feedback and distance judgment were core first-person problems; a larger hit zone made near-misses count — [Game Developer GDC 2007 report](https://www.gamedeveloper.com/game-platforms/gdc-arkane-s-colantonio-takes-on-first-person-melee) [fetched]
- Zeno Clash: "a certain degree of disorientation is required to achieve immersion"; Mirror's Edge cited as coherent camera motion in action — [bit-tech](https://bit-tech.net/reviews/gaming/pc/zeno-clash-interview-into-the-unknown/3/) [fetched]

**Why first-person melee feels floaty**
- "Each swing of a sword or an axe generally feels like it's phasing right through an enemy, leaving any significant damage dealt left up to the imagination"; cause: RPGs prioritize stat impact over felt impact; Elder Scrolls, Fallout and Prey named; Avowed's fix cited as "noticeable hitstop each time a weapon lands a blow" plus impact sound and stamina pressure (opinion article, no developer quotes) — [Josh Cotts, Game Rant, Feb 2025](https://gamerant.com/avowed-melee-combat-first-person-rpg-weight-responsive-impact/) [fetched]
- Input lag above ~160 ms feels sluggish; under 100 ms feels tight — [GDKeys](https://gdkeys.com/keys-to-combat-design-1-anatomy-of-an-attack/) [fetched]

**Comfort and accessibility in first person**
- "Avoid (or provide option to disable) any difference between controller movement and camera movement" (Intermediate); allow FOV adjustment (Intermediate); set an appropriate default FOV (Basic); avoid flickering images (Basic) — [Game Accessibility Guidelines full list](https://gameaccessibilityguidelines.com/full-list/) [fetched]

### Inferences
- Minimum impact kit for a first fight: short hitstop on both fighters scaled by hit strength (fists shorter, rod longer), a small camera kick in the hit direction, target flinch chosen by hit direction plus knockback, a layered impact sound (swing whoosh + body thud + vocal grunt), and an on-hit particle. Screen shake must be optional because camera motion not driven by the player is an accessibility item.
- For release 2 co-op, Sakurai's third-player point matters: a global time freeze does not work with more than one player. Hitstop should freeze only the attacker's and target's animations (and the attacker's camera), not global time. Keeping hitstop as a per-fighter presentation effect separate from combat rules matches the project's co-op separation rule. (Inference.)
- Tom Francis's and Techland's comments together suggest two failure modes: too little reaction (same stagger every time) and too much (every hit overpowering). Tuning should vary reactions by hit type rather than simply increasing intensity.
- Decide FOV and viewmodel framing early; Arkane had to redo all animations after widening FOV.

### Gaps
- No published frame counts for hitstop in any first-person melee game were found.
- "Juice It or Lose It" and "Art of Screenshake" content is from summaries, not the videos.
- No Monolith or Techland GDC talk specifically on melee feel was found (searches for a Dying Light GDC melee talk returned only hands-on previews).

## 6. How do comparable stylized or cozy-toned games present violence?

### Takeaway
Stylized references keep violence **non-realistic and usually non-lethal**: knockouts with quick recovery (Party Animals), comedic humiliation (Bully), infirmary instead of death (The Escapists 2), and Schedule I explicitly describes its combat as "non-realistic violence." Lethal first-person references (Condemned, Dying Light, KCD) are realistic and gory. No reference combines cute characters with deliberate killing of a named person, so the first combat case has no close precedent.

### Cited Findings
- Schedule I's Steam mature-content description: "It also contains non-realistic violence in the form of melee and firearm combat" (TVGS, released March 24, 2025) — [Steam store page](https://store.steampowered.com/app/3164500/Schedule_I/) [fetched]
- ESRB definitions: Fantasy Violence = "Violent actions of a fantasy nature, involving human or non-human characters in situations easily distinguishable from real life"; Violence = "Scenes involving aggressive conflict. May contain bloodless dismemberment"; Intense Violence = "Graphic and realistic-looking depictions of physical conflict. May involve extreme and/or realistic blood, gore, weapons and depictions of human injury and death"; Blood = "Depictions of blood" — [ESRB ratings guide](https://www.esrb.org/ratings-guide/) [fetched]
- Party Animals: cute animal characters are temporarily knocked out and recover unless thrown off the map or into hazards — [TheGamer](https://www.thegamer.com/party-animals-how-to-play-guide/); [Wikipedia](https://en.wikipedia.org/wiki/Party_Animals_(video_game)) [excerpt]
- Bully: fights end in knockout or fleeing; finishers are humiliations (spit in the face) rather than injuries — [Bully Fandom wiki](https://bully.fandom.com/wiki/Fighting) [excerpt][fan]
- The Escapists 2: player knockout leads to the infirmary, not death — [Escapists Fandom wiki](https://theescapists.fandom.com/wiki/Combat_System_(The_Escapists_2)) [excerpt][fan]
- Condemned: melee designed so "You must be able to see his eyes, touch his skin, and hear his breathing"; "Combat in an action game should complement the atmosphere and feel of the game being created" — [Game Developer](https://www.gamedeveloper.com/design/design-lesson-101---i-condemned-criminal-origins-i-) [fetched]
- Dying Light 2 uses dismemberment via per-archetype hierarchical cutting zones (realistic gore end of the spectrum) — [3DVF](https://3dvf.com/en/redaction/dying-light-2-stay-human-our-interview-with-techland-about-the-gut-feeling-update-focused-on-combat-animations-and-physics/) [fetched]
- Lethal Company (cute-but-eerie first-person co-op) uses a simple wind-up shovel swing with a brief stun — [Lethal Company Fandom wiki](https://lethal-company.fandom.com/wiki/Shovel) [excerpt][fan]

### Inferences
- Heir's Condemned lesson (combat should match the game's atmosphere) applies directly: for an "eerily cozy" tone, the eerie part may come from restraint (sound dropping out, the body staying in the cell, other inmates reacting) rather than gore. (Proposed.)
- The ESRB wording suggests stylized, bloodless fights with cute characters would likely sit under "Fantasy Violence" or "Violence," while realistic blood and death trend toward "Intense Violence." This is an inference from definitions, not a rating prediction.
- Because the first combat case ends in a deliberate kill, the tone contrast between cute characters and killing is a design question for the user, not something the references settle. Options range from comedic KO-then-death (Party Animals style body) to a quiet, uncomfortable moment. (Open.)

### Gaps
- No developer commentary found on presenting lethal violence with cute characters specifically.
- Gang Beasts, Totally Accurate Battle Simulator and similar comedic physics brawlers were not researched individually.
- Steam's own content survey categories were not researched.
