# Formats C# code to the project standard (docs/engineering-standards.md) using PrisonGame/.editorconfig.
# Fixes layout in all project C# code and adds missing braces in new code. Never renames anything:
# renaming serialized fields needs [FormerlySerializedAs], so naming problems are only reported by check.ps1.
#   -Staged  format only the C# files currently staged for commit (re-stage them afterwards)
param([switch]$Staged)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'code-paths.ps1')

$files = Get-CodeFiles -StagedOnly:$Staged
if ($Staged -and $files.Count -eq 0) {
    Write-Host 'No staged C# files to format.'
    exit 0
}

Write-Host 'Formatting layout...'
& dotnet format whitespace $AssetsRoot --folder --include @(Get-IncludeArgs $files)
if ($LASTEXITCODE -ne 0) { throw "dotnet format whitespace failed (exit $LASTEXITCODE)." }

$newCode = @($files | Where-Object { $_ -like 'Assets/PrisonGame/*' })
if (-not $Staged) { $newCode = @('Assets/PrisonGame/') }
if ($newCode.Count -gt 0) {
    foreach ($project in Get-NewCodeProjects -Files $(if ($Staged) { $newCode } else { $null })) {
        Write-Host "Adding missing braces in $(Split-Path -Leaf $project)..."
        & dotnet format style $project --no-restore --severity warn --diagnostics IDE0011 --include $newCode
        if ($LASTEXITCODE -ne 0) { throw "dotnet format style failed for $project (exit $LASTEXITCODE)." }
    }
}

Write-Host 'Done. Review the changes; if you are committing, stage the formatted files again.'
