[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ServiceName,

    [Parameter()]
    [string]$DocumentName = "v1"
)

$configuration = "Debug"

function Update-SwaggerTitle {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SwaggerPath,

        [Parameter(Mandatory = $true)]
        [string]$Title
    )

    $swaggerContent = Get-Content -Path $SwaggerPath -Raw
    if ([string]::IsNullOrEmpty($swaggerContent)) {
        return
    }

    $updatedSwaggerContent = [regex]::Replace(
        $swaggerContent,
        '"title":\s*"[^"]+"',
        ('"title": "{0}"' -f $Title),
        1
    )

    Set-Content -Path $SwaggerPath -Value $updatedSwaggerContent -Encoding UTF8
}

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
    $toolManifestPath = Join-Path $repoRoot ".config/dotnet-tools.json"

    $projectDir = Join-Path $repoRoot "src/Services/$ServiceName"
    $csprojPath = Join-Path $projectDir "$ServiceName.csproj"
    if (-not (Test-Path $csprojPath -PathType Leaf)) {
        throw "未找到项目文件: $csprojPath"
    }
    if (-not (Test-Path $toolManifestPath -PathType Leaf)) {
        throw "未找到 dotnet tool 清单: $toolManifestPath"
    }

    $targetFramework = Get-TargetFramework -CsprojPath $csprojPath
    $assemblyPath = Join-Path $projectDir "bin/$configuration/$targetFramework/$ServiceName.dll"
    $swaggerOutputPath = Join-Path $projectDir "swagger.json"

    Push-Location $repoRoot
    try {
        dotnet tool restore --tool-manifest $toolManifestPath
        if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
            throw "dotnet tool restore 执行失败"
        }
    }
    finally {
        Pop-Location
    }

    dotnet build $csprojPath --configuration $configuration
    if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
        throw "dotnet build 执行失败: $csprojPath"
    }

    if (-not (Test-Path $assemblyPath -PathType Leaf)) {
        throw "未找到程序集: $assemblyPath"
    }

    Push-Location $repoRoot
    try {
        dotnet tool run swagger -- tofile --output $swaggerOutputPath $assemblyPath $DocumentName
        if ($null -ne $LASTEXITCODE -and $LASTEXITCODE -ne 0) {
            throw "swagger 文档生成失败: $assemblyPath"
        }
    }
    finally {
        Pop-Location
    }

    $swaggerTitle = $ServiceName -replace 'Service$', ''
    Update-SwaggerTitle -SwaggerPath $swaggerOutputPath -Title $swaggerTitle
}
catch {
    Write-Error $_
    exit 1
}