# Analyzer setup trial in this project

Topic: engineering-standards · Date: 2026-09-26 · Serves: STD-01 (rollout step 6)
Searched: none; this records direct tests in the project (Unity 6000.3.24f1, Microsoft.Unity.Analyzers 1.27.0, Unity CLI 1.0.0-beta.8). The compiler arguments Unity used were read from `PrisonGame/Library/Bee/artifacts/1900b0aE.dag/*.rsp`.
Assumptions: none. This corrects two points in [the style notes](csharp-style-and-analysis.md#2-can-unity-63-enforce-rules-automatically-at-compile-time), which were based on a summary of Unity's manual rather than a test.

## 1. Does the analyzer load, and where does it apply?

**Takeaway:** it loads, and it applies to every assembly that references the assembly whose folder holds it. That includes the prototype's `Assembly-CSharp`, because Unity references every auto-referenced assembly from it. Only the severity decides what shows in the Console.

**Findings** (Primary, project test):
- Unity 6.3 compiles with `Editor/Data/DotNetSdkRoslyn/Microsoft.CodeAnalysis.dll` 4.3.0. Microsoft.Unity.Analyzers 1.27.0 references Microsoft.CodeAnalysis 3.11, so Unity accepts it.
- With the DLL in `Assets/PrisonGame/Scripts/Core/Runtime/Analyzers/`, labelled `RoslynAnalyzer` and disabled for all platforms, `-analyzer:".../Microsoft.Unity.Analyzers.dll"` appeared in the compiler arguments for `PrisonGame.Core`, `PrisonGame.Combat`, `PrisonGame.Combat.Tests.EditMode`, `Assembly-CSharp` and `Assembly-CSharp-Editor`.
- A probe class with an empty `Update()` produced no Console output at default severity, in either new or prototype code. `UNT0001` is not a warning by default.

## 2. How can severities be raised for new code only?

**Takeaway:** with a ruleset named after the assembly and placed beside its `.asmdef`. `.editorconfig` severities and ruleset includes had no effect.

**Findings** (Primary, project test):
- `.editorconfig` with `dotnet_diagnostic.UNT0001.severity = warning`, in `PrisonGame/` or in `PrisonGame/Assets/`, was never passed to the compiler (no `-analyzerconfig` argument), and no warning appeared.
- `PrisonGame.Combat.ruleset` beside `PrisonGame.Combat.asmdef`, setting `UNT0001` to `Warning`, was passed as `-ruleset:` and produced `warning UNT0001: The Unity message 'Update' is empty` for the new-code probe only. The prototype probe stayed silent.
- A ruleset named `PrisonGame.ruleset` beside the same `.asmdef` was not passed to the compiler. While both files existed, Unity logged that the folder "already contains a ruleset file ... will be ignored". The name must match the assembly.
- A ruleset whose only content was `<Include Path="..\..\PrisonGame.ruleset" Action="Default" />` compiled without raising the included rules.
- `tools/check.ps1 -Unity` with the probe present failed with "1 compiler or analyzer warning(s) in new code", and passed after the probe was removed.

**Inferences:**
- Agreed setup, applied in [Engineering standards](../../../engineering-standards.md#analyzers):
  - a master ruleset, `Assets/PrisonGame/Scripts/PrisonGame.ruleset`
  - an identical `<AssemblyName>.ruleset` copy beside each new-code `.asmdef`, verified by `tools/check.ps1`
  - `.editorconfig` used only for `dotnet format` (layout, naming, braces)

**Gaps:** why Unity did not pass `.editorconfig` was not established. The manual's example may assume a setting or IDE integration not present here.
