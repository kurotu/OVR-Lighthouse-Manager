# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
