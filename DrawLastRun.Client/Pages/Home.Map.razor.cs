using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.UI.Blazor;
using DrawLastRun.Client.Services;
using NetTopologySuite.Geometries;

namespace DrawLastRun.Client.Pages;

public partial class Home : IDisposable
{
    private void DisplayLoadedRoute()
    {
        if (_map is null || _loadedRoute is null) return;
        RemoveLayer(ref _loadedLayer);
        var points = _loadedRoute.Points.Select(RouteGeometryService.Project).ToArray();
        var feature = new GeometryFeature(RouteGeometryService.CreateLine(points));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.FromArgb(220, 220, 45, 45), 4) });
        _loadedLayer = new MemoryLayer { Name = "Loaded GPX route", Features = new[] { feature } };
        _map.Layers.Add(_loadedLayer);
        CenterOnLoadedRoute();
    }

    private void CenterOnLoadedRoute()
    {
        if (_map is null || _loadedRoute is null) return;
        var points = _loadedRoute.Points.Select(RouteGeometryService.Project).ToArray();
        var extent = new MRect(points.Min(point => point.X), points.Min(point => point.Y), points.Max(point => point.X), points.Max(point => point.Y));
        _map.Navigator.ZoomToBox(extent.Grow(Math.Max(extent.Width, extent.Height) * .1));
    }

    private void RefreshDrawnLayer()
    {
        if (_map is null || _drawnPoints.Count < 2) return;
        RemoveLayer(ref _drawnLayer);
        var feature = new GeometryFeature(RouteGeometryService.CreateLine(_drawnPoints));
        feature.Styles.Add(new VectorStyle { Line = new Pen(Color.FromArgb(220, 30, 100, 220), 4) });
        _drawnLayer = new MemoryLayer { Name = "Drawn route", Features = new[] { feature } };
        _map.Layers.Add(_drawnLayer);
        _map.RefreshGraphics();
    }

    private void RemoveLayer(ref MemoryLayer? layer)
    {
        if (_map is not null && layer is not null)
        {
            _map.Layers.Remove(layer);
            layer = null;
            _map.RefreshGraphics();
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender || _mapControl?.Map is not { } map) return;
        _map = map;
        _mapControl.MapPointerPressed += OnPressed;
        _mapControl.MapPointerMoved += OnMoved;
        _mapControl.MapPointerReleased += OnReleased;
        map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        map.Navigator.MouseWheelAnimation.UseContinuousMouseWheelZoom = true;
        if (_loadedRoute is not null) CenterOnLoadedRoute();
    }

    private void ZoomIn() => _map?.Navigator.ZoomIn();

    private void ZoomOut() => _map?.Navigator.ZoomOut();

    public void Dispose()
    {
        if (_mapControl is not null)
        {
            _mapControl.MapPointerPressed -= OnPressed;
            _mapControl.MapPointerMoved -= OnMoved;
            _mapControl.MapPointerReleased -= OnReleased;
        }
    }
}
