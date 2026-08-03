# PassSafe - Secure Offline Password Manager

<div align="center">
  <img src="https://img.shields.io/badge/.NET-MAUI-512BD4?style=for-the-badge&logo=dotnet" alt=".NET MAUI" />
  <img src="https://img.shields.io/badge/Security-AES--GCM-2ea44f?style=for-the-badge&logo=security" alt="AES-GCM Encryption" />
  <img src="https://img.shields.io/badge/Platform-Android%20%7C%20Windows-blue?style=for-the-badge" alt="Platforms" />
  <img src="https://img.shields.io/badge/Status-Active-success?style=for-the-badge" alt="Status" />
</div>

<br/>

**PassSafe** is a modern, privacy-first, and completely offline password manager built with **.NET MAUI**. It is designed with one core principle: **Your data never leaves your device.** 

No cloud sync, no tracking, no telemetry. Just you, your master key, and your passwords secured by military-grade encryption.

## ✨ Key Features

- 🔒 **Zero-Knowledge Architecture:** Operates 100% offline. No servers, no accounts.
- 🛡️ **Military-Grade Encryption:** Uses **AES-GCM (256-bit)** for individual passwords and **SQLCipher** for full database encryption at rest.
- 👆 **Biometric Authentication:** Unlocks your vault seamlessly using Fingerprint or FaceID.
- 🎨 **Dynamic Theming & Localization:** Supports Light/Dark modes, custom accent colors, and real-time language switching (English, Turkish, Russian).
- 📊 **Password Analyzer:** Automatically audits your vault to find weak, short, or reused passwords.
- 🔄 **Secure Backup & Restore:** Export your encrypted `.sqlite` vault locally and import it to any device.
- ✂️ **Auto-Lock:** Automatically locks the app in the background.

## 📱 Screenshots

> *Screenshots will be added soon.*
<!-- Drag and drop your screenshots here: 
| Home | Generator | Analyzer | Settings |
|:---:|:---:|:---:|:---:|
| [Add Screenshot Here] | [Add Screenshot Here] | [Add Screenshot Here] | [Add Screenshot Here] |
-->

## 🛠️ Tech Stack & Architecture

- **Framework:** .NET MAUI (C# & XAML)
- **Architecture:** MVVM (Model-View-ViewModel) using `CommunityToolkit.Mvvm`
- **Database:** SQLite-net with `SQLCipher` for full DB encryption
- **Cryptography:** Native `System.Security.Cryptography.AesGcm`
- **UI Components:** Mopups, MauiIcons, CommunityToolkit.Maui

## 📥 Installation

### Android
1. Go to the [Releases](../../releases) tab.
2. Download the latest `PassSafe-vX.X.X.apk` file.
3. Install the APK on your Android device (You may need to enable "Install from unknown sources").

### Windows (Comming soon...)
<!--
1. Go to the [Releases](../../releases) tab.
2. Download the latest `PassSafe-Windows.msix` file.
3. *Note: Since this is a self-signed package, you need to right-click the file -> Properties -> Digital Signatures -> Details -> View Certificate -> Install Certificate -> Local Machine -> Trusted Root Certification Authorities.*
4. Double click and install.
-->

## 💻 How to Build from Source

To clone and run this application locally, you'll need the following prerequisites installed on your computer:

### Prerequisites
- [Visual Studio 2026](https://visualstudio.microsoft.com/) (latest version recommended)
- **.NET Multi-platform App UI development** workload enabled in Visual Studio Installer
- .NET SDK 10 (recommended)

### Steps to Compile
1. **Clone the repository:**
   ```bash
   git clone https://github.com/Tentex1/PassSafe.git
   cd PassSafe
   
2. **Open the project:**

   Open the PassSafe.sln file using Visual Studio 2026.
   
3. **Restore NuGet Packages:**

   Right-click on the Solution in the Solution Explorer and select "Restore NuGet Packages".

4. **Select the Target Framework:**

   For **Android:** Select an Android Emulator or a connected physical Android device from the debug dropdown menu.

   For **Windows:** Select Windows Machine.

5. **Build and Run:**

   Press F5 or click the Play button to compile and run the application.

Note: If you are building for Windows, make sure Developer Mode is enabled in your Windows Settings.

## 🤝 Contributing
Contributions, issues, and feature requests are highly welcome! Feel free to check the [issues page](../../issues).

## 📜 License
This project is open-source and available under the [**GNU General Public License v3.0**](LICENSE).
