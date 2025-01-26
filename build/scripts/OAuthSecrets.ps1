Param(
    [string]$ClientId,
    [string]$ClientSecret,
)

# Set GitHub OAuth Client App configuration if build-time parameters are present
$OAuthConfigFilePath = (Join-Path $env:Build_RootDirectory "GitHubExtension\Configuration\OAuthConfiguration.cs")
if (![string]::IsNullOrWhitespace($ClientId)) {
    (Get-Content $OAuthConfigFilePath).Replace("%BUILD_TIME_GITHUB_CLIENT_ID_PLACEHOLDER%", $ClientId) | Set-Content $OAuthConfigFilePath
}
else {
    Write-Host "ClientId not found at Build-time"
}

if (![string]::IsNullOrWhitespace($ClientSecret)) {
    (Get-Content $OAuthConfigFilePath).Replace("%BUILD_TIME_GITHUB_CLIENT_SECRET_PLACEHOLDER%", $ClientSecret) | Set-Content $OAuthConfigFilePath
}
else {
    Write-Host "ClientSecret not found at Build-time"
}
