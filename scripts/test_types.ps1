$bytes = [System.IO.File]::ReadAllBytes('C:\Users\yusuf\.nuget\packages\microsoft.openapi\2.7.5\lib\net8.0\Microsoft.OpenApi.dll')
$asm = [System.Reflection.Assembly]::Load($bytes)

$typeSecReq = $asm.GetType('Microsoft.OpenApi.OpenApiSecurityRequirement')
Write-Output "--- OpenApiSecurityRequirement Base / Interfaces ---"
$typeSecReq.GetInterfaces() | ForEach-Object { Write-Output $_.FullName }

$typeSecRef = $asm.GetType('Microsoft.OpenApi.OpenApiSecuritySchemeReference')
Write-Output "--- OpenApiSecuritySchemeReference Constructors ---"
$typeSecRef.GetConstructors() | ForEach-Object { Write-Output $_.ToString() }
