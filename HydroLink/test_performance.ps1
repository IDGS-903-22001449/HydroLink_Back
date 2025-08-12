# Script de prueba para verificar las optimizaciones de rendimiento
# Ejecutar desde el directorio raíz del proyecto

Write-Host "🚀 Testing Performance Optimizations - HydroLink" -ForegroundColor Green
Write-Host "=" * 60

$baseUrl = "https://localhost:5001" # Cambia por tu URL
$productId = 1 # Cambia por un ID de producto válido

Write-Host "`n1. Testing producto list (should be fast without PDFs)" -ForegroundColor Yellow
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/productos" -Method GET -ContentType "application/json"
    $stopwatch.Stop()
    
    Write-Host "✅ Lista de productos obtenida en $($stopwatch.ElapsedMilliseconds)ms" -ForegroundColor Green
    Write-Host "   📦 Productos encontrados: $($response.Count)"
    
    if ($response.Count -gt 0) {
        $firstProduct = $response[0]
        Write-Host "   📝 Primer producto tiene manual: $($firstProduct.tieneManual)"
        Write-Host "   📄 PDF incluido en respuesta: $($firstProduct.manualUsuarioPdf -ne $null)" -ForegroundColor $(if($firstProduct.manualUsuarioPdf -eq $null) {"Green"} else {"Red"})
    }
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n2. Testing single product (optimized - no PDF)" -ForegroundColor Yellow
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/productos/$productId" -Method GET -ContentType "application/json"
    $stopwatch.Stop()
    
    Write-Host "✅ Producto individual obtenido en $($stopwatch.ElapsedMilliseconds)ms" -ForegroundColor Green
    Write-Host "   📦 Producto: $($response.nombre)"
    Write-Host "   📝 Tiene manual: $($response.tieneManual)"
    Write-Host "   📄 PDF incluido: $($response.manualUsuarioPdf -ne $null)" -ForegroundColor $(if($response.manualUsuarioPdf -eq $null) {"Green"} else {"Red"})
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n3. Testing single product WITH PDF (should be slower)" -ForegroundColor Yellow
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/productos/$productId?includePdf=true" -Method GET -ContentType "application/json"
    $stopwatch.Stop()
    
    Write-Host "✅ Producto con PDF obtenido en $($stopwatch.ElapsedMilliseconds)ms" -ForegroundColor Green
    Write-Host "   📦 Producto: $($response.nombre)"
    Write-Host "   📝 Tiene manual: $($response.tieneManual)"
    Write-Host "   📄 PDF incluido: $($response.manualUsuarioPdf -ne $null)" -ForegroundColor $(if($response.manualUsuarioPdf -ne $null) {"Green"} else {"Red"})
    if ($response.manualUsuarioPdf) {
        Write-Host "   📊 Tamaño PDF (approx): $([math]::Round($response.manualUsuarioPdf.Length / 1024, 2)) KB"
    }
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n4. Testing PDF-only endpoint" -ForegroundColor Yellow
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/productos/$productId/manual-pdf" -Method GET -ContentType "application/json"
    $stopwatch.Stop()
    
    Write-Host "✅ PDF específico obtenido en $($stopwatch.ElapsedMilliseconds)ms" -ForegroundColor Green
    Write-Host "   📦 Producto: $($response.nombreProducto)"
    Write-Host "   📄 PDF obtenido: $($response.manualPdf -ne $null)" -ForegroundColor $(if($response.manualPdf -ne $null) {"Green"} else {"Red"})
    if ($response.manualPdf) {
        Write-Host "   📊 Tamaño PDF (approx): $([math]::Round($response.manualPdf.Length / 1024, 2)) KB"
    }
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n" + "=" * 60
Write-Host "✨ Performance test completed!" -ForegroundColor Green
Write-Host "`nExpected results:" -ForegroundColor Cyan
Write-Host "  • Tests 1-2 should be VERY fast (< 500ms)"
Write-Host "  • Test 3 should be slower (includes full PDF)"
Write-Host "  • Test 4 should be fast and efficient for PDF only"
Write-Host "  • PDFs should only be included when explicitly requested"

Write-Host "`n📚 Usage in your frontend:" -ForegroundColor Cyan
Write-Host "  • Use /api/productos for listings (fast)"
Write-Host "  • Use /api/productos/{id} for product details (fast)" 
Write-Host "  • Use /api/productos/{id}/manual-pdf for PDF viewing (efficient)"
Write-Host "  • Check 'tieneManual' before showing manual buttons"
