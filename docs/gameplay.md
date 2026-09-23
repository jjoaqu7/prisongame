# Gameplay

## Agreed foundation

First-person movement and interaction inside the prison. Daily routines, trading, favors, relationships, jobs, and cell improvements should make prison life enjoyable before escape becomes a major goal.

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

Start with a few ordinary goods such as coffee, snacks, and toiletries. Different inmates have different needs and limited supplies. Services and information can become additional opportunities later.

Favors are promises to specific people. Helping someone might secure an introduction or a future service. Some requests may have deadlines or consequences for delay; show those when the request begins and track them automatically. A simple notebook should show requirements, progress, timing, and what each person owes. The balance between timed and untimed favors remains open.

Decide whether money, barter, or a mixture is the main exchange system before implementing pricing. Begin with understandable stock and demand rules; the economy's complexity should serve player decisions.

## Relationships, jobs, and the cell - proposed details

- NPCs can distinguish trust, fear, and suspicion. Present the states that affect the player's current decisions through simple indicators, dialogue, and behavior. Avoid making the player infer an important active penalty from subtle dialogue alone. The exact model remains open.
- Let jobs create different contacts and access: kitchen, laundry, and library work are candidates.
- Make cell improvements visible and useful through storage, comfort, and workspace.
- Give the cellmate habits and preferences that affect everyday life. Clearly communicate any resulting obligation or risk to the player's belongings. The extent of upkeep and loss remains open.
- Later, trusted inmates could handle parts of an operation, with costs and limits.

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
