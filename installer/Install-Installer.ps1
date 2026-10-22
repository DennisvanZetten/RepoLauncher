param(
    [Parameter(Mandatory = $true)]
    [string]$PackagePath,

    [Parameter(Mandatory = $true)]
    [string]$CertificatePath
)

$ErrorActionPreference = "Stop"

$package = (Resolve-Path -LiteralPath $PackagePath).Path
$certificateFile = (Resolve-Path -LiteralPath $CertificatePath).Path
$certificate = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($certificateFile)

if ($certificate.Subject -ne "CN=Repo Launcher Local") {
    throw "Het certificaat heeft een onverwachte uitgever: $($certificate.Subject)"
}

$trustedPeople = Get-ChildItem Cert:\CurrentUser\TrustedPeople |
    Where-Object { $_.Thumbprint -eq $certificate.Thumbprint }

if (-not $trustedPeople) {
    Import-Certificate -FilePath $certificateFile -CertStoreLocation "Cert:\CurrentUser\TrustedPeople" | Out-Null
}

$packageDirectory = Split-Path -Parent $package
$x64RuntimeDependency = Join-Path $packageDirectory "Dependencies\x64\Microsoft.WindowsAppRuntime.1.7.msix"

if (Test-Path -LiteralPath $x64RuntimeDependency) {
    Add-AppxPackage -Path $package -DependencyPath $x64RuntimeDependency
}
else {
    Add-AppxPackage -Path $package
}

Write-Host "Repo Launcher is geïnstalleerd voor de huidige gebruiker." -ForegroundColor Green
