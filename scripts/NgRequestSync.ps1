# 前端请求同步脚本 - 从 AdminService 和 ApiService 生成 Angular 请求服务
$location = Get-Location

Write-Host "🔄 Generating Angular request services from AdminService..." -ForegroundColor Cyan
perigon generate request https://localhost:17001/swagger/v1/swagger.json ../src/ClientApp/WebApp/src/app/services -t angular

Write-Host "🔄 Generating Angular request services from ApiService..." -ForegroundColor Cyan
perigon generate request https://localhost:17002/swagger/v1/swagger.json ../src/ClientApp/WebApp/src/app/services -t angular

Write-Host "✅ Angular request services generated successfully!" -ForegroundColor Green

Set-Location $location