using System;
using System.Linq;

namespace DrivingTime.Pipeline;

public readonly record struct ProjectedPoint(double East, double North, double Height);
public readonly record struct EnginePoint(double X, double Y, double Z);
public readonly record struct GeographicPoint(double LatitudeDegrees, double LongitudeDegrees);
public enum VerticalDatum { Synthetic, N2000, N60, Ellipsoidal, Unknown }
public enum ProjectedAxisOrder { EastNorth, NorthEast }
public enum GeographicAxisOrder { LatitudeLongitude, LongitudeLatitude }

/// <summary>Adapters require observed format axis order; never infer it from a field named X.</summary>
public static class SourceAxes
{
    public static ProjectedPoint Projected(double first, double second, double height, ProjectedAxisOrder order)
    {
        CoordinateFrame.Finite(first, second, height);
        return order switch
        {
            ProjectedAxisOrder.EastNorth => new(first, second, height),
            ProjectedAxisOrder.NorthEast => new(second, first, height),
            _ => throw new ArgumentException("Declare projected source axis order.")
        };
    }

    public static GeographicPoint Geographic(double first, double second, GeographicAxisOrder order) => order switch
    {
        GeographicAxisOrder.LatitudeLongitude => new(first, second),
        GeographicAxisOrder.LongitudeLatitude => new(second, first),
        _ => throw new ArgumentException("Declare geographic source axis order.")
    };
}

/// <summary>Immutable configured origin; all calculations stay double until asset serialization.</summary>
public sealed class CoordinateFrame
{
    public ProjectedPoint Origin { get; }
    public VerticalDatum Datum { get; }
    public double TileSizeMetres { get; }

    public CoordinateFrame(ProjectedPoint origin, VerticalDatum datum, double tileSizeMetres)
    {
        Finite(origin.East, origin.North, origin.Height, tileSizeMetres);
        if (datum is not (VerticalDatum.N2000 or VerticalDatum.Synthetic))
            throw new ArgumentException("Frame requires N2000 or explicitly synthetic heights.");
        if (tileSizeMetres <= 0) throw new ArgumentException("tileSizeMetres must be positive.");
        Origin = origin;
        Datum = datum;
        TileSizeMetres = tileSizeMetres;
    }

    public EnginePoint ToEngine(ProjectedPoint point, VerticalDatum sourceDatum)
    {
        if (sourceDatum != Datum)
            throw new ArgumentException("Height datum mismatch: validate/convert heights to the frame datum before mapping.");
        Finite(point.East, point.North, point.Height);
        var result = new EnginePoint(point.East - Origin.East, point.Height - Origin.Height, -(point.North - Origin.North));
        Finite(result.X, result.Y, result.Z);
        return result;
    }

    public ProjectedPoint ToProjected(EnginePoint point)
    {
        Finite(point.X, point.Y, point.Z);
        var result = new ProjectedPoint(Origin.East + point.X, Origin.North - point.Z, Origin.Height + point.Y);
        Finite(result.East, result.North, result.Height);
        return result;
    }

    // Tile anchor is its southwest corner on the origin-anchored projected grid.
    // Height origin stays common across tiles; negative indices use floor, not truncation.
    public (int East, int North) TileOf(ProjectedPoint point)
    {
        Finite(point.East, point.North);
        return (checked((int)Math.Floor((point.East - Origin.East) / TileSizeMetres)),
            checked((int)Math.Floor((point.North - Origin.North) / TileSizeMetres)));
    }

    public EnginePoint TileTranslation(int east, int north)
    {
        var result = new EnginePoint(east * TileSizeMetres, 0, -north * TileSizeMetres);
        Finite(result.X, result.Z);
        return result;
    }

    public EnginePoint ToTileLocal(ProjectedPoint point, VerticalDatum sourceDatum, int east, int north)
    {
        var world = ToEngine(point, sourceDatum);
        var anchor = TileTranslation(east, north);
        var result = new EnginePoint(world.X - anchor.X, world.Y, world.Z - anchor.Z);
        Finite(result.X, result.Y, result.Z);
        return result;
    }

    internal static void Finite(params double[] values)
    {
        if (values.Any(value => !double.IsFinite(value)))
            throw new ArgumentException("Coordinates must be finite doubles in the declared units.");
    }
}
