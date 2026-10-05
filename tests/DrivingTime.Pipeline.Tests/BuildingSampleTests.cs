using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using DrivingTime.Pipeline;
using Xunit;

public class BuildingSampleTests
{
    private static string Fixture => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "citygml-building.xml"));
    private static Bounds RoadRegion => new() { MinEast = 384236.0361374, MinNorth = 6674060.89320437, MaxEast = 384636.0361374, MaxNorth = 6674360.89320437 };
    private static BuildingReport Inspect(string xml) => BuildingXml.Inspect(Encoding.UTF8.GetBytes(xml), RoadRegion);

    [Fact]
    public void ObservedSubsetIncludesForwardReferencesAndSeparatesAddressHeights()
    {
        var report = Inspect(Fixture); var building = Assert.Single(report.Buildings);
        Assert.Equal(2, building.GeometryCounts[(BuildingXml.Gml + "Polygon").ToString()]);
        Assert.Equal(1, building.Lod1Polygons); Assert.Equal(10, building.CoordinateTuples);
        Assert.Equal(12, building.GeometryBounds.MinHeight); Assert.Equal(20, building.GeometryBounds.MaxHeight);
        Assert.Equal(0, Assert.Single(building.AddressPoints).Height);
        Assert.Equal("resolved-in-building", Assert.Single(report.References).Status);
        Assert.Single(building.ExternalMetadata); Assert.True(building.OverlapsRoadRegion);
        Assert.Equal("2026-10-01T12:00:00", report.SourceTimestamp);
    }
    [Fact]
    public void Gk25ProjectionMatchesIndependentCityAddressAnchor()
    {
        var actual = BuildingXml.ToTm35(25495142, 6675054);
        Assert.InRange(Math.Abs(actual.East - 384236.0361374), 0, 0.01);
        Assert.InRange(Math.Abs(actual.North - 6674210.89320437), 0, 0.01);
    }
    [Theory]
    [InlineData("http://www.opengis.net/wfs/2.0", "http://www.opengis.net/wfs", "version")]
    [InlineData("http://www.opengis.net/citygml/building/2.0", "http://www.opengis.net/citygml/building/3.0", "building namespace")]
    [InlineData("http://www.opengis.net/gml", "http://www.opengis.net/gml/3.2", "GML namespace")]
    [InlineData("crs:EPSG::3900", "crs:EPSG::5702", "vertical datum")]
    [InlineData("srsDimension=\"3\"", "srsDimension=\"2\"", "dimension")]
    [InlineData("numberReturned=\"1\"", "numberReturned=\"2\"", "count")]
    [InlineData("25495240 6675037 12 25495250", "NaN 6675037 12 25495250", "nonfinite")]
    public void RejectsWrongNamespacesDatumsDimensionsAndMalformedPositions(string before, string after, string reason)
    {
        Assert.NotEmpty(reason);
        Assert.ThrowsAny<Exception>(() => Inspect(Fixture.Replace(before, after)));
    }
    [Fact]
    public void DtdAndTruncatedXmlAreRejected()
    {
        Assert.Throws<XmlException>(() => Inspect(Fixture.Replace("<!-- Repository-authored", "<!DOCTYPE w:FeatureCollection [<!ENTITY external SYSTEM 'file:///etc/passwd'>]><!-- Repository-authored")));
        Assert.Throws<XmlException>(() => Inspect(Fixture[..^30]));
    }
    [Fact]
    public void ReferencesAreReportedWithoutFollowingExternalUrls()
    {
        Assert.Equal("external-not-fetched", Assert.Single(Inspect(Fixture.Replace("#synthetic-roof", "https://example.invalid/geometry.xml#roof")).References).Status);
        Assert.Equal("unresolved-local", Assert.Single(Inspect(Fixture.Replace("#synthetic-roof", "#missing")).References).Status);
        Assert.Equal("unsupported-target-type", Assert.Single(Inspect(Fixture.Replace("#synthetic-roof", "#synthetic-solid")).References).Status);
    }
    [Fact]
    public void DuplicateIdsUnclosedRingsAndOverReturnedSamplesFail()
    {
        Assert.Throws<ArgumentException>(() => Inspect(Fixture.Replace("g:id=\"synthetic-roof\"", "g:id=\"synthetic-polygon\"")));
        Assert.Throws<ArgumentException>(() => Inspect(Fixture.Replace("25495240 6675037 12</g:posList>", "25495241 6675037 12</g:posList>")));
        var document = XDocument.Parse(Fixture);
        var member = document.Root!.Element(BuildingXml.Wfs + "member")!;
        var duplicate = new XElement(member);
        foreach (var id in duplicate.Descendants().Attributes(BuildingXml.Gml + "id")) id.Value += "-second";
        document.Root.Add(duplicate); document.Root.SetAttributeValue("numberReturned", 2);
        Assert.Throws<ArgumentException>(() => BuildingXml.Inspect(Encoding.UTF8.GetBytes(document.ToString()), RoadRegion, 1));
    }
    [Fact]
    public void HolesUnsupportedGeometryAndNonOverlapRemainVisible()
    {
        var doc = XDocument.Parse(Fixture);
        var polygon = doc.Descendants(BuildingXml.Gml + "Polygon").First();
        polygon.Add(new XElement(BuildingXml.Gml + "interior", new XElement(polygon.Element(BuildingXml.Gml + "exterior")!.Elements().Single())));
        doc.Descendants(BuildingXml.Bldg + "Building").Single().Add(new XElement(BuildingXml.Gml + "Curve"));
        var report = Inspect(doc.ToString());
        Assert.Equal(1, report.Buildings[0].InteriorRings);
        Assert.Contains(report.Buildings[0].Diagnostics, d => d.Contains("unsupported GML construct"));
        var far = new Bounds { MinEast = 384700, MinNorth = 6674500, MaxEast = 384720, MaxNorth = 6674520 };
        Assert.False(BuildingXml.Inspect(Encoding.UTF8.GetBytes(Fixture), far).Buildings[0].OverlapsRoadRegion);
    }
    [Fact]
    public async Task ByteCapAppliesToStreamAndConfigBoundsAreLimited()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => BuildingSampleCommand.ReadBounded(new MemoryStream(new byte[BuildingXml.MaxBytes + 1])));
        var request = new BuildingRequest(1, new Bounds { MinEast = 25495042, MinNorth = 6674954, MaxEast = 25495242, MaxNorth = 6675154 }, 1, "cache", RoadRegion);
        Assert.Contains("count=1", request.Url());
        Assert.Throws<ArgumentException>(() => (request with { Count = 6 }).Url());
        Assert.Throws<ArgumentException>(() => (request with { Region = request.Region with { MaxEast = request.Region.MinEast + 201 } }).Url());
    }
    [Fact]
    public async Task ImportPreservesBytesAndUnknownRetrievalThenOfflineReplayDetectsTampering()
    {
        var root = Path.Combine(Path.GetTempPath(), "building-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var input = Path.Combine(root, "input.xml"); File.WriteAllText(input, Fixture);
            var config = Path.Combine(root, "config.json");
            var request = new BuildingRequest(1, new Bounds { MinEast = 25495042, MinNorth = 6674954, MaxEast = 25495242, MaxNorth = 6675154 }, 1, "cache", RoadRegion);
            File.WriteAllText(config, JsonSerializer.Serialize(request, RoadSample.Json));
            Assert.Equal(0, await BuildingSampleCommand.Run(["--config", config, "--input", input]));
            var metadata = Directory.GetFiles(Path.Combine(root, "cache"), "import-provenance.json", SearchOption.AllDirectories).Single();
            var p = JsonSerializer.Deserialize<BuildingProvenance>(File.ReadAllText(metadata), RoadSample.Json)!;
            Assert.Null(p.RetrievedUtc); Assert.Null(p.HttpStatus); Assert.Equal("local-file", p.Mode);
            var payload = Path.GetDirectoryName(metadata)!;
            var raw = Path.Combine(payload, "raw.xml");
            Assert.Equal(File.ReadAllBytes(input), File.ReadAllBytes(raw));
            // Synthetic network metadata exercises offline replay; no live request occurs in tests.
            p = p with { Mode = "network", HttpStatus = 200, RetrievedUtc = p.RecordedUtc, Source = request.Url() };
            File.WriteAllText(Path.Combine(payload, "network-provenance.json"), JsonSerializer.Serialize(p, RoadSample.Json));
            File.WriteAllText(Path.Combine(Directory.GetParent(payload)!.FullName, "latest.json"), JsonSerializer.Serialize(p, RoadSample.Json));
            var before = File.ReadAllBytes(raw);
            Assert.Equal(0, await BuildingSampleCommand.Run(["--config", config]));
            Assert.Equal(before, File.ReadAllBytes(raw));
            File.AppendAllText(raw, " ");
            Assert.Equal(2, await BuildingSampleCommand.Run(["--config", config]));
        }
        finally { Directory.Delete(root, true); }
    }
}
