using System.Globalization;
using System.Xml.Linq;
using Mapsui;
using Mapsui.Projections;
using NetTopologySuite.Geometries;
using DrawLastRun.Client.Models;

namespace DrawLastRun.Client.Services;

public static class RouteGeometryService
{
    public static IReadOnlyList<GpxPoint> ReadPoints(XDocument document) => document.Descendants()
        .Where(element => element.Name.LocalName is "trkpt" or "rtept")
        .Select(ReadPoint)
        .OfType<GpxPoint>()
        .ToList();

    public static GpxPoint? ReadPoint(XElement element) =>
        double.TryParse(element.Attribute("lat")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
        && double.TryParse(element.Attribute("lon")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude)
            ? new(latitude, longitude)
            : null;

    public static MPoint Project(GpxPoint point)
    {
        var projected = SphericalMercator.FromLonLat(point.Longitude, point.Latitude);
        return new(projected.x, projected.y);
    }

    public static LineString CreateLine(IEnumerable<MPoint> points) =>
        new(points.Select(point => new Coordinate(point.X, point.Y)).ToArray());

    public static RouteComparison Compare(LineString loaded, LineString drawn)
    {
        var deviation = (loaded.Coordinates.Average(point => drawn.Distance(new Point(point)))
            + drawn.Coordinates.Average(point => loaded.Distance(new Point(point)))) / 2;
        var score = (int)Math.Round(100 * Math.Exp(-deviation / 300));
        return new(Math.Clamp(score, 0, 100), deviation);
    }

    public static double DistanceSquared(MPoint first, MPoint second)
    {
        var x = first.X - second.X;
        var y = first.Y - second.Y;
        return x * x + y * y;
    }
}
