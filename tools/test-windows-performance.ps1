param([int]$Width=1280,[int]$Height=720,[switch]$Visible,[int]$FrameRate=-1,[switch]$UseSavedFrameLimit,[ValidateSet("WindowsPerf","WindowsSample")][string]$BuildDirectory="WindowsPerf")
$ErrorActionPreference='Stop'
$gameRoot=Split-Path -Parent $PSScriptRoot
$folder=Join-Path $gameRoot "PrisonGame\Builds\$BuildDirectory"
$exe=Join-Path $folder 'PrisonGame.exe'
$suffix=if($Visible){"-visible-$FrameRate"}else{""}
$evidence=Join-Path $folder ("Performance\${Width}x${Height}$suffix")
New-Item -ItemType Directory -Path $evidence -Force | Out-Null
foreach($name in @('report.json','frames.csv','render.png','player.log','benchmark-save.json','benchmark-save.json.bak','benchmark-save.json.tmp')){
 $path=Join-Path $evidence $name
 if(Test-Path -LiteralPath $path){Remove-Item -LiteralPath $path}
}
$log=Join-Path $evidence 'player.log'
$argsList=@('-sample-performance','-screen-fullscreen','0','-screen-width',"$Width",'-screen-height',"$Height",'-logFile',('"'+$log+'"'))
$windowStyle='Hidden'
if($Visible){$argsList+='-sample-visible';if(-not $UseSavedFrameLimit){$argsList+=@('-sample-fps',"$FrameRate")};$windowStyle='Normal'}
# Visible mode is explicitly requested for interactive window/UI profiling.
$process=Start-Process -FilePath $exe -WorkingDirectory $folder -ArgumentList $argsList -WindowStyle $windowStyle -PassThru
# Short polling lets the invoking tool return a session; the timeout only governs this child.
$deadline=(Get-Date).AddSeconds(120)
$samples=[System.Collections.Generic.List[object]]::new()
while(-not $process.HasExited -and (Get-Date) -lt $deadline){
 Start-Sleep -Seconds 1
 $process.Refresh()
 if(-not $process.HasExited){$samples.Add([pscustomobject]@{elapsedSeconds=[math]::Round(((Get-Date)-$process.StartTime).TotalSeconds,2);workingSetBytes=$process.WorkingSet64;privateBytes=$process.PrivateMemorySize64})}
}
$samples | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidence 'process-memory.json')
if(-not $process.HasExited){Stop-Process -Id $process.Id;throw 'Performance run timed out'}
$report=Join-Path $evidence 'report.json'
if(-not(Test-Path -LiteralPath $report)){throw "No report; inspect $log"}
$data=Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
$data | ConvertTo-Json -Depth 8
if($process.ExitCode -ne 0 -or $data.status -ne 'COMPLETE'){throw "Performance fixture failed (exit $($process.ExitCode))"}
