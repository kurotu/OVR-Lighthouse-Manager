dotnet dotnet-project-licenses -i OVRLighthouseManager -t --use-project-assets-json -m
echo "OVR Lighthouse Manager uses the following third party libraries:" > ThirdPartyLicenses.md
echo "" >> ThirdPartyLicenses.md
cat licenses.md >> ThirdPartyLicenses.md
rm licenses.md
