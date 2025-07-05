dotnet publish OVRLighthouseManager\OVRLighthouseManager.csproj -p:Configuration=Release -p:PublishProfile=win-x64 -o OVRLighthouseManager\bin\win-x64\publish
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "Installer\Installer.iss"
