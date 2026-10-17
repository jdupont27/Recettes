@echo off
echo --- Deploiement de RecetteApp sur Fly.io ---

set FLY="C:\Users\sephi\AppData\Local\Microsoft\WinGet\Packages\Fly-io.flyctl_Microsoft.Winget.Source_8wekyb3d8bbwe\flyctl.exe"

%FLY% deploy --app recetteapp

echo --- TERMINE ! ---
pause
