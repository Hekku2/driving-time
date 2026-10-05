// Read-only preflight; no packages, downloads, editor launch, restore of the solution or export.
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Xml.Linq;

if (args.Length == 1 && args[0] is "--help" or "-h")
{
    Console.WriteLine("Usage: bash scripts/check-environment.sh (or dotnet run --file scripts/check-environment.cs)");
    Console.WriteLine("Overrides: GODOT4 (editor path), XDG_DATA_HOME (template data directory).");
    Console.WriteLine("Exit 0: required checks pass; 1: missing/failed requirement; 2: invalid arguments.");
    return 0;
}
if (args.Length != 0) { Console.Error.WriteLine("Unknown arguments. Use --help."); return 2; }

var repo = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(SourceFile())!, ".."));
var failures = 0;
Console.WriteLine($"Development preflight: {repo}");
try
{
    var kernel = OperatingSystem.IsLinux() ? File.ReadAllText("/proc/sys/kernel/osrelease") : "";
    Check(kernel.Contains("microsoft-standard-WSL2", StringComparison.OrdinalIgnoreCase), "WSL2 Linux",
        "Run in WSL2; keep the checkout and development tools in its Linux filesystem.");
    Check(RuntimeInformation.OSArchitecture == Architecture.X64, "x64 architecture",
        "The selected Linux editor and Windows export use x64.");
    Check(!repo.StartsWith("/mnt/", StringComparison.Ordinal), "Checkout in Linux filesystem",
        "Move the development checkout into your WSL home; transfer only exported game files to Windows.");

    using var config = JsonDocument.Parse(File.ReadAllText(Path.Combine(repo, "global.json")));
    var pin = config.RootElement.GetProperty("sdk").GetProperty("version").GetString()!;
    var sdks = await Run("dotnet", "--list-sdks");
    Check(sdks.Ok && sdks.Output.Split('\n').Any(line => line.StartsWith("10.", StringComparison.Ordinal)),
        ".NET 10 SDK installed", "Install a Linux .NET 10 SDK; see docs/SETUP.md step 2.");
    var selected = await Run("dotnet", "--version");
    Check(selected.Ok && selected.Output.Trim() == pin, $"Repository SDK {pin}",
        $"Install Linux SDK {pin}; global.json requires this exact SDK. {selected.Output}");
    var dotnet = Find("dotnet");
    Check(dotnet != null && IsElf(dotnet), "Linux dotnet executable", "Use Linux dotnet on PATH, not Windows interop.");
    var git = await Run("git", "--version");
    Check(git.Ok, $"Git: {git.Output.Trim()}", "Install Git in WSL: sudo apt install git");

    var game = XDocument.Load(Path.Combine(repo, "game/DrivingTime.csproj"));
    var engine = game.Root!.Attribute("Sdk")!.Value.Split('/')[1];
    var expectedVersion = $"{engine}.stable.mono";
    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    var portable = Path.Combine(home, $".local/share/driving-time/godot-{engine}/Godot_v{engine}-stable_mono_linux.x86_64");
    var editor = Environment.GetEnvironmentVariable("GODOT4")
        ?? (File.Exists(portable) ? portable : Find("godot-mono") ?? Find("godot4") ?? Find("godot"));
    if (editor != null) editor = Find(editor) ?? editor;
    var version = editor != null ? await Run(editor, "--headless", "--version") : (false, "not found");
    Check(version.Item1 && version.Item2.Trim().StartsWith(expectedVersion + ".", StringComparison.Ordinal),
        $"Godot .NET {engine}: {version.Item2.Trim()}",
        $"Download the complete Linux x64 .NET editor {engine} (including GodotSharp); see docs/SETUP.md step 3. Set GODOT4 to its executable if installed elsewhere.");
    Check(editor != null && IsElf(editor) && Directory.Exists(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(editor))!, "GodotSharp")),
        "Complete Linux Godot .NET editor", "Extract the full .NET editor archive, including GodotSharp, beside the Linux executable.");

    var dataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? Path.Combine(home, ".local/share");
    var templates = Path.Combine(dataHome, "godot/export_templates", expectedVersion);
    var missing = new[] { "version.txt", "windows_debug_x86_64.exe", "windows_release_x86_64.exe" }
        .Where(name => !File.Exists(Path.Combine(templates, name)) || new FileInfo(Path.Combine(templates, name)).Length == 0).ToArray();
    Check(missing.Length == 0 && File.ReadAllText(Path.Combine(templates, "version.txt")).Trim() == expectedVersion,
        $"Matching Windows x64 export templates: {templates}",
        $"Install matching {engine} .NET export templates via docs/SETUP.md step 3; missing: {string.Join(", ", missing)}. Standard-edition templates do not suffice.");

    var display = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISPLAY"))
        || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"));
    Console.WriteLine(display ? "INFO: Display configured; graphical editor/driver operation still needs a manual launch."
        : "WARN: No display configured. Enable WSLg for graphical editing; headless preparation can continue.");
    foreach (var tool in new[] { "curl", "unzip", "sha256sum" })
        Console.WriteLine(Find(tool) != null ? $"INFO: Setup helper tool {tool} available."
            : $"WARN: {tool} missing; needed only by scripts/setup-godot-wsl.sh. Install in WSL (sudo apt install {(tool == "sha256sum" ? "coreutils" : tool)}), or use manual archive installation.");
    Console.WriteLine("INFO: First build needs NuGet packages: dotnet restore DrivingTime.sln --locked-mode (network/cache required; not checked here).");
    Console.WriteLine("INFO: No Python, Node.js, GIS CLI or Windows development SDK is required. GUI, audio, package access and actual build/export remain unverified by this preflight.");
}
catch (Exception error)
{
    Check(false, "Preflight configuration", $"{error.Message} Check repository files and permissions.");
}
Console.WriteLine(failures == 0 ? "READY: Required inventory checks passed; review warnings before graphical work."
    : $"NOT READY: {failures} required check(s) failed. Follow the guidance above and rerun.");
return failures == 0 ? 0 : 1;

void Check(bool ok, string label, string remedy)
{
    Console.WriteLine($"{(ok ? "PASS" : "FAIL")}: {label}");
    if (!ok) { failures++; Console.WriteLine($"  {remedy}"); }
}
string? Find(string name)
{
    if (name.Contains(Path.DirectorySeparatorChar)) return File.Exists(name) ? Path.GetFullPath(name) : null;
    return (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator)
        .Where(path => path.Length > 0).Select(path => Path.Combine(path, name)).FirstOrDefault(File.Exists);
}
bool IsElf(string path)
{
    if (!File.Exists(path)) return false;
    using var stream = File.OpenRead(path);
    Span<byte> magic = stackalloc byte[4];
    return stream.Read(magic) == 4 && magic.SequenceEqual(new byte[] { 0x7f, (byte)'E', (byte)'L', (byte)'F' });
}
async Task<(bool Ok, string Output)> Run(string executable, params string[] arguments)
{
    try
    {
        var start = new ProcessStartInfo(executable) { WorkingDirectory = repo, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); return (false, "probe timed out after 15 seconds"); }
        return (process.ExitCode == 0, ((await output) + (await error)).Trim());
    }
    catch (Exception error) { return (false, error.Message); }
}
static string SourceFile([CallerFilePath] string path = "") => path;
