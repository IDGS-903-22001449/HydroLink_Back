#!/usr/bin/env pwsh
# Diagnóstico de HydroLink - Script para identificar problemas de rendimiento y conectividad

Write-Host "🔍 DIAGNÓSTICO DE HYDROLINK" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
Write-Host ""

# Verificar si el backend está ejecutándose
Write-Host "📡 Verificando estado del backend..." -ForegroundColor Yellow
try {
    $backendResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/purchase/test/1" -Method GET -TimeoutSec 10
    if ($backendResponse.StatusCode -eq 200) {
        Write-Host "✅ Backend respondiendo correctamente" -ForegroundColor Green
        $content = $backendResponse.Content | ConvertFrom-Json
        Write-Host "   Producto de prueba: $($content.Nombre)" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Backend no responde en http://localhost:5000" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Verificar el frontend
Write-Host "🌐 Verificando estado del frontend..." -ForegroundColor Yellow
try {
    $frontendResponse = Invoke-WebRequest -Uri "http://localhost:4200" -Method GET -TimeoutSec 5
    if ($frontendResponse.StatusCode -eq 200) {
        Write-Host "✅ Frontend respondiendo correctamente" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Frontend no responde en http://localhost:4200" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Verificar procesos
Write-Host "🔧 Verificando procesos..." -ForegroundColor Yellow
$dotnetProcess = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -like "*HydroLink*" -or $_.ProcessName -eq "dotnet" }
if ($dotnetProcess) {
    Write-Host "✅ Proceso .NET Core detectado (PID: $($dotnetProcess[0].Id))" -ForegroundColor Green
} else {
    Write-Host "❌ No se detectó proceso .NET Core ejecutándose" -ForegroundColor Red
}

$nodeProcess = Get-Process -Name "node" -ErrorAction SilentlyContinue
if ($nodeProcess) {
    Write-Host "✅ Proceso Node.js detectado (PID: $($nodeProcess[0].Id))" -ForegroundColor Green
} else {
    Write-Host "❌ No se detectó proceso Node.js (Angular) ejecutándose" -ForegroundColor Red
}

Write-Host ""

# Verificar conectividad de red
Write-Host "🌍 Verificando conectividad de red..." -ForegroundColor Yellow
$networkAdapter = Get-NetAdapter | Where-Object { $_.Status -eq "Up" -and $_.MediaType -ne "802.11" } | Select-Object -First 1
if ($networkAdapter) {
    Write-Host "✅ Conexión de red activa: $($networkAdapter.Name)" -ForegroundColor Green
} else {
    Write-Host "⚠️  Estado de red incierto" -ForegroundColor Yellow
}

Write-Host ""

# Verificar puertos
Write-Host "🚪 Verificando puertos..." -ForegroundColor Yellow
$port5000 = Get-NetTCPConnection -LocalPort 5000 -ErrorAction SilentlyContinue
if ($port5000) {
    Write-Host "✅ Puerto 5000 en uso (Backend)" -ForegroundColor Green
} else {
    Write-Host "❌ Puerto 5000 no está en uso" -ForegroundColor Red
}

$port4200 = Get-NetTCPConnection -LocalPort 4200 -ErrorAction SilentlyContinue
if ($port4200) {
    Write-Host "✅ Puerto 4200 en uso (Frontend)" -ForegroundColor Green
} else {
    Write-Host "❌ Puerto 4200 no está en uso" -ForegroundColor Red
}

Write-Host ""

# Estadísticas de rendimiento del sistema
Write-Host "⚡ Rendimiento del sistema..." -ForegroundColor Yellow
$cpu = Get-WmiObject -Class Win32_Processor | Measure-Object -Property LoadPercentage -Average
$memory = Get-WmiObject -Class Win32_OperatingSystem
$memoryUsage = [math]::Round((($memory.TotalVisibleMemorySize - $memory.FreePhysicalMemory) / $memory.TotalVisibleMemorySize) * 100, 2)

Write-Host "   CPU: $([math]::Round($cpu.Average, 2))% de uso promedio" -ForegroundColor Gray
Write-Host "   RAM: $($memoryUsage)% en uso" -ForegroundColor Gray

if ($cpu.Average -gt 80) {
    Write-Host "⚠️  CPU con alta utilización" -ForegroundColor Yellow
}
if ($memoryUsage -gt 85) {
    Write-Host "⚠️  RAM con alta utilización" -ForegroundColor Yellow
}

Write-Host ""

# Recomendaciones
Write-Host "💡 RECOMENDACIONES:" -ForegroundColor Cyan
Write-Host "==================" -ForegroundColor Cyan

if (-not $backendResponse) {
    Write-Host "1. Inicie el backend con:" -ForegroundColor White
    Write-Host "   cd `"C:\Users\kevin\source\repos\HydroLink\HydroLink`"" -ForegroundColor Gray
    Write-Host "   dotnet run" -ForegroundColor Gray
    Write-Host ""
}

if (-not $frontendResponse) {
    Write-Host "2. Inicie el frontend con:" -ForegroundColor White
    Write-Host "   cd `"D:\Documentos\INGENIERIA\9no Cuatri\Desarrollo WEB Integral\HydroLink`"" -ForegroundColor Gray
    Write-Host "   ng serve" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "3. Para probar la compra optimizada:" -ForegroundColor White
Write-Host "   - Use datos de tarjeta válidos (mínimo 13 dígitos)" -ForegroundColor Gray
Write-Host "   - Verifique que el cliente tenga perfil completo" -ForegroundColor Gray
Write-Host "   - Monitor los logs del backend para errores" -ForegroundColor Gray

Write-Host ""
Write-Host "🔚 Diagnóstico completado" -ForegroundColor Cyan
