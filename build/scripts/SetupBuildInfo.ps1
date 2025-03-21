Param(
    [string]$Version,
    [switch]$IsAzurePipelineBuild = $false
)

$env:Build_RootDirectory = (Get-Item $PSScriptRoot).parent.parent.FullName
$env:msix_version = build\Scripts\CreateBuildInfo.ps1 -Version $Version -IsAzurePipelineBuild $IsAzurePipelineBuild

$appxmanifestPath = (Join-Path $env:Build_RootDirectory "GitHubExtension\Package.appxmanifest")

[Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq")
$xIdentity = [System.Xml.Linq.XName]::Get("{http://schemas.microsoft.com/appx/manifest/foundation/windows10}Identity");

# Update the appxmanifest
$appxmanifest = [System.Xml.Linq.XDocument]::Load($appxmanifestPath)
$appxmanifest.Root.Element($xIdentity).Attribute("Version").Value = $env:msix_version
$appxmanifest.Save($appxmanifestPath)
