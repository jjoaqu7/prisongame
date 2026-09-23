Yes. The slot game you mean is CloverPit, where you manipulate a slot machine to meet escalating debt payments or fall through the floor.

My strongest recommendation for a solo project is a compact game with one satisfying activity, escalating pressure, and upgrades that let players discover powerful combinations. Your two references fit that direction, although Schedule I is a considerably larger production target.

I checked Steam’s live charts, store pages, and a selection of player reviews on September 12, 2026.

These are the most relevant patterns I found. The figures below are Steam’s displayed last-30-day positive review percentage and review count, not sales estimates. The explanations and scope assessments are my analysis.

Type of game	Current reception	What seems appealing	Fit for a solo developer
Games about building powerful combinations	CloverPit: 89%, 457 reviews. Balatro: 97%, 1,523	Familiar rules become surprising as upgrades interact. Players feel clever when a build takes off.	Strong fit for a contained setting, but balancing the combinations takes substantial work.
Hands-on business simulators	Schedule I: 95%, 3,517. TCG Card Shop Simulator: 94%, 577	Perform tangible work, earn money, improve the operation, and see your progress physically.	Reasonable with one location. Employees, vehicles, combat, and an open world expand scope quickly.
Co-op games that create funny disasters	PEAK: 92%, 19,373	A simple shared objective produces rescues, mistakes, arguments, and stories.	Attractive audience, but networking and multiplayer testing add substantial work.
Ordinary jobs with horror complications	Shift At Midnight: 87%, 2,610	Familiar routines become tense because customers or events can threaten the operation.	A contained workplace helps. Multiple enemies and elaborate survival systems increase scope.
Short games about accumulating upgrades	Digseum: 94%, 73	Frequent progress, satisfying collection, and a complete experience without a huge time commitment.	One of the more manageable directions. Pacing must carry the experience.
Cozy activities in strange settings	Creature Kitchen: 99%, 478	Cooking, discovery, and befriending creatures inside an unusual atmosphere.	Plausible with restrained content, though creatures and environments require art work.

Co-op also has substantial current activity: Steam’s live chart showed roughly 41,000 players in PEAK and 35,000 in R.E.P.O. when checked. That establishes an active audience, but does not establish that multiplayer is the best production choice for you. Steam most-played chart

These examples demonstrate satisfied, active audiences. They do not establish unmet demand or the odds that another similar game will succeed.

The most useful connection between Schedule I and CloverPit is the feeling of gaining control over a system. You start constrained, learn how things interact, invest in improvements, and eventually achieve something that initially seemed impossible. Schedule I spreads that experience across a growing business; CloverPit concentrates it into individual runs.

The quota creates pressure. The interesting decisions create the game.

A useful player-review finding supports that distinction. In the small sample I read, Schedule I players praised the production-and-profit routine, while one criticism concerned accumulating money without enough meaningful uses. TCG players praised collecting and shopkeeping, while some described the later game becoming repetitive. These are qualitative examples, not a representative survey. Schedule I player reviews, TCG player reviews

For your design, I would take four lessons from this:

Make the basic action enjoyable before adding progression. Turning valves, dismantling objects, arranging products, or operating a machine should feel satisfying immediately.
Make upgrades change decisions. An upgrade that redirects output or introduces a new interaction does more design work than another small percentage increase.
Give players influence over failure. Randomness can create suspense, but players should usually be able to identify a decision they would change next time.
Give the premise a visible mechanical twist. A different setting alone is a weak reason to choose your game over an established favorite.

One direction I would seriously prototype is a strange industrial machine game set in one room.

For example: you operate an ancient boiler plant beneath a mysterious building. Each shift demands more power. You route steam between machines, decide when to overclock equipment, and buy questionable components. One component converts waste heat into output; another rewards dangerous pressure; combining them creates a powerful but unstable setup.

That would let you combine:

Schedule I’s satisfaction of operating and improving something tangible.
CloverPit’s deadlines, tension, and combinations.
A distinctive interaction built around routing, pressure, and machinery.

This is an untested concept, not a demonstrated market gap. Its advantage is that a few interacting components could create variety inside one environment. The main design risk is making those interactions understandable.

I would begin with one machine, three interacting components, a few upgrades, and a ten-minute playable shift. Test whether unfamiliar players understand the choices and voluntarily want another attempt. Their behavior will tell you more about your particular opportunity than another list of hit games.

Have you built a playable game before, or would this be your first?

but remember I won't be alone, I have codex (you) and MCP to help
What does this mean?

"a strange industrial machine game set in one room. "
AND WAIT what about,:

