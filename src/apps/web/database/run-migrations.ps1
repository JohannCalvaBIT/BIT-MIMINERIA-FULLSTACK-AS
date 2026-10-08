<#
.SYNOPSIS
Ejecuta los scripts SQL versionados en orden (Script-Only, G-DB-02).

.PARAMETER Server
Instancia SQL Server o Azure SQL.

.PARAMETER Database
Base de datos destino.

.PARAMETER Username
Usuario SQL (local). Si se omite, se asume autenticación integrada/Managed Identity.

.PARAMETER Password
Password SQL (local). Solo para desarrollo; en producción usar Managed Identity.
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$Server,

    [Parameter(Mandatory = $true)]
    [string]$Database,

    [string]$Username,

    [string]$Password
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$files = Get-ChildItem -Path "$root\Migrations" -Filter 'v*.sql' | Sort-Object Name

$authArgs = @('-S', $Server, '-d', $Database, '-C')
if ($Username) {
    $authArgs += @('-U', $Username, '-P', $Password)
}

foreach ($file in $files) {
    if ($file.Name -like '*_rollback.sql') { continue }
    Write-Host "Ejecutando $($file.Name)..."
    sqlcmd @authArgs -i $file.FullName -b
    if ($LASTEXITCODE -ne 0) {
        throw "Migración falló: $($file.Name)"
    }
}

Write-Host 'Migraciones aplicadas correctamente.'
