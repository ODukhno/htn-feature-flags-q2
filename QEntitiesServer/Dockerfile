FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY QEntitiesServer.csproj .
RUN dotnet restore
COPY . .
RUN dotnet build -c Release

FROM build AS publish
RUN dotnet publish -c Release -o /publish

FROM base AS final
WORKDIR /app
COPY --from=publish /publish .
RUN chmod +x ./QEntitiesServer

ENTRYPOINT ["/app/QEntitiesServer"]