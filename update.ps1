# Script per ricompilare, aggiornare lo zip e inviare le modifiche su GitHub con un solo comando!
param(
    [string]$message = "Update plugin"
)

Write-Host "Compilazione in corso..." -ForegroundColor Cyan
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Host "Errore durante la compilazione. Aggiornamento annullato." -ForegroundColor Red
    exit 1
}

Write-Host "Aggiornamento di latest.zip..." -ForegroundColor Cyan
Copy-Item "bin\Release\SeeInWiki\latest.zip" "latest.zip" -Force

Write-Host "Invio su GitHub..." -ForegroundColor Cyan
git add .
git commit -m $message
git push

Write-Host "Fatto! Modifiche caricate su GitHub con successo." -ForegroundColor Green
