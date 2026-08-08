using Mapsui;
using DrawLastRun.Client.Services;

namespace DrawLastRun.Client.Pages;

public partial class Home
{
    private void ToggleDrawing()
    {
        if (_loadedRoute is null || _isComparing) return;
        _isDrawing = !_isDrawing;
        if (_isDrawing) ClearDrawing();
    }

    private void ClearDrawing()
    {
        if (_isComparing) return;
        _drawnPoints.Clear();
        _comparison = null;
        RemoveLayer(ref _drawnLayer);
    }

    private void OnPressed(object? sender, MapEventArgs args)
    {
        if (!_isDrawing || _isComparing) return;
        args.Handled = true;
        _drawnPoints.Clear();
        _drawnPoints.Add(args.WorldPosition);
        RefreshDrawnLayer();
    }

    private void OnMoved(object? sender, MapEventArgs args)
    {
        if (!_isDrawing || _isComparing || args.GestureType != Mapsui.Manipulations.GestureType.Drag) return;
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
}
