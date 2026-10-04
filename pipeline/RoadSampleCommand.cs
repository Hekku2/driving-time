using System.Text;
using System.Text.Json;

namespace DrivingTime.Pipeline;

public static class RoadSampleCommand
{
    public const string Usage = "road-sample --config <file.json> [--input <raw-overpass.json> | --refresh]; default uses verified cache, otherwise one live request.";
    private sealed record Provenance(int SchemaVersion, string RequestSha256, string PayloadSha256, int ByteCount,
        string Mode, string Source, DateTimeOffset ImportedOrRetrievedUtc, DateTimeOffset? RetrievedUtc,
        string Endpoint, string Method, string Body, string Query, RoadBounds RequestBounds,
        Dictionary<string, string[]> ResponseHeaders, int? HttpStatus, string Generator, string OsmBase,
        string HorizontalCrs, string VerticalDatum, string License, string Attribution, string TermsUrl,
        string Limitations);

    public static async Task<int> Run(string[] args)
    {
        try
        {
            if (args.Length is not (2 or 3 or 4) || args[0] != "--config" ||
                (args.Length == 3 && args[2] != "--refresh") || (args.Length == 4 && args[2] != "--input"))
                throw new ArgumentException(Usage);
            var configPath = Path.GetFullPath(args[1]);
            var request = JsonSerializer.Deserialize<RoadRequest>(File.ReadAllBytes(configPath), RoadSample.Json)
                ?? throw new ArgumentException("Road configuration is null.");
            var query = request.Query();
            var body = "data=" + Uri.EscapeDataString(query);
            var requestHash = RoadSample.Hash(Encoding.UTF8.GetBytes(RoadSample.Endpoint + "\nPOST\n" + body));
            var cacheRoot = Path.GetFullPath(request.CachePath, Path.GetDirectoryName(configPath)!);
            var input = args.Length == 4 ? Path.GetFullPath(args[3]) : null;
            var directory = Path.Combine(cacheRoot, requestHash);
            byte[] bytes;
            Provenance provenance;
            if (input == null && args.Length != 3 && Directory.Exists(directory))
            {
                var pointer = Path.Combine(directory, "latest.json");
                if (File.Exists(pointer))
                {
                    provenance = JsonSerializer.Deserialize<Provenance>(File.ReadAllBytes(pointer), RoadSample.Json)
                        ?? throw new ArgumentException("Cache provenance is null; use --refresh.");
                    if (provenance.Mode != "network" || provenance.HttpStatus != 200 || provenance.RetrievedUtc == null || provenance.RequestSha256 != requestHash || provenance.Query != query || provenance.PayloadSha256.Length != 64 ||
                        provenance.PayloadSha256.Any(c => !Uri.IsHexDigit(c)))
                        throw new ArgumentException("Cache request/provenance mismatch; use --refresh.");
                    bytes = await ReadBounded(File.OpenRead(Path.Combine(directory, provenance.PayloadSha256, "raw.json")));
                    if (bytes.Length != provenance.ByteCount || RoadSample.Hash(bytes) != provenance.PayloadSha256)
                        throw new ArgumentException("Cache checksum/size mismatch; restore bytes or use --refresh.");
                    var cached = Validate(bytes, request);
                    Console.WriteLine("Verified offline cache: " + Path.Combine(directory, provenance.PayloadSha256));
                    Console.WriteLine(JsonSerializer.Serialize(RoadSample.Summary(cached, request), RoadSample.Json));
                    return 0;
                }
            }
            var headers = new Dictionary<string, string[]>();
            int? status = null;
            DateTimeOffset? retrieved = null;
            if (input != null)
                bytes = await ReadBounded(File.OpenRead(input));
            else
            {
                Directory.CreateDirectory(cacheRoot);
                // Cross-process lock and persisted attempt time bound repeated invocations using this cache root.
                using var gate = new FileStream(Path.Combine(cacheRoot, "request.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                var lastPath = Path.Combine(cacheRoot, "last-attempt.txt");
                if (File.Exists(lastPath) && DateTimeOffset.TryParse(File.ReadAllText(lastPath), out var last) && DateTimeOffset.UtcNow - last < TimeSpan.FromSeconds(2))
                    throw new ArgumentException("Requests must be at least 2 seconds apart; wait before retrying.");
                File.WriteAllText(lastPath, DateTimeOffset.UtcNow.ToString("O"));
                using var handler = new HttpClientHandler { AllowAutoRedirect = false };
                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };
                client.DefaultRequestHeaders.UserAgent.ParseAdd("DrivingTime-P10/1.0 (bounded offline Helsinki prototype)");
                using var message = new HttpRequestMessage(HttpMethod.Post, RoadSample.Endpoint)
                { Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded") };
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                status = (int)response.StatusCode;
                headers = response.Headers.Concat(response.Content.Headers).ToDictionary(h => h.Key, h => h.Value.ToArray());
                if (status != 200)
                    throw new ArgumentException($"Overpass HTTP {status}; no retries. Retry-After: {response.Headers.RetryAfter?.ToString() ?? "not supplied"}. Respect service limits; use the documented Overpass Turbo raw JSON export and --input fallback.");
                if (response.Content.Headers.ContentLength > RoadSample.MaxBytes) throw new ArgumentException("Response exceeds 10 MiB; narrow the region.");
                bytes = await ReadBounded(await response.Content.ReadAsStreamAsync(timeout.Token), timeout.Token);
                retrieved = DateTimeOffset.UtcNow;
            }
            var extract = Validate(bytes, request);
            var payloadHash = RoadSample.Hash(bytes);
            provenance = new(1, requestHash, payloadHash, bytes.Length, input == null ? "network" : "local-file",
                input ?? RoadSample.Endpoint, DateTimeOffset.UtcNow, retrieved, RoadSample.Endpoint, "POST", body, query, request.Bounds(),
                headers, status, extract.Generator, extract.OsmBase, "EPSG:4326; lat/lon degrees", "none; no elevation supplied",
                "ODbL-1.0 (source declaration; local file origin unverified)", "© OpenStreetMap contributors", "https://www.openstreetmap.org/copyright",
                "Local imports record import time, not retrieval time; original source/query unverified. Complete-way geometry; approximate EUREF-FIN request projection; no physical road/height validation. Do not redistribute unreviewed raw data.");
            var output = Path.Combine(directory, payloadHash);
            Directory.CreateDirectory(output);
            Atomic(Path.Combine(output, "raw.json"), bytes);
            var meta = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(provenance, RoadSample.Json) + "\n");
            Atomic(Path.Combine(output, input == null ? "network-provenance.json" : "import-provenance.json"), meta);
            Atomic(Path.Combine(output, "summary.json"), Encoding.UTF8.GetBytes(JsonSerializer.Serialize(RoadSample.Summary(extract, request), RoadSample.Json) + "\n"));
            if (input == null) Atomic(Path.Combine(directory, "latest.json"), meta);
            Console.WriteLine("Validated raw cache: " + output);
            Console.WriteLine(JsonSerializer.Serialize(RoadSample.Summary(extract, request), RoadSample.Json));
            return 0;
        }
        catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException or JsonException or HttpRequestException or OperationCanceledException or NullReferenceException)
        {
            Console.Error.WriteLine("Road sample error: " + e.Message);
            Console.Error.WriteLine("Check config/raw JSON and permissions; timeouts/network errors need the Overpass Turbo raw JSON --input fallback. No automatic retry or mirror switching.");
            return 2;
        }
    }
    public static RoadExtract Validate(byte[] bytes, RoadRequest request)
    {
        var extract = RoadSample.Parse(bytes);
        var b = request.Bounds();
        foreach (var way in extract.Ways)
        {
            if (!way.Tags.TryGetValue("name", out var name) || name != request.StreetName)
                throw new ArgumentException($"Way {way.Id} does not match configured streetName.");
            if (!way.Geometry.Any(p => p.LatitudeDegrees >= b.South && p.LatitudeDegrees <= b.North && p.LongitudeDegrees >= b.West && p.LongitudeDegrees <= b.East))
                throw new ArgumentException($"Way {way.Id} has no vertex in request bounds; check input/region.");
        }
        return extract;
    }
    public static async Task<byte[]> ReadBounded(Stream stream, CancellationToken cancellationToken = default)
    {
        using (stream)
        using (var memory = new MemoryStream())
        {
            var buffer = new byte[65536];
            int count;
            while ((count = await stream.ReadAsync(buffer, cancellationToken)) != 0)
            {
                if (memory.Length + count > RoadSample.MaxBytes) throw new ArgumentException("Input/response exceeds 10 MiB; narrow the sample.");
                memory.Write(buffer, 0, count);
            }
            return memory.ToArray();
        }
    }
    private static void Atomic(string path, byte[] bytes)
    {
        var partial = path + "." + Guid.NewGuid().ToString("N") + ".partial";
        try { File.WriteAllBytes(partial, bytes); File.Move(partial, path, true); }
        finally { if (File.Exists(partial)) File.Delete(partial); }
    }
}
