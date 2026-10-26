$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$web = Join-Path $root 'web'
if (-not (Test-Path (Join-Path $web 'index.html'))) {
    throw "Не найден web/index.html. Запустите скрипт из клонированного репозитория."
}

$python = Get-Command py -ErrorAction SilentlyContinue
if (-not $python) { $python = Get-Command python -ErrorAction SilentlyContinue }
if (-not $python) {
    Write-Host "Python не найден. Откройте web/index.html напрямую в браузере."
    Start-Process (Join-Path $web 'index.html')
    exit 0
}

$port = 8080
while (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue) {
    $port++
    if ($port -gt 8090) { throw "Порты 8080–8090 заняты. Закройте локальные серверы и повторите." }
}

$arguments = '-m http.server ' + $port + ' --directory "' + $web + '"'
Start-Process -FilePath $python.Source -ArgumentList $arguments -WorkingDirectory $root
Start-Sleep -Seconds 2
Start-Process "http://localhost:$port"
Write-Host "Веб-версия запущена: http://localhost:$port"