a game where you are doing "labor" but are also in a sort of prison
so sort of an escape room, similar to Cloverpit, the captures or ppl holding you give you money based on what you have
you can buy from them upgrades and so forth to increase your money/multipliers for that level and global upgrades
and you need to make certain amount to progress to next level or escape
and so forth

Yes. Your prison-labor idea gives the game a clear premise: your captors profit from your work, and you use the money and equipment they give you to work toward freedom. That is a coherent foundation.

And agreed: you would have me helping with implementation, debugging, tools, and iteration. We can approach this as a project we develop together. Your creative direction and experience playing each version will guide what we build.

By “a strange industrial machine game set in one room,” I meant that most gameplay happens inside a small, interactive workspace. You operate equipment, make decisions, purchase improvements, and watch that workspace change. The machinery was an example. Your prison could revolve around processing scrap, assembling products, sorting valuables, or another kind of labor.

The room becomes somewhere players learn to understand and eventually control.

Picture this version of your idea:

You wake up in a prison workshop. There is a workbench, a machine, a delivery hatch, a payment terminal, and a locked exit. A contract on the wall lists the shifts you must complete to earn release.

Your captors supply materials and purchase whatever you produce. They also operate the equipment shop. Selling you better tools makes sense because better-equipped prisoners produce more valuable goods.

Each shift follows a clear sequence:

Receive materials and a quota. You have limited time or machine operations to meet it.
Perform the labor. Your decisions determine the value of the output.
Sell what you produced. The captors collect the required payment.
Spend your surplus. Buy equipment, modifiers, or consumables.
Advance to the next shift. The demands increase, and your developing setup gives you new ways to meet them.

For example, earning 160 credits against a 100-credit quota leaves 60. You buy a 40-credit upgrade and carry 20 forward. Initially, purchases could happen between shifts so the consequences are easy to understand.

The crucial design question is what makes the labor itself interesting. The prison, money, and upgrades provide motivation. The moment-to-moment activity still needs satisfying decisions.

For one possible prototype, imagine a salvage press:

You receive discarded machinery. You load parts, select how to process them, and control the pressure. Gentle processing preserves valuable components but takes more operations. Aggressive processing is faster but can destroy them. You decide whether to extract something carefully, crush it for guaranteed material value, or risk a more profitable result.

Then upgrades change those decisions:

A recovery attachment makes damaged components valuable.
A sorting attachment rewards consecutive batches of the same material.
A heat converter turns accumulated machine heat into a payout bonus, encouraging risky operation.

Now a player can discover a strategy: “My machine profits from damaged parts and high heat, so aggressive processing actually works for this build.”

That is where your multipliers become interesting. They emerge from how the player works.

Your upgrade idea also has three possible layers:

Upgrade layer	How long it lasts	Example
Shift upgrade	The current level	A lubricant that allows extra machine operations
Equipment upgrade	The entire escape attempt	A new attachment that changes processing rules
Permanent unlock	Survives failed attempts	A blueprint that makes a new attachment available in future attempts

I would make equipment upgrades the main progression initially. Permanent unlocks could introduce additional strategies. Permanent percentage bonuses are also possible, but they require care: players should feel that learning helps them escape, rather than that failure is compulsory until they accumulate enough bonuses.

Escape should also feel measurable. Show the remaining contract, completed shifts, and a visible final challenge. For the first version, meeting the final requirement could actually open the exit. We can develop a more elaborate escape later if the core game calls for it.

A useful first prototype would be one room, one labor activity, three shifts, and six interacting upgrades. That gives us enough to test your actual premise: whether working under pressure, choosing improvements, and building toward freedom makes someone want another attempt.

The salvage press is just one candidate. What kind of labor were you picturing?

You can have a substantial hands-on role in making the artwork. I would build our workflow around you designing, modeling, painting, and arranging a small set of assets, with me helping with the technical setup and iteration.

For this game, the aesthetic could come from the machine’s shape, the lighting, the printed instructions, the condition of the room, and how everything moves. Those decisions can carry your personality throughout the game.

Think of a game object as several layers. A payment terminal, for example, has:

Layer	What it contributes	What you could create
3D model	Shape and proportions	A bulky cabinet, oversized lever, narrow payment slot
Materials and textures	Surface appearance	Chipped paint, dull metal, fingerprints, printed labels
Lighting	Mood and visual focus	A warm desk lamp surrounded by cold overhead lighting
Graphic design	The institution’s identity	Quota displays, receipts, warning signs, upgrade packaging
Animation and effects	Weight and personality	A sticking lever, rattling panel, flickering screen

You can start with simple geometry and still make something distinctive by making these layers work together.

For your prison, I see three plausible visual directions:

