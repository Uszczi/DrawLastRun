# Draw Last Run

Draw Last Run is a browser-based route memory game for runners. Load a previous activity from a GPX file, draw the route from memory on an interactive map, and compare the drawn route with the original.

## Features

- Import GPX files containing track points or route points.
- Automatically center the map on the imported route without revealing it.
- Draw a remembered route directly on the map.
- Pause map navigation while drawing for a more natural tracing experience.
- Reveal the loaded route only when comparison is requested.
- Display the drawn route and the original route as separate map layers.
- Report a similarity score and average deviation in meters.
- Responsive layout for desktop and mobile screens.

## Technology

- .NET 10
- Blazor Web App
- Blazor WebAssembly for the interactive map page
- Mapsui 5.1 for map rendering and interaction
- OpenStreetMap tiles
- NetTopologySuite for route geometry and distance calculations
- .NET Aspire AppHost for local orchestration

## Project Structure

```text
DrawLastRun.slnx
|-- DrawLastRun.AppHost/          .NET Aspire orchestration
|-- DrawLastRun.Web/              ASP.NET Core host and static assets
|-- DrawLastRun.Client/           Blazor WebAssembly UI and map experience
|   |-- Pages/Home.razor          Page markup
|   |-- Pages/Home.razor.cs       Page and map interaction logic
|   |-- Pages/Home.razor.css      Scoped page styles
|   |-- Models/                   Route models
|   `-- Services/                 GPX and geometry operations
`-- DrawLastRun.ServiceDefaults/  Shared Aspire service configuration
```

Mapsui runs in the WebAssembly client. This is intentional: its Blazor canvas integration depends on browser JavaScript APIs and should not be rendered by the server interactive mode.

## Requirements

- .NET SDK 10.0 or later
- Internet access for OpenStreetMap tiles
- A GPX file with at least two valid `trkpt` or `rtept` elements

## Run Locally

From the repository root:

```powershell
dotnet run --project .\DrawLastRun.Web\DrawLastRun.Web.csproj
```

Open the HTTPS URL printed by ASP.NET Core, normally:

```text
https://localhost:7094
```

To run through .NET Aspire instead:

```powershell
dotnet run --project .\DrawLastRun.AppHost\DrawLastRun.AppHost.csproj
```

The Aspire dashboard will provide the application URL and resource status.

## Usage

1. Select a GPX file in the `Load a GPX run` section.
2. Wait for the map to center on the route area. The original route remains hidden.
3. Select `Start drawing`.
4. Hold the pointer and trace the route from memory.
5. Select `Finish drawing`.
6. Select `Compare routes` to reveal the original route and calculate the result.
7. Use `Clear` to discard the current drawing and try again.

The loaded route is shown in red. The remembered route is shown in blue.

## GPX Handling

The client reads GPX files locally in the browser. No file is uploaded to a server. The current parser accepts:

- `trkpt` elements from GPX tracks
- `rtept` elements from GPX routes
- Latitude and longitude values using invariant decimal formatting
- Files up to 25 MB

Files without at least two valid points are rejected with an error message.

## Comparison

Both routes are projected to the map's Spherical Mercator coordinate system. The comparison measures the average symmetric distance between route vertices and the opposing route geometry. The similarity score applies a distance tolerance so small hand-drawing inaccuracies do not dominate the result.

The score is intended as a practical feedback signal, not a certified GPS accuracy measurement. Drawing quality depends on route scale, map zoom, pointer precision, and the number of points captured during tracing.

## Development

Build the client project:

```powershell
dotnet build .\DrawLastRun.Client\DrawLastRun.Client.csproj
```

Build the Web host:

```powershell
dotnet build .\DrawLastRun.Web\DrawLastRun.Web.csproj
```

Build the full solution:

```powershell
dotnet build .\DrawLastRun.slnx
```
