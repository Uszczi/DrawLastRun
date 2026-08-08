using System.Xml;
using System.Xml.Linq;
using DrawLastRun.Client.Models;
using DrawLastRun.Client.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace DrawLastRun.Client.Pages;

public partial class Home
{
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
}
