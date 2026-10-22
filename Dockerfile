FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore LeafWeighing.Api/LeafWeighing.Api.csproj
RUN dotnet publish LeafWeighing.Api/LeafWeighing.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

CMD ["sh", "-c", "dotnet LeafWeighing.Api.dll --urls http://0.0.0.0:${PORT:-10000}"]
