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

dotnet tool restore
# 从 appsettings.Development.json 读取数据库类型
$appSettingsPath = "../src/AppHost/appsettings.Development.json"
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

$location = Get-Location

Set-Location ../src/Services/MigrationService
if ([string]::IsNullOrWhiteSpace($Name)) {
    $Name = [DateTime]::Now.ToString("yyyyMMdd-HHmmss")
}
dotnet build
if ($Name -eq "Remove") {
    dotnet ef migrations remove -c DefaultDbContext --no-build --project ../../Definition/EntityFramework
}
else {
    dotnet ef migrations add $Name -c DefaultDbContext --no-build --project ../../Definition/EntityFramework 
}

Set-Location $location