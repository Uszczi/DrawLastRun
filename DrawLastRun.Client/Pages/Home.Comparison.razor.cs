using DrawLastRun.Client.Services;

namespace DrawLastRun.Client.Pages;

public partial class Home
{
    private async Task CompareRoutes()
    {
        if (_loadedRoute is null || _drawnPoints.Count < 2 || _isDrawing || _isComparing) return;

        _isComparing = true;
        _comparison = null;

        try
        {
            await ReportComparisonProgress(15, "Preparing routes...");
            var loaded = RouteGeometryService.CreateLine(_loadedRoute.Points.Select(RouteGeometryService.Project));
            var drawn = RouteGeometryService.CreateLine(_drawnPoints);

            await ReportComparisonProgress(45, "Comparing route geometry...");
            var comparison = RouteGeometryService.Compare(loaded, drawn);

            await ReportComparisonProgress(85, "Displaying the result...");
            DisplayLoadedRoute();
            _comparison = comparison;
            await ReportComparisonProgress(100, "Comparison complete.");
        }
        finally
        {
            _isComparing = false;
        }
    }

    private async Task ReportComparisonProgress(int progress, string status)
    {
        _comparisonProgress = progress;
        _comparisonStatus = status;
        await InvokeAsync(StateHasChanged);
        await Task.Delay(1);
    }
}
