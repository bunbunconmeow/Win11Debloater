# Contributing to Win11Debloater

First off, huge thanks for taking the time to contribute! 🎉

This project aims to provide a modern, safe, and modular Windows 11 Debloater using **WPF** and **iNKORE.UI.WPF**. Whether you are fixing a bug, adding a new script, or improving the UI, your help is welcome.

## ⚠️ Important Warning

**Safety First:** This application modifies system settings, registry keys, and services.
*   **Always test your changes in a Virtual Machine (VM)** before submitting a Pull Request.
*   Code that causes boot loops, breaks critical Windows functionality (without warning), or downloads unsafe binaries will be rejected immediately.

## 🛠️ Development Setup

To get started with the code:

1.  **Prerequisites:**
    *   **Visual Studio 2026** (with .NET Desktop Development workload).
    *   **.NET Framework** (or newer, depending on your target).
    *   **Git**.
2.  **Dependencies:**
    *   The project relies on `iNKORE.UI.WPF` for the UI styling. Ensure nuget packages are restored correctly upon build.
3.  **Clone the Repo:**
    ```bash
    git clone https://github.com/bunbunconmeow/Win11Debloater.git
    ```

## 🏗️ Project Structure

The project follows a standard WPF structure with a navigation-based approach:

*   **`MainWindow.xaml`**: Hosts the main navigation (Navbar) and the Content Frame.
*   **`Pages/`**: Contains the individual views (XAML + Codebehind):
    *   `WelcomePage`: Intro & Stats.
    *   `DebloatPage`: Core removal logic.
    *   `SoftwareInstallerPage`: Package manager integration (Winget/Chocolatey etc.).
    *   `CommunityScriptsPage`: External/Custom scripts.
    *   `DefenderPage`: Security toggle settings.
    *   `HardeningPage`: Advanced security tweaks.
*   **`Scripts/`** or **`Helpers/`**: Contains the PowerShell logic or system interaction code.

## 🎨 UI/UX Guidelines

We strive for a native Windows 11 look and feel.

*   **Theme:** Use `iNKORE.UI.WPF` controls and styles. Do not use standard bulky WPF controls unless styled to match.
*   **English Only:** All UI labels, tooltips, and messages must be in English.
*   **Icons:** Use consistent iconography (e.g., Segoe Fluent Icons) as defined in the main navigation.
*   **Responsiveness:** Ensure pages look good on the default window size (900x550) but can handle resizing.

## 📝 Code Style

*   **C#:** Follow standard C# naming conventions (PascalCase for methods/classes, camelCase for local variables).
*   **Comments:** Comment complex logic, especially registry tweaks or PowerShell execution blocks. 

## 🚀 workflow

1.  **Fork the repository**.
2.  Create a **new branch** for your feature or fix:
    *   `feature/new-hardening-tweak`
    *   `fix/nav-button-alignment`
3.  **Commit** your changes with clear messages.
4.  **Push** to your fork.
5.  Submit a **Pull Request (PR)** to the `main` branch.

## 🛡️ Adding New Scripts/Tweaks

If you are adding a new Debloat or Hardening option:

1.  **Idempotency:** The script should check if the setting is already applied before trying to apply it.
2.  **Reversibility:** Ideally, provide logic to revert the change (if applicable).
3.  **Description:** clearly describe in the UI or code comments what the tweak does.
    *   *Bad:* "Fix Privacy"
    *   *Good:* "Disable Telemetry Service (DiagTrack)"

## 🐛 Bug Reports

If you find a bug, please create an Issue using the template provided. Include:
*   Windows Build Version.
*   Steps to reproduce.
*   Expected vs. Actual behavior.

Thank you for making Windows cleaner and faster!
