using System.Text;
using System.Text.Json;
using DrivingTime.Pipeline;
using Xunit;

namespace DrivingTime.Pipeline.Tests;

public sealed class RoadSampleTests
{
    private static byte[] Fixture => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "osm-road-sample.json"));
    private static RoadRequest Request => JsonSerializer.Deserialize<RoadRequest>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "road-sample.json")), RoadSample.Json)!;

    [Fact]
    public void PreservesIdsRawTagsMissingValuesAndMeasuredBounds()
    {
        var extract = RoadSampleCommand.Validate(Fixture, Request);
        Assert.Equal(new long[] { 101, 102 }, extract.Ways.Select(w => w.Id));
        Assert.Equal("-1", extract.Ways[0].Tags["oneway"]);
        Assert.False(extract.Ways[1].Tags.ContainsKey("oneway"));
        Assert.Null(extract.Ways[1].Timestamp);
        using var summary = JsonDocument.Parse(JsonSerializer.Serialize(RoadSample.Summary(extract, Request), RoadSample.Json));
        var root = summary.RootElement;
        Assert.Equal(3, root.GetProperty("uniqueNodeCount").GetInt32());
        Assert.Equal(60.18828858, root.GetProperty("actualBounds").GetProperty("south").GetDouble());
        Assert.Equal(24.916, root.GetProperty("actualBounds").GetProperty("east").GetDouble());
        var tags = root.GetProperty("tags");
        Assert.Equal(1, tags.GetProperty("oneway").GetProperty("missing").GetInt32());
        Assert.Equal(1, tags.GetProperty("maxspeed").GetProperty("values").GetProperty("walk").GetInt32());
        Assert.Equal(2, tags.GetProperty("access").GetProperty("missing").GetInt32());
    }

    [Theory]
    [InlineData("\"nodes\": [1, 2]", "\"nodes\": [1]")]
    [InlineData("\"lon\": 24.916", "\"missing\": 24.916")]
    [InlineData("\"id\": 102", "\"id\": 101")]
    [InlineData("\"lon\": 24.916", "\"lon\": 999")]
    [InlineData("\"version\": 0.6", "\"remark\": \"timeout\", \"version\": 0.6")]
    [InlineData("\"nodes\": [1, 2]", "\"nodes\": [1, -2]")]
    [InlineData("\"highway\": \"residential\"", "\"highway\": null")]
    [InlineData("\"version\": 0.6", "\"version\": 0.6, \"version\": 0.6")]
    [InlineData("\"timestamp_osm_base\"", "\"missing\"")]
    [InlineData("\"lon\": 24.914 }, { \"lat\": 60.1884", "\"lon\": 24.915 }, { \"lat\": 60.1884")]
    public void DetectsMalformedIncompleteOrConflictingData(string before, string after)
    {
        Assert.Throws<ArgumentException>(() => RoadSample.Parse(Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(Fixture).Replace(before, after))));
    }

    [Theory]
    [InlineData("<html>error</html>")]
    [InlineData("{}")]
    [InlineData("{\"version\":0.6")]
    public void RejectsErrorBodiesAndTruncation(string input) => Assert.Throws<ArgumentException>(() => RoadSample.Parse(Encoding.UTF8.GetBytes(input)));

    [Fact]
    public void EnforcesAreaAndQueryContract()
    {
        var request = Request;
        var b = request.Bounds();
        var projection = new Tm35FinProjection();
        foreach (var e in new[] { request.Region.MinEast, request.Region.MaxEast })
        foreach (var n in new[] { request.Region.MinNorth, request.Region.MaxNorth })
        {
            var p = projection.ToEurefFin(e, n);
            Assert.InRange(p.LatitudeDegrees, b.South, b.North);
            Assert.InRange(p.LongitudeDegrees, b.West, b.East);
        }
        Assert.Contains("[\"highway\"][\"name\"=\"Stenbäckinkatu\"]", request.Query());
        Assert.EndsWith("out meta geom;", request.Query());
        Assert.Throws<ArgumentException>(() => (request with { Region = request.Region with { MaxEast = 400000 } }).Query());
        Assert.Throws<ArgumentException>(() => RoadSampleCommand.Validate(Fixture, request with { StreetName = "Other" }));
    }

    [Fact]
    public async Task ImportKeepsExactRawBytesAndDoesNotCreateNetworkPointer()
    {
        var temp = Path.Combine(Path.GetTempPath(), "p10-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            var config = Path.Combine(temp, "config.json");
            var input = Path.Combine(temp, "input.json");
            File.WriteAllText(config, JsonSerializer.Serialize(Request with { CachePath = "cache" }, RoadSample.Json));
            File.WriteAllBytes(input, Fixture);
            Assert.Equal(0, await RoadSampleCommand.Run(["--config", config, "--input", input]));
            var raw = Assert.Single(Directory.GetFiles(temp, "raw.json", SearchOption.AllDirectories));
            Assert.Equal(Fixture, File.ReadAllBytes(raw));
            Assert.Empty(Directory.GetFiles(temp, "latest.json", SearchOption.AllDirectories));
            using var meta = JsonDocument.Parse(File.ReadAllText(Assert.Single(Directory.GetFiles(temp, "import-provenance.json", SearchOption.AllDirectories))));
            Assert.Equal(RoadSample.Hash(Fixture), meta.RootElement.GetProperty("payloadSha256").GetString());
            Assert.Equal(JsonValueKind.Null, meta.RootElement.GetProperty("retrievedUtc").ValueKind);
            Assert.Equal(0, await RoadSampleCommand.Run(["--config", config, "--input", raw]));
            File.WriteAllText(input, "{}");
            Assert.Equal(2, await RoadSampleCommand.Run(["--config", config, "--input", input]));
            Assert.Single(Directory.GetFiles(temp, "raw.json", SearchOption.AllDirectories));
            // Simulate a recorded successful retrieval, then prove default cache uses no network and detects tampering.
            var import = File.ReadAllText(Path.Combine(Path.GetDirectoryName(raw)!, "import-provenance.json"));
            var pointer = Path.Combine(Directory.GetParent(Path.GetDirectoryName(raw)!)!.FullName, "latest.json");
            var simulated = System.Text.Json.Nodes.JsonNode.Parse(import)!;
            simulated["mode"] = "network"; simulated["httpStatus"] = 200; simulated["retrievedUtc"] = "2026-10-04T00:00:00Z";
            File.WriteAllText(pointer, simulated.ToJsonString());
            Assert.Equal(0, await RoadSampleCommand.Run(["--config", config]));
            File.AppendAllText(raw, " ");
            Assert.Equal(2, await RoadSampleCommand.Run(["--config", config]));
        }
        finally { Directory.Delete(temp, true); }
    }

    [Fact]
    public async Task ByteLimitStopsBeforeParsingOrCaching()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => RoadSampleCommand.ReadBounded(new MemoryStream(new byte[RoadSample.MaxBytes + 1])));
    }
}
