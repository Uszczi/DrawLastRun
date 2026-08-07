namespace DrawLastRun.Client.Models;

public sealed record GpxPoint(double Latitude, double Longitude);

public sealed record LoadedRoute(string Name, IReadOnlyList<GpxPoint> Points);

public sealed record RouteComparison(int Score, double DeviationMeters);
