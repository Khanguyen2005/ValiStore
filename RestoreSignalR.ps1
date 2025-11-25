# PowerShell script to restore SignalR references
# Run this in Package Manager Console: .\RestoreSignalR.ps1

Write-Host "?? Restoring SignalR packages..." -ForegroundColor Cyan

# Update specific packages to force reference refresh
Update-Package Microsoft.AspNet.SignalR -Reinstall -IgnoreDependencies
Update-Package Microsoft.AspNet.SignalR.Core -Reinstall -IgnoreDependencies
Update-Package Microsoft.AspNet.SignalR.SystemWeb -Reinstall -IgnoreDependencies
Update-Package Microsoft.Owin -Reinstall -IgnoreDependencies
Update-Package Microsoft.Owin.Host.SystemWeb -Reinstall -IgnoreDependencies
Update-Package Microsoft.Owin.Security -Reinstall -IgnoreDependencies
Update-Package Owin -Reinstall -IgnoreDependencies

Write-Host "? SignalR packages restored!" -ForegroundColor Green
Write-Host "?? Now rebuild the solution: Build -> Rebuild Solution" -ForegroundColor Yellow
