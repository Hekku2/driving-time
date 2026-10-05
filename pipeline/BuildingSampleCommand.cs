using System.Text;
using System.Text.Json;

namespace DrivingTime.Pipeline;

public sealed record BuildingRequest(int SchemaVersion, Bounds Region, int Count, string CachePath, Bounds RoadRegion)
{
    public string Url()
    {
        CoordinateFrame.Finite(Region.MinEast, Region.MinNorth, Region.MaxEast, Region.MaxNorth,
            RoadRegion.MinEast, RoadRegion.MinNorth, RoadRegion.MaxEast, RoadRegion.MaxNorth);
        if (SchemaVersion != 1 || Count is < 1 or > 5 || string.IsNullOrWhiteSpace(CachePath) ||
            Region.MinEast < 25494642 || Region.MaxEast > 25495642 || Region.MinNorth < 6674554 || Region.MaxNorth > 6675554 ||
            Region.MaxEast <= Region.MinEast || Region.MaxNorth <= Region.MinNorth ||
            Region.MaxEast - Region.MinEast > 200 || Region.MaxNorth - Region.MinNorth > 200 ||
            RoadRegion.MinEast < 383736.0361374 || RoadRegion.MaxEast > 384736.0361374 ||
            RoadRegion.MinNorth < 6673710.89320437 || RoadRegion.MaxNorth > 6674710.89320437 ||
            RoadRegion.MaxEast <= RoadRegion.MinEast || RoadRegion.MaxNorth <= RoadRegion.MinNorth)
            throw new ArgumentException("Building config requires schema v1, count 1–5, a positive <=200 m native GK25 box within the selected square, a road region within the projected square and cachePath.");
        return FormattableString.Invariant($"https://kartta.hel.fi/3d/citydb-wfs/wfs?service=WFS&version=2.0.0&request=GetFeature&typeNames=bldg:Building&count={Count}&bbox={Region.MinEast:R},{Region.MinNorth:R},{Region.MaxEast:R},{Region.MaxNorth:R},urn:ogc:def:crs:EPSG::3879");
    }
}
public sealed record BuildingProvenance(int SchemaVersion, string RequestSha256, string PayloadSha256, int ByteCount,
    string Mode, string Source, string RequestUrl, DateTimeOffset RecordedUtc, DateTimeOffset? RetrievedUtc,
    int? HttpStatus, Dictionary<string, string[]> ResponseHeaders, string HorizontalCrs, string VerticalDatum,
    string AxisOrder, string License, string Attribution, string TermsUrl, string TermsCheckedDate, string Limitations);

