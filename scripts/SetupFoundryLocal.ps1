# Setup / start Microsoft Foundry Local and verify chat model
# Usage:
#   pwsh ./scripts/SetupFoundryLocal.ps1                           # 默认安装 + 启动 + 下载默认 chat 模型
#   pwsh ./scripts/SetupFoundryLocal.ps1 -ChatModel qwen3-0.6b     # 指定 chat 模型
#
# 执行内容：
# 1. winget 安装 Foundry Local（已安装则跳过）
# 2. foundry service start
# 3. foundry service status 获取实际 endpoint（默认 http://127.0.0.1:55655）
# 4. foundry model run <chat>        （首次自动下载）
# 5. foundry model list 校验
#
# 备注：
# - Foundry Local 1.x 目录当前仅提供 CPU 聊天/工具模型（如 qwen3-0.6b），暂未发布 embedding 模型，
#   因此脚本不再种子 embedding；RAG/向量化请继续使用 OpenAI / DeepSeek / Azure 等兼容 embedding provider。
# - MigrationService 种子 AIModelProvider.BaseUrl 为 http://127.0.0.1:55655/v1，
#   若实际端口不同（通过 `foundry service status` 查看），需要在 AdminService 管理页面或 SQL 更新该 Provider 的 BaseUrl。

[CmdletBinding()]
param(
    [string] $ChatModel = "qwen3-0.6b"
)

$ErrorActionPreference = "Stop"

function Write-Step($msg) { Write-Host "==> $msg" -ForegroundColor Cyan }

# 1. Install
if (-not (Get-Command foundry -ErrorAction SilentlyContinue)) {
    Write-Step "Installing Microsoft.FoundryLocal via winget..."
    winget install --id Microsoft.FoundryLocal --accept-package-agreements --accept-source-agreements
    # 刷新 PATH（当前会话）
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")
} else {
    Write-Step "Foundry Local already installed."
}

# 2. Start service
Write-Step "Starting Foundry Local service..."
try {
    foundry service start | Out-Null
} catch {
    Write-Warning "foundry service start failed: $_"
}

# 3. Status
Write-Step "Fetching service status..."
$status = foundry service status
$status | Out-Host

$endpoint = $null
foreach ($line in $status) {
    if ($line -match "http://[^\s]+") {
        $endpoint = $Matches[0]
        break
    }
}

if ([string]::IsNullOrWhiteSpace($endpoint)) {
    Write-Warning "Could not auto-detect endpoint. Default assumed: http://127.0.0.1:55655"
    $endpoint = "http://127.0.0.1:55655"
}

$baseUrl = ($endpoint.TrimEnd('/')) + "/v1"
Write-Step "Detected BaseUrl: $baseUrl"

# 4. Chat model
Write-Step "Pulling chat model: $ChatModel"
foundry model run $ChatModel --non-interactive 2>$null | Out-Null

# 5. Verify
Write-Step "Verifying models..."
foundry model list | Out-Host

Write-Host ""
Write-Host "Foundry Local ready" -ForegroundColor Green
Write-Host "   BaseUrl: $baseUrl" -ForegroundColor Green
Write-Host "   Chat model: $ChatModel" -ForegroundColor Green
Write-Host ""
Write-Host "如果实际端口与种子值 http://127.0.0.1:55655/v1 不一致，请在 AdminService 中更新 FoundryLocal 提供商 BaseUrl。" -ForegroundColor Yellow
