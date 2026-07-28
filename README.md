# 🛡️ PassSafe - Secure Offline Password Manager

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
- ✂️ **Auto-Lock & Clipboard Clear:** Automatically locks the app in the background and clears copied passwords after a set duration.

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

### Windows (MSIX)
1. Go to the [Releases](../../releases) tab.
2. Download the latest `PassSafe-Windows.msix` file.
3. *Note: Since this is a self-signed package, you need to right-click the file -> Properties -> Digital Signatures -> Details -> View Certificate -> Install Certificate -> Local Machine -> Trusted Root Certification Authorities.*
4. Double click and install.

## 🤝 Contributing
Contributions, issues, and feature requests are highly welcome! Feel free to check the [issues page](../../issues).

## 📜 License
This project is open-source and available under the **MIT License**.
