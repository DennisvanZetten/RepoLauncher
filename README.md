# Repo Launcher

Repo Launcher is a Windows app built with .NET MAUI Blazor Hybrid and MudBlazor. The installer is packaged as an MSIX and signed locally with a temporary self-signed certificate.

## Building the MSIX

Open PowerShell in the `RepoLauncher` directory and run:

```powershell
./installer/Build-Installer.ps1
```

The MSIX package, its public certificate, and the required Windows App Runtime dependency are placed in a new directory under `artifacts/installer/`. The build creates a local code-signing certificate in the Personal certificate store, uses its thumbprint to sign the package, and then removes the certificate. No PFX file containing a private key is created on disk.

## Installing

Trust the package certificate and install the MSIX for the current Windows user:

```powershell
./installer/Install-Installer.ps1 `
  -PackagePath "./artifacts/installer/<timestamp>/RepoLauncher_1.0.0.1_x64.msix" `
  -CertificatePath "./artifacts/installer/<timestamp>/RepoLauncher_1.0.0.1_x64.cer"
```

Use the actual version numbers in the package name, or run `Install.ps1` from the generated output directory. The local certificate includes code-signing and explicit Basic Constraints extensions so that Windows' installation script can validate it. Windows may require sideloading to be enabled. The certificate is added to `CurrentUser\TrustedPeople` so the package can be installed from this local publisher.

## Distribution note

This certificate is intended only for personal/local installation. To distribute the package to other users, you need a certificate trusted by their Windows installations or you can distribute it through the Microsoft Store. Never share a PFX file containing a private key.
