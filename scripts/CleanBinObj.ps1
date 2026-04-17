[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [Parameter()]
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$resolvedRepositoryRoot = [System.IO.Path]::GetFullPath($RepositoryRoot)

if (-not (Test-Path $resolvedRepositoryRoot -PathType Container)) {
    throw "Repository root not found: $resolvedRepositoryRoot"
}

$projectFiles = Get-ChildItem -Path $resolvedRepositoryRoot -Recurse -Filter *.csproj -File -ErrorAction SilentlyContinue
$projectDirectories = $projectFiles |
    ForEach-Object { $_.Directory.FullName } |
    Sort-Object -Unique

foreach ($projectDirectory in $projectDirectories) {
    foreach ($folderName in @('bin', 'obj')) {
        $targetPath = Join-Path $projectDirectory $folderName

        if (-not (Test-Path $targetPath -PathType Container)) {
            continue
        }

        if (-not $PSCmdlet.ShouldProcess($targetPath, 'Remove build output directory')) {
            continue
        }

        try {
            Remove-Item -Path $targetPath -Recurse -Force -ErrorAction Stop
            Write-Host "Removed: $targetPath"
        }
        catch {
            Write-Warning "Skipped failed removal: $targetPath. $($_.Exception.Message)"
        }
    }
}