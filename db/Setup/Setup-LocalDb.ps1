<#
.SYNOPSIS
    Stands up the legacy database on SQL Server LocalDB (installed with Visual Studio's
    ".NET desktop development" / "ASP.NET and web development" workload, or standalone
    via the "SqlLocalDB.msi" from Microsoft).

.USAGE
    From a Developer PowerShell / regular PowerShell prompt, from the repo root:
        .\db\Setup\Setup-LocalDb.ps1

    Re-run any time; every script here is idempotent (CREATE OR ALTER / IF NOT EXISTS / MERGE).
#>

$ErrorActionPreference = "Stop"

$instance = "(localdb)\MSSQLLocalDB"
$database = "SettlementFeeDb"
$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

function Invoke-Script {
    param([string]$Path, [string]$Database = "master")
    Write-Host "Running $Path ..." -ForegroundColor Cyan
    sqlcmd -S $instance -d $Database -i $Path -b
    if ($LASTEXITCODE -ne 0) {
        throw "sqlcmd failed on $Path (exit code $LASTEXITCODE)"
    }
}

# Make sure the LocalDB instance is actually running
sqlcmd -S $instance -Q "SELECT 1" -b | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Starting LocalDB instance MSSQLLocalDB..." -ForegroundColor Yellow
    sqllocaldb start "MSSQLLocalDB"
}

Invoke-Script -Path (Join-Path $repoRoot "db\Setup\00_CreateDatabase.sql")
Invoke-Script -Path (Join-Path $repoRoot "db\Seed\CurrencyFeeFloor.sql") -Database $database
Invoke-Script -Path (Join-Path $repoRoot "db\Procedures\usp_CalculateSettlementFee.sql") -Database $database

Write-Host "`nDone. Connection string:" -ForegroundColor Green
Write-Host "Server=(localdb)\MSSQLLocalDB;Database=SettlementFeeDb;Trusted_Connection=True;TrustServerCertificate=True;"
