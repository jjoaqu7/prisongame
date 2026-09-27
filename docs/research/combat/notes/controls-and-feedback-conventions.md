# Common conventions for grab, kill and throw keys, timed blocks and damage feedback

Topic: combat · Date: 2026-09-27 · Serves: ENCOUNTER-01 steps 4 and 6b (RESEARCH-01 continued)
Searched: web search across game manuals, key-binding references, fan wikis, forums and design write-ups; 13 searches, 3 pages read in full.
Assumptions: the user asked on September 27 to "use standardized or most common option" for the open grab and throw questions and for step 4, so each question asks what most games do, not what is best in theory.

## 1. One key both grabs and kills a downed body: tap or hold?

**Takeaway:** Holding a key is the common way to confirm a deliberate or irreversible action, and a short press is used for quick, frequent actions. Apex Legends, a widely played first-person game, finishes a knocked-down enemy by holding E. So "tap E grabs, hold E kills" follows the common pattern.

**Findings:**
- Apex Legends: "find an opponent that's knocked down, face them and hold the E key for a few seconds" to perform a finisher ([GameWatcher](https://www.gamewatcher.com/news/apex-legends-finishers-execute-how-to), Secondary, read in full).
- Designers use a timed hold "for important actions you don't want to perform by accidentally misclicking" ([UX Collective, Zlobin](https://uxdesign.cc/a-game-sound-designers-guide-to-button-interactions-6837dd5cc977), Community, Excerpt).
- Unity's Input System has built-in Tap and Hold interactions that tell a short press from a long one on the same binding ([Unity Input System manual, Interactions](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.4/manual/Interactions.html), Official, package 1.4 manual; the interactions still exist in current versions).
- Hitman games use a context prompt for dragging bodies. Hitman: Absolution picks a body up with B and drops it with E ([Feral manual](https://support.feralinteractive.com/docs/en/hitmanabsolution/latest/steam/manual), Official, Excerpt). That is a separate drag key, not a tap/hold split, so it does not contradict the pattern.

**Inferences:**
- Proposed: tap E grabs, holding E for the agreed 1.5 s kills, and on a dead body E only grabs. The kill shows progress while held, and releasing early cancels it.

**Gaps:** no source compares tap-to-grab with hold-to-kill on the same body.

## 2. Which key throws a held weapon, when LMB attacks with it?

**Takeaway:** G is the common PC throw key. Chivalry 2, a first-person melee game where weapons can be thrown, uses G for "Throw", and G is the usual grenade key in PC shooters. Dying Light throws a melee weapon by aiming with RMB and clicking LMB, which would clash with this game's RMB block.

**Findings:**
- Chivalry 2 default keyboard controls list "Throw: G" ([DefKey](https://defkey.com/chivalry-2-shortcuts), Community, read in full).
- Many PC players and games use G for grenades ([ResetEra thread](https://www.resetera.com/threads/pc-gamers-whats-your-throw-grenade-key.108444/), Community, Excerpt).
- Dying Light: middle mouse throws throwables, and a melee weapon is thrown by holding RMB to aim and clicking LMB ([Steam discussion](https://steamcommunity.com/app/239140/discussions/0/517142892066773137), Community, Excerpt).
- Half-Life 2 picks objects up with E and throws a held object with the left mouse button ([Steam discussion](https://steamcommunity.com/app/290930/discussions/0/1735506744738063195/), Community, Excerpt). This is the already agreed LMB throw for non-weapon objects.

**Inferences:**
- Proposed: G throws whatever is held, weapons included. LMB still throws non-weapon objects and bodies, as already agreed. A held weapon is thrown only with G, because LMB attacks with it.

**Gaps:** Mordhau's default throw key could not be read (the key-binding page shows it only as images).

## 3. Throw strength, and whether thrown objects hurt

**Takeaway:** No common number exists. Engines expose it as a tuning value: Half-Life 2 has a `player_throwforce` setting defaulting to 1000 (a force scaled by object mass, not a speed). Half-Life 2's designers set "collisions should cause damage" as a principle, and a cut Half-Life 2 throwing weapon did damage only above a speed threshold. So the common pattern is that a fast-moving thrown object hurts, and a slow one does not.

**Findings:**
- `player_throwforce 1000` changes the force with which the player throws physics objects in Half-Life 2 ([Steam guide, console commands](https://steamcommunity.com/sharedfiles/filedetails/?id=3087273607), Community, Excerpt).
- Valve's physics principles for Half-Life 2 included "collisions should cause damage, objects should be used as cover" ([Slashdot, GDC report](https://slashdot.org/story/06/03/24/1926234/gdc---physics-in-half-life-2), Secondary, Excerpt).
- Objects thrown with Half-Life 2's cut Brickbat "will do a certain amount of damage only after they have reached a certain velocity" ([The Cutting Room Floor](https://tcrf.net/Proto:Half-Life_2_(Windows)/September_26th_2003_Build/Cut_Weapons), Fan, Excerpt).
- Unity throw scripts commonly set the thrown Rigidbody's velocity to the camera's forward direction times a tunable speed ([GameDev.net thread](https://www.gamedev.net/forums/topic/684678-pickup-and-throw-script-unity/), Community, Excerpt).

**Inferences:**
- Proposed: keep the September 27 starting values (objects about 8 m/s, bodies about 4 m/s) as tuning values. A thrown object deals damage only when it hits above a speed threshold. The book and rod land a light hit (a few damage and a stagger). Light items such as snack packs do no damage. Tuned in the step 6b playtest.

**Gaps:** no published first-person throw speed was found.

## 4. How long is a timed-block window, and how is spamming handled?

**Takeaway:** Sekiro, the best-known timed-block (deflect) game, uses 0.2 s (12 frames at 60 fps) before the hit. Its window shrinks when guard is pressed repeatedly, so spamming the block button stops working. The agreed 0.25 s is slightly more generous than Sekiro, which fits the research's case for a forgiving window.

**Findings:**
- Sekiro's deflect window is 12 frames by default, about 0.2 s before the attack hits, and it shrinks if guard was recently released, down to 4 frames or none if pressed too quickly ([Fextralife Sekiro wiki, Deflection](https://sekiroshadowsdietwice.wiki.fextralife.com/Deflection), Fan, Excerpt).
- Elden Ring's parry is considered much tighter and less readable than Sekiro's ([Steam discussions](https://steamcommunity.com/app/1245620/discussions/0/4417479689920001975), Community, Excerpt).

**Inferences:**
- Proposed: keep the agreed 0.25 s window. Add the common spam guard in its simplest form: raising the block again less than 0.4 s after lowering it gives a normal block, not a timed one. This is a tuning value.

**Gaps:** the exact shrink formula in Sekiro is from a fan wiki.

## 5. How do first-person games show that the player was hit?

**Takeaway:** A brief red flash or red screen edge is the most common signal: Doom flashes the screen red, and Battlefield greys and darkens the screen edges as damage builds. Directional indicators show where off-screen damage came from, which matters less in a one-on-one fight in front of the player.

**Findings:**
- Doom flashes the screen red on damage. Battlefield shifts toward greyscale with a growing edge vignette (darkened screen edges). Games combine a centre indicator with screen-edge indicators for direction ([Stephenson, UX analysis of FPS damage indicators](https://medium.com/@jasper.stephenson/a-ux-analysis-of-first-person-shooter-damage-indicators-59ac9d41caf8), Community, Excerpt).
- A pulsing red border for critical health and a brief blur after heavy hits are common additions ([Focal Crafters, vignette in games](https://focalcrafters.com/what-is-vignette-in-games/), Community, Excerpt).

**Inferences:**
- Proposed: a short, faint red screen-edge flash when the player takes damage, weaker for a blocked hit, with a small camera kick that the existing camera-shake setting turns off. No directional indicator for this one-on-one fight. The agreed fight-only health bar stays the main readout.

**Gaps:** none that affect step 4.
