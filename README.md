# 🧹 Unused Assets Finder for Unity

A Unity Editor tool that helps you **find and safely remove unused assets** from your project — improving build size, loading times, and overall optimization.

---

## ✨ Features

- 🎯 Scans **only scenes listed in Build Settings** (or a specific scene)
- ⚙️ **Excludes** unwanted file types like `.cs` and `.shader`
- 🧺 **Safe delete:** moves unused assets to `Assets/UnusedAssetsBackup` instead of deleting permanently
- 🧠 Works entirely inside the **Unity Editor** — no external dependencies
- 🧾 Clear and simple result window with detailed asset list

---

## 🧭 How to Use

1. In Unity, open:
Tools → Optimization → Unused Assets Finder

2. Choose your options:
- ✅ Exclude `.cs` / `.shader` files  
- ✅ Scan only build scenes or select a specific scene  
3. Click **“Scan Project”**  
4. Review the results and **safely delete** unused assets with one click.  

---

## 📦 Install via Git URL

Add this line to your Unity project’s `Packages/manifest.json` under `"dependencies"`:

```json
"com.bilgehan.unusedassetsfinder": "https://github.com/bilgehandk/UnusedAssetFinder.git"

Or directly from the Unity Editor:
Window → Package Manager → Add package from Git URL...

Then paste:
https://github.com/bilgehandk/UnusedAssetFinder.git
