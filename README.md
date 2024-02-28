[ English | [日本語](./README_JP.md) ]

# OVR Lighthouse Manager

A tool to manage the power of SteamVR base stations.

You can control the power of the base stations without HTC Vive or Valve Index by linking it to the start and end of SteamVR.

<a href="https://kurotu.booth.pm/items/5315515">
    <img src="https://asset.booth.pm/static-images/banner/200x40_01.png" alt="Booth"></img>
</a>
<a href="https://kurotu.gumroad.com/l/uaqwv">
    <img src="https://img.shields.io/badge/GUMROAD-36a9ae?style=for-the-badge&logo=gumroad&logoColor=ping&labelColor=black&color=black" height="40px" alt="Gumroad"></img>
</a>

<img src="./Screenshots/Screenshot-EN-Light.png" alt="OVR Lighthouse Manager" width="489px" ></img>

## Introduction

SteamVR has a feature to automatically turn on the base stations when SteamVR starts and sleep them when SteamVR ends. However, this feature does not work without HTC VIVE or Valve Index.
OVR Lighthouse Manager allows you to control the power of base stations without HTC Vive or Valve Index with Bluetooth LE.
It would be useful when using the combination of standalone headsets (Quest, PICO, etc.) and VIVE trackers.

## Features

- Power control of base stations (power on, sleep, standby) with Bluetooth LE.
- Automated power control of base stations linked to the start and end of SteamVR

## Required Environment

- Windows 11
- Windows 10 version 1809 or later
- SteamVR Base Station 1.0 or 2.0
- Bluetooth LE

## How to Use

### Initial Setup

1. Start OVR Lighthouse Manager from the start menu.
2. The surrounding base stations are automatically listed.
3. Turn on **Manage Base Stations**.
4. Select base stations you want to link to the start and end of SteamVR from the list.

> [!NOTE]
> Enter ID (8 characters) printed on the back label when using base station 1.0.

After above setup, the software automatically controls the power of base stations by linking to the start and end of SteamVR.
Registration with startup or serviceization is not required.

## License

[GPLv3](./LICENSE)

## Contact

- VRCID: kurotu
- X: [@kurotu](https://twitter.com/kurotu)
- GitHub: [kurotu/OVR-Lighthouse-Manager](https://github.com/kurotu/OVR-Lighthouse-Manager)
