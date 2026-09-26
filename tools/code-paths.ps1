# Shared by format.ps1 and check.ps1: which C# code follows the project standard.
# Dot-source this file; it defines $RepoRoot, $ProjectRoot and helper functions.

$RepoRoot = Split-Path -Parent $PSScriptRoot
$ProjectRoot = Join-Path $RepoRoot 'PrisonGame'
# Layout checks run on the Assets folder only, so dotnet format never walks Unity's large Library cache.
$AssetsRoot = Join-Path $ProjectRoot 'Assets'

# Project-relative folders whose C# code is formatted. tools/unity snippets are a record of past
# scene builds and are not formatted; Assets/TutorialInfo is Unity's template code.
$FormattedFolders = @('Assets/PrisonGame/', 'Assets/Prototype/', 'Assets/Editor/')

# Returns project-relative paths: the formatted folders, or only the staged C# files inside them.
function Get-CodeFiles([switch]$StagedOnly) {
    if (-not $StagedOnly) { return $FormattedFolders }
    $staged = @(& git -C $RepoRoot diff --cached --name-only --diff-filter=ACMR)
    if ($LASTEXITCODE -ne 0) { throw 'git diff --cached failed.' }
    $result = @()
    foreach ($path in $staged) {
        if ($path -notlike 'PrisonGame/*.cs') { continue }
        $relative = $path.Substring('PrisonGame/'.Length)
        foreach ($folder in $FormattedFolders) {
            if ($relative.StartsWith($folder)) { $result += $relative; break }
        }
    }
    return $result
}

# Paths for `dotnet format whitespace $AssetsRoot --folder --include`, which are relative to Assets.
function Get-IncludeArgs($paths) { return @($paths | ForEach-Object { $_.Substring('Assets/'.Length) }) }

# Unity generates one .csproj per assembly (Git ignores them). New code lives in the PrisonGame.* assemblies.
# With -Files, returns only the projects that compile at least one of those project-relative files.
function Get-NewCodeProjects([string[]]$Files) {
    $projects = @(Get-ChildItem -Path $ProjectRoot -Filter 'PrisonGame.*.csproj' | ForEach-Object { $_.FullName })
    if ($projects.Count -eq 0) {
        Write-Warning 'No PrisonGame.*.csproj files found. Open the project in Unity once so it generates them; naming and brace checks are skipped until then.'
        return @()
    }
    if (-not $Files) { return $projects }
    return @($projects | Where-Object {
        $text = Get-Content -LiteralPath $_ -Raw
        @($Files | Where-Object { $text.Contains($_.Replace('/', '\')) }).Count -gt 0
    })
}
