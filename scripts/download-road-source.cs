// Download the configured bounded OSM sample through the existing C# pipeline.
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;

if (args is ["--help"] or ["-h"])
{
    Console.WriteLine("Usage: dotnet run --file scripts/download-road-source.cs [--refresh]");
    Console.WriteLine("Restores locked pipeline dependencies, then verifies/reuses the road cache or downloads one bounded sample.");
    Console.WriteLine("--refresh: request a fresh sample instead of reusing the cache; no automatic retries.");
    Console.WriteLine("Configuration: config/road-sample.json. Run with the pinned Linux .NET SDK in WSL2.");
    Console.WriteLine("Selects the verified payload/provenance in config/road-graph.json; conversion remains offline and separate.");
    return 0;
}
if (args.Length != 0 && args is not ["--refresh"])
{
    Console.Error.WriteLine("Unknown arguments. Use --help.");
    return 2;
}
try
{
    if (!OperatingSystem.IsLinux() ||
        !File.ReadAllText("/proc/sys/kernel/osrelease").Contains("microsoft-standard-WSL2", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Run with the Linux .NET SDK inside WSL2.");
    var repo = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(SourceFile())!, ".."));
    var restored = await Run(repo, ["restore", "pipeline/DrivingTime.Pipeline.csproj", "--locked-mode"]);
    if (restored != 0) return restored;
    var command = new List<string>
    {
        "run", "--project", "pipeline", "--no-restore", "--", "road-sample", "--config", "config/road-sample.json"
    };
    command.AddRange(args);
    string? cacheDirectory = null;
    var result = await Run(repo, command.ToArray(), line =>
    {
        foreach (var prefix in new[] { "Verified offline cache: ", "Validated raw cache: " })
            if (line.StartsWith(prefix, StringComparison.Ordinal)) cacheDirectory = line[prefix.Length..];
    });
    if (result == 0)
    {
        if (cacheDirectory == null) throw new InvalidDataException("Retrieval succeeded without reporting its verified cache directory; graph configuration was not updated.");
        var rawPath = Path.Combine(cacheDirectory, "raw.json");
        var provenancePath = Path.Combine(cacheDirectory, "network-provenance.json");
        var provenance = JsonNode.Parse(File.ReadAllText(provenancePath))!;
        using var raw = File.OpenRead(rawPath);
        if (provenance["schemaVersion"]!.GetValue<int>() != 1 ||
            provenance["byteCount"]!.GetValue<long>() != raw.Length ||
            provenance["payloadSha256"]!.GetValue<string>() != Convert.ToHexStringLower(SHA256.HashData(raw)))
            throw new InvalidDataException("Downloaded cache integrity mismatch; graph configuration was not updated.");
        var graphPath = Path.Combine(repo, "config/road-graph.json");
        var graph = JsonNode.Parse(File.ReadAllText(graphPath))!;
        var configDirectory = Path.GetDirectoryName(graphPath)!;
        var rawRelative = Path.GetRelativePath(configDirectory, rawPath);
        var provenanceRelative = Path.GetRelativePath(configDirectory, provenancePath);
        if (graph["inputPath"]!.GetValue<string>() != rawRelative ||
            graph["provenancePath"]!.GetValue<string>() != provenanceRelative)
        {
            graph["inputPath"] = rawRelative;
            graph["provenancePath"] = provenanceRelative;
            var temporary = graphPath + "." + Guid.NewGuid().ToString("N") + ".partial";
            try
            {
                File.WriteAllText(temporary, graph.ToJsonString(new() { WriteIndented = true }) + "\n");
                File.Move(temporary, graphPath, overwrite: true);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
        Console.WriteLine("Road source cache ready and selected in config/road-graph.json. Conversion remains a separate offline step; see docs/PIPELINE.md.");
    }
    return result;
}
catch (Exception error)
{
    Console.Error.WriteLine($"Road source download failed: {error.Message}");
    return 1;
}

static async Task<int> Run(string repo, string[] arguments, Action<string>? observe = null)
{
    var start = new ProcessStartInfo("dotnet") { WorkingDirectory = repo, UseShellExecute = false, RedirectStandardOutput = true };
    foreach (var argument in arguments) start.ArgumentList.Add(argument);
    using var process = Process.Start(start) ?? throw new IOException("Could not start Linux dotnet.");
    while (await process.StandardOutput.ReadLineAsync() is { } line)
    {
        Console.WriteLine(line);
        observe?.Invoke(line);
    }
    await process.WaitForExitAsync();
    if (process.ExitCode != 0) Console.Error.WriteLine($"dotnet {arguments[0]} failed (exit {process.ExitCode}); no automatic retry.");
    return process.ExitCode;
}
static string SourceFile([CallerFilePath] string path = "") => path;
