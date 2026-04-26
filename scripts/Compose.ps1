[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet("Generate", "Build", "Up", "Down", "Ps", "Logs", "All")]
    [string]$Action = "All",

    [Parameter()]
    [ValidateSet("podman", "docker")]
    [string]$Runtime = "podman",

    [Parameter()]
    [string]$OutputPath,

    [Parameter()]
    [string]$ProjectName = "aiagent",

    [Parameter()]
    [string]$Tag = "local",

    [Parameter()]
    [ValidateSet("Development", "Production")]
    [string]$AppEnvironment = "Development"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $scriptRoot ".."))
$appHostProject = Join-Path $repoRoot "src\AppHost\AppHost.csproj"
$appSettingsPath = Join-Path $repoRoot "src\AppHost\appsettings.Development.json"

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repoRoot "artifacts\compose"
}

function Write-Step {
    param([string]$Message)

    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Assert-Command {
    param([string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Command '$Name' was not found in PATH."
    }
}

function New-Directory {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        $null = New-Item -ItemType Directory -Path $Path -Force
    }
}

function Write-Utf8File {
    param(
        [string]$Path,
        [string]$Content
    )

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Content.TrimStart("`r", "`n"), $utf8NoBom)
}

function Get-JsonWithComments {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        return @{}
    }

    $content = Get-Content -Raw -Path $Path
    $content = [System.Text.RegularExpressions.Regex]::Replace($content, '(?m)^\s*//.*$', '')

    if ([string]::IsNullOrWhiteSpace($content)) {
        return @{}
    }

    return $content | ConvertFrom-Json -AsHashtable
}

function Get-ComposeConfig {
    $settings = Get-JsonWithComments -Path $appSettingsPath
    $components = if ($settings.ContainsKey("Components")) { $settings["Components"] } else { @{} }
    $databaseType = if ($components.ContainsKey("Database")) { [string]$components["Database"] } else { "PostgreSQL" }
    $vectorStoreType = if ($components.ContainsKey("VectorStore")) { [string]$components["VectorStore"] } else { "Qdrant" }

    return [ordered]@{
        DatabaseType = $databaseType
        RuntimeBaseImage = "mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra"
        ContainerOs = "linux"
        ContainerArch = "x64"
        NuGetCacheRoot = (Join-Path $repoRoot ".cache\nuget")
        DbPort = if ($databaseType -eq "SqlServer") { 11433 } else { 15432 }
        CachePort = 16379
        QdrantPort = 16333
        QdrantGrpcPort = 16334
        NatsPort = 14222
        OllamaPort = 11434
        ApiPort = 15002
        AdminPort = 15001
        FileProcessorPort = 15003
        DevPassword = "AIAgent_Dev@{0}" -f ([DateTime]::UtcNow.ToString("yyyy"))
        NatsPassword = "AIAgent_Nats_{0}" -f ([DateTime]::UtcNow.ToString("yyyy"))
        OllamaSeedBaseUrl = "http://ollama:11434/v1"
        FoundryLocalSeedBaseUrl = "http://host.containers.internal:55655/v1"
        VectorStoreType = $vectorStoreType
    }
}

function Get-ImageMap {
    return [ordered]@{
        ADMINSERVICE_IMAGE = "localhost/$ProjectName-admin:$Tag"
        APISERVICE_IMAGE = "localhost/$ProjectName-api:$Tag"
        FILEPROCESSORSERVICE_IMAGE = "localhost/$ProjectName-fileprocessor:$Tag"
        MIGRATIONSERVICE_IMAGE = "localhost/$ProjectName-migration:$Tag"
    }
}
function Get-NuGetBuildEnvironment {
    param([hashtable]$Config)

    $packagesPath = Join-Path $Config.NuGetCacheRoot "packages"
    $httpCachePath = Join-Path $Config.NuGetCacheRoot "v3-cache"
    $scratchPath = Join-Path $Config.NuGetCacheRoot "scratch"
    $tempPath = Join-Path $Config.NuGetCacheRoot "temp"

    foreach ($path in @($Config.NuGetCacheRoot, $packagesPath, $httpCachePath, $scratchPath, $tempPath)) {
        New-Directory -Path $path
    }

    return [ordered]@{
        NUGET_PACKAGES = $packagesPath
        NUGET_HTTP_CACHE_PATH = $httpCachePath
        NUGET_SCRATCH = $scratchPath
        TEMP = $tempPath
        TMP = $tempPath
    }
}

