// Run from the repository in WSL: dotnet run --file scripts/export-windows.cs
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

try
{
    if (args.Length == 1 && args[0] is "--help" or "-h")
    {
        Console.WriteLine("Usage: dotnet run --file scripts/export-windows.cs -- [--destination <Windows-or-WSL-path>]");
        Console.WriteLine(@"Default destination: W:\projektit\driving-time. Set GODOT4 to override the Linux .NET editor.");
        Console.WriteLine("Builds/imports/exports in WSL, then copies and verifies every runtime file. Close the Windows game first.");
        return 0;
    }
    if (args.Length != 0 && (args.Length != 2 || args[0] != "--destination"))
        throw new InvalidOperationException("Unknown arguments. Use --help.");
    if (!OperatingSystem.IsLinux() || !File.ReadAllText("/proc/sys/kernel/osrelease").Contains("microsoft", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Run this script with the Linux .NET SDK inside WSL2.");

    var repo = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(SourceFile())!, ".."));
    var godot = Environment.GetEnvironmentVariable("GODOT4")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64");
    if (!File.Exists(godot))
        throw new InvalidOperationException($"Godot .NET editor missing: {godot}. Follow docs/SETUP.md or set GODOT4 to its full Linux path.");

    var requested = args.Length == 2 ? args[1] : @"W:\projektit\driving-time";
    string destination;
    if (requested.Length >= 3 && char.IsAsciiLetter(requested[0]) && requested[1] == ':' && requested[2] is '\\' or '/')
    {
        var drive = (await Run("wslpath", ["-u", requested[..3]], repo)).Trim();
        if (!Directory.Exists(drive))
            throw new InvalidOperationException($"Windows drive is not mounted: {drive}. Mount {requested[..2]} in WSL before exporting.");
        destination = (await Run("wslpath", ["-u", requested], repo)).Trim();
    }
    else if (Path.IsPathFullyQualified(requested))
        destination = requested;
    else
        throw new InvalidOperationException("Destination must be an absolute Windows drive path or WSL path.");
    destination = Path.TrimEndingDirectorySeparator(Path.GetFullPath(destination));
    if (Within(destination, repo) || Within(repo, destination))
        throw new InvalidOperationException("Destination must be separate from the source checkout; transfer only exported runtime files.");

    // A fresh export cannot accidentally validate stale output from a previous build.
    var artifact = Path.Combine(repo, "artifacts", "windows", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")[..8]);
    Directory.CreateDirectory(artifact);
    Console.WriteLine($"Artifact: {artifact}\nDestination: {destination}");
    await Run("dotnet", ["build", "DrivingTime.sln", "-c", "ExportRelease"], repo, Path.Combine(artifact, "build.log"));
    await Run("dotnet", ["run", "--project", "pipeline", "-c", "Release", "--no-build", "--no-restore", "--",
        "--config", "config/synthetic-scene.json", "--stage-fixture", "game/Generated/P08"], repo, Path.Combine(artifact, "fixture.log"));
    await Run(godot, ["--headless", "--path", Path.Combine(repo, "game"), "--editor", "--import"], repo, Path.Combine(artifact, "import.log"));
    var runtime = Path.Combine(artifact, "runtime");
    Directory.CreateDirectory(runtime);
    await Run(godot, ["--headless", "--path", Path.Combine(repo, "game"), "--export-release", "Windows Desktop", Path.Combine(runtime, "DrivingTime.exe")], repo, Path.Combine(artifact, "export.log"));

    foreach (var relative in new[] { "DrivingTime.exe", "DrivingTime.pck",
        "data_DrivingTime_windows_x86_64/DrivingTime.dll",
        "data_DrivingTime_windows_x86_64/DrivingTime.runtimeconfig.json",
        "data_DrivingTime_windows_x86_64/coreclr.dll",
        "data_DrivingTime_windows_x86_64/hostfxr.dll" })
    {
        var path = Path.Combine(runtime, relative);
        if (!File.Exists(path) || new FileInfo(path).Length == 0)
            throw new InvalidOperationException($"Incomplete export: {relative} missing or empty. See {artifact}/export.log. Nothing copied.");
    }

    var files = Directory.GetFiles(runtime, "*", SearchOption.AllDirectories);
    Directory.CreateDirectory(destination);
    long bytes = 0;
    foreach (var source in files)
    {
        var target = Path.Combine(destination, Path.GetRelativePath(runtime, source));
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(source, target, overwrite: true);
        using var sourceStream = File.OpenRead(source);
        using var targetStream = File.OpenRead(target);
        if (!SHA256.HashData(sourceStream).AsSpan().SequenceEqual(SHA256.HashData(targetStream)))
            throw new IOException($"Copy verification failed: {target}. Close the game and rerun.");
        bytes += new FileInfo(source).Length;
    }
    Console.WriteLine($"Success: copied and SHA-256 verified {files.Length} runtime files ({bytes / 1048576.0:F1} MiB).");
    Console.WriteLine($"Run {requested.TrimEnd('\\', '/')}{(requested.Contains(':') ? '\\' : '/')}DrivingTime.exe on Windows.");
    Console.WriteLine($"Build/import/export logs remain in WSL: {artifact}");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Export failed: {exception.Message}");
    Console.Error.WriteLine("If copying failed, the destination may be incomplete. Close the game and rerun before playing.");
    return 1;
}

static string SourceFile([CallerFilePath] string path = "") => path;
static bool Within(string path, string parent) => path == parent || path.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.Ordinal);

static async Task<string> Run(string executable, string[] arguments, string workingDirectory, string? logPath = null)
{
    Console.WriteLine($"> {Path.GetFileName(executable)} {string.Join(" ", arguments)}");
    var start = new ProcessStartInfo(executable)
    {
        WorkingDirectory = workingDirectory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true
    };
    foreach (var argument in arguments)
        start.ArgumentList.Add(argument);
    using var process = Process.Start(start) ?? throw new IOException($"Could not start {executable}.");
    var stdout = process.StandardOutput.ReadToEndAsync();
    var stderr = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    var output = await stdout + "\n" + await stderr;
    Console.Write(output);
    if (logPath != null)
        await File.WriteAllTextAsync(logPath, output);
    // Godot may exit 0 after .NET publish or import errors.
    if (process.ExitCode != 0 || output.Contains("ERROR:", StringComparison.OrdinalIgnoreCase)
        || output.Contains("SCRIPT ERROR:", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException($"{Path.GetFileName(executable)} failed (exit {process.ExitCode}). See {logPath ?? "output above"}.");
    return output;
}