Direction	What it might look like	How you would make it
Institutional industrial	Faded teal walls, cream enamel machines, amber lights, stamped serial numbers	Relatively simple 3D shapes, a few shared materials, carefully designed labels
Handmade and unsettling	Crooked machines, exaggerated knobs, mismatched repairs, painted surfaces	Simple models with deliberately unusual proportions and more individual painting
Illustrated prison workshop	A drawn room, layered machinery, limited moving parts	Mostly drawing and painting, with gameplay viewed from a fixed position

I would test institutional industrial first. It suits your premise and gives you approachable objects to model: cabinets, pipes, switches, trays, and machinery.

A particular twist could make it yours: the prison presents forced labor as a cheerful productivity program. Friendly instructional posters, immaculate accounting, and pleasant colors sit alongside visibly exhausted machinery. That idea would guide the typography, props, colors, and interface consistently.

For tools, I would start with a small combination:

Blender for constructing the room, machines, and props. Your first modeling skills would be moving and resizing shapes, extending faces, rounding edges, and assigning materials.
Krita for sketches, painted surface details, signs, labels, and interface artwork. It is free and designed for digital painting.
The game engine for assembling the playable scene and judging the final lighting, scale, and interaction. If we choose Godot, it supports importing Blender scenes and recommends glTF for 3D interchange. Godot documentation

If you have used CAD, your familiarity with shapes and assemblies should help. Game modeling introduces different priorities, particularly appearance, animation, and efficient geometry.

AI can support that process while leaving you directly involved in the art. We can explore visual concepts, critique your sketches, develop palettes, and work through modeling problems. I can also write Blender scripts for repetitive geometry and setup; Blender exposes its tools through Python. Blender scripting

One distinction matters: a concept image describes the intended appearance; a playable asset needs working structure. A generated picture of a machine does not supply its separate moving lever, interior surfaces, or interaction setup. We would use the picture as a reference while building those parts.

I would begin with this exercise:

Sketch three payment terminals. Paper is fine. Explore their proportions and where the player’s attention goes.
Choose one and give it a few visual rules. For example: cream body, dark metal, amber display, oversized controls, printed institutional labels.
Build it from basic shapes in Blender. Then make one label or receipt in Krita. Concentrate wear around places that would actually be touched.
Place it in a small test scene. Add one wall, one light, and a working lever. Check it from the player’s actual viewpoint.

That gives you an object you personally designed and made, while teaching the complete path from an idea to something inside the game.

what about a unit based game? wouldnt it be much more easy 

Yes. If you mean a game where you place, upgrade, and combine units on a small board, that could be considerably easier to make than the first-person prison workshop we discussed. Especially the artwork.

The savings come from a fixed viewpoint, limited movement, and reusable rules. “Unit based” can describe very different workloads, though. My scope assessment would be:

Format	What you build	Relative workload
Workers placed in slots	Assign workers, resolve production, purchase improvements	Lower
Small autobattler	Assemble a team, choose positioning, watch combat resolve	Lower to moderate with simple targeting
Turn-based tactical game	Move units around a grid and choose their actions	Moderate, with more movement rules and enemy behavior
Real-time strategy game	Command units moving freely around maps	High, with navigation, collisions, AI, and more animation

Two useful references are Stacklands, which represents villagers and resources as cards that interact when stacked, and Super Auto Pets, which revolves around assembling animals with interacting abilities for automatic battles. They illustrate compact ways to represent units and their actions.

For your artwork, this could be a much more approachable way to develop a distinctive aesthetic yourself. A unit could be a drawing or a paper cutout with a few poses. We could animate it through small hops, tilts, flashes, and effects. You could focus on character silhouettes, expressions, colors, the board, and interface design. Fully animated characters facing multiple directions would bring the workload back up.

It also fits how we could develop together: I can build the shared placement, upgrade, and ability systems, while you create the units and shape their appearance. Additional units can reuse those systems, although unusual abilities still require implementation and testing.

If you wanted to retain the prison idea, imagine controlling a small crew in a workshop:

A scrapper produces raw material.
A machinist consumes material to produce more valuable goods.
A technician improves an adjacent machine’s output.

Each round, you arrange the crew, start the shift, collect earnings, pay the quota, and choose a recruit or upgrade. Limited spaces create decisions: another producer might earn more immediately, while a support unit could make an existing combination stronger.

That could begin with one board, three unit types, three shifts, and six upgrades. The prison aesthetic could come through illustrated inmate portraits, stamped paperwork, barred workshop slots, and announcements from your captors.

The remaining challenge is making the choices interesting. If one arrangement always wins, players have solved the game. Different offers, constraints, and viable combinations need to give them reasons to reconsider their setup. You also shift the experience toward managing a crew, so the feeling of personally being trapped would need deliberate attention.

Were you imagining units that fight, or workers you arrange to produce things?

