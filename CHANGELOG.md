# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `All On` and `All Off` buttons to the base stations list.
- `Identify` menu button to identify base stations. Their LED will blink white for a while.
- Option to send messages to multiple base stations simultaneously.

### Changed
- Unnecessary menu items are now hidden based on base station version.

### Fixed
- Changed settings were not reflected to the settings page after backing to the main page.

## [1.2.1] - 2024-08-11

### Changed

- Changed the way to iterate over the base stations.
- Changed the retry logic for Bluetooth LE connection.

## [1.2.0] - 2024-04-28

### Added

- Standby mode on SteamVR shutdown.

## [1.1.1] - 2024-03-07

### Fixed

- Base stations that are paired to PC were not found.

## [1.1.0] - 2024-03-01

### Added

- Support for Base Station 1.0.
- System tray icon in the taskbar notification area.
- `Minimize the window on launched by SteamVR` setting to minimize the window.
- `Minimize to the task tray icon` setting to hide the window.
- `Enable Debug Logs` setting to output debug-level logs.

### Fixed

- Settings file was modified by multiple threads at the same time.

### Changed

- Rewrite Bluetooth LE connection process.
- Change default `Manage Base Stations` setting to `On`.
- Change the logs folder to `%APPDATA%\Local\OVR-Lighthouse-Manager\Logs`.

## [1.0.0] - 2023-12-05

Initial release.
