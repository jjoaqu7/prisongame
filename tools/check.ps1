# Checks code against the project standard (docs/engineering-standards.md). Exit code 0 means everything passed.
#   (default)  layout of all project C# code; naming and braces in new code (Assets/PrisonGame)
#   -Staged    the same checks on staged C# files only; the Git pre-commit hook uses this
#   -Unity     also refresh and recompile in the open Unity Editor, fail on compiler or analyzer warnings
#              in new code, and run the edit-mode tests through the Unity CLI
# Fix layout and braces with tools/format.ps1. Naming is fixed by hand (see the standard).
param([switch]$Staged, [switch]$Unity)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'code-paths.ps1')
$failures = @()

$files = Get-CodeFiles -StagedOnly:$Staged
if ($Staged -and $files.Count -eq 0) {
    Write-Host 'Code check: no staged C# files.'
}
else {
    Write-Host 'Checking layout...'
    & dotnet format whitespace $AssetsRoot --folder --verify-no-changes --include @(Get-IncludeArgs $files)
    if ($LASTEXITCODE -ne 0) { $failures += 'Layout: run tools/format.ps1 (add -Staged for staged files), then stage the files again.' }

    $newCode = @($files | Where-Object { $_ -like 'Assets/PrisonGame/*' })
    if (-not $Staged) { $newCode = @('Assets/PrisonGame/') }
    if ($newCode.Count -gt 0) {
        foreach ($project in Get-NewCodeProjects -Files $(if ($Staged) { $newCode } else { $null })) {
            Write-Host "Checking naming and braces in $(Split-Path -Leaf $project)..."
            & dotnet format style $project --verify-no-changes --no-restore --severity warn --include $newCode
            if ($LASTEXITCODE -ne 0) {
                $failures += "Naming or braces in $(Split-Path -Leaf $project): see the IDE1006/IDE0011 lines above. Braces: tools/format.ps1. Names: rename by hand, with [FormerlySerializedAs] on serialized fields."
            }
        }
    }
}

# No spaces in asset file or folder names (Unity's command-line tools mishandle them).
# Fix with PrisonGame.EditorTools.AssetNaming.RemoveSpaces, which keeps references intact.
$spaced = @(Get-ChildItem -Path $AssetsRoot -Recurse -Force | Where-Object { $_.Name -like '* *' -and $_.Name -notlike '*.meta' })
if ($spaced.Count -gt 0) {
    $spaced | Select-Object -First 10 | ForEach-Object { Write-Host "  $($_.FullName.Substring($ProjectRoot.Length + 1))" }
    $failures += "Names: $($spaced.Count) asset file or folder name(s) contain spaces (first ones listed above). Run PrisonGame.EditorTools.AssetNaming.RemoveSpaces in Unity."
}

# Analyzer severities: every new-code assembly needs <AssemblyName>.ruleset beside its .asmdef, identical to
# the master copy. Unity ignores rulesets with other names and does not follow ruleset includes.
$master = Join-Path $AssetsRoot 'PrisonGame/Scripts/PrisonGame.ruleset'
if (-not (Test-Path -LiteralPath $master)) { $failures += 'Rulesets: master PrisonGame/Scripts/PrisonGame.ruleset is missing.' }
else {
    $masterText = Get-Content -LiteralPath $master -Raw
    foreach ($asmdef in Get-ChildItem -Path (Join-Path $AssetsRoot 'PrisonGame') -Recurse -Filter '*.asmdef') {
        $name = (Get-Content -LiteralPath $asmdef.FullName -Raw | ConvertFrom-Json).name
        $ruleset = Join-Path $asmdef.DirectoryName "$name.ruleset"
        if (-not (Test-Path -LiteralPath $ruleset)) { $failures += "Rulesets: $name has no $name.ruleset beside its .asmdef. Copy PrisonGame/Scripts/PrisonGame.ruleset there under that name." }
        elseif ((Get-Content -LiteralPath $ruleset -Raw) -ne $masterText) { $failures += "Rulesets: $name.ruleset differs from the master copy PrisonGame/Scripts/PrisonGame.ruleset." }
    }
}

if ($Unity) {
    $cli = if ($env:UNITY_CLI) { $env:UNITY_CLI } else { Join-Path $env:LOCALAPPDATA 'Unity\bin\unity.exe' }
    if (-not (Test-Path -LiteralPath $cli)) { throw "Unity CLI not found at $cli. Set UNITY_CLI to its path." }
    Write-Host 'Refreshing assets and recompiling in the open Unity Editor...'
    $consoleStart = & $cli command console --project-path $ProjectRoot --no-banner --json --tail 1 2>$null | Out-String | ConvertFrom-Json
    & $cli command eval --project-path $ProjectRoot --no-banner 'AssetDatabase.Refresh(); return 1;' 120000 | Out-Null
    & $cli command recompile --project-path $ProjectRoot --no-banner | Out-Null
    $status = $null
    for ($i = 0; $i -lt 100; $i++) {
        Start-Sleep -Seconds 3
        $raw = & $cli command recompile_status --project-path $ProjectRoot --no-banner --json 2>$null | Out-String
        if ($raw -match '"status\\?"\s*:\s*\\?"(completed|up_to_date)') { $status = ($raw | ConvertFrom-Json).data.result; break }
    }
    if ($null -eq $status) { $failures += 'Unity: recompile did not finish within 5 minutes.' }
    elseif ($status -match '"failed"\s*:\s*true') { $failures += "Unity: compilation failed: $status" }
    else {
        Start-Sleep -Seconds 3
        $since = $consoleStart.data.result
        $log = & $cli command console --project-path $ProjectRoot --no-banner --json --since $since.cursor --since_session $since.session --level warn 2>$null | Out-String | ConvertFrom-Json
        $newCodeWarnings = @($log.data.result.entries | ForEach-Object { ($_.message -split "`n")[0] } | Where-Object { $_ -match '^Assets[\\/]PrisonGame[\\/]' })
        if ($newCodeWarnings.Count -gt 0) {
            $newCodeWarnings | ForEach-Object { Write-Host "  $_" }
            $failures += "Unity: $($newCodeWarnings.Count) compiler or analyzer warning(s) in new code (listed above)."
        }
        Write-Host 'Running edit-mode tests...'
        Start-Sleep -Seconds 5
        # The Unity CLI server restarts after a domain reload, so retry briefly if it is not reachable yet.
        $summary = $null
        for ($attempt = 1; $attempt -le 5 -and $null -eq $summary; $attempt++) {
            $raw = & $cli command run_tests --project-path $ProjectRoot --no-banner --json --timeout 600 --mode editor 2>$null | Out-String
            try { $summary = ($raw | ConvertFrom-Json).data.result.Summary } catch { $summary = $null }
            if ($null -eq $summary) { Start-Sleep -Seconds 5 }
        }
        if ($null -eq $summary) { $failures += 'Unity: tests did not report a result (is the Editor idle and outside Play mode?).' }
        else {
            Write-Host "Edit-mode tests: $($summary.Passed) passed, $($summary.Failed) failed, $($summary.Skipped) skipped of $($summary.Total)."
            if ($summary.Failed -gt 0 -or $summary.Total -eq 0) { $failures += 'Unity: edit-mode tests failed or none ran.' }
        }
    }
}

if ($failures.Count -gt 0) {
    Write-Host ''
    Write-Host 'Code check FAILED:' -ForegroundColor Red
    $failures | ForEach-Object { Write-Host "  - $_" }
    exit 1
}
Write-Host 'Code check passed.' -ForegroundColor Green
exit 0