Also what about in this prison, as you progress you can collect "pieces" these pieces allow you to espcae when you combine with other pieces, so you have various unique escape routes based on what you collect throughout the game.
And between shifts you can talk to other inmates as well, gain reputation/aura which allows you to trade higher value items and stuff, as well as control a gang → entire prison eventually and get income this way, so an economy aspect would be needed here as well.
for schedule I what is that cartoon style? kind of really liked that minimalist simple style, is it base defualt to unity or something or the deeveloper made these himself ?

Those additions can fit together well: labor earns resources, relationships open opportunities, and you choose whether to invest in escaping or gaining power inside the prison. That gives the quotas a larger purpose.

The escape pieces could become the main source of discovery across a playthrough. I would let players collect three kinds of things: physical components, useful information, and cooperation from other inmates. Combining them completes an escape plan.

For example, these are possible fictional routes:

Escape route	What you assemble	What makes its gameplay different
Maintenance route	A device built from workshop components, maintenance information, a mechanic’s help	Rewards production upgrades and salvaging
False transfer	Document fragments, a disguise, a contact’s favor	Rewards trading and relationships
Coordinated diversion	Supplies, a signaling device, a committed crew	Rewards recruitment and gang influence

Completing the requirements could unlock a short final challenge specific to that route.

The most interesting pieces would also have another use. A rare motor could improve your workshop, be traded for an inmate’s loyalty, or complete an escape device. Keeping it for escape now costs you something elsewhere.

That creates a decision such as: “I could finish this route soon, but selling the component would let me recruit someone who opens a different route.”

I would reveal enough of each plan early for players to pursue it deliberately. Random finds can change which route looks promising, while jobs, favors, and expensive trades provide ways to obtain missing essentials. Otherwise, progress can become waiting for one lucky drop.

The inmate relationships and gang economy could provide the choices between shifts. A simple structure would be:

Complete your shift and settle the quota.
Choose a limited number of social actions, such as visiting an inmate, fulfilling a favor, trading, or recruiting.
Invest in production, your crew, or an escape project.
Resolve prison events and begin the next shift.

Limiting those actions matters. If you have time to visit only two people, choosing whom to help becomes meaningful.

For reputation or “aura,” I would distinguish three things without creating three complicated leveling systems:

Reputation: Your general standing, which opens introductions and higher-value trading opportunities.
Personal trust: Whether a particular inmate will share information, extend credit, or join your plan. Initially, this could simply be stranger, contact, ally.
Gang power: The members and operations you actually control.

That allows someone to recognize your influence while still refusing to trust you. Reputation would grow through fulfilled promises and consequential choices.

Gang income could start with one operation, such as supplying workshop goods through a recruited trader. Its earnings should account for supplies, crew shares, and possible losses. Demand and available stock can limit how much it earns each shift. We can begin with straightforward prices and quantities; a fully simulated market is unnecessary.

Your units also gain competing assignments: a useful inmate could work in production, run a trading operation, or help prepare an escape. That connects the economy directly to the unit gameplay.

Eventually controlling the prison should probably be an alternative victory path. If becoming powerful is enjoyable, players may reasonably want to stay. Escaping and becoming the prison’s dominant gang could be two different endings, with opportunities to change direction along the way.

Schedule I’s visual style is best described as stylized 3D with cartoon proportions and simplified surfaces. “Low-poly stylized characters” is a useful search phrase, although a simple appearance does not necessarily mean extremely few polygons.

It uses Unity, as confirmed by Unity’s own release roundup, but that appearance is deliberately created, rather than Unity’s default look. Models determine shapes; materials, textures, and lighting determine how those shapes appear. Unity supplies tools for assembling and rendering them. Unity’s explanation of meshes and materials

I found a particularly useful source: Tyler explaining his character workflow himself.

He modeled and rigged the characters in Blender. Rigging gives a character the internal skeleton used for animation.
Much of the clothing consists of 2D textures layered onto the body, created using Photoshop and Substance Painter.
He modeled accessories in Blender.
He used Mixamo for much of the animation.

That comes from his character-creation post and replies. The official credits also list Tyler as designer, programmer, and artist, and Cody To as graffiti artist. This establishes custom artwork and outside contributions; it does not establish that Tyler personally made every asset.

For your game, a reusable character body with different faces, hair, uniforms, and accessories would be a practical approach. You could design those variations yourself, giving important inmates recognizable personalities without building every character from scratch.

Unit gameplay can also use those 3D characters. A fixed camera overlooking a prison workshop or cellblock could preserve much of the production simplicity we discussed while moving toward the cartoon aesthetic you like.

I would make the first playable version one cellblock, four inmates, two escape routes, and one recruitable income operation. That would let us test whether production, relationships, and escape preparation create interesting competing choices before expanding to control of the entire prison.