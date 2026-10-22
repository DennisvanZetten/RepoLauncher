param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64"
)

$ErrorActionPreference = "Stop"
$projectDirectory = Split-Path -Parent $PSScriptRoot
$projectFile = Join-Path $projectDirectory "RepoLauncher.csproj"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputDirectory = Join-Path $projectDirectory "artifacts\installer\$timestamp"
$certificate = $null

New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null

try {
    $certificate = New-SelfSignedCertificate `
        -Type Custom `
        -Subject "CN=Repo Launcher Local" `
        -KeyUsage DigitalSignature `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -TextExtension @(
            "2.5.29.37={text}1.3.6.1.5.5.7.3.3",
            "2.5.29.19={text}CA=FALSE"
        )

    dotnet publish $projectFile `
        --configuration $Configuration `
        --framework net10.0-windows10.0.19041.0 `
        --runtime $RuntimeIdentifier `
        -p:TargetFrameworks=net10.0-windows10.0.19041.0 `
        -p:RestoreSources=https://api.nuget.org/v3/index.json `
        -p:WindowsPackageType=MSIX `
        -p:AppxPackageSigningEnabled=true `
        "-p:PackageCertificateThumbprint=$($certificate.Thumbprint)" `
        "-p:PackageLocation=$outputDirectory\"

    if ($LASTEXITCODE -ne 0) {
        throw "MSIX bouwen is mislukt (exitcode $LASTEXITCODE)."
    }

    $appPackagesDirectory = Join-Path $projectDirectory "bin\$Configuration\net10.0-windows10.0.19041.0\$RuntimeIdentifier\AppPackages"
    $packageDirectory = Get-ChildItem -Path $appPackagesDirectory -Directory -Filter "*_Test" |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $packageDirectory) {
        throw "De gegenereerde MSIX-installatiemap is niet gevonden onder $appPackagesDirectory."
    }

    Copy-Item -Path (Join-Path $packageDirectory.FullName "*") -Destination $outputDirectory -Recurse -Force

    Write-Host "MSIX-installer gemaakt in: $outputDirectory" -ForegroundColor Green
    Write-Host "Voer Install.ps1 uit vanuit deze map om de app te installeren."
}
finally {
    if ($certificate) {
        Remove-Item -LiteralPath "Cert:\CurrentUser\My\$($certificate.Thumbprint)" -Force -ErrorAction SilentlyContinue
    }
}
