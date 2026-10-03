using System.Globalization;
using System.Text.Json;
using DrivingTime.Pipeline;
using Xunit;

namespace DrivingTime.Pipeline.Tests;

public sealed class ConfigurationTests
{
    private static string Sample => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "synthetic-scene.json"));

    [Theory]
    [InlineData("\"schemaVersion\": 1", "\"schemaVersion\": 2", "schemaVersion")]
    [InlineData("\"maxEast\": 30", "\"maxEast\": -30", "positive extents")]
    [InlineData("\"east\": 0", "\"east\": 31", "inside region")]
    [InlineData("\"tileSizeMetres\": 500", "\"tileSizeMetres\": 0", "tileSizeMetres")]
    [InlineData("synthetic-local-metres", "EPSG:4326", "coordinateSystem")]
    [InlineData("generated/synthetic-scene.manifest.json", "../escape.json", "manifestPath")]
    [InlineData("generated/synthetic-scene.manifest.json", "/tmp/escape.json", "manifestPath")]
    [InlineData("\"maxEast\": 30", "\"maxEast\": 1e999", "finite")]
    public void RejectsInvalidSemantics(string before, string after, string message)
    {
        var error = Assert.Throws<ArgumentException>(() => SceneConfiguration.Parse(Sample.Replace(before, after)));
        Assert.Contains(message, error.Message);
    }

    [Theory]
    [InlineData("{", "Invalid configuration JSON")]
    [InlineData("null", "not null")]
    [InlineData("{}", "Invalid configuration JSON")]
    public void RejectsIncompleteJson(string json, string message)
    {
        Assert.Contains(message, Assert.Throws<ArgumentException>(() => SceneConfiguration.Parse(json)).Message);
    }

    [Fact]
    public void RejectsUnknownAndDuplicateFieldsAndNullNestedObjects()
    {
        Assert.Throws<ArgumentException>(() => SceneConfiguration.Parse(Sample.Replace("\"sceneId\"", "\"sceneID\"")));
        Assert.Throws<ArgumentException>(() => SceneConfiguration.Parse(Sample.Replace("\"schemaVersion\": 1", "\"schemaVersion\": 1, \"schemaVersion\": 1")));
        Assert.Throws<ArgumentException>(() => SceneConfiguration.Parse(Sample.Replace("{ \"east\": 0, \"north\": 0, \"height\": 0 }", "null")));
    }

    [Fact]
    public void AcceptsBoundaryOriginAndRejectsOverflowingExtent()
    {
        Assert.Equal(30, SceneConfiguration.Parse(Sample.Replace("\"east\": 0", "\"east\": 30")).Origin.East);
        var json = Sample.Replace("\"minEast\": -30", "\"minEast\": -1e308").Replace("\"maxEast\": 30", "\"maxEast\": 1e308");
        Assert.Contains("finite positive extents", Assert.Throws<ArgumentException>(() => SceneConfiguration.Parse(json)).Message);
    }

    [Fact]
    public void ManifestHasStableOrderingAndCultureIndependentBytes()
    {
        var config = SceneConfiguration.Parse(Sample);
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fi-FI");
            var first = SyntheticManifest.Serialize(config, "fixture-hash");
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            Assert.Equal(first, SyntheticManifest.Serialize(config, "fixture-hash"));
            using var doc = JsonDocument.Parse(first);
            Assert.Equal(1, doc.RootElement.GetProperty("schemaVersion").GetInt32());
            var objects = doc.RootElement.GetProperty("objects").EnumerateArray().ToArray();
            Assert.Equal(new[] { "ground", "reference-box" }, objects.Select(item => item.GetProperty("id").GetString()));
            Assert.Equal(60, objects[0].GetProperty("sizeMetres").GetProperty("east").GetDouble());
            Assert.Equal(JsonValueKind.Null, doc.RootElement.GetProperty("provenance").GetProperty("retrievalDate").ValueKind);
            Assert.EndsWith("\n", first);
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [Fact]
    public void CliResolvesOutputRelativeToConfigAndPreservesExistingOutputOnValidationFailure()
    {
        var directory = Path.Combine(Path.GetTempPath(), "driving-time-p06-" + Guid.NewGuid());
        Directory.CreateDirectory(directory);
        try
        {
            var input = Path.Combine(directory, "scene.json");
            File.WriteAllText(input, Sample);
            Assert.Equal(0, Program.Main(["--config", input]));
            var output = Path.Combine(directory, "generated", "synthetic-scene.manifest.json");
            var first = File.ReadAllBytes(output);
            Assert.Equal(0, Program.Main(["--config", input]));
            Assert.Equal(first, File.ReadAllBytes(output));
            File.WriteAllText(input, Sample.Replace("\"tileSizeMetres\": 500", "\"tileSizeMetres\": -1"));
            Assert.Equal(2, Program.Main(["--config", input]));
            Assert.Equal(first, File.ReadAllBytes(output));
            Assert.Equal(2, Program.Main(["--config", Path.Combine(directory, "missing.json")]));
            Assert.Equal(2, Program.Main(["--unknown"]));
        }
        finally { Directory.Delete(directory, recursive: true); }
    }
}
