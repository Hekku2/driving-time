# Run the P02 scene

Develop, import, build and export in WSL2. Windows receives only the exported runtime folder. Prerequisites and portable editor installation: [SETUP.md](SETUP.md). These commands use Linux SDK 10.0.112, Godot 4.5.2 .NET and its matching .NET export templates.

From WSL Bash:

```bash
cd /home/hekku2/projects/personal/driving-time
export DOTNET_ROOT=/usr/lib/dotnet
export PATH="$DOTNET_ROOT:$PATH"
export GODOT4="$HOME/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64"
dotnet --version
"$GODOT4" --headless --version
dotnet build DrivingTime.sln
"$GODOT4" --headless --path game --editor --import
"$GODOT4" --headless --path game --quit-after 10
```

Expected: SDK 10.0.112, Godot 4.5.2.stable.mono, successful build and `P02: main 3D scene ready (C#).` without errors. Initial restore needs NuGet network access. The game targets net8.0 with Godot.NET.Sdk/4.5.2; SDK version and game target differ intentionally. Keep the editor's complete GodotSharp directory. The root solution is selected by `dotnet/project/solution_directory=".."` in project.godot. Its configurations are Debug and ExportRelease (Godot SDK names); an optional compiled release check is `dotnet build DrivingTime.sln -c ExportRelease --no-restore`. Godot's export performs the Windows publish.

For the manual WSLg check:

```bash
"$GODOT4" --path game --editor
```

Open Main.tscn and press F6 (this scene) or F5 (main project). Expect an orange 2 m cube resting on a blue-gray 20 m square floor, illuminated by a directional light, viewed from above at an angle. Check the Output/Debugger for startup errors, then stop with F8. A direct game launch is `"$GODOT4" --path game`. Close its window to exit. There are no driving controls, vehicle, map data or collision behavior in P02.

Export the release in WSL:

```bash
mkdir -p artifacts/windows
"$GODOT4" --headless --path game --export-release "Windows Desktop" ../artifacts/windows/DrivingTime.exe
```

The output path is relative to game/project.godot. Inspect output and logs: an exit code alone is insufficient, because a failed .NET export can leave an incomplete executable/PCK. Expect a successful .NET publish and a `data_DrivingTime_windows_x86_64` runtime directory alongside DrivingTime.exe and DrivingTime.pck. The preset disables code signing and executable resource modification, requires no credentials, and keeps the PCK and .NET output separate. Build output, imports and exports are ignored by Git.

Copy the entire output folder after a successful export, preserving its contents:

```bash
mkdir -p /mnt/c/TEMP/driving-time-P02
cp -a artifacts/windows/. /mnt/c/TEMP/driving-time-P02/
```

Keep the source checkout, editor, SDK and caches in WSL. In Windows PowerShell, run:

```powershell
Set-Location C:\TEMP\driving-time-P02
.\DrivingTime.exe
```

Expect the same floor and cube. Close the window, then check the startup log if needed:

```powershell
.\DrivingTime.exe --headless --quit-after 10 --log-file C:/TEMP/driving-time-P02/startup.log
Get-Content .\startup.log
```

Report the WSLg scene and native Windows visual checks separately, including any errors. Passing command smoke checks does not complete appearance acceptance. Actual session results and environment limitations are recorded in [TESTING.md](TESTING.md).

Official references checked 2026-10-03: [C# prerequisites and desktop support](https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html), [command-line import/export](https://docs.godotengine.org/en/4.5/tutorials/editor/command_line_tutorial.html), [Windows export](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_for_windows.html), and [solution directory setting](https://docs.godotengine.org/en/4.5/classes/class_projectsettings.html#class-projectsettings-property-dotnet-project-solution-directory). These explain configuration; local execution evidence is separate.
