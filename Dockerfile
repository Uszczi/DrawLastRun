FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

# The client project performs a native WebAssembly publish.
RUN dotnet workload install wasm-tools
RUN dotnet restore DrawLastRun.Web/DrawLastRun.Web.csproj
RUN dotnet publish DrawLastRun.Web/DrawLastRun.Web.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "DrawLastRun.Web.dll"]