public static class BuildingSampleCommand
{
    public const string Usage = "building-sample --config <file.json> [--refresh | --input <CityGML-WFS.xml>]; default verifies cache or downloads one bounded sample";
    public static async Task<int> Run(string[] args)
    {
        try
        {
            if (args.Length is not (2 or 3 or 4) || args[0] != "--config" ||
                args.Length == 3 && args[2] != "--refresh" || args.Length == 4 && args[2] != "--input")
                throw new ArgumentException(Usage);
            var configPath = Path.GetFullPath(args[1]);
            var request = JsonSerializer.Deserialize<BuildingRequest>(File.ReadAllBytes(configPath), RoadSample.Json)
                ?? throw new ArgumentException("Building configuration is null.");
            var url = request.Url();
            var requestHash = RoadSample.Hash(Encoding.UTF8.GetBytes("GET\n" + url));
            var cacheRoot = Path.GetFullPath(request.CachePath, Path.GetDirectoryName(configPath)!);
            var requestDirectory = Path.Combine(cacheRoot, requestHash);
            var pointer = Path.Combine(requestDirectory, "latest.json");
            var input = args.Length == 4 ? Path.GetFullPath(args[3]) : null;
            byte[] bytes;
            BuildingProvenance provenance;
            BuildingReport report;
            if (input == null && args.Length == 2 && File.Exists(pointer))
            {
                provenance = JsonSerializer.Deserialize<BuildingProvenance>(await File.ReadAllBytesAsync(pointer), RoadSample.Json)
                    ?? throw new ArgumentException("Cache provenance is null.");
                if (provenance.RequestSha256 != requestHash || provenance.RequestUrl != url || provenance.Mode != "network" ||
                    provenance.HttpStatus != 200 || provenance.RetrievedUtc == null ||
                    provenance.PayloadSha256.Length != 64 || provenance.PayloadSha256.Any(c => !Uri.IsHexDigit(c)))
                    throw new ArgumentException("Cache request/provenance mismatch; restore cache or use --refresh.");
                var directory = Path.Combine(requestDirectory, provenance.PayloadSha256);
                bytes = await ReadBounded(File.OpenRead(Path.Combine(directory, "raw.xml")));
                Verify(bytes, provenance);
                var stored = JsonSerializer.Deserialize<BuildingProvenance>(await File.ReadAllBytesAsync(Path.Combine(directory, "network-provenance.json")), RoadSample.Json);
                if (JsonSerializer.Serialize(stored, RoadSample.Json) != JsonSerializer.Serialize(provenance, RoadSample.Json))
                    throw new ArgumentException("Cache pointer and network provenance differ; restore the original cache.");
                report = BuildingXml.Inspect(bytes, request.RoadRegion, request.Count);
                // Recompute the report for current inspection configuration; source/provenance stay unchanged.
                Atomic(Path.Combine(directory, "inspection.json"), JsonSerializer.SerializeToUtf8Bytes(report, RoadSample.Json));
                Print(directory, report, "Verified offline building cache");
                return 0;
            }
            var headers = new Dictionary<string, string[]>();
            DateTimeOffset? retrieved = null;
            int? status = null;
            if (input != null) bytes = await ReadBounded(File.OpenRead(input));
            else
            {
                Directory.CreateDirectory(cacheRoot);
                using var gate = new FileStream(Path.Combine(cacheRoot, "request.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                var last = Path.Combine(cacheRoot, "last-attempt.txt");
                if (File.Exists(last) && DateTimeOffset.TryParse(File.ReadAllText(last), out var previous) && DateTimeOffset.UtcNow - previous < TimeSpan.FromSeconds(2))
                    throw new ArgumentException("Building requests must be at least two seconds apart; no automatic retries.");
                File.WriteAllText(last, DateTimeOffset.UtcNow.ToString("O"));
                using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(30) };
                client.DefaultRequestHeaders.UserAgent.ParseAdd("DrivingTime-BuildingSample/1.0");
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                status = (int)response.StatusCode;
                if (status != 200) throw new ArgumentException($"Building WFS HTTP {status}; no retries. Retry-After: {response.Headers.RetryAfter?.ToString() ?? "not supplied"}. Use a saved bounded WFS XML with --input if necessary.");
                if (response.Content.Headers.ContentLength > BuildingXml.MaxBytes) throw new ArgumentException("Building response exceeds 4 MiB; narrow the region/count.");
                headers = response.Headers.Concat(response.Content.Headers).ToDictionary(h => h.Key, h => h.Value.ToArray());
                bytes = await ReadBounded(await response.Content.ReadAsStreamAsync(timeout.Token), timeout.Token);
                retrieved = DateTimeOffset.UtcNow;
            }
            report = BuildingXml.Inspect(bytes, request.RoadRegion, request.Count);
            var hash = RoadSample.Hash(bytes);
            provenance = new(1, requestHash, hash, bytes.Length, input == null ? "network" : "local-file", input ?? url,
                url, DateTimeOffset.UtcNow, retrieved, status, headers, "EPSG:3879 ETRS-GK25", "N2000 / EPSG:3900; provider/response declaration, independent height control unverified",
                "Observed physical E,N,H metres; differs from EPSG authority N,E axis order", "CC-BY-4.0 (provider declaration; local import origin unverified)",
                "City of Helsinki, City Survey Services", "https://www.hel.fi/en/decision-making/information-on-helsinki/maps-and-geospatial-data/helsinki-3d", "2026-10-05",
                "Bounded partial WFS response, no paging or referenced-resource downloads. Schema locations are declarations, not downloaded/validated XSDs. Local import records import time and unknown original retrieval. Raw sources stay ignored; no redistribution review performed.");
            var output = Path.Combine(requestDirectory, hash);
            Directory.CreateDirectory(output);
            Atomic(Path.Combine(output, "raw.xml"), bytes);
            Atomic(Path.Combine(output, input == null ? "network-provenance.json" : "import-provenance.json"), JsonSerializer.SerializeToUtf8Bytes(provenance, RoadSample.Json));
            Atomic(Path.Combine(output, "inspection.json"), JsonSerializer.SerializeToUtf8Bytes(report, RoadSample.Json));
            if (input == null) Atomic(pointer, JsonSerializer.SerializeToUtf8Bytes(provenance, RoadSample.Json));
            Print(output, report, "Validated building cache");
            return 0;
        }
        catch (Exception error) when (error is ArgumentException or IOException or UnauthorizedAccessException or JsonException or
            System.Xml.XmlException or InvalidOperationException or FormatException or OverflowException or HttpRequestException or OperationCanceledException)
        {
            Console.Error.WriteLine("Building sample failed: " + error.Message);
            Console.Error.WriteLine("Check bounded config, XML namespaces/geometry and cache integrity; see docs/PIPELINE.md.");
            return 2;
        }
    }
    public static void Verify(byte[] bytes, BuildingProvenance provenance)
    {
        if (provenance.SchemaVersion != 1 || provenance.ByteCount != bytes.Length || provenance.PayloadSha256 != RoadSample.Hash(bytes))
            throw new ArgumentException("Building raw payload checksum/size/schema mismatch; restore bytes or explicitly refresh.");
    }
    public static async Task<byte[]> ReadBounded(Stream stream, CancellationToken cancellation = default)
    {
        using (stream)
        using (var output = new MemoryStream())
        {
            var buffer = new byte[8192];
            int count;
            while ((count = await stream.ReadAsync(buffer, cancellation)) > 0)
            {
                if (output.Length + count > BuildingXml.MaxBytes) throw new ArgumentException("Building input exceeds 4 MiB.");
                output.Write(buffer, 0, count);
            }
            return output.ToArray();
        }
    }
    private static void Atomic(string path, byte[] bytes)
    {
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".partial";
        try { File.WriteAllBytes(temporary, bytes); File.Move(temporary, path, true); }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
    private static void Print(string directory, BuildingReport report, string label)
    {
        Console.WriteLine(label + ": " + directory);
        Console.WriteLine($"Buildings: {report.NumberReturned}/{report.NumberMatched}; polygons: {report.Buildings.Sum(b => b.GeometryCounts.GetValueOrDefault((BuildingXml.Gml + "Polygon").ToString()))}; references: {report.References.Length}; road-region overlap: {report.Buildings.Count(b => b.OverlapsRoadRegion)}.");
        foreach (var note in report.Diagnostics.Concat(report.Buildings.SelectMany(b => b.Diagnostics))) Console.WriteLine("NOTE: " + note);
        Console.WriteLine("Offline inspection: " + Path.Combine(directory, "inspection.json"));
    }
}
