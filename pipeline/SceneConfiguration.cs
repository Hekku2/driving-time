using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrivingTime.Pipeline;

public sealed record Bounds
{
    public required double MinEast { get; init; }
    public required double MinNorth { get; init; }
    public required double MaxEast { get; init; }
    public required double MaxNorth { get; init; }
}

public sealed record Origin
{
    public required double East { get; init; }
    public required double North { get; init; }
    public required double Height { get; init; }
}

public sealed record SceneConfiguration
{
    public required int SchemaVersion { get; init; }
    public required string SceneId { get; init; }
    public required string CoordinateSystem { get; init; }
    public required Bounds Region { get; init; }
    public required Origin Origin { get; init; }
    public required double TileSizeMetres { get; init; }
    public required string ManifestPath { get; init; }

    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        AllowDuplicateProperties = false,
        WriteIndented = true
    };

    public static SceneConfiguration Parse(string json)
    {
        SceneConfiguration config;
        try
        {
            config = JsonSerializer.Deserialize<SceneConfiguration>(json, JsonOptions)
                ?? throw new ArgumentException("Configuration must be a JSON object, not null.");
        }
        catch (JsonException error)
        {
            throw new ArgumentException($"Invalid configuration JSON at {error.Path ?? "$"}: {error.Message}", error);
        }
        config.Validate();
        return config;
    }

    private void Validate()
    {
        if (SchemaVersion != 1)
            throw new ArgumentException("schemaVersion must be 1.");
        if (string.IsNullOrWhiteSpace(SceneId) || SceneId.Length > 64 ||
            SceneId.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
            throw new ArgumentException("sceneId must contain 1–64 ASCII letters, digits, '-' or '_'.");
        if (CoordinateSystem != "synthetic-local-metres")
            throw new ArgumentException("coordinateSystem must be 'synthetic-local-metres'; geographic transforms are not supported yet.");
        if (Region is null || Origin is null)
            throw new ArgumentException("region and origin must be non-null objects.");
        double[] coordinates = [Region.MinEast, Region.MinNorth, Region.MaxEast, Region.MaxNorth,
            Origin.East, Origin.North, Origin.Height];
        if (coordinates.Any(value => !double.IsFinite(value)))
            throw new ArgumentException("region and origin coordinates must be finite numbers in metres.");
        var width = Region.MaxEast - Region.MinEast;
        var depth = Region.MaxNorth - Region.MinNorth;
        if (!double.IsFinite(width) || !double.IsFinite(depth) || width <= 0 || depth <= 0)
            throw new ArgumentException("region must have finite positive extents: minEast < maxEast and minNorth < maxNorth.");
        if (Origin.East < Region.MinEast || Origin.East > Region.MaxEast ||
            Origin.North < Region.MinNorth || Origin.North > Region.MaxNorth)
            throw new ArgumentException("origin east/north must lie inside region bounds (edges allowed).");
        if (!double.IsFinite(TileSizeMetres) || TileSizeMetres <= 0)
            throw new ArgumentException("tileSizeMetres must be a finite positive number.");
        if (string.IsNullOrWhiteSpace(ManifestPath) || Path.IsPathRooted(ManifestPath) ||
            ManifestPath.Contains('\\') || ManifestPath.Contains(':') ||
            ManifestPath.Split('/').Any(part => part is ".." or "." or "") ||
            ManifestPath.Any(char.IsControl) || !ManifestPath.EndsWith(".json", StringComparison.Ordinal))
            throw new ArgumentException("manifestPath must be a relative .json file path using '/', without '.', '..', empty segments or control characters.");
    }
}
