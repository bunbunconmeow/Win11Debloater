# Win11Debloater & System Hardener

![License](https://img.shields.io/badge/License-BSD%203--Clause-blue.svg)
![Status](https://img.shields.io/badge/Status-Early%20Development-orange)
![Platform](https://img.shields.io/badge/Platform-Windows%2011-blue)
![Tech](https://img.shields.io/badge/Made%20with-C%23%20%2F%20WPF%20%2F%20iNKORE.UI-purple)

A modern, modular, and security-focused Windows 11 optimization tool. Built with **WPF** and **iNKORE.UI**, it offers a sleek user interface similar to AtlasOS but acts as a standalone application to debloat, harden, and customize your system.

> [!WARNING]
> **EARLY DEVELOPMENT**
> This project is currently in the early stages of development. Features are subject to change, and bugs may be present. **Always create a System Restore Point before using any modules.**

---

## Screenshots

| Home & Dashboard | Debloater |
|:---:|:---:|
| ![Home](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/HomeScreen.png?raw=true) | ![Debloater](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/Debloater.png?raw=true) |

| System Hardening | Defender Options |
|:---:|:---:|
| ![Hardening](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/SystemHardening.png?raw=true) | ![Defender](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/DefenderOptions.png?raw=true) |

<details>
<summary><strong>View more screenshots (Registry, Anonymizer, Installer...)</strong></summary>

| Registry Tweaks | Anonymizer |
|:---:|:---:|
| ![Registry](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/RegistryTweaks.png?raw=true) | ![Anonymizer](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/Anonymizer.png?raw=true) |

| Software Installer | Community Scripts |
|:---:|:---:|
| ![Installer](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/SoftwareInstaller.png?raw=true) | ![Scripts](https://github.com/bunbunconmeow/Win11Debloater/blob/main/GitHub/Images/CommunityScripts.png?raw=true) |

</details>

---

## Key Features

### Smart Debloater engine
Unlike other tools that generally rely on PowerShell wrappers, this engine uses **native Win32 APIs** where possible for speed and reliability.
*   **Remove Bloatware:** Bulk removal of UWP apps (Teams, Cortana, Bing, OneDrive, Widgets).
*   **Telemetry Disable:** Blocks diagnostic data, "Tailored Experiences," and edge-case telemetry via Registry.
*   **Context Menu:** Clean up the right-click menu and restore classic capabilities.
*   **Startup Manager:** Manage high-impact startup items.
*   **Edge Cleanup:** Policies to restrict Edge background services and updates.

### System Hardening
Advanced security features that go beyond standard settings.
*   **System Integrity:** Checks and configures Secure Boot, WDAC (Windows Defender Application Control), and Hypervisor-Enforced Code Integrity.
*   **Credential Guard:** Enables VBS (Virtualization-Based Security) to protect system secrets.
*   **Exploit Protection:** Configures system-wide mitigation settings.
*   **Attack Surface Reduction (ASR):** Minimizes vulnerability vectors in apps and Office.

### Defender Control options
Granular control over Windows Security with one-click presets.
*   **Presets:** Quick switching between Standard, Strictly Secure, or Minimal/Gaming modes.
*   **Toggle Features:** Manually control Real-time Protection, Cloud Protection, SmartScreen, Script Scanning, and Tamper Protection.

### Anonymizer & Privacy
*   **Data Randomization:** Features to generate random MAC addresses and unique system identifiers.
*   **Registry Privacy:** Disable advertising IDs, tracking, and unsolicited feedback.

### Utilities
*   **Software Installer:** Quickly install essential software (Browsers, Runtimes, Tools).
*   **Registry Tweaks:** UI customization (Taskbar alignment, Snap Assist, File Extensions, Chat Icon removal).
*   **Community Scripts:** A dedicated section to import and run trusted community batches/scripts safely.

---

## Installation & Usage

1.  Download the latest release from the [Releases Page](https://github.com/bunbunconmeow/Win11Debloater/releases).
2.  Extract the ZIP file.
3.  Right-click `Win11Debloater.exe` and select **Run as Administrator**.
4.  Navigate using the sidebar to the desired module.

> **Note:** The application requires Administrator privileges to modify Registry keys, Services, and System Policies.

---

## Disclaimer

This software modifies core Windows configurations. While every effort has been made to ensure safety and stability:
*   **You use this software at your own risk.**
*   The developers are not responsible for any data loss, boot loops, or system instability.
*   Some "features" removed (like OneDrive or Defender) may be difficult to restore without reinstalling Windows components.

---

## Contributing

This project is in early development! Pull requests, bug reports, and feature suggestions are highly welcome.

1.  Fork the repository.
2.  Create your feature branch (`git checkout -b feature/AmazingFeature`).
3.  Commit your changes (`git commit -m 'Add some AmazingFeature'`).
4.  Push to the branch (`git push origin feature/AmazingFeature`).
5.  Open a Pull Request.

---

## License

This project is licensed under the **BSD 3-Clause License**. See the [LICENSE](LICENSE) file for details.