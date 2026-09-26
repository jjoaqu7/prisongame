# Field naming: what C# and Unity developers actually use

Topic: engineering-standards · Date: 2026-09-25 · Serves: RESEARCH-02 decision 1
Searched: Microsoft's C# naming conventions and the .NET runtime coding style, Google's C# style guide, Unity's engine package source (URP), Unity's Boss Room sample game, the Unity style guide's example file, a September 2026 Unity forum thread, Unity's scripting reference for `FormerlySerializedAs` and `NicifyVariableName`, and Rider's Unity naming support; about 12 searches and page reads.
Assumptions: none. Added after the user pointed out that decision 1's first recommendation ("keep camelCase") rested on avoiding rename work, not on what developers do. Project facts:
- The prototype has 43 `[SerializeField]` fields, stored by name in 17 scenes and 7 prefabs.
- `tools/unity` scripts look fields up by name 46 times (`FindProperty("...")`).
- The new combat code has 4 serialized fields, used only by `Combat01_Sandbox`.

## 1. Is there a standard for private field names?

**Takeaway:** there are two established standards, not one. The wider C# world (Microsoft's .NET teams and Google) uses `_camelCase` for private fields. Unity Technologies' own code, both the engine packages and its official sample game, uses `m_PascalCase`, with `k_` for constants and `s_` for statics. Plain `camelCase` with no prefix, which the prototype uses, matches neither. Unity's style guide allows it, and some experienced developers prefer it, but no major published standard uses it.

**Findings:**
- Microsoft: "Private and internal non-constant instance fields start with an underscore (`_`)". Static fields start with `s_`, constants use PascalCase, and these rules are "adopted from the .NET Runtime team's coding style" ([Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names), Official, updated 2026-07-14).
- .NET runtime team: "We use `_camelCase` for internal and private fields and use `readonly` where possible." It uses Allman braces, each brace on its own line, and says "Never use single-line form" for statements like `if (x) throw ...;` ([dotnet/runtime coding style](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md), Primary).
- Google: private, protected and internal fields are `_camelCase`, with a 100-column limit and braces always used ([Google C# style guide](https://google.github.io/styleguide/csharp-style.html), Primary).
- Unity's engine packages: URP's camera data declares `[SerializeField] bool m_RenderPostProcessing`, `[SerializeField] LayerMask m_VolumeLayerMask` and `const string k_GizmoPath` ([Unity Graphics repository](https://github.com/Unity-Technologies/Graphics/blob/master/Packages/com.unity.render-pipelines.universal/Runtime/UniversalAdditionalCameraData.cs), Primary, read 2026-09-25).
- Unity's official sample game: Boss Room's `ServerCharacter` declares `[SerializeField] ClientCharacter m_ClientCharacter`, `private bool m_BrainEnabled` and `private float m_KilledDestroyDelaySeconds` ([Boss Room](https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop/blob/main/Assets/Scripts/Gameplay/GameplayObjects/Character/ServerCharacter.cs), Primary, read 2026-09-25).
- Unity's style guide example shows three allowed private-field forms side by side (`elapsedTimeInHours`, `_elapsedTimeInDays`, `m_elapsedTimeInSeconds`). Its serialized fields and conventions use `m_` (`[SerializeField] bool m_isPlayerDead`), `s_` for statics and `k_` for constants. It also says not to omit braces and to use one declaration per line ([StyleExample.cs](https://github.com/thomasjacobsen-unity/Unity-Code-Style-Guide/blob/master/StyleExample.cs), Primary, by the lead of Unity's style guide e-book).
- Practitioners are split three ways. A September 2026 Unity forum thread has users reporting `_` (called the "industry standard" outside Unity), `m_`, and no prefix at all. One user notes Unity's C++ engine and serialized data use `m_`; another notes "C# has largely adopted either the `_` or no prefix at all"; no Unity staff replied ([Unity Discussions](https://discussions.unity.com/t/how-do-you-think-about-utilizing-underscore-as-prefix-for-private-fields-against-m-style/1736838), Community, 2026-09-17 to 20).
- JetBrains Rider has a separate, configurable naming rule just for Unity serialized fields, so the tool treats this as a per-project choice ([Rider support](https://rider-support.jetbrains.com/hc/en-us/community/posts/4414090580242-C-code-style-settings-for-serialized-Field-in-Unity), Community).

**Inferences:**
- Proposed: adopt the Microsoft .NET convention for new code: `_camelCase` private fields, `s_camelCase` private statics, PascalCase constants and public members, Allman braces, one statement per line. It is the most widely published C# standard (Microsoft and Google). Unity's own style guide lists it. Tools and AI agents meet it in most C# code they see; that last point is an inference, not a measured fact.
- Proposed alternative, equally legitimate: Unity's house style (`m_PascalCase`, `k_PascalCase`, `s_PascalCase`). Choose it if matching the code of Unity's own packages and samples matters more than matching general C#.
- Open: which of the two.

**Gaps:** no survey or usage statistics of Unity projects' naming were found; "common practice" here means published standards and Unity's own code, not a count of projects.

## 2. What does changing the convention cost in a Unity project?

**Takeaway:** renaming a serialized field loses its saved Inspector values unless the field carries `[FormerlySerializedAs("oldName")]`, so a mass rename of the prototype touches every scene and prefab that stores those fields. The prefix itself costs nothing visible, because Unity strips `m_` and `_` from Inspector labels.

**Findings:**
- `[FormerlySerializedAs("hitpoints")]` lets you "rename a field without losing its serialized value". Unity's own URP code carries such attributes, for example `[FormerlySerializedAs("renderShadows"), SerializeField] bool m_RenderShadows` ([Unity 6.3 scripting reference](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Serialization.FormerlySerializedAsAttribute.html), Official, 6.3; URP source above).
- Unity's Inspector label function "will insert spaces before capital letters and remove optional `m_`, `_` or `k` followed by uppercase letter in front of the name", so `m_TheOtherVariable` displays as "The Other Variable" ([Unity 6.3 scripting reference](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/ObjectNames.NicifyVariableName.html), Official, 6.3).
- The forum thread above cites the same cost as a reason some developers avoid prefixes entirely (Community).

**Inferences:**
- Proposed: apply the chosen convention to all new code now, and rename the 4 combat fields immediately, while only the sandbox scene uses them. Scope the naming rules in `.editorconfig` to `Assets/PrisonGame/**`.
- Proposed: do not mass-rename the prototype's 43 serialized fields. Rename a prototype file's fields when a task rewrites that file, with `[FormerlySerializedAs]` on each serialized field, and recheck the scenes that use it.

**Gaps:** none material.
