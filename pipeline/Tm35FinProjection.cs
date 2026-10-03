using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace DrivingTime.Pipeline;

/// <summary>Bounded horizontal EUREF-FIN / TM35FIN projection, not a general EPSG resolver.</summary>
public sealed class Tm35FinProjection
{
    // Explicit GRS80 and NLS TM35FIN parameters. Library interface uses lon,lat despite EPSG lat,lon.
    public const string GeographicWkt = """
        GEOGCS["EUREF-FIN",DATUM["EUREF-FIN",SPHEROID["GRS 1980",6378137,298.257222101]],PRIMEM["Greenwich",0],UNIT["degree",0.0174532925199433],AXIS["Latitude",NORTH],AXIS["Longitude",EAST]]
        """;
    public static string ProjectedWkt => $"""
        PROJCS["EUREF-FIN / TM35FIN(E,N)",{GeographicWkt},PROJECTION["Transverse_Mercator"],PARAMETER["latitude_of_origin",0],PARAMETER["central_meridian",27],PARAMETER["scale_factor",0.9996],PARAMETER["false_easting",500000],PARAMETER["false_northing",0],UNIT["metre",1],AXIS["Easting",EAST],AXIS["Northing",NORTH],AUTHORITY["EPSG","3067"]]
        """;
    private readonly MathTransform forward;
    private readonly MathTransform inverse;

    public Tm35FinProjection()
    {
        var systems = new CoordinateSystemFactory();
        var transform = new CoordinateTransformationFactory().CreateFromCoordinateSystems(
            systems.CreateFromWkt(GeographicWkt), systems.CreateFromWkt(ProjectedWkt));
        forward = transform.MathTransform;
        inverse = forward.Inverse();
    }

    public (double East, double North) FromEurefFin(GeographicPoint point)
    {
        Validate(point);
        var result = forward.Transform(new[] { point.LongitudeDegrees, point.LatitudeDegrees });
        CoordinateFrame.Finite(result[0], result[1]);
        return (result[0], result[1]);
    }

    public (double East, double North) FromWgs84(GeographicPoint point, bool allowApproximateDatum = false)
    {
        if (!allowApproximateDatum)
            throw new ArgumentException("EPSG:4326 needs an explicit approximate WGS84-to-EUREF-FIN datum policy; precise epoch-aware conversion is unsupported.");
        // Deliberate datum approximation: treat WGS84 angles as EUREF-FIN, then project on GRS80.
        return FromEurefFin(point);
    }

    public GeographicPoint ToEurefFin(double east, double north)
    {
        CoordinateFrame.Finite(east, north);
        var result = inverse.Transform(new[] { east, north });
        var point = new GeographicPoint(result[1], result[0]);
        Validate(point);
        return point;
    }

    private static void Validate(GeographicPoint point)
    {
        CoordinateFrame.Finite(point.LatitudeDegrees, point.LongitudeDegrees);
        // Conservative Finland envelope for this prototype, not a polygonal EPSG area-of-use test.
        if (point.LatitudeDegrees is < 59 or > 71 || point.LongitudeDegrees is < 19 or > 32)
            throw new ArgumentException("TM35FIN adapter accepts only the bounded Finland envelope (latitude 59–71°, longitude 19–32°); check axis order and units.");
    }
}
