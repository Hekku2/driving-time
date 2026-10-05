# Run and export the driving prototype

The default scene is `HelsinkiRoad.tscn`; follow [the first-road route](#first-helsinki-road-drive). `VehicleExperiment.tscn` retains the synthetic **VehicleBody3D experiment**; `Main.tscn` retains the **CharacterBody3D baseline**. These comparison scenes use the same loop, camera, controls, obstacles and ramp. All scenes retain R and fall recovery. The user reported slight jitter in the baseline on 2026-10-03; the cause and improvement remain unverified.

Historical vehicle Windows artifact: **20261003-184950-a7195a8f**, exported/transferred successfully (188 files, SHA-256 verified) to `W:\projektit\driving-time`. Do not assume it remains at the destination. No Windows export of the first-road scene was performed; export was explicitly skipped by request. Native startup and visual comparison remain pending.

## First Helsinki road drive

From the repository root in WSL, regenerate offline and build before starting the editor:

```bash
dotnet run --project pipeline --no-restore -- road-graph --config config/road-graph.json
dotnet run --project pipeline --no-restore -- road-strip --config config/road-strip.json
dotnet build DrivingTime.sln --no-restore
bash scripts/launch-godot-wsl.sh --path game --editor
```

If the source cache is missing, first run `dotnet run --file scripts/download-road-source.cs`; it selects the verified source files. Let Godot finish importing, then press **F5**. The scene creates its nodes on startup; its empty editor root is expected. Expect the orange car near START, facing along one continuous dark asphalt strip; MID and END labels sit beside the road, with green temporary ground below it. There are no buildings or lane markings.

1. Drive slowly from START toward MID using W/Up, with A/D or Left/Right for the gentle bends. Spawn is 6 m along the centreline, at local (58.86814, 0.45, −53.80392) m for the current asset; the nose faces northeast along the strip. Expect stable support with no fall-through or sideways placement jump.
2. Continue toward END (100 m). Brake with Space before the end, around 90 m; this is an open surface with no end barrier. Reverse briefly with S/Down. Expect the chase camera to remain behind the car and asphalt/collision to stay continuous across bends. Note jitter, clipping or steering problems.
3. Press R while stopped or moving. Expect return to the same spawn/heading, cleared motion and camera reset. The suspension settles briefly. Driving onto the surrounding ground intentionally drops about 0.15 m; driving beyond that bounded ground eventually recovers below Y=−10 m.

Report scene/platform, whether spawn/START/MID/END align, road continuity/wheel support, steering/braking/camera/reset observations and console errors. This manual drive remains unverified. Height Y=0 is synthetic; the 8.8 m width comes from an OSM tag, not a survey. Physical access/construction is unverified. Temporary ground extends only 8 m beyond the existing mesh bounds.

`game/HelsinkiRoad.json` controls manifest path, spawn distance/clearance and ground margin/drop. Road path, region and origin remain in [the road-strip configuration](PIPELINE.md#single-flat-road-strip); runtime reads the staged manifest's origin/centreline, applies the shared double-precision coordinate contract and casts only after localization. START/MID/END reference nodes sit on the centreline at 0/50/100 m; their non-colliding labels/poles are offset outside the road edge.

## Vehicle driving comparison

1. Close the previous game and launch `W:\projektit\driving-time\DrivingTime.exe` after transferring the vehicle release. Drive a loop slowly, then accelerate/brake and reverse. W drives the headlights/nose forward; S brakes forward motion before reversing; Space overrides throttle. A/D steer the front wheels; corner slowly. Expect suspension-supported movement and physical pitch/roll, with animated wheels rather than an upright sliding chassis.
2. Climb the green ramp from its low end, brake, reverse back and hit an interior obstacle. Expect wheel support and solid chassis contact. A physical car can roll or drift on slopes; the old arcade promise of zero slope drift does not apply. The ramp's high end is a drop.
3. Press R while moving/turning or after tipping. Expect return facing -Z at the start stripe, cleared linear/angular motion and controls, and camera reset. The body then settles briefly onto its suspension. Holding driving keys resumes acceleration on the next tick. Drive off an open floor edge, release input and expect automatic recovery below Y=-10 m.
4. Close the game and compare the baseline in Windows PowerShell: `.\DrivingTime.exe res://Main.tscn`. Launch the experiment explicitly with `.\DrivingTime.exe res://VehicleExperiment.tscn`. Repeat each at `--max-fps 30 --disable-vsync`, 60 and 144 if useful. Keep platform, caps and route alike; report jitter while straight, turning and on the ramp, braking/steering feel, and preferred version. Caps do not prove achieved FPS. No smoothness improvement or native visual pass is claimed yet.

WSL equivalents: `"$GODOT4" --path game res://VehicleExperiment.tscn` and `"$GODOT4" --path game res://Main.tscn`. F5 launches the first-road scene; F6 runs the selected comparison scene. Inspector tuning on the experiment Car: DriveForce 2800 per driven rear wheel, BrakeForce 55, forward/reverse force cutoffs 12/4 m/s, steering 28 degrees with 3 rad/s response, suspension stiffness 30, mass 1000 kg. These are provisional values, not a validated realistic model; the speed cutoffs taper propulsion rather than clamping momentum. Vehicle suspension/wheel geometry is created by C# on startup. The baseline tuning and arcade driving and recovery guides below remain specific to `Main.tscn`.

The chassis uses -Z as its nose, so the engine-force sign is reversed from Godot's documented +Z vehicle front. Normal motion is engine-integrated; reset teleports through `_IntegrateForces`. The experiment's spawn is (14,0.45,8), providing suspension clearance. The existing camera parameters and global 60 Hz interpolation remain unchanged for comparison. This experiment does not diagnose camera/rendering jitter.

Export/update from WSL: `dotnet run --file scripts/export-windows.cs`. Keep all runtime companion files together. Automated evidence: [vehicle checks](TESTING.md#vehiclebody3d-comparison).

## Arcade baseline

The historical collision and recovery artifact was `20261003-164321-f5563ae3`; the vehicle-comparison release replaced it at W: at the time of transfer. The baseline scene and arcade driving/chase camera driving tests below still apply when launching `res://Main.tscn` explicitly.

## Collision and recovery manual test

1. Launch `W:\projektit\driving-time\DrivingTime.exe`. Drive into the red block, gold pillar and purple low block inside the loop, first slowly, then faster. Expect contact to stop/slide the car without passing through the solids. Hold S to back away; there should be no long delay braking invisible forward speed after a head-on impact.
2. Enter the green ramp on the west/left straight at X=-14 from its low end near Z=6, heading toward -Z. Climb slowly, brake halfway and release the pedals. Expect the car to rise, stop without sliding away, and reverse back onto flat ground. Its box stays upright; the high end near Z=-6 is a drop, so use reverse for a gentle descent.
3. While moving/turning, release the driving keys and press R. Also reset after driving near the pale wall with a compressed camera. Expect immediate return to the white stripe at (14,0,8), facing -Z, 0 km/h, and the camera immediately behind the spawn without lag or retained compression. Hold R briefly: it should reset once per press. Holding throttle after reset starts fresh acceleration on the next physics tick.
4. Drive off an unobstructed floor edge (X or Z beyond ±30 m), release the pedals and wait. Below Y=-10 m, expect the same stationary spawn and camera reset. Repeat R once more. Report artifact, platform, impacts, ramp behavior and any camera smear/clipping. These visual/feel checks remain unverified; G1 stays open until evidence is recorded.

Car Inspector: SpawnPath defaults to `../Spawn`; move/rotate the Spawn Marker3D in Main.tscn to define recovery pose. RecoveryHeight defaults to -10 m. Keep spawn above the limit and clear of solids; there is no automatic search for a new safe spawn. Collision is one 1.8 × 0.7 × 3.4 m upright chassis box; cabin and wheels are cosmetic. No suspension, tipping or realistic bounce. MoveAndSlide sweeps translation, while yaw changes are direct; very sharp turns into corners need visual coverage. Obstacles/ramp are synthetic meter-space geometry, generated by Main.cs during play. Automated results: [TESTING.md](TESTING.md#collision-and-safe-recovery).

## Chase camera manual test

1. Launch `W:\projektit\driving-time\DrivingTime.exe` on Windows, or run the game through WSLg using the commands below. Drive a tight loop at roughly 10-20 km/h, then make both left and right turns. Expect smooth follow, the car and road ahead in view, and no abrupt heading flips. Stop and reverse briefly; the camera stays behind the car rather than switching modes.
2. On the starting straight (car at X=14, facing -Z), stop near the white stripe and turn left while moving slowly toward the loop interior. This swings the rear camera toward the pale wall at X=20. Also drive slowly alongside it and turn away/back. Expect the camera to pull in before entering the wall and ease outward once clear, without wall-interior views or flicker.
3. Repeat with 30 and 144 FPS caps where practical. Report platform, artifact, smoothness and any clipping. R now resets if stranded. Record chase camera feedback separately from recovery acceptance.

Select ChaseCamera in the scene Inspector after building to tune Distance (7 m), Height (3 m above the 1.4 m pivot), LookAhead (4 m), FollowResponse (6/s), MaximumLag (1.2 m) and ReturnSpeed (4 m/s). Heading lag is bounded to 36 degrees. The swept sphere radius is 0.35 m, with collision margin and 0.1 m camera near plane. Basic convex obstacles only; the pivot must stay outside solids. Very close walls can partially hide the car. No alternate camera modes, cinematic effects or advanced enclosed-space handling. Automated checks and sources: [TESTING.md](TESTING.md#chase-camera).

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

Expected: SDK 10.0.112, Godot 4.5.2.stable.mono, successful build and `P05: collision and recovery scene ready (C#), physics=60` without errors. Initial restore needs NuGet network access. The game targets net8.0 with Godot.NET.Sdk/4.5.2; SDK version and game target differ intentionally. Keep the editor's complete GodotSharp directory. The root solution is selected by `dotnet/project/solution_directory=".."` in project.godot. Its configurations are Debug and ExportRelease (Godot SDK names); an optional compiled release check is `dotnet build DrivingTime.sln -c ExportRelease --no-restore`. Godot's export performs the Windows publish.

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
| R | Reset to the Spawn marker and clear motion/camera state |
| Release pedals | Coast down gradually |

Physical key positions are used. Opposing acceleration keys cancel each other; opposing steering keys cancel each other. Bright headlights mark the nose (-Z); red lights mark the tail. Steering has no effect when stopped.

Manual test (two minutes): accelerate along the start straight, slow before each corner and steer around the loop. Space should stop the car and keep it stopped while held. Hold S to reverse; check the positive speed reading and REV indicator. Release pedals to coast, and steer in both directions while moving. Expect responsive movement and a stable upright car. The west straight now has the ramp; blocks sit inside the loop. Corners need roughly 10–20 km/h. The limits are 43.2 km/h forward and 14.4 km/h reverse. R resets the car; leaving the 60 m ground triggers fall recovery. The chase camera follows the car.

After building, select Car in Main.tscn to tune exported properties: Acceleration 6, Braking 12 and CoastDeceleration 2 (m/s²); ForwardLimit 12 and ReverseLimit 4 (m/s); SteeringDegrees 75 (degrees/s at 4 m/s or faster, scaled toward zero at rest). These are arcade settings, not tire/suspension parameters. Save scene overrides to retain tuning; integration checks expect the default values.

For manual frame-rate comparison, run `"$GODOT4" --path game --max-fps 30 --disable-vsync --audio-driver Dummy`, then repeat with 60 and 144. Use the same controls; compare stopping/steering and whether movement changes noticeably. A cap does not guarantee achieved FPS. Prefer native Windows comparisons if WSLg software rendering is slow. Report the platform, caps and observed feel. Automated checks are documented in [TESTING.md](TESTING.md#flat-driving-loop).

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

The current transferred release includes collision and recovery; native Windows startup and drive remain unverified. Copy overwrites matching files but preserves unrelated/stale files; it is not atomic. If interrupted or a file is locked, close the game and rerun before playing. Fresh local exports and logs are retained and not pruned automatically. Report the WSLg scene and native Windows drive separately, including any errors. Passing command checks does not complete appearance/feel acceptance. The old desktop scene export at `C:\TEMP\driving-time-P02` remains available for its original floor/cube visual check; collision and recovery does not close it automatically. Actual session results and environment limitations are recorded in [TESTING.md](TESTING.md).

Official references checked 2026-10-03: [C# prerequisites and desktop support](https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html), [command-line import/export](https://docs.godotengine.org/en/4.5/tutorials/editor/command_line_tutorial.html), [Windows export](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_for_windows.html), and [solution directory setting](https://docs.godotengine.org/en/4.5/classes/class_projectsettings.html#class-projectsettings-property-dotnet-project-solution-directory). These explain configuration; local execution evidence is separate.
## Pipeline CLI

Pipeline runs entirely in WSL using pinned SDK 10.0.112. Initial locked restore, configuration/manifest v1 and offline steps are in [PIPELINE.md](PIPELINE.md). After restore, from the repository root:

```bash
dotnet run --project pipeline/DrivingTime.Pipeline.csproj --no-restore -- --config config/synthetic-scene.json
```

Expect exit 0 and the generated manifest path; invalid input exits 2. No downloads or extra runtime. Pipeline outputs/tools stay in WSL; only exported game runtime files transfer to Windows.

## Synthetic asset inspection

Generate/build/import using [PIPELINE.md](PIPELINE.md#synthetic-glb-staging). Open `game/AssetInspection.tscn` in the WSL Godot editor and run that scene (F6), or run:

```bash
"$HOME/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64" --path game res://AssetInspection.tscn
```

For native Windows, close the current game and run the export helper in WSL. Then from Windows PowerShell:

```powershell
& 'W:\projektit\driving-time\DrivingTime.exe' 'res://AssetInspection.tscn'
```

Use Left/Right arrows to orbit. Expect a solid orange 10 × 10 × 10 m cube on a 30 m ground slab, a red east marker (+X), a green north marker (-Z), and a blue 6 m vertical marker (+Y). Labels should stay attached to each marker. Orbit fully around: all outside faces remain visible with flat lighting and correct colors, with no holes, inverted faces, reflection or extra rotation. Compare editor and Windows export. Report the artifact/platform, scale/direction/material observations and any console errors. Passing headless mesh checks does not establish visual acceptance. This is a separate inspection scene; normal startup remains the vehicle experiment.

## Road strip inspection

Generate the assets using [the offline road strip commands](PIPELINE.md#single-flat-road-strip), then open the existing Godot project in the WSL editor. In a temporary unsaved 3D scene, drag `Generated/P12/road.glb` from the FileSystem dock, select its mesh and press F to frame it. Inspect from above and at an oblique angle: expect one plain dark asphalt strip, 100 m along its gently bending centreline, 8.8 m nominal width, flat Y=0, no holes, inverted faces or spikes. Its local bounds are X=51.4614..138.0052, Z=−111.1080..−46.5271 metres, preserving the configured origin.

Add `Generated/P12/collision.glb` separately at identity transform. Enable editable children on the instance and select its CollisionShape3D child with editor gizmos visible. Expect a StaticBody3D and a concave flat shape tracing the same strip, with no second rendered asphalt mesh. Inspect it separately by temporarily hiding the visual instance. Do not save a new gameplay scene for this check. Report visual/collision results; the headless import/ray check does not establish appearance or driving feel. The [dedicated driving scene](#first-helsinki-road-drive) integrates car/spawn/ground; native export/playtesting remains unverified.
