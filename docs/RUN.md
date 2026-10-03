# Run the P04 chase-camera driving loop

The current Windows artifact is `20261003-161322-f7a5a660`, exported/transferred on 2026-10-03; native launch and drive remain unverified. P02/P03 manual acceptance remains open. The controls and P03 driving test below still apply.

## P04 Manual Test

1. Launch `W:\projektit\driving-time\DrivingTime.exe` on Windows, or run the game through WSLg using the commands below. Drive a tight loop at roughly 10-20 km/h, then make both left and right turns. Expect smooth follow, the car and road ahead in view, and no abrupt heading flips. Stop and reverse briefly; the camera stays behind the car rather than switching modes.
2. On the starting straight (car at X=14, facing -Z), stop near the white stripe and turn left while moving slowly toward the loop interior. This swings the rear camera toward the pale wall at X=20. Also drive slowly alongside it and turn away/back. Expect the camera to pull in before entering the wall and ease outward once clear, without wall-interior views or flicker.
3. Repeat with 30 and 144 FPS caps where practical. Report platform, artifact, smoothness and any clipping. Stay on the floor; restart if stranded. No reset/recovery until P05. P04 remains awaiting-manual-test until this feedback is recorded.

Select ChaseCamera in the scene Inspector after building to tune Distance (7 m), Height (3 m above the 1.4 m pivot), LookAhead (4 m), FollowResponse (6/s), MaximumLag (1.2 m) and ReturnSpeed (4 m/s). Heading lag is bounded to 36 degrees. The swept sphere radius is 0.35 m, with collision margin and 0.1 m camera near plane. Basic convex obstacles only; the pivot must stay outside solids. Very close walls can partially hide the car. No alternate camera modes, cinematic effects or advanced enclosed-space handling. Automated checks and sources: [TESTING.md](TESTING.md#p04--chase-camera).

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

Expected: SDK 10.0.112, Godot 4.5.2.stable.mono, successful build and `P04: chase camera driving loop ready (C#), physics=60` without errors. Initial restore needs NuGet network access. The game targets net8.0 with Godot.NET.Sdk/4.5.2; SDK version and game target differ intentionally. Keep the editor's complete GodotSharp directory. The root solution is selected by `dotnet/project/solution_directory=".."` in project.godot. Its configurations are Debug and ExportRelease (Godot SDK names); an optional compiled release check is `dotnet build DrivingTime.sln -c ExportRelease --no-restore`. Godot's export performs the Windows publish.

For the manual WSLg check:

```bash
"$GODOT4" --path game --editor
```

Open Main.tscn and press F6 (this scene) or F5 (main project). Expect an orange box car at the white starting stripe, a flat dark square loop on blue-gray ground, a perspective chase camera and a km/h readout. A pale wall sits outside the start straight's right edge. The car/loop/wall/HUD primitives are created by C# when running; the static editor view contains the floor, light, camera rig and car node. Check the Output/Debugger for errors, then stop with F8. A direct game launch is `"$GODOT4" --path game`. Close its window to exit.

| Key | Control |
|---|---|
| W / Up | Accelerate forward; brake first if reversing |
| S / Down | Brake forward motion to zero, then reverse while held |
| A D / Left Right | Steer; steering direction reverses when backing |
| Space | Brake to zero in either direction; overrides throttle |
| Release pedals | Coast down gradually |

Physical key positions are used. Opposing acceleration keys cancel each other; opposing steering keys cancel each other. Bright headlights mark the nose (-Z); red lights mark the tail. Steering has no effect when stopped.

Manual test (two minutes): accelerate along the start straight, slow before each corner and steer around the loop. Space should stop the car and keep it stopped while held. Hold S to reverse; check the positive speed reading and REV indicator. Release pedals to coast, and steer in both directions while moving. Expect responsive movement, a stable upright car and no obstacles in the lane. Corners need roughly 10–20 km/h. The limits are 43.2 km/h forward and 14.4 km/h reverse. Stay on the 60 m ground; restart the game if you leave it (recovery belongs to P05). Camera follow is now provided by P04.

After building, select Car in Main.tscn to tune exported properties: Acceleration 6, Braking 12 and CoastDeceleration 2 (m/s²); ForwardLimit 12 and ReverseLimit 4 (m/s); SteeringDegrees 75 (degrees/s at 4 m/s or faster, scaled toward zero at rest). These are arcade settings, not tire/suspension parameters. Save scene overrides to retain tuning; integration checks expect the default values.

For manual frame-rate comparison, run `"$GODOT4" --path game --max-fps 30 --disable-vsync --audio-driver Dummy`, then repeat with 60 and 144. Use the same controls; compare stopping/steering and whether movement changes noticeably. A cap does not guarantee achieved FPS. Prefer native Windows comparisons if WSLg software rendering is slow. Report the platform, caps and observed feel. Automated checks are documented in [TESTING.md](TESTING.md#p03--flat-driving-loop).

Build, import, export and transfer the current release from WSL with one command:

```bash
dotnet run --file scripts/export-windows.cs
```

The C# script uses the pinned SDK and `GODOT4` (defaulting to the installed Godot 4.5.2 .NET editor). It builds ExportRelease, imports headlessly, then exports the Windows Desktop preset to a fresh `artifacts/windows/<UTC timestamp>-<unique suffix>/runtime` folder. It rejects command failures and reported Godot errors, checks EXE/PCK and essential .NET runtime files, copies every runtime file to `W:\projektit\driving-time`, and checks SHA-256 hashes. Build/import/export logs stay beside the runtime folder in WSL. Generated output is ignored by Git. The preset disables signing and resource modification and needs no credentials.

Close any running Windows game before rerunning. W: must be mounted in WSL (normally `/mnt/w`). To override the destination or show help:

```bash
dotnet run --file scripts/export-windows.cs -- --destination 'W:\projektit\driving-time'
dotnet run --file scripts/export-windows.cs -- --help
```

Keep the source checkout, editor, SDK and caches in WSL. In Windows PowerShell, run:

```powershell
Set-Location W:\projektit\driving-time
.\DrivingTime.exe
```

Expect the same car, loop and controls. Perform the two-minute drive. For native rendering comparisons, use `.\DrivingTime.exe --max-fps 30 --disable-vsync`, repeating at 60 and 144. Close the window, then check the startup log if needed:

```powershell
.\DrivingTime.exe --headless --quit-after 10 --log-file W:/projektit/driving-time/startup.log
Get-Content .\startup.log
```

The current transferred release includes P04; native Windows startup and drive remain unverified. Copy overwrites matching files but preserves unrelated/stale files; it is not atomic. If interrupted or a file is locked, close the game and rerun before playing. Fresh local exports and logs are retained and not pruned automatically. Report the WSLg scene and native Windows drive separately, including any errors. Passing command checks does not complete appearance/feel acceptance. The old P02 export at C:\TEMP\driving-time-P02 remains available for its original floor/cube visual check; P04 does not close it automatically. Actual session results and environment limitations are recorded in [TESTING.md](TESTING.md).

Official references checked 2026-10-03: [C# prerequisites and desktop support](https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html), [command-line import/export](https://docs.godotengine.org/en/4.5/tutorials/editor/command_line_tutorial.html), [Windows export](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_for_windows.html), and [solution directory setting](https://docs.godotengine.org/en/4.5/classes/class_projectsettings.html#class-projectsettings-property-dotnet-project-solution-directory). These explain configuration; local execution evidence is separate.
