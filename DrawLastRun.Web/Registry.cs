using DrawLastRun.Web.Components;

namespace DrawLastRun.Web;

public static class Registry
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        builder.AddServiceDefaults();

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        return builder;
    }

    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        app.MapRazorComponents<App>()
            .AddAdditionalAssemblies(typeof(DrawLastRun.Client.ClientAssemblyMarker).Assembly)
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode();

        app.MapDefaultEndpoints();

        return app;
    }
}
