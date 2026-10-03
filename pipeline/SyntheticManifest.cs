using System.Text.Json;

namespace DrivingTime.Pipeline;

// Descriptive synthetic source coordinates only. Engine transforms and mesh assets belong to P07/P08.
public sealed record SyntheticPrimitive(string Id, string Kind, Origin Centre, Origin SizeMetres);
public sealed record Provenance(string Source, string License, string? RetrievalDate,
    string ConfigurationSha256, string VerticalDatum, string Limitations);
public sealed record SyntheticManifest(int SchemaVersion, string SceneId, string CoordinateSystem,
    Bounds Region, Origin Origin, double TileSizeMetres, Provenance Provenance,
    SyntheticPrimitive[] Objects)
{
    public static string Serialize(SceneConfiguration config, string configurationSha256)
    {
        var width = config.Region.MaxEast - config.Region.MinEast;
        var depth = config.Region.MaxNorth - config.Region.MinNorth;
        SyntheticPrimitive[] objects =
        [
            new("reference-box", "box", config.Origin with { Height = config.Origin.Height + 1 },
                new Origin { East = 2, North = 2, Height = 2 }),
            new("ground", "plane", new Origin
                {
                    East = config.Region.MinEast + width / 2,
                    North = config.Region.MinNorth + depth / 2,
                    Height = config.Origin.Height
                }, new Origin { East = width, North = depth, Height = 0 })
        ];
        var manifest = new SyntheticManifest(1, config.SceneId, config.CoordinateSystem,
            config.Region, config.Origin, config.TileSizeMetres,
            new Provenance("Repository-authored synthetic fixture; no downloaded data",
                "Repository code terms; no third-party dataset license applies", null,
                configurationSha256, "Synthetic height in metres; no geographic vertical datum",
                "Descriptions only; no meshes, engine axis mapping, geographic CRS or tile lifecycle"),
            objects.OrderBy(item => item.Id, StringComparer.Ordinal).ToArray());
        return JsonSerializer.Serialize(manifest, SceneConfiguration.JsonOptions) + "\n";
    }
}
