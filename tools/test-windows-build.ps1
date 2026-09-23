# Run opt-in checks inside the Development executable; ordinary launches do not run them.
$ErrorActionPreference = 'Stop'
$gameRoot = Split-Path -Parent $PSScriptRoot
$buildFolder = Join-Path $gameRoot 'PrisonGame\Builds\Windows'
$executable = Join-Path $buildFolder 'PrisonGame.exe'
$evidenceFolder = Join-Path $buildFolder 'SmokeCheck'
if (-not (Test-Path -LiteralPath $executable)) { throw "Build not found: $executable" }
New-Item -ItemType Directory -Path $evidenceFolder -Force | Out-Null
$report = Join-Path $evidenceFolder 'results.txt'
$log = Join-Path $evidenceFolder 'player.log'
# Remove only prior evidence files so an old success cannot be mistaken for this run.
foreach ($file in @($report, $log, (Join-Path $evidenceFolder 'startup.png'))) {
    if (Test-Path -LiteralPath $file) { Remove-Item -LiteralPath $file }
}
$arguments = @('-prototype-smoke-test', '-screen-fullscreen', '0', '-screen-width', '1280', '-screen-height', '720', '-logFile', ('"' + $log + '"'))
$testProcess = Start-Process -FilePath $executable -WorkingDirectory $buildFolder -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (-not $testProcess.WaitForExit(45000)) {
    Stop-Process -Id $testProcess.Id
    throw 'Smoke check timed out after 45 seconds. Inspect SmokeCheck/player.log.'
}
$testProcess.Refresh()
if (-not (Test-Path -LiteralPath $report)) { throw "No smoke-check report produced. Inspect $log" }
Get-Content -LiteralPath $report
if ($testProcess.ExitCode -ne 0 -or (Get-Content -LiteralPath $report -TotalCount 1) -notlike 'PASS*') {
    throw "Standalone smoke check failed (exit $($testProcess.ExitCode))."
}
Write-Output "Evidence: $evidenceFolder"
