# Open in NeoVim

A Visual Studio 2026 extension that opens selected files and folders in a new Neovim window. Also compatible with Visual Studio 2022 17.14 or later.

## Install and use

1. [Download OpenInNeovim.vsix](https://github.com/scastarnado/OpenInNeovim_VisualStudioExtension/releases/latest/download/OpenInNeovim.vsix) from the [latest release](https://github.com/scastarnado/OpenInNeovim_VisualStudioExtension/releases/latest). No build is required.
2. Double-click the downloaded `OpenInNeovim.vsix` and follow the VSIX installer. Close Visual Studio if prompted, then restart it.
3. Right-click a file, physical folder, project, or solution in Solution Explorer and choose **Open in NeoVim**. You can also right-click inside the code editor to open its current file.

Files open directly. Projects and solutions open their containing directory, using Neovim's directory browser. Multiple selected items open separate windows. Virtual nodes without a physical path (such as references and virtual solution folders) do not offer the command. Unsaved editor changes are not automatically saved; Neovim reads the file on disk.

The extension finds `nvim.exe` on PATH or in standard Program Files, local-user, and Scoop installation directories. If needed, set **Tools > Options > Open in NeoVim > General > Executable path** to the full path of `nvim.exe` or a Neovim GUI executable. Restart Visual Studio after changing the system PATH.

## Build

Requires Windows and Visual Studio 2026 with the .NET desktop development workload. NuGet supplies the Visual Studio SDK and VSIX build tools.

From a Developer PowerShell for Visual Studio:

```powershell
msbuild OpenInNeovim.sln /restore /p:Configuration=Release
```

The build produces an installable `.vsix`. To debug, set the extension project to start Visual Studio's `devenv.exe` with `/RootSuffix Exp` and install the VSIX into that experimental instance first.

Microsoft documents Visual Studio 2026's support for the Visual Studio 2022 SDK here: https://learn.microsoft.com/en-us/visualstudio/extensibility/migration/update-visual-studio-extension?view=visualstudio

## Verification

Run `dotnet run --project tests/LauncherChecks` to check Windows argument escaping and executable discovery without opening an editor. In Visual Studio, check file, folder, project, solution, multiple selection, and editor context menus; try paths containing spaces and a custom executable path.
