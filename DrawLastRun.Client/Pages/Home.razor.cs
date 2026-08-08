using DrawLastRun.Client.Models;
using Mapsui;
using Mapsui.Layers;
using Mapsui.UI.Blazor;

namespace DrawLastRun.Client.Pages;

public partial class Home
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
    private bool _isComparing;
    private int _comparisonProgress;
    private string? _comparisonStatus;
}
