// Offline research inspection only; no retrieval or pipeline conversion.
// dotnet run --file scripts/p09-inspect.cs -- artifacts/p09
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml.Linq;

var folder = args.Length == 1 ? args[0] : "artifacts/p09";
using var origin = JsonDocument.Parse(File.ReadAllText(Path.Combine(folder, "p09-origin.json")));
var feature = origin.RootElement.GetProperty("features")[0];
var coordinates = feature.GetProperty("geometry").GetProperty("coordinates");
var east = coordinates[0].GetDouble();
var north = coordinates[1].GetDouble();
Console.WriteLine($"Origin {feature.GetProperty("id")}: E={east:R}, N={north:R} EPSG:3067");
Console.WriteLine($"Region E=[{east - 500:R},{east + 500:R}], N=[{north - 500:R},{north + 500:R}]");

var gml = XDocument.Load(Path.Combine(folder, "p09-building.gml"));
XNamespace b = "http://www.opengis.net/citygml/building/2.0";
XNamespace g = "http://www.opengis.net/gml";
XNamespace core = "http://www.opengis.net/citygml/2.0";
var building = gml.Descendants(b + "Building").Single();
Console.WriteLine($"Building {building.Attribute(g + "id")?.Value}; lower={building.Descendants(g + "lowerCorner").First().Value}; upper={building.Descendants(g + "upperCorner").First().Value}");
Console.WriteLine("Creation dates (object metadata, not survey dates): " + string.Join(",", building.Descendants(core + "creationDate").Select(x => x.Value).Distinct().Order()));
Console.WriteLine($"lod1Solid={building.Elements(b + "lod1Solid").Count()}, lod2Solid={building.Elements(b + "lod2Solid").Count()}");

// Deliberately restricted TIFF inspection for this tiny uncompressed probe, not a production reader.
var bytes = File.ReadAllBytes(Path.Combine(folder, "p09-terrain.tif"));
if (bytes[0] != 'M' || bytes[1] != 'M') throw new InvalidDataException("Probe must be big-endian TIFF.");
ushort U16(int at) => BinaryPrimitives.ReadUInt16BigEndian(bytes.AsSpan(at, 2));
uint U32(int at) => BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(at, 4));
double F64(int at) => BitConverter.Int64BitsToDouble(BinaryPrimitives.ReadInt64BigEndian(bytes.AsSpan(at, 8)));
if (U16(2) != 42) throw new InvalidDataException("Not classic TIFF.");
var ifd = checked((int)U32(4));
var tags = new Dictionary<ushort, (ushort Type, uint Count, int At)>();
for (var i = 0; i < U16(ifd); i++)
{
    var at = ifd + 2 + 12 * i;
    var type = U16(at + 2);
    var count = U32(at + 4);
    var size = type switch { 3 => 2, 4 => 4, 5 or 12 => 8, 2 => 1, _ => throw new InvalidDataException($"Unsupported probe TIFF type {type}") };
    tags.Add(U16(at), (type, count, count * size <= 4 ? at + 8 : checked((int)U32(at + 8))));
}
uint Scalar(ushort tag) => tags[tag].Type == 3 ? U16(tags[tag].At) : U32(tags[tag].At);
var width = Scalar(256); var height = Scalar(257);
var offsetTag = (ushort)(tags.ContainsKey(273) ? 273 : 324);
if (width != 4 || height != 4 || Scalar(258) != 32 || Scalar(259) != 1 || Scalar(339) != 3 || tags[offsetTag].Count != 1)
    throw new InvalidDataException("Expected 4x4, Float32, uncompressed single-strip/tile probe.");
Console.WriteLine("TIFF tags: " + string.Join(",", tags.Keys));
var transformation = tags[34264];
var pixelX = F64(transformation.At); var pixelY = -F64(transformation.At + 40);
var west = F64(transformation.At + 24); var top = F64(transformation.At + 56);
Console.WriteLine($"TIFF: {width}x{height}, Float32, compression=none; scale={pixelX:R},{pixelY:R}; top-left={west:R},{top:R}");
if (tags.TryGetValue(34735, out var keys))
{
    for (var k = 0; k < U16(keys.At + 6); k++)
    {
        var at = keys.At + 8 + 8 * k;
        Console.WriteLine($"GeoKey {U16(at)}: location={U16(at + 2)}, count={U16(at + 4)}, value={U16(at + 6)}");
    }
}
var offset = checked((int)Scalar(offsetTag));
var stride = offsetTag == 273 ? checked((int)width) : checked((int)Scalar(322));
Console.WriteLine($"Storage: {(offsetTag == 273 ? "strip" : "tile")}, row stride={stride}");
var samples = Enumerable.Range(0, 16).Select(i => BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(offset + (i / 4 * stride + i % 4) * 4, 4)))).ToArray();
Console.WriteLine("Samples by row: " + string.Join("; ", Enumerable.Range(0, 4).Select(r => string.Join(",", samples.Skip(r * 4).Take(4).Select(x => x.ToString("R", System.Globalization.CultureInfo.InvariantCulture))))));
// Pixel-is-area cell containing the source point; exact boundaries choose east/south cell.
var sourceEast = feature.GetProperty("properties").GetProperty("e").GetDouble();
var sourceNorth = feature.GetProperty("properties").GetProperty("n").GetDouble();
var col = (int)Math.Floor((sourceEast - west) / pixelX);
var row = (int)Math.Floor((top - sourceNorth) / pixelY);
if (col < 0 || col >= width || row < 0 || row >= height || samples[row * 4 + col] == -32767 || !float.IsFinite(samples[row * 4 + col]))
    throw new InvalidDataException("Origin outside probe or NoData.");
Console.WriteLine($"Origin cell row={row}, col={col}, center E={west + (col + .5) * pixelX:R}, N={top - (row + .5) * pixelY:R}; height={(double)samples[row * 4 + col]:R} (provider N2000)");
if (File.Exists("config/source-registry.json"))
{
    using var registry = JsonDocument.Parse(File.ReadAllText("config/source-registry.json"));
    var area = registry.RootElement.GetProperty("area");
    var recorded = area.GetProperty("origin");
    if (recorded.GetProperty("east").GetDouble() != east || recorded.GetProperty("north").GetDouble() != north ||
        recorded.GetProperty("height").GetDouble() != (double)samples[row * 4 + col])
        throw new InvalidDataException("Registry origin differs from measured probe.");
    var bounds = area.GetProperty("bounds");
    if (bounds.GetProperty("maxEast").GetDouble() - bounds.GetProperty("minEast").GetDouble() != 1000 ||
        bounds.GetProperty("maxNorth").GetDouble() - bounds.GetProperty("minNorth").GetDouble() != 1000)
        throw new InvalidDataException("Registry area must be 1000m square.");
    foreach (var probe in registry.RootElement.GetProperty("probes").EnumerateArray())
    {
        var path = Path.Combine(folder, probe.GetProperty("file").GetString()!);
        if (new FileInfo(path).Length != probe.GetProperty("bytes").GetInt64() ||
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant() != probe.GetProperty("sha256").GetString())
            throw new InvalidDataException($"Recorded probe differs: {path}");
    }
    Console.WriteLine("Registry JSON, 1000m bounds, measured origin and all recorded probe hashes: PASS");
}
foreach (var path in Directory.GetFiles(folder).Where(p => Path.GetFileName(p).StartsWith("p09-", StringComparison.Ordinal)).Order())
    Console.WriteLine($"{Path.GetFileName(path)} {new FileInfo(path).Length} bytes SHA256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant()}");
