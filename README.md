# Unity Project Browser App

A high-end Unity Project Manager desktop application built with **Avalonia UI** and **C#** following the MVVM pattern.

## 🚀 Features (Implemented)

- **Netflix-Style UI**: Modern dark mode theme with Acrylic/Mica effects and responsive project cards.
- **Project Discovery**: Automatic scanning of "Watch Folders" for Unity projects by parsing `ProjectVersion.txt`.
- **Unity Hub Integration**: Auto-detection of Unity Hub installations and support for modern Hub v3 formats (`projects-v1.json`, `editors-v2.json`).
- **Unity Learn Portal**: Integrated search system for fetching and searching tutorials directly from `learn.unity.com`.
- **Docs Hub**: Dedicated documentation portal with quick access to the Unity Manual and Scripting API.
- **Package Manager**: Parse and display project dependencies from `Packages/manifest.json`.
- **Details View**: Interactive overlay for exploring project dependencies, metadata, and launching editors.
- **Tools**: Includes a "Clean Library" tool for resolving project corruption.
- **Persistence**: Remembers your active tab, Documentation links, and watch folders across sessions using JSON.

## 🛠️ Technical Stack

- **Framework**: Avalonia UI (v11+)
- **Architecture**: MVVM (CommunityToolkit.Mvvm)
- **Styling**: FluentAvalonia (for the "Netflix" Mica/Acrylic look)
- **Language**: C# 12 / .NET 9

## 📸 Preview

*(Add screenshots here)*

## 🚧 Roadmap

- [ ] Project Editing (Rename UI, Change Unity Version UI)
- [ ] Direct Unity Installation Download Integration
- [ ] Theme Customization
- [ ] Cloud Sync for Watch Folders

## 📝 License

MIT
