# Production pipeline for first-person melee combat (solo developer, Unity 6.3 URP, Steam)

Research date: 2026-09-25. Pages were read on that date through a fetch tool that summarizes them, or through search-result summaries (marked "search summary"). Licensing and platform policies change often, so re-check each linked page before relying on it. This is informational, not legal advice. Items marked "confirm" need official confirmation or a lawyer.

## Key Question 1: How to develop melee combat iteratively (order, feel before content, data-driven tuning, debug visualization, recorded test scenarios, playtesting)

### Takeaway
The most relevant postmortem, Arkane's 2007 GDC talk on first-person melee in Dark Messiah, points to this order: a long prototype phase in a combat sandbox, responsiveness first, anticipation animations and readable enemy tells, a limit on how many enemies attack at once, and blind playtests from the start. The same talk says building levels before the combat sandbox led to cramped spaces. Naughty Dog's melee structure (attack data with timeline events, separate from NPC behavior data) fits a data-driven Unity setup. Unity's Input System can record and replay input for repeatable test scenarios.

### Cited Findings
**First-person melee lessons (Dark Messiah, GDC 2007, Raphael Colantonio)**
- The team had "NO PUBLISHER. Long prototype phase," built on existing FPS middleware (Source engine) so they could iterate on mechanics rather than engine problems, with "A very agile squad for designing mechanics." — [GDC 2007 talk text, archive.org](https://archive.org/stream/GDC2007Colantonio/GDC2007-Colantonio_djvu.txt); [GDC Vault listing](https://gdcvault.com/play/557/The-Challenges-of-Designing-First); [slides PDF](https://media.gdcvault.com/gdc07/slides/S3736i1.pdf)
- The talk names "Gauging distance" as a core problem specific to first-person melee. — [archive.org](https://archive.org/stream/GDC2007Colantonio/GDC2007-Colantonio_djvu.txt)
- Manual target lock "Worked, but too clunky." Their fix: "Apply multiple hit zones that follow the sword animation, and apply the first damage only" (sweep hit volumes along the attack animation and count only the first hit). — [archive.org](https://archive.org/stream/GDC2007Colantonio/GDC2007-Colantonio_djvu.txt)
- They used separate "Third person" and "First person" models, "compensating the vertical pitch + blend on hit." — [archive.org](https://archive.org/stream/GDC2007Colantonio/GDC2007-Colantonio_djvu.txt)
- Enemies crowded around the player at close range, which "is a problem in first person POV." The fix was a "token system. No more than 2 enemies in contact within the same Squad." — [archive.org](https://archive.org/stream/GDC2007Colantonio/GDC2007-Colantonio_djvu.txt)
- "Anticipation animations: A key element for [first] person melee combat." They "had to redo a fair amount of work." — [archive.org](https://archive.org/stream/GDC2007Colantonio/GDC2007-Colantonio_djvu.txt)
- On blind playtesting: "The most convincing tool... It's cheap, and way more effective than curves analysis... I wish we had started it earlier." Players "hated the slow combat. First person calls for highly responsive controls." — [archive.org](https://archive.org/stream/GDC2007Colantonio/GDC2007-Colantonio_djvu.txt)
- Dropped experiments: "'Ragdoll hits' perf hit, instead we made tons of animations" and "Damage map... totally useless." Lesson on order: "The level Design was done before the combat Sandbox. -> too small environments." — [archive.org](https://archive.org/stream/GDC2007Colantonio/GDC2007-Colantonio_djvu.txt)

**Data structure for melee (The Last of Us Part II, GDC 2021, Allen Chou; article dated 2021-07-21)**
- A melee attack is defined by the animations for attacker and target, start and end conditions (range, facing, line of motion), and timeline events that mark hit frames, target tracking, and invincibility windows. — [Game Developer](https://www.gamedeveloper.com/design/how-naughty-dog-defined-melee-attacks-and-behaviors-in-i-the-last-of-us-part-ii-i-)
- A melee behavior (the NPC side) is defined by the attacks available in each state, where to move (close in or keep distance), movement type, and conditions for entering and leaving the state. — [Game Developer](https://www.gamedeveloper.com/design/how-naughty-dog-defined-melee-attacks-and-behaviors-in-i-the-last-of-us-part-ii-i-)
- They lengthened attack tells, made them more obvious, and added dodges so skilled players can avoid damage. — [Game Developer](https://www.gamedeveloper.com/design/how-naughty-dog-defined-melee-attacks-and-behaviors-in-i-the-last-of-us-part-ii-i-)

**Other GDC talks on prototyping melee (listed, not fully reviewed)**
- "Master of the Katana: Melee Combat in Ghost of Tsushima" (Chris Zimmerman, Sucker Punch, GDC 2021) covers prototyping, iteration, and playtesting. — [Class Central listing](https://www.classcentral.com/course/youtube-master-of-the-katana-melee-combat-in-ghost-of-tsushima-157953)
- "Unsynced: The Last of Us Melee System" — [GDC Vault](https://gdcvault.com/play/1020368/Unsynced-The-Last-of-Us); "Designing and Implementing a Satisfying Sword Fight" (Michael Chang: pacing, camera, rigging, animating) — [GDC Vault](https://gdcvault.com/play/1015226/Designing-and-Implementing-a-Satisfying)

**Hitstop (a brief freeze on impact)**
- Hitstop pauses the animation and physics of both attacker and target for a few frames on impact. It adds weight (stronger hits get more) and confirms the hit to both sides. — [Celia Wagar, CritPoints (2017)](https://critpoints.net/2017/05/17/hitstophitfreezehitlaghitpausehitshit/); [Sakurai's Famitsu column, Source Gaming translation (2015)](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/)

**Recording repeatable test scenarios (Unity Input System)**
- `InputEventTrace` records input events for one device or all input into a buffer "for testing purposes or for replaying recorded input." `ReplayController` can replay by original timestamps (`PlayAllEventsAccordingToTimestamps`), one frame at a time, or one event at a time. `InputRecorder` is a component wrapper with an inspector for recording and playback. Traces must be disposed of or they leak native memory. — [InputEventTrace API](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.2/api/UnityEngine.InputSystem.LowLevel.InputEventTrace.html); [ReplayController API (1.10)](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.10/api/UnityEngine.InputSystem.LowLevel.InputEventTrace.ReplayController.html); [InputRecorder API](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.1/api/UnityEngine.InputSystem.InputRecorder.html)

### Inferences
- Suggested order for this project, built from the sources above:
  1. A greybox arena scene with one inmate dummy, not the prison layout. Dark Messiah's regret was building levels before the sandbox.
  2. Player punch, block, and dodge with hit volumes swept along the swing (first hit only), tuned for responsiveness.
  3. Hitstop, camera kick, placeholder sound, and hit reactions.
  4. Enemy tells and one enemy attack, then an attack-token limit before a second enemy is added.
  5. Improvised weapons (book, metal rod) as data variants of the same attack model.
  6. Death and knockdown, which the neighbor-cell kill case needs.
  7. Real animation and audio only once the greybox feels right.
- Data-driven tuning: store each attack as a Unity ScriptableObject (a data asset file editable in the Inspector) holding damage, reach, windup, active and recovery times, hitstop length, and tell duration. This mirrors Naughty Dog's "attack plus timeline events" split and lets AI agents change numbers without touching code. I did not fetch a Unity doc page for this.
- Debug view: draw hit volumes, reach, and attack tokens with Unity Gizmos, and show attack phase as text. No source fetched; this is standard Unity practice.
- Automated checks the agents can run: Play Mode tests that replay recorded `InputEventTrace` files against the dummy and assert hit counts, damage, and state changes. The developer's own playtests then judge feel. Replays drift if timing is not deterministic, so assert on outcomes with tolerances rather than exact frames.
- Co-op readiness (per AGENTS.md): keep attack data and hit resolution in rules code that takes the attacking player as a parameter. First-person arms, camera kick, and hitstop presentation are local, per-player visuals.
- Playtesting: the developer is the only tester now. Dark Messiah's evidence favors adding blind testers (people who have never seen the game, given no instructions) early for combat feel.

### Gaps
- The Ghost of Tsushima, Unsynced, and Michael Chang talks were not reviewed in depth (video or paywalled). Only their listings are cited.
- No fetched Unity documentation for ScriptableObjects, Gizmos, or the Unity Test Framework. Those points are inferences.
- No sourced numbers for typical hitstop length or attack timing windows in first-person melee.

## Key Question 2: Animation sourcing (Mixamo, Asset Store, Cascadeur, video mocap, first-person arms) and fitting animations to a custom rig (Humanoid vs Generic)

### Takeaway
Mixamo is still free and royalty-free for commercial games. But it had outages in 2025, and an Adobe support reply called it "not supported anymore," so download and archive anything you plan to use. Unity Asset Store animation packs are royalty-free for games under the EULA effective 2024-12-04, which also bans using assets as AI or machine-learning training data or inputs. Cascadeur's free plan cannot export FBX and is non-commercial. Indie is about $11/month (billed annually) under $100k revenue, and retargeting is Pro-only. DeepMotion's free tier is non-commercial. Rokoko Vision has a free tier with FBX export. To reuse third-party humanoid clips, the custom NPC rig must be configured as Unity Humanoid (15 required bones, T-pose). Generic clips only transfer between identical skeletons.

### Cited Findings
**Mixamo (Adobe)**
- Mixamo is free with no extra purchase or subscription. Characters and animations are royalty-free for personal, commercial, and non-profit projects, including video games. Credit is not required. You may not redistribute raw character or animation files, for example as engine templates or asset-store packages. — [Adobe Mixamo FAQ](https://helpx.adobe.com/creative-cloud/faq/mixamo-faq.html) (search summary; the page returned HTTP 403 to direct fetch); [Adobe Community thread](https://community.adobe.com/t5/mixamo-discussions/the-license-to-use-mixamo/m-p/13228937)
- Auto-rigger limits: humanoid characters only, with distinguishable head, body, arms, and legs. It may fail on heavily deformed proportions. It needs a neutral or T-pose, no large extra appendages (wings, tails, large hair or clothing), no extra scene objects, and no gaps between parts (for example, no floating heads). It accepts FBX and OBJ, and roughly 10k to 30k polygons is comfortable. — [Adobe Mixamo FAQ](https://helpx.adobe.com/en/creative-cloud/faq/mixamo-faq.html) (search summary); polygon range from [Meshy tutorial](https://www.meshy.ai/tutorials/how-to-use-mixamo-with-meshy) (secondary)
- Availability: users reported a major outage starting 2025-06-16 and another on 2025-09-24. One support worker reportedly told a user "Mixamo is not supported anymore" and that it "May be fixed or May not be fixed." I found no official Adobe shutdown announcement. — [Adobe Community: Mixamo service down](https://community.adobe.com/questions-696/mixamo-service-down-589868); [Adobe Community: down 24 Sept 2025](https://community.adobe.com/questions-696/mixamo-is-down-24-september-2025-589891); [StatusGator](https://statusgator.com/services/adobe-creative-cloud/mixamo)

**Unity Asset Store (Standard EULA, effective 2024-12-04)**
- Permitted: incorporating assets into an application (s.2.2.1(a)), monetizing them within a Licensed Product (s.2.2.1(d)), and modifying them for that purpose (s.2.2.1(e)). Standard (non-Extension) assets may be installed on unlimited computers you own (s.2.3.1); Extension Assets are limited to 2 computers (s.2.3.2). — [Unity Asset Store EULA](https://unity.com/legal/as-terms)
- Prohibited: selling or distributing assets on their own (s.2.2.1.1(b)), pooling purchases with third parties (s.2.2.1.1(a)), and using the Store or Assets "for purposes such as training an artificial intelligence or machine learning model without the express consent," including "gathering, aggregation, extraction, scraping" for "data sets" or "inputs for artificial intelligence" (s.2.2.1.1(g)). "Restricted Assets" carry their own extra terms (s.2.9). — [Unity Asset Store EULA](https://unity.com/legal/as-terms)
- Example unarmed or melee packs (price, rig type, and quality not checked):
  - [Fist Punch and Unarmed Combat Animation Pack](https://assetstore.unity.com/packages/3d/animations/fist-punch-and-unarmed-combat-animation-pack-319622)
  - [ANIMSET: COMBAT BARE FISTS](https://assetstore.unity.com/packages/3d/animations/animset-combat-bare-fists-191923)
  - [Fighting Animset Pro (Kubold)](https://assetstore.unity.com/packages/3d/animations/fighting-animset-pro-64666)
  - [AnimPro Pack Brawler](https://assetstore.unity.com/packages/3d/animations/animpro-pack-brawler-107574)
  - [Human Melee Animations FREE](https://assetstore.unity.com/packages/3d/animations/human-melee-animations-free-165785) (search summary says it supports Mecanim retargeting)
  - [Fighter Pack Bundle FREE](https://assetstore.unity.com/packages/3d/animations/fighter-pack-bundle-free-36286)

**Cascadeur (official plans page)**
- Free: "Only non-commercial use," 300 frames and 120 joints per scene, and "Export to .casc format only. .fbx and .dae are not available." — [Cascadeur plans](https://cascadeur.com/plans)
- Indie: $11/month billed annually or $27/month monthly. "The revenue must be less than $100k per year." Pro: $46/month annually or $69/month monthly, with "Commercial use with no limits." — [Cascadeur plans](https://cascadeur.com/plans)
- All paid plans include inbetweening (auto-generated frames between key poses), physics-based tools, and animation unbaking. Animation retargeting is Pro and Teams only. — [Cascadeur plans](https://cascadeur.com/plans)
- Yearly Indie and Pro licenses give access to perpetual builds, so you can keep using released versions after the subscription ends. — [Cascadeur plans](https://cascadeur.com/plans); [Cascadeur licensing FAQ blog](https://cascadeur.com/blog/general/cascadeurs-new-licensing-structure-comprehensive-faq)
- Price conflict: third-party aggregators list Indie at "€12/user/month" or "$8/month billed annually." The official page shows $11/month annual. Use the official page. — [Capterra](https://www.capterra.com/p/10015822/Cascadeur/) vs [Cascadeur plans](https://cascadeur.com/plans)

**Video-based mocap**
- Rokoko: the Starter plan is free with 30 seconds/month of Vision AI video capture and FBX export. Basic is $10/month annually (600 s/month), Plus $20/month annually (3000 s/month, adds BVH), and Pro $50/month annually. Motion Library animations may be used "in any of your projects, including commercial ones." The pricing page did not spell out commercial rights for Vision capture output. — [Rokoko pricing](https://www.rokoko.com/pricing); [Rokoko Vision](https://www.rokoko.com/products/vision)
- A secondary source says Rokoko text-to-motion and video-to-motion output "can be used commercially." — search summary citing [TATO Studio comparison](https://tato.studio/blog/best-ai-video-to-mocap) (secondary; confirm with Rokoko's terms)
- DeepMotion Animate 3D: the free plan gives 60 seconds/month for "personal, non-commercial use." Paid plans include a commercial license (reported Starter $15/month, Freelancer $48/month, Studio $199/month). — [DeepMotion Animate 3D pricing](https://www.deepmotion.com/pricing-animate3d) (search summary; prices not directly fetched)
- Move AI: described as pay-per-scene, higher-accuracy capture. No license or pricing details were retrieved. — [Film Threat overview](https://filmthreat.com/features/best-ai-motion-capture-tools-for-independent-filmmakers/) (secondary)

**Unity Humanoid vs Generic (Unity 6.3 manual)**
- "A Humanoid model is a specific structure, containing at least 15 bones organized in a way that loosely conforms to an actual human skeleton." "A Generic model is everything else." A Generic rig needs a Root node, and "Copy From Other Avatar" works only when files share "the same bone structure." — [Unity 6.3 Manual: Generic animations](https://docs.unity3d.com/6000.3/Documentation/Manual/GenericAnimations.html)
- Humanoid setup requires a T-pose ("the required pose for the character to be in, in order to make an Avatar"). Unity maps bones automatically by analyzing the rig, and you can assign bones manually if that fails. Descriptive bone names help. — [Unity 6.3 Manual: Configuring the Avatar](https://docs.unity3d.com/6000.3/Documentation/Manual/ConfiguringtheAvatar.html); [Unity 6.3 Manual: Creating models for animation](https://docs.unity3d.com/6000.3/Documentation/Manual/UsingHumanoidChars.html)
- Example chains from Unity: "HIPS - spine - chest - shoulders - arm - forearm - hand", "HIPS - spine - chest - neck - head", and "HIPS - UpLeg - Leg - foot - toe - toe_end". Export as .fbx at correct scale (1 unit = 1 m). — [Unity 6.3 Manual: Creating models for animation](https://docs.unity3d.com/6000.3/Documentation/Manual/UsingHumanoidChars.html)
- The 15 required bones are Hips, Spine, Head, and Left/Right UpperArm, LowerArm, Hand, UpperLeg, LowerLeg, and Foot. Chest, Neck, and Shoulders are recommended but optional. — [AndrewAltimit humanoid-bones reference (GitHub, secondary)](https://github.com/AndrewAltimit/avatar/blob/main/docs/reference/humanoid-bones.md); [Unity Discussions bone list thread](https://discussions.unity.com/t/archive-help-unity-humanoid-avatar-bone-name-structure-list/893820). Unity's own pages above state only "at least 15"; Unity's `HumanTrait` API is the primary check.

**First-person arms rendering and structure**
- Dark Messiah used separate first-person and third-person models. — [GDC 2007 text](https://archive.org/stream/GDC2007Colantonio/GDC2007-Colantonio_djvu.txt)
- In URP, arms or held items can be drawn with Camera Stacking (a second overlay camera with its own field of view) so they don't clip through walls. Unity's official URP tutorial instead uses a custom render pass or Render Objects renderer feature on a dedicated layer, because camera stacking "has many drawbacks." — [Unity Discussions: URP FPS object clipping](https://discussions.unity.com/t/urp-fps-object-camera-clipping-into-walls-official-srp-tutorial/835118)

### Inferences
- First step for this project: check the NPC rig's Rig tab in Import Settings (Animation Type). If it is Generic, try Humanoid. It needs the 15 bones, a T-pose (or Enforce T-Pose), and a clean mapping in the Avatar window. Without Humanoid, no Mixamo or Asset Store body clip will play on it.
- Stylized "cute" proportions (large head, short limbs) retarget but may look off. Fists may miss the target or pass through the head, and short arms reduce reach. Plan to fix contact points with Unity's Animation Rigging package (IK) or author key clips directly on the rig in Blender or Cascadeur. This is based on how Humanoid retargeting works; I found no source testing it on stylized rigs.
- Retargeting inside Cascadeur is Pro-only. Indie users would animate directly on the imported custom rig (feasible, since Cascadeur imports FBX rigs) or rely on Unity Humanoid retargeting at runtime.
- First-person arms: the likely route is a separate arms mesh and rig, either Generic or a trimmed Humanoid. Clips are authored for the camera view (punch, block, grab, swing a book or rod) in Blender or Cascadeur and drawn on a dedicated layer through a URP Render Objects feature. Full-body mocap clips seldom read well from the eye position.
- Mixamo risk: download every candidate clip now (FBX for Unity, "without skin" for animation-only files) and log them in the license register with the download date. The service may disappear without notice.
- AI caution: the Asset Store EULA's ban on assets as "inputs for artificial intelligence" could arguably cover feeding raw asset files into an AI coding agent's context. That is a stretch compared with manipulating assets through Unity Editor APIs, but it is untested. Confirm with Unity or a lawyer if agents will read asset contents directly.
- Steam disclosure: Rokoko or DeepMotion AI-generated motion may count as "Pre-Generated" AI content for Steam's content survey (see Question 4). Confirm with Valve.

### Gaps
- The Adobe Mixamo FAQ and General Terms could not be fetched directly (HTTP 403). The terms come from search summaries and community threads. No official statement on Mixamo's future or an AI-training clause was found.
- Rokoko Vision's commercial terms for capture output, Move AI licensing and pricing, and DeepMotion's exact current prices were not confirmed on primary pages.
- No sourced guide for Humanoid limits: twist and extra bones are not retargeted, finger fidelity, and CPU cost vs Generic. Unity's pages fetched did not cover these.
- No sourced first-person arms animation pipeline or first-person arms packs for Unity 6 URP.
- The individual Asset Store packs were not checked for Humanoid compatibility, price, or Unity 6 support.

## Key Question 3: Combat audio sourcing (commercial-safe sources) and layering/variation for impacts

### Takeaway
Commercial-safe sources that fit the existing CC0 license register include Kenney's CC0 "Impact Sounds" pack (130 files) and the annual free Sonniss GDC Game Audio Bundle (royalty-free, no attribution, but no AI/ML training and no selling the sounds as-is). Build each punch from layers (movement whoosh, contact transient, low thud, cloth/skin, vocal effort), then vary it. Unity 6's Audio Random Container (Unity 2023.2 and later) randomizes clip choice, pitch (in cents), and volume (in dB) without code.

### Cited Findings
- Kenney "Impact Sounds": 130 files, license "Creative Commons CC0." — [Kenney](https://kenney.nl/assets/impact-sounds)
- Sonniss #GameAudioGDC bundle: worldwide, non-exclusive, royalty-free license for personal and commercial projects in all media, with no attribution required. You may modify the sounds but may not claim authorship or sell them individually. Use for AI/ML training is prohibited. The 2026 bundle is listed. — [Sonniss license](https://sonniss.com/gdc-bundle-license/); [GDC 2026 bundle](https://gdc.sonniss.com/); [Sonniss archive](https://sonniss.com/gameaudiogdc/) (search summary of license terms)
- Impact sounds are often built from three layers: a sharp transient ("initial crack"), a mid-low body for weight, and a tail (debris, echo, reverb). — [SFX Engine blog](https://sfxengine.com/blog/impact-sound-effect) (secondary)
- A convincing fight hit combines a fast movement cue, a contact transient, a low body thud, cloth or skin detail, and breath or vocal effort, sometimes with bone, debris, or room tone. "Give each layer a job, then trim aggressively." If a hit gets lost, "brighten the contact layer or shorten the tail before turning the whole stack up." — [Pixflow blog](https://pixflow.net/blog/punch-impact-sound-effects-for-fight-scenes/); [add.app punching SFX](https://add.app/sound-effects/punching-sound-effects/) (search summary; exact attribution between the two is uncertain; secondary sources)
- Unity Audio Random Container: available in Unity 2023.2 and later. It picks clips from a list with volume and pitch variation "without using a script." Clicking the dice icon enables randomization, and a slider sets the range. Volume is in dB and adds to the Audio Source; pitch is in cents and combines with the Audio Source pitch. Individual clips can be disabled or lowered in dB. — [Unity Learn: Basics of the Audio Random Container](https://learn.unity.com/tutorial/the-basics-of-the-audio-random-container); [Unity 6.0 Manual: Create a randomized playlist](https://docs.unity3d.com/6000.0/Documentation/Manual/Create-randomized-playlist.html); [Unity Manual: ARC fundamentals](https://docs.unity3d.com/6000.5/Documentation/Manual/AudioRandomContainer-fundamentals.html); [Game Dev Beginner](https://gamedevbeginner.com/how-to-use-the-random-audio-container-in-unity/)

### Inferences
- A practical punch recipe for this project: one Audio Random Container per layer type. For example: whoosh (3 to 4 clips), contact (4 to 6 clips, pitch ±100 to 200 cents), body thud (3 clips), and vocal grunt (separate, not on every hit to avoid repetition). Trigger them together from the hit event with hitstop. Use heavier thud and lower pitch for the metal rod and a papery slap layer for the book.
- Body falls and deaths: reuse thud layers plus cloth, and add a distinct "final" sting so the neighbor-cell kill reads clearly. Keep it cartoonish to match the cute "eerily cozy" art direction.
- Grunts and vocal effort: CC0 vocal packs are scarce and voices are recognizable. Recording your own voice, pitched and processed, avoids licensing questions.
- Record each Sonniss file in the license register as "Sonniss GDC [year] bundle license" (not CC0), with the pack name.

### Gaps
- I did not identify specific CC0 packs for grunts, pain vocals, or body falls. Freesound CC0 search was not re-checked in this pass.
- The full Sonniss license text was not fetched directly. The terms above come from search summaries of the official license page.
- Unity ARC play modes (sequential, shuffle, random), "avoid repeat" behavior, and voice limits in Unity 6.3 were not confirmed on the 6.3 manual page.
- No authoritative (GDC or AAA audio) source on melee impact layering was retrieved. The layering sources are blogs.

## Key Question 4: Steam requirements, content disclosure, and regional/legal considerations for a game where the player can kill characters

### Takeaway
Before review, Steam requires a Content Survey with three parts: general content (used to generate regional ratings), mature content (violence and sex must be disclosed honestly, including content in the build that is not reachable), and generative AI. Since 2024-11-15, Germany hides any game without a valid age rating. A digital-only game can get one through Valve's own rating process based on the content survey, without paying USK, but must not reuse an IARC-generated USK rating from another store. Steam releases need the $100 Steam Direct fee (recouped after $1,000 adjusted gross revenue), a public Coming Soon page for at least two weeks, and store-page and build reviews (3 to 5 business days each). Steam's own onboarding page states a 21-day wait after paying the fee; third-party guides say 30 days.

### Cited Findings
**Content Survey**
- The survey must be completed before the game is submitted for review. Answers support Valve's pre-release review and let customers filter by preference. — [Steamworks: Content Survey](https://partner.steamgames.com/doc/gettingstarted/contentsurvey)
- Part 1 (general content) generates regional rating-board ratings shown on the store page at release, or you can enter official ratings instead. — [Steamworks: Content Survey](https://partner.steamgames.com/doc/gettingstarted/contentsurvey)
- Part 2 (mature content): "Because mature content such as depictions of violence and sex can be sensitive in nature, it is important that you make honest and accurate disclosures." You "must disclose all the adult content you've uploaded in your builds, even if it's not accessible or presented in your product." These answers "determine how your game appears relative to each customer's preferences." — [Steamworks: Content Survey](https://partner.steamgames.com/doc/gettingstarted/contentsurvey)
- Part 3 (generative AI) covers AI content shipped in the game ("Pre-Generated") and content made while the game runs ("Live-Generated," which requires describing guardrails). — [Steamworks: Content Survey](https://partner.steamgames.com/doc/gettingstarted/contentsurvey)
- The fetched Steamworks page did not show the mature-content category names or default filter states. Community sources say "Adult Only Sexual Content" is the only category hidden by default, and "Frequent Violence or Gore" is a separate user-selectable filter. — [Steam Community help thread](https://steamcommunity.com/discussions/forum/1/4526764179302310626/); [gamepressure](https://www.gamepressure.com/newsroom/how-to-remove-hentai-and-mature-content-games-from-your-steam-fee/z477b4) (secondary, unconfirmed by Valve docs)

**Germany**
- "Starting on November 15, 2024, Steam will no longer display games to customers in Germany if the game is missing a valid age rating." Existing owners keep access. — [Steamworks: Age Ratings Mandatory in Germany](https://partner.steamgames.com/doc/gettingstarted/contentsurvey/germany); [Steamworks announcement](https://steamcommunity.com/groups/steamworks/announcements/detail/4678768276768588864)
- The rating can come from the USK (Germany's official age-rating body) or from Valve's own process, which "takes into account customer feedback, input from Valve's content review teams and the content survey." "Your game cannot receive a content review rating from Valve if you have not filled in the game's content survey." — [Steamworks: Germany](https://partner.steamgames.com/doc/gettingstarted/contentsurvey/germany)
- "You must not submit a USK rating on Steam if it has been generated through the IARC process on a third-party store." "There are certain kinds of content that are not allowed for sale to customers in Germany. If present in your game, this content must be disclosed in the content questionnaire." — [Steamworks: Germany](https://partner.steamgames.com/doc/gettingstarted/contentsurvey/germany)
- A USK rating is mandatory for titles also released on physical media. — [gamesmarket.global](https://www.gamesmarket.global/starting-15-november-steam-stops-displaying-games-without-age-ratings-in-germany-9f53287a33246314f88b549952a581fb/); [heise online](https://www.heise.de/en/news/Steam-Why-some-of-the-best-indie-games-will-soon-be-hidden-in-Germany-9960810.html) (secondary)
- The Content Survey page also links a separate mandatory age-rating page for Indonesia. — [Steamworks: Content Survey](https://partner.steamgames.com/doc/gettingstarted/contentsurvey); [Steamworks: Indonesia](https://partner.steamgames.com/doc/gettingstarted/contentsurvey/indonesia) (Indonesia page not fetched)

**Australia and IARC**
- Under Australia's National Classification Scheme, all computer games offered through online storefronts must be classified. Participating storefronts can use the free IARC questionnaire (IARC is the International Age Rating Coalition, a shared online rating tool) to generate a rating. — [Australian Classification: Classify a computer game](https://www.classification.gov.au/for-industry/apply-for-classification/classify-computer-game); [Australian Classification: IARC tool](https://www.classification.gov.au/for-industry/develop-and-use-classification-tool/use-iarc-global-rating-tool) (search summary; both pages timed out on direct fetch)
- Steam is outside IARC. Valve relies on its content survey instead of adopting PEGI, ESRB, or USK ratings automatically, and a missing local rating can still cause restrictions in Germany or Australia. — [Skala blog](https://www.skala.io/blog/age-ratings-for-games-a-practical-guide-for-game-development-startups) (secondary)
- A headline reports that "Schedule I" was restricted on Steam in Australia because of a classification issue, showing Steam does geo-restrict titles there. — [Notebookcheck](https://www.notebookcheck.net/Schedule-I-restricted-on-Steam-in-Australia-due-to-classification-issue.1019089.0.html) (headline only; article returned 403)

**Steam release process and fees**
- Steam Direct: "$100 USD (or equivalent) fee for each new app." It is recoupable "after your product has at least $1,000.00 Adjusted Gross Revenue." Steam wallet funds cannot pay it. VAT or GST may apply. — [Steamworks: App fee](https://partner.steamgames.com/doc/gettingstarted/appfee)
- Waiting period, from Steam's onboarding page (verbatim, fetched 2026-09-25): "A 21-day waiting period between when you paid the app fee and when you can release that game." Third-party 2026 guides say 30 days, possibly an older figure (not verified). Treat the Steamworks page as current, but check it when paying. — [Steamworks: Onboarding](https://partner.steamgames.com/doc/gettingstarted/onboarding); conflicting: [The Game Marketer (2026 guide)](https://www.thegamemarketer.com/insight-posts/how-to-publish-your-game-on-steam-guide)
- "You'll need to prepare your store page and put up a publicly-visible 'coming soon' page for at least two weeks." You can release once the store page is approved, it has been Coming Soon for 2 weeks or more, and the build is reviewed. — [Steamworks: Onboarding](https://partner.steamgames.com/doc/gettingstarted/onboarding); [Steamworks: Release Process](https://partner.steamgames.com/doc/store/releasing)
- Onboarding also needs a legal name matching bank and tax documents, and a tax interview (W-9 or W-8BEN). Tax verification "may take 2-7 business days." — [Steamworks: Onboarding](https://partner.steamgames.com/doc/gettingstarted/onboarding)
- Review: the store page "typically takes 3-5 business days." Submit at least 7 business days before the target date. The store page must be submitted before the build. Screenshots must "only contain gameplay." All store-listed features must be implemented. Once approved, updates do not need re-review. Games with adult content submit store page and build together, and review takes longer. — [Steamworks: Review Process](https://partner.steamgames.com/doc/store/review_process)

### Inferences
- This game must disclose violence in the mature-content section. The player can kill inmates, and that content must be declared even if it is behind a flag or unfinished in a build. Whether it counts as "Frequent Violence or Gore" depends on how often and how graphically killing occurs. Cute stylized art does not by itself exempt it. Make the call when filling in the survey and flag it for review.
- Germany: a Steam-only digital release can use Valve's self-rating from the survey, with no USK fee needed. Do not copy an IARC-generated rating from itch.io or another store onto Steam.
- Australia: the law requires classification for online games, and Steam is not an IARC storefront. How a Steam-only indie game meets this in practice is unclear from the sources. Confirm with the Australian Classification Board or a lawyer before release. The simplest fallback is to exclude Australia at launch if unresolved (Steamworks supports region restrictions; not sourced here).
- AI disclosure: this game is built with AI coding agents. Whether AI-assisted code needs disclosure, versus AI-generated content shipped to players (for example AI mocap clips or AI art), is not settled by the fetched text. Confirm with Valve if any AI-generated animations or assets ship.
- Schedule: pay the fee early. The 21- or 30-day wait and the 2-week Coming Soon window can overlap, but store review (3 to 5 business days) must finish before Coming Soon starts.

### Gaps
- Steam's Rules and Guidelines (what may not be published) could not be read; the fetch returned only navigation. No Steam policy text on depicting violence against characters was retrieved.
- Exact Steam mature-content category labels, default filter states, and any age gate for violence were not confirmed on a Valve page.
- USK age thresholds for stylized killing, whether German refusal or "indexing" (placing a title on Germany's restricted-media list) is realistic for this content, and Australian R18+ and Refused Classification criteria for violence were not retrieved. These need the USK and Australian classification guidelines, or a lawyer.
- No sourced information on other regions that restrict violent games on Steam (for example China, where Steam's global store is not officially licensed, or Korea's GRAC rating board).