function Get-ServiceDefinitions {
    return @(
        [ordered]@{
            ComposeService = "migrationservice"
            ProjectPath = "src/Services/MigrationService/MigrationService.csproj"
            Repository = "$ProjectName-migration"
            ImageKey = "MIGRATIONSERVICE_IMAGE"
        },
        [ordered]@{
            ComposeService = "apiservice"
            ProjectPath = "src/Services/ApiService/ApiService.csproj"
            Repository = "$ProjectName-api"
            ImageKey = "APISERVICE_IMAGE"
        },
        [ordered]@{
            ComposeService = "adminservice"
            ProjectPath = "src/Services/AdminService/AdminService.csproj"
            Repository = "$ProjectName-admin"
            ImageKey = "ADMINSERVICE_IMAGE"
        },
        [ordered]@{
            ComposeService = "fileprocessorservice"
            ProjectPath = "src/Services/FileProcessorService/FileProcessorService.csproj"
            Repository = "$ProjectName-fileprocessor"
            ImageKey = "FILEPROCESSORSERVICE_IMAGE"
        }
    )
}

function Invoke-AspirePublish {
    Assert-Command -Name "aspire"
    New-Directory -Path $OutputPath

    Write-Step "Publishing Docker Compose artifacts with Aspire into $OutputPath"
    & aspire publish --apphost $appHostProject --output-path $OutputPath --environment $AppEnvironment --non-interactive
    if ($LASTEXITCODE -ne 0) {
        throw "'aspire publish' failed."
    }
}

function Write-ComposeEnvFile {
    param(
        [hashtable]$Config,
        [hashtable]$ImageMap
    )

    Write-Utf8File -Path (Join-Path $OutputPath ".env") -Content @"
ADMINSERVICE_IMAGE=$($ImageMap.ADMINSERVICE_IMAGE)
ADMINSERVICE_PORT=8080
APISERVICE_IMAGE=$($ImageMap.APISERVICE_IMAGE)
APISERVICE_PORT=8080
DEV_PASSWORD=$($Config.DevPassword)
FILEPROCESSORSERVICE_IMAGE=$($ImageMap.FILEPROCESSORSERVICE_IMAGE)
FILEPROCESSORSERVICE_PORT=8080
MIGRATIONSERVICE_IMAGE=$($ImageMap.MIGRATIONSERVICE_IMAGE)
NATS_PASSWORD=$($Config.NatsPassword)
API_HOST_PORT=$($Config.ApiPort)
ADMIN_HOST_PORT=$($Config.AdminPort)
FILEPROCESSOR_HOST_PORT=$($Config.FileProcessorPort)
DB_HOST_PORT=$($Config.DbPort)
CACHE_HOST_PORT=$($Config.CachePort)
NATS_HOST_PORT=$($Config.NatsPort)
QDRANT_HTTP_HOST_PORT=$($Config.QdrantPort)
QDRANT_GRPC_HOST_PORT=$($Config.QdrantGrpcPort)
OLLAMA_HOST_PORT=$($Config.OllamaPort)
OLLAMA_SEED_BASE_URL=$($Config.OllamaSeedBaseUrl)
FOUNDRYLOCAL_SEED_BASE_URL=$($Config.FoundryLocalSeedBaseUrl)
"@
}

function Test-ComposeServicePresent {
    param(
        [string]$ComposeContent,
        [string]$ServiceName
    )

    return [System.Text.RegularExpressions.Regex]::IsMatch(
        $ComposeContent,
        "(?m)^  $([System.Text.RegularExpressions.Regex]::Escape($ServiceName)):$"
    )
}

function Add-ComposeServiceBlock {
    param(
        [System.Collections.Generic.List[string]]$Lines,
        [string]$ComposeContent,
        [string]$ServiceName,
        [string[]]$BlockLines
    )

    if (-not (Test-ComposeServicePresent -ComposeContent $ComposeContent -ServiceName $ServiceName)) {
        return
    }

    $Lines.Add("  ${ServiceName}:")
    foreach ($line in $BlockLines) {
        $Lines.Add("    $line")
    }
}

