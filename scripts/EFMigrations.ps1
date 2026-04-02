# 生成迁移脚本
# 参数
param (
    [Parameter()]
    [string]
    $Name = $null,
    [Parameter()]
    [string]
    $DatabaseType = "PostgreSQL"
)

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $scriptRoot ".."))
$appSettingsPath = Join-Path $repoRoot "src\AppHost\appsettings.Development.json"
$migrationServicePath = Join-Path $repoRoot "src\Services\MigrationService"
$entityFrameworkProjectPath = Join-Path $repoRoot "src\Definition\EntityFramework\EntityFramework.csproj"

$toolManifestPath = @(
    (Join-Path $repoRoot ".config\dotnet-tools.json"),
    (Join-Path $repoRoot "dotnet-tools.json")
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if ($toolManifestPath) {
    Push-Location $repoRoot
    try {
        dotnet tool restore
    }
    finally {
        Pop-Location
    }
}
else {
    Write-Host "ℹ️ No dotnet tool manifest found under repo root, skipping 'dotnet tool restore'."
}

# 从 appsettings.Development.json 读取数据库类型
$IsMultiTenant = $true

if (Test-Path $appSettingsPath) {
    try {
        $config = Get-Content $appSettingsPath | ConvertFrom-Json
        if ($null -ne $config.Components.Database) {
            $DatabaseType = $config.Components.Database
            Write-Host "✅ Database type from appsettings: $DatabaseType"
        }
        if ($null -ne $config.Components.IsMultiTenant) {
            $IsMultiTenant = $config.Components.IsMultiTenant
            Write-Host "✅ IsMultiTenant from appsettings: $IsMultiTenant"
        }
    }
    catch {
        Write-Warning "Failed to read or parse $appSettingsPath. Using default database type: $DatabaseType"
    }
}

$env:Components__Database = $DatabaseType
Write-Host "✅ Set environment variable 'Components__Database' to '$DatabaseType' for this session."

$env:Components__IsMultiTenant = $IsMultiTenant
Write-Host "✅ Set environment variable 'Components__IsMultiTenant' to '$IsMultiTenant' for this session."

if (-not (Test-Path $migrationServicePath)) {
    throw "MigrationService path not found: $migrationServicePath"
}

if (-not (Test-Path $entityFrameworkProjectPath)) {
    throw "EntityFramework project path not found: $entityFrameworkProjectPath"
}

Push-Location $migrationServicePath
try {
    if ([string]::IsNullOrWhiteSpace($Name)) {
        $Name = [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
    }

    dotnet build
    if ($Name -eq "Remove") {
        dotnet ef migrations remove -c DefaultDbContext --no-build --project $entityFrameworkProjectPath
    }
    else {
        dotnet ef migrations add $Name -c DefaultDbContext --no-build --project $entityFrameworkProjectPath
    }
}
finally {
    Pop-Location
}