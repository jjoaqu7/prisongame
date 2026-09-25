# Two separate Player processes test persistence without touching the manual save slot.
$ErrorActionPreference = 'Stop'
$gameRoot = Split-Path -Parent $PSScriptRoot
$buildFolder = Join-Path $gameRoot 'PrisonGame\Builds\WindowsSample'
$executable = Join-Path $buildFolder 'PrisonGame.exe'
$evidenceFolder = Join-Path $buildFolder 'SampleCheck'
if (-not (Test-Path -LiteralPath $executable)) { throw "Build not found: $executable" }
New-Item -ItemType Directory -Path $evidenceFolder -Force | Out-Null
foreach ($name in @('write-results.txt','read-results.txt','progress.json','progress.json.bak','progress.json.tmp','expected.json','invalid.json','write-startup.png','read-startup.png','completed.png','write-player.log','read-player.log')) {
    $file = Join-Path $evidenceFolder $name
    if (Test-Path -LiteralPath $file) { Remove-Item -LiteralPath $file }
}
foreach ($phase in @('write','read')) {
    $report = Join-Path $evidenceFolder ($phase + '-results.txt')
    $log = Join-Path $evidenceFolder ($phase + '-player.log')
    $arguments = @(('-sample-check-' + $phase), '-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',('"' + $log + '"'))
    $testProcess = Start-Process -FilePath $executable -WorkingDirectory $buildFolder -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if (-not $testProcess.WaitForExit(45000)) {
        Stop-Process -Id $testProcess.Id
        throw "Sample $phase timed out. Inspect $log"
    }
    $testProcess.Refresh()
    if (-not (Test-Path -LiteralPath $report)) { throw "No report produced. Inspect $log" }
    Get-Content -LiteralPath $report
    if ($testProcess.ExitCode -ne 0 -or (Get-Content -LiteralPath $report -TotalCount 1) -notlike 'PASS*') { throw "Sample $phase failed (exit $($testProcess.ExitCode))." }
}
Write-Output "Evidence: $evidenceFolder"
