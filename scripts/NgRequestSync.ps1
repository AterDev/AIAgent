# 前端请求同步脚本示例
$location = Get-Location

perigon ng https://localhost:17001/swagger/v1/swagger.json -o ../src/ClientApp/WebApp/src/app

Set-Location $location