#!/bin/bash
set -eu

VERSION=${1}

sed -i -b -e "s/\[Unreleased\]/\[${VERSION}\] - $(date -I)/g" CHANGELOG*.md
sed -i -b -e "s/#define MyAppVersion \".*\"/#define MyAppVersion \"${VERSION}\"/g" Installer/Installer.iss
sed -i -b -e "s/<Version>.*<\/Version>/<Version>${VERSION}<\/Version>/g" OVRLighthouseManager/OVRLighthouseManager.csproj

git add CHANGELOG*.md Installer/Installer.iss OVRLighthouseManager/OVRLighthouseManager.csproj
git commit -m "Version ${VERSION}"
git tag "${VERSION}"
