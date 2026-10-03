using DrivingTime.Pipeline;
using Xunit;
using Xunit.Abstractions;

namespace DrivingTime.Pipeline.Tests;

public sealed class CoordinateTests(ITestOutputHelper output)
{
    // Arbitrary synthetic large numbers exercise precision; these are NOT a geographic origin.
    private static CoordinateFrame Frame => new(new(300000.125, 6700000.25, 12.5), VerticalDatum.Synthetic, 500);

    [Theory]
    [InlineData(100, 0, 0, 100, 0, 0)]
    [InlineData(-100, 0, 0, -100, 0, 0)]
    [InlineData(0, 100, 0, 0, 0, -100)]
    [InlineData(0, -100, 0, 0, 0, 100)]
    [InlineData(0, 0, 37.25, 0, 37.25, 0)]
    public void CardinalsAndElevation(double e, double n, double h, double x, double y, double z)
    {
        var f = Frame;
        Assert.Equal(new EnginePoint(x, y, z), f.ToEngine(new(f.Origin.East + e, f.Origin.North + n, f.Origin.Height + h), f.Datum));
    }

    [Fact]
    public void Measured100MetreSegmentAndHandedness()
    {
        var f = Frame;
        var a = f.ToEngine(f.Origin, f.Datum);
        // Independently known 60/80/100 triangle in projected metres.
        var b = f.ToEngine(new(f.Origin.East + 60, f.Origin.North + 80, f.Origin.Height), f.Datum);
        Assert.Equal(100, Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Z - a.Z, 2)), 10);
        // Mapped east cross mapped north = up. Thus ENU -> engine XYZ determinant is +1.
        var east = f.ToEngine(new(f.Origin.East + 1, f.Origin.North, f.Origin.Height), f.Datum);
        var north = f.ToEngine(new(f.Origin.East, f.Origin.North + 1, f.Origin.Height), f.Datum);
        Assert.Equal(new EnginePoint(0, 1, 0), new EnginePoint(east.Y * north.Z - east.Z * north.Y,
            east.Z * north.X - east.X * north.Z, east.X * north.Y - east.Y * north.X));
    }

    [Fact]
    public void DoublePrecisionRoundTripsAndFloatOnlyAfterLocalizing()
    {
        var f = Frame;
        var p = new ProjectedPoint(f.Origin.East + 0.001234, f.Origin.North - 0.002345, 49.123456);
        var local = f.ToEngine(p, f.Datum);
        var restored = f.ToProjected(local);
        Assert.InRange(Math.Abs(p.East - restored.East), 0, 1e-9);
        Assert.InRange(Math.Abs(p.North - restored.North), 0, 1e-9);
        Assert.InRange(Math.Abs(p.Height - restored.Height), 0, 1e-12);
        Assert.InRange(Math.Abs((float)local.X - local.X), 0, 1e-9);
        Assert.True(Math.Abs((float)p.North - p.North) > 0.01);
    }

    [Theory]
    [InlineData(500, 500, 1, 1, 0, 0)]
    [InlineData(-0.25, -0.5, -1, -1, 499.75, -499.5)]
    [InlineData(499.75, 0, 0, 0, 499.75, 0)]
    public void TileBoundariesAndWorldRecomposition(double e, double n, int te, int tn, double lx, double lz)
    {
        var f = Frame;
        var p = new ProjectedPoint(f.Origin.East + e, f.Origin.North + n, 25);
        Assert.Equal((te, tn), f.TileOf(p));
        var local = f.ToTileLocal(p, f.Datum, te, tn);
        Assert.Equal(new EnginePoint(lx, 12.5, lz), local);
        var anchor = f.TileTranslation(te, tn);
        Assert.Equal(f.ToEngine(p, f.Datum), new EnginePoint(local.X + anchor.X, local.Y, local.Z + anchor.Z));
        // Same border vertex can be represented by both adjacent tiles.
        var neighbor = f.ToTileLocal(p, f.Datum, te - 1, tn);
        Assert.Equal(local.X, neighbor.X - 500);
    }

    [Fact]
    public void ExplicitSourceAxesAndHeightRejection()
    {
        Assert.Equal(SourceAxes.Projected(123, 456, 7, ProjectedAxisOrder.EastNorth),
            SourceAxes.Projected(456, 123, 7, ProjectedAxisOrder.NorthEast));
        Assert.Equal(SourceAxes.Geographic(60, 25, GeographicAxisOrder.LatitudeLongitude),
            SourceAxes.Geographic(25, 60, GeographicAxisOrder.LongitudeLatitude));
        foreach (var datum in new[] { VerticalDatum.Unknown, VerticalDatum.N60, VerticalDatum.Ellipsoidal, VerticalDatum.N2000 })
            Assert.Throws<ArgumentException>(() => Frame.ToEngine(new(1, 2, 3), datum));
        Assert.Throws<ArgumentException>(() => new CoordinateFrame(new(0, 0, 0), VerticalDatum.Unknown, 500));
        Assert.Throws<ArgumentException>(() => Frame.ToEngine(new(double.NaN, 2, 3), Frame.Datum));
        Assert.Throws<ArgumentException>(() => new CoordinateFrame(new(0, 0, 0), VerticalDatum.Synthetic, 0));
    }

    [Fact]
    public void ProjNetAgainstPublishedNlsExample10()
    {
        // FGI Tiedote 30 (corrected 2009-12-10), printed pp.106–107 / PDF pp.109–110.
        // Inputs are published radians, results metres; no source dataset or origin selection.
        var projection = new Tm35FinProjection();
        var point = new GeographicPoint(1.102369021930 * 180 / Math.PI, 0.372098448769 * 180 / Math.PI);
        var actual = projection.FromEurefFin(point);
        output.WriteLine($"NLS example 10: E={actual.East:R}, N={actual.North:R}; residual E={actual.East - 214141.4227:R}, N={actual.North - 7016196.1450:R} m");
        Assert.InRange(Math.Abs(actual.East - 214141.4227), 0, 0.01);
        Assert.InRange(Math.Abs(actual.North - 7016196.1450), 0, 0.01);
        var back = projection.ToEurefFin(214141.4227, 7016196.1450);
        Assert.InRange(Math.Abs(back.LatitudeDegrees - point.LatitudeDegrees), 0, 1e-7);
        Assert.InRange(Math.Abs(back.LongitudeDegrees - point.LongitudeDegrees), 0, 1e-7);
    }

    [Theory]
    [InlineData(60.17, 24.94)]
    [InlineData(60, 27)]
    [InlineData(70, 31)]
    [InlineData(60, 19.5)] // Beyond nominal UTM zone 35: TM35FIN covers Finland.
    public void ProjectionRoundTripsAndLibraryAxisSemantics(double lat, double lon)
    {
        var projection = new Tm35FinProjection();
        var p = new GeographicPoint(lat, lon);
        var projected = projection.FromEurefFin(p);
        if (lon == 27) Assert.InRange(Math.Abs(projected.East - 500000), 0, 1e-6);
        var back = projection.ToEurefFin(projected.East, projected.North);
        Assert.InRange(Math.Abs(back.LatitudeDegrees - lat), 0, 1e-7);
        Assert.InRange(Math.Abs(back.LongitudeDegrees - lon), 0, 1e-7);
        Assert.Throws<ArgumentException>(() => projection.FromWgs84(p));
        Assert.Equal(projected, projection.FromWgs84(p, allowApproximateDatum: true));
        Assert.Throws<ArgumentException>(() => projection.FromEurefFin(new(lon, lat)));
    }
}
