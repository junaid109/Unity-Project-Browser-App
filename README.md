# Unity Project Browser App

A high-end Unity Project Manager desktop application built with **Avalonia UI** and **C#** following the MVVM pattern.

## 🚀 Features (Implemented)

- **Netflix-Style UI**: Modern dark mode theme with Acrylic/Mica effects and responsive project cards.
- **Project Discovery**: Automatic scanning of "Watch Folders" for Unity projects by parsing `ProjectVersion.txt`.
- **Unity Hub Integration**: Auto-detection of Unity Hub installations and manual path support.
- **Package Manager**: Parse and display project dependencies from `Packages/manifest.json`.
- **Details View**: Interactive overlay for exploring project dependencies and metadata.

## 🛠️ Technical Stack

- **Framework**: Avalonia UI (v11+)
- **Architecture**: MVVM (CommunityToolkit.Mvvm)
- **Styling**: FluentAvalonia (for the "Netflix" Mica/Acrylic look)
- **Language**: C# 12 / .NET 9

## 📸 Preview

*(Add screenshots here)*

## 🚧 Roadmap

- [ ] Project Editing (Rename, Change Unity Version)
- [ ] "Clean Library" tool for corrupt projects
- [ ] Direct "Open in Editor" button
- [ ] Data persistence for user preferences via JSON

## 📝 License

MIT
