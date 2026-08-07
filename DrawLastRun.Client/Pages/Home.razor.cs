using System.Xml;
using System.Xml.Linq;
using DrawLastRun.Client.Models;
using DrawLastRun.Client.Services;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.UI.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using NetTopologySuite.Geometries;

namespace DrawLastRun.Client.Pages;

public partial class Home : IDisposable
{
    private MapControl? _mapControl;
    private Map? _map;
    private MemoryLayer? _loadedLayer;
    private MemoryLayer? _drawnLayer;
    private LoadedRoute? _loadedRoute;
    private readonly List<MPoint> _drawnPoints = new();
    private RouteComparison? _comparison;
    private string? _loadError;
    private bool _isLoading;
    private bool _isDrawing;
    private async Task LoadGpx(InputFileChangeEventArgs args)
    {
        _isLoading = true;
        _loadError = null;
        _comparison = null;
        _drawnPoints.Clear();
        RemoveLayer(ref _drawnLayer);

        try
        {
            await using var stream = args.File.OpenReadStream(25 * 1024 * 1024);
            var document = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
            var points = RouteGeometryService.ReadPoints(document);
            if (points.Count < 2) throw new InvalidDataException("The selected GPX file does not contain enough route points.");
            _loadedRoute = new LoadedRoute(args.File.Name, points);
            RemoveLayer(ref _loadedLayer);
            CenterOnLoadedRoute();
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or XmlException)
        {
            _loadError = exception.Message;
            _loadedRoute = null;
        }
        finally
        {
            _isLoading = false;
        }
    }

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

    private void ToggleDrawing()
    {
        if (_loadedRoute is null) return;
        _isDrawing = !_isDrawing;
        if (_isDrawing) ClearDrawing();
    }

    private void ClearDrawing()
    {
        _drawnPoints.Clear();
        _comparison = null;
        RemoveLayer(ref _drawnLayer);
    }

    private void CompareRoutes()
    {
        if (_loadedRoute is null || _drawnPoints.Count < 2) return;
        DisplayLoadedRoute();
        var loaded = RouteGeometryService.CreateLine(_loadedRoute.Points.Select(RouteGeometryService.Project));
        var drawn = RouteGeometryService.CreateLine(_drawnPoints);
        _comparison = RouteGeometryService.Compare(loaded, drawn);
    }

    private void OnPressed(object? sender, MapEventArgs args)
    {
        if (!_isDrawing) return;
        args.Handled = true;
        _drawnPoints.Clear();
        _drawnPoints.Add(args.WorldPosition);
        RefreshDrawnLayer();
    }

    private void OnMoved(object? sender, MapEventArgs args)
    {
        if (!_isDrawing || args.GestureType != Mapsui.Manipulations.GestureType.Drag) return;
        args.Handled = true;
        if (_drawnPoints.Count == 0 || RouteGeometryService.DistanceSquared(_drawnPoints[^1], args.WorldPosition) > 16)
        {
            _drawnPoints.Add(args.WorldPosition);
            RefreshDrawnLayer();
        }
    }

    private void OnReleased(object? sender, MapEventArgs args)
    {
        if (_isDrawing) args.Handled = true;
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
