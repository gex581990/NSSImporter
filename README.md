# NSSImporter
<div align="center">
  <img width="256" height="480" alt="NSSImporter Tool Promotional Design" src="https://github.com/user-attachments/assets/0cc2ae15-563f-4b48-8ac1-a431302bd7b1" />
  </div>
<div align="center">
A Windows **Fluent/WinUI 3** desktop app to manage **Nilesoft Shell** `.nss` imports with precision.

It mirrors the full-featured PowerShell workflow—now in a GUI—with:

- Drag-and-drop or manual file selection for the source `.nss`
- Auto-detect (or browse to) **Nilesoft Shell** install path (portable supported)
- Copies new imports into `imports/snippets/` (always)
- Scans **all** of `imports/` (and subfolders) for same-named files
- Duplicate **file** handling:
  - **Overwrite/Update** with the new file
  - **Keep** → backup to `imports/backup/old*.nss` (sequential: `old.<name>.nss`, `old2.<name>.nss`, …)
  - **Select multiple to keep** (backed up) and delete the rest
  - **Delete** selected
  - **Single** or **Batch** mode for duplicates
- Duplicate **import lines** across configs (incl. `shell.nss`):
  - **Transfer** (remove old import(s) and add to the new target automatically—no second destination prompt)
  - **Remove** (delete old import line(s) and proceed)
  - **Keep** (skip adding to avoid duplicate)
- **In-file import conflicts** (same filename imported more than once, even with different paths):
  - **Backup** the extra file(s) to `imports/backup/old*.nss` and remove their import, or
  - **Keep as import** by renaming the file(s) to `alt.<name>.nss` (sequential: `alt2`, …) and auto-updating the import line(s)
- Target selection (recursive under `imports/`) + **`shell.nss`**
- Precise insertion:
  - **Standalone** (top-level), **Create new menu**, or **Insert into existing menu**
  - Colorized, numbered previews; choose exact index or append to end
  - Proper indentation; nested menus supported
- New-menu builder:
  - **Title** (required)
  - **Mode** (optional; `none|single|multi_unique|multi_single|multiple`)
  - **Type**(s) (optional; supports `*`, `File`, `Directory|Dir`, `Drive`, `USB`, `DVD`, `Fixed`, `VHD`, `Removable`, `Remote`, `Back`, granular `back.*`, `Desktop`, `Namespace`, `Computer`, `Recyclebin`, `Taskbar`)
  - Blank mode/type → omitted (Nilesoft defaults apply)
- Writes with backup (`.bak`) before edits
- One-click **Restart** of Nilesoft Shell via:
  ```
  "<ShellRoot>\shell.exe" -restart
  ```
</div>
---

## Requirements

- **Windows 10 21H2+** or **Windows 11**
- **Visual Studio 2022** (17.7+) with:
  - **.NET Desktop Development**
  - **Windows App SDK / WinUI 3** (Windows App SDK **1.5+**)
  - **MSIX Packaging Tools**
