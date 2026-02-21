[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ServiceName,

    [Parameter()]
    [string]$DocumentName = "v1"
)

$location = Get-Location
$configuration = "Debug"
function Get-TargetFramework {
    param([Parameter(Mandatory = $true)][string]$CsprojPath)

    [xml]$csproj = Get-Content -Raw -Path $CsprojPath
    $groups = @($csproj.Project.PropertyGroup)

    foreach ($group in $groups) {
        if ($group.TargetFramework) {
            return $group.TargetFramework.Trim()
        }
    }

    foreach ($group in $groups) {
        if ($group.TargetFrameworks) {
            return $group.TargetFrameworks.Split(';')[0].Trim()
        }
    }
    throw "无法从项目文件读取 TargetFramework/TargetFrameworks: $CsprojPath"
}

try {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

    $projectDir = Join-Path $repoRoot "src/Services/$ServiceName"
    $csprojPath = Join-Path $projectDir "$ServiceName.csproj"
    if (-not (Test-Path $csprojPath -PathType Leaf)) {
        throw "未找到项目文件: $csprojPath"
    }

    $targetFramework = Get-TargetFramework -CsprojPath $csprojPath
    $assemblyPath = Join-Path $projectDir "bin/$configuration/$targetFramework/$ServiceName.dll"
    $swaggerOutputPath = Join-Path $projectDir "swagger.json"

    Set-Location $repoRoot

    dotnet tool restore
    if (-not (Test-Path $assemblyPath -PathType Leaf)) {
        throw "未找到程序集: $assemblyPath"
    }
    Set-Location $projectDir
    dotnet tool run swagger -- tofile --output $swaggerOutputPath $assemblyPath $DocumentName
}
catch {
    Write-Error $_
    exit 1
}
finally {
    Set-Location $location
}