using System.Security.Cryptography;
using System.Text;

namespace DrivingTime.Pipeline;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "road-sample") return RoadSampleCommand.Run(args[1..]).GetAwaiter().GetResult();
        if (args.Length > 0 && args[0] == "road-graph") return RoadGraphCommand.Run(args[1..]);
        const string usage = "Usage: --config <file.json> [--stage-fixture <directory>] or road-sample --config <file.json> [--input <raw.json> | --refresh] or road-graph --config <file.json> (see docs/PIPELINE.md)";
        if (args is ["--help"])
        {
            Console.WriteLine(usage);
            return 0;
        }
        if ((args.Length != 2 && args.Length != 4) || args[0] != "--config" ||
            string.IsNullOrWhiteSpace(args[1]) || (args.Length == 4 && (args[2] != "--stage-fixture" || string.IsNullOrWhiteSpace(args[3]))))
        {
            Console.Error.WriteLine(usage);
            return 2;
        }
        try
        {
            var fullConfigPath = Path.GetFullPath(args[1]);
            var bytes = File.ReadAllBytes(fullConfigPath);
            var config = SceneConfiguration.Parse(new UTF8Encoding(false, true).GetString(bytes));
            if (args.Length == 4)
            {
                GlbFixture.Write(config, Convert.ToHexStringLower(SHA256.HashData(bytes)), args[3]);
                return 0;
            }
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
