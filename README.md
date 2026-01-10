# SecVers Debloat (Win11Debloater)

![License](https://img.shields.io/badge/License-BSD%203--Clause-blue.svg)
![Status](https://img.shields.io/badge/Status-Early%20Development-orange)
![Platform](https://img.shields.io/badge/Platform-Windows%2010%20%2F%2011-blue)
![Tech](https://img.shields.io/badge/Made%20with-C%23%20%2F%20WPF%20%2F%20iNKORE.UI-purple)

**SecVers Debloat** is a modern, modular Windows optimization tool built to remove bloatware, reduce telemetry, harden security, and improve system performance on Windows 10/11.

Built with **C#**, **WPF**, and the **iNKORE.UI** library, it delivers a Fluent Design-inspired UI (similar to AtlasOS/Settings) while remaining a standalone desktop application. A dual-engine scripting system lets you automate tasks using both PowerShell and JavaScript.

> [!WARNING]
> **USE AT YOUR OWN RISK**
> This tool modifies core system configurations, Registry keys, and hardware identifiers. Always create a **System Restore Point** before applying changes.

---

## SecVersLHE Toolkit

SecVers Debloat is part of the **SecVersLHE** toolkit. For the full toolkit overview, related components, and usage guidance, see the project wiki: https://github.com/bunbunconmeow/Win11Debloater/wiki

---

## Screenshots

| Dashboard | Debloater Engine |
|:---:|:---:|
| ![Home](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/HomeScreen.png?raw=true) | ![Debloater](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/Debloater.png?raw=true) |

| System Hardening | Defender & Exclusions |
|:---:|:---:|
| ![Hardening](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/SystemHardening.png?raw=true) | ![Defender](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/DefenderOptions.png?raw=true) |

<details>
<summary><strong>View more screenshots (Script Engine, Anonymizer, Installer...)</strong></summary>

| Registry & UI Tweaks | Anonymizer (HWID) |
|:---:|:---:|
| ![Registry](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/RegistryTweaks.png?raw=true) | ![Anonymizer](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/Anonymizer.png?raw=true) |

| Software Installer | Script Library |
|:---:|:---:|
| ![Installer](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/SoftwareInstaller.png?raw=true) | ![Scripts](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/CommunityScripts.png?raw=true) |

</details>

---

## Features

### 🚀 Smart Debloater Engine
A native removal engine designed for speed and reliability.
*   **Bulk App Removal:** Remove pre-installed UWP bloatware (Cortana, Teams, BingWeather, Feedback Hub, etc.).
*   **Search & Filter:** Quickly find specific packages to remove via the search bar.
*   **Tabbed Interface:** Organized view of Core Apps, System Apps, and optional Features.

### 📜 Scripting & Automation (Dual-Engine)
An advanced environment to run community or custom tweaks safely.
*   **JavaScript Interpreter:** Native support for executing `.js` automation scripts for system tasks.
*   **PowerShell Integration:** Execute standard `.ps1` scripts directly from the UI.
*   **Import System:** Bring in local scripts to build your own library.

### 🕵️ Anonymizer & HWID Spoofing
Advanced privacy tools to randomize system fingerprints and identifiers.
*   **HWID Management:** Randomize Machine GUIDs, Product IDs, and Serial Numbers.
*   **Privacy Reset:** Clear/Reset Advertising ID and Telemetry ID.
*   **Registration Data:** Randomize "Registered Owner" and "Organization" info.
*   **Install Date:** Randomize the Windows Installation Date to obfuscate system age.
*   *> Note: Spoofing HWIDs may affect software licenses (e.g., Windows Activation, Games).*

### 🛡️ System Hardening & Defender Control
Granular control over Windows Security beyond the standard Settings app.
*   **Exclusions Manager:** View and remove file, folder, and process exclusions that malware might hide in.
*   **Hardening Presets:** One-click profiles (Standard, Strictly Secure, Gaming).
*   **Security Tweaks:** Configure Real-time Protection, Cloud Protection, SmartScreen, and ASR (Attack Surface Reduction) rules.

### 🎨 Registry & UI Customization
Restoring the classic feel of Windows.
*   **Classic Context Menu:** Restore the Windows 10 "Full" right-click menu (requires Explorer restart).
*   **Taskbar Cleanup:** Disable Widgets, Chat Icons, and Search bars.
*   **Impact Analysis:** The UI shows risk and impact levels for every tweak before applying.

### 📦 Software Installer
Quickly deploy essential runtimes and software (winget-based).
*   **Runtimes:** .NET Desktop Runtime (6 & 8), Visual C++ 2015-2022 Redist (x64/x86), DirectX, Java Runtime (JRE).
*   **Browsers & Tools:** Install Chrome, Firefox, VS Code, and other essentials in seconds.

---

## Installation

1.  Download the latest release from the [Releases](https://github.com/bunbunconmeow/Win11Debloater/releases) page.
2.  Run `SecVers_Debloat.exe` as **Administrator**.

## Requirements

*   **OS:** Windows 10 or Windows 11
*   **Runtime:** .NET Framework 4.8 or higher (Desktop Runtime)

## Development

*   **Language:** C#
*   **Framework:** WPF (.NET Framework 4.8)
*   **UI Library:** [iNKORE.UI.WPF.Modern](https://github.com/iNKORE-NET/UI.WPF.Modern)
*   **IDE:** Visual Studio 2022

## Disclaimer

This software is provided "as is", without warranty of any kind. The authors are not responsible for any damage to your computer, data loss, or license invalidation caused by the use of this tool.
Some features (especially HWID Spoofing) may violate Terms of Service of certain software vendors.

---
*Made with ❤️ by the SecVers team*
