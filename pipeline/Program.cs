using System.Security.Cryptography;
using System.Text;

namespace DrivingTime.Pipeline;

public static class Program
{
    public static int Main(string[] args)
    {
        const string usage = "Usage: dotnet run --project pipeline/DrivingTime.Pipeline.csproj --no-restore -- --config <file.json>";
        if (args is ["--help"])
        {
            Console.WriteLine(usage);
            return 0;
        }
        if (args is not ["--config", var configPath] || string.IsNullOrWhiteSpace(configPath))
        {
            Console.Error.WriteLine(usage);
            return 2;
        }
        try
        {
            var fullConfigPath = Path.GetFullPath(configPath);
            var bytes = File.ReadAllBytes(fullConfigPath);
            var config = SceneConfiguration.Parse(new UTF8Encoding(false, true).GetString(bytes));
            var output = Path.GetFullPath(config.ManifestPath, Path.GetDirectoryName(fullConfigPath)!);
            if (output == fullConfigPath)
                throw new ArgumentException("manifestPath must differ from the configuration file path.");
            var manifest = SyntheticManifest.Serialize(config, Convert.ToHexStringLower(SHA256.HashData(bytes)));
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            File.WriteAllText(output, manifest, new UTF8Encoding(false));
            Console.WriteLine($"Wrote synthetic manifest v1: {output}");
            return 0;
        }
        catch (Exception error) when (error is ArgumentException or IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Pipeline error: {error.Message}");
            Console.Error.WriteLine("Check --config, schema fields and output directory permissions; see docs/PIPELINE.md.");
            return 2;
        }
    }
}
