dotnet publish OVRLighthouseManager\OVRLighthouseManager.csproj -p:Configuration=Release -p:PublishProfile=win10-x64 -o OVRLighthouseManager\bin\win10-x64\publish
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "Installer\Installer.iss"