- .NET **8** (`net8.0-windows`)
- Access to the Nilesoft Shell folder (often `C:\Program Files\Nilesoft Shell\`)

> Editing under `Program Files` may require running as Administrator.

---

## Project Layout

```
NSSImporter/
├─ NSSImporter.sln
├─ NSSImporter.csproj
├─ Package.appxmanifest
├─ app.manifest
├─ App.xaml
├─ App.xaml.cs
├─ Program.cs
├─ Assets/
│  ├─ AppIcon.ico
│  ├─ Square44x44Logo.png
│  ├─ Square150x150Logo.png
│  ├─ SplashScreen.png
│  └─ (optional) StoreLogo.png
├─ Helpers/
│  ├─ DialogUtils.cs
│  ├─ FileBackupUtils.cs
│  └─ PathUtils.cs
├─ Models/
│  ├─ DuplicateResolver.cs
│  ├─ ImportHandler.cs
│  └─ NssFileManager.cs
└─ Views/
   ├─ MainWindow.xaml
   ├─ MainWindow.xaml.cs
   ├─ HomeView.xaml
   ├─ HomeView.xaml.cs
   ├─ PreviewDialog.xaml
   ├─ PreviewDialog.xaml.cs
   ├─ MenuInsertDialog.xaml
   └─ MenuInsertDialog.xaml.cs
```

---

## Build & Run

1. Open `NSSImporter.sln` in Visual Studio 2022.
2. Confirm Windows App SDK workloads are installed.
3. Set configuration to **Debug | Any CPU** (or **Release** for packaging).
4. Build → Run.

### Packaging (MSIX)

1. Switch to **Release**.
2. Right-click the project → **Publish** → **Create App Packages (MSIX)**.
3. Follow the wizard (test certificate is fine for local install).
4. Install the MSIX (trust the certificate if prompted).

---

## Using the App (Flows)

### Flow 1 — Choose Source `.nss`
- Drag & drop a `.nss` into the window, or click **Browse**.

### Flow 2 — Locate Nilesoft Shell
- If `C:\Program Files\Nilesoft Shell\` exists, it’s used automatically.
- If not, you’ll be prompted to **Browse** to the Nilesoft Shell root (portable supported).  
  The app validates that `shell.nss` and `imports/` exist.

### Flow 3 — Handle Duplicate **Files** (same filename under `imports/...`)
- **One** match:
  - **Overwrite/Update** with the selected file
  - **Keep** → move existing to `imports/backup/old*.nss`
  - **Cancel**
- **Multiple** matches:
  - Choose **Single** or **Batch** mode (explained in-app).
  - For each duplicate (or all in batch):
    - **Keep** → backup to `imports/backup/old*.nss`
    - **Delete**
  - New file is then copied to **`imports/snippets/<name>.nss`**.
- Keep operations are **non-destructive** and **never overwrite**: backups are sequential (`old`, `old2`, …).

### Flow 4 — Handle Duplicate **Imports** (same filename imported somewhere)
- The app scans all `.nss` (and `shell.nss`) for lines like `import '...<name>.nss'`.

Options:
- **Transfer**: remove old import(s) and **add to the new target automatically** (no second destination prompt for this step).
- **Remove**: delete old import(s), continue normally.
- **Keep**: skip adding to avoid duplicate import of the same filename.

**In-file import conflicts** (same filename imported more than once, different paths):
- **Backup** the extra file(s) to `imports/backup/old*.nss` and remove their import, **or**
- **Keep as import** by renaming the file(s) to `alt.<name>.nss` (sequential: `alt2`, …) and auto-updating the import line(s).

### Flow 5 — Choose Destination
- Pick any `.nss` under `imports/` **or** select **`shell.nss`**.

### Flow 6 — Insert
- Choose **Standalone**, **Create new menu**, or **Insert into existing menu**.
- **Create Menu**: Title required; Mode/Types optional (left blank → omitted).
- **Preview** (colorized & numbered):
  - Standalone shows top-level lines.
  - Existing menu preview shows the menu’s body.
  - Insert **before** an index or **append** to end.
- Writes with `.bak` backup and preserves indentation & nesting.

### Flow 7 — Restart Nilesoft Shell
Runs:
```
"<ShellRoot>\shell.exe" -restart
```

---

## Branding (Icons & Images)

### 1) Add assets

Place images here:

```
NSSImporter/Assets/
```

Suggested:
- `Assets/AppIcon.ico` (multi-size: 16/24/32/48/64/128/256)
- `Assets/Square44x44Logo.png`
- `Assets/Square150x150Logo.png`
- `Assets/SplashScreen.png` (620×300 or 1240×600, transparent preferred)
- (Optional) `Assets/StoreLogo.png`

### 2) Project icon (`.csproj`)

Add to the first `<PropertyGroup>` of `NSSImporter.csproj`:

```xml
<ApplicationIcon>Assets\AppIcon.ico</ApplicationIcon>
```

Ensure assets are included:

```xml
<ItemGroup>
  <Content Include="Assets\**\*.*" />
</ItemGroup>
```

### 3) Package manifest icons (`Package.appxmanifest`)

Update:

```xml
<uap:VisualElements
    DisplayName="NSSImporter"
    Description="Nilesoft Shell .nss importer"
    Square150x150Logo="Assets\Square150x150Logo.png"
    Square44x44Logo="Assets\Square44x44Logo.png"
    BackgroundColor="transparent">
  <uap:DefaultTile Square150x150Logo="Assets\Square150x150Logo.png" />
  <uap:SplashScreen Image="Assets\SplashScreen.png" BackgroundColor="transparent" />
</uap:VisualElements>
```

### 4) Use images in XAML

```xml
<Image Source="ms-appx:///Assets/Square150x150Logo.png" Width="64" Height="64"/>
```

---

## Conventions & Rules

- New import lines use **single quotes** and **forward slashes**:
  ```
  import 'imports/snippets/MySnippet.nss'
  ```
- A given **filename** may not be imported more than once within a target—even if the paths differ.
- Backups live in `imports/backup/`:
  - `old.<name>.nss`, `old2.<name>.nss`, …
- “Alt” variants are for kept in-file conflicts:
  - `alt.<name>.nss`, `alt2.<name>.nss`, … (and import lines are updated accordingly)

---

## Troubleshooting

- **Access denied**: run as **Administrator** if Nilesoft Shell is under `Program Files`.
- **Portable Shell**: when prompted, browse to the folder that contains `shell.nss` and `imports/`.
- **No menus**: use **Create new menu** or **Standalone**; the preview shows valid insertion points.
- **Restart seems ineffective**: confirm `<ShellRoot>\shell.exe` exists (portable paths supported).

---

## Roadmap

- Undo stack in the UI (beyond `.bak`)
- Multi-select insertion targets
- Dark/light theme toggle
- Command-line switches (headless/batch)

---

## License

MIT (or specify another license if you prefer).