function Write-ComposeOverride {
    param(
        [hashtable]$Config,
        [string]$ComposeContent
    )

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add("services:")

    $databaseContainerPort = if ($Config.DatabaseType -eq "SqlServer") { 1433 } else { 5432 }
    Add-ComposeServiceBlock -Lines $lines -ComposeContent $ComposeContent -ServiceName "database" -BlockLines @(
        "ports:",
        ('  - "${DB_HOST_PORT}:' + $databaseContainerPort + '"')
    )

    Add-ComposeServiceBlock -Lines $lines -ComposeContent $ComposeContent -ServiceName "cache" -BlockLines @(
        "ports:",
        '  - "${CACHE_HOST_PORT}:6379"'
    )

    Add-ComposeServiceBlock -Lines $lines -ComposeContent $ComposeContent -ServiceName "nats" -BlockLines @(
        "ports:",
        '  - "${NATS_HOST_PORT}:4222"'
    )

    if ($Config.VectorStoreType -eq "Qdrant") {
        Add-ComposeServiceBlock -Lines $lines -ComposeContent $ComposeContent -ServiceName "qdrant" -BlockLines @(
            "ports:",
            '  - "${QDRANT_HTTP_HOST_PORT}:6333"',
            '  - "${QDRANT_GRPC_HOST_PORT}:6334"'
        )
    }

    Add-ComposeServiceBlock -Lines $lines -ComposeContent $ComposeContent -ServiceName "ollama" -BlockLines @(
        "ports:",
        '  - "${OLLAMA_HOST_PORT}:11434"'
    )

    Add-ComposeServiceBlock -Lines $lines -ComposeContent $ComposeContent -ServiceName "migrationservice" -BlockLines @(
        "pull_policy: never",
        "environment:",
        '  AIAgent__Seed__OllamaBaseUrl: "${OLLAMA_SEED_BASE_URL}"',
        '  AIAgent__Seed__FoundryLocalBaseUrl: "${FOUNDRYLOCAL_SEED_BASE_URL}"'
    )

    Add-ComposeServiceBlock -Lines $lines -ComposeContent $ComposeContent -ServiceName "apiservice" -BlockLines @(
        "pull_policy: never",
        "ports:",
        '  - "${API_HOST_PORT}:${APISERVICE_PORT}"'
    )

    Add-ComposeServiceBlock -Lines $lines -ComposeContent $ComposeContent -ServiceName "adminservice" -BlockLines @(
        "pull_policy: never",
        "ports:",
        '  - "${ADMIN_HOST_PORT}:${ADMINSERVICE_PORT}"'
    )

    Add-ComposeServiceBlock -Lines $lines -ComposeContent $ComposeContent -ServiceName "fileprocessorservice" -BlockLines @(
        "pull_policy: never",
        "ports:",
        '  - "${FILEPROCESSOR_HOST_PORT}:${FILEPROCESSORSERVICE_PORT}"'
    )

    Write-Utf8File -Path (Join-Path $OutputPath "docker-compose.override.yaml") -Content ($lines -join [Environment]::NewLine)
}

function New-ComposeArtifacts {
    param([hashtable]$Config)

    Invoke-AspirePublish

    $composePath = Join-Path $OutputPath "docker-compose.yaml"
    $composeContent = Get-Content -Raw -Path $composePath
    $imageMap = Get-ImageMap

    Write-ComposeEnvFile -Config $Config -ImageMap $imageMap
    Write-ComposeOverride -Config $Config -ComposeContent $composeContent
}

function Invoke-ContainerBuild {
    param(
        [hashtable]$Config,
        [hashtable]$Service,
        [string]$Image
    )

    $imageParts = $Image.Split(':', 2)
    $repository = $imageParts[0]
    $tag = if ($imageParts.Count -gt 1) { $imageParts[1] } else { "latest" }
    $nugetEnvironment = Get-NuGetBuildEnvironment -Config $Config
    $originalEnvironment = @{}

    foreach ($entry in $nugetEnvironment.GetEnumerator()) {
        $originalEnvironment[$entry.Key] = [Environment]::GetEnvironmentVariable($entry.Key, "Process")
        [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value, "Process")
    }

    Write-Step "Publishing container image $Image"
    try {
        & dotnet publish $Service.ProjectPath `
            -c Release `
            --os $Config.ContainerOs `
            --arch $Config.ContainerArch `
            /t:PublishContainer `
            -p:EnableSdkContainerSupport=true `
            -p:ContainerRepository=$repository `
            -p:ContainerImageTag=$tag `
            -p:ContainerBaseImage=$($Config.RuntimeBaseImage) `
            -p:NuGetAudit=false `
            -p:DebugSymbols=false `
            -p:DebugType=None
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to build image '$Image'."
        }
    }
    finally {
        foreach ($entry in $originalEnvironment.GetEnumerator()) {
            [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value, "Process")
        }
    }
}

function Invoke-Compose {
    param([string[]]$Arguments)

    & $Runtime compose --env-file (Join-Path $OutputPath ".env") -f (Join-Path $OutputPath "docker-compose.yaml") -f (Join-Path $OutputPath "docker-compose.override.yaml") @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "'$Runtime compose $($Arguments -join ' ')' failed."
    }
}

Assert-Command -Name $Runtime

$config = Get-ComposeConfig
$images = Get-ImageMap
$services = Get-ServiceDefinitions

$needsArtifacts = $Action -in @("Generate", "Build", "Up", "All")
$needsBuild = $Action -in @("Build", "All")
$needsUp = $Action -in @("Up", "All")

if ($needsArtifacts) {
    New-ComposeArtifacts -Config $config
}

if ($needsBuild) {
    foreach ($service in $services) {
        Invoke-ContainerBuild -Config $config -Service $service -Image $images[$service.ImageKey]
    }
}

if ($needsUp) {
    Invoke-Compose -Arguments @("up", "-d")
    Invoke-Compose -Arguments @("ps")
}

switch ($Action) {
    "Down" {
        Invoke-Compose -Arguments @("down", "--remove-orphans")
    }
    "Ps" {
        Invoke-Compose -Arguments @("ps")
    }
    "Logs" {
        Invoke-Compose -Arguments @("logs", "--tail", "200")
    }
}
