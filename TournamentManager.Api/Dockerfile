FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY TournamentManager.Api/TournamentManager.Api.csproj TournamentManager.Api/
COPY TournamentManager.Core/TournamentManager.Core.csproj TournamentManager.Core/
RUN dotnet nuget locals all --clear

RUN dotnet restore ./TournamentManager.Api/TournamentManager.Api.csproj \
    --verbosity normal \
    --force \
    --ignore-failed-sources

COPY TournamentManager.Api/. TournamentManager.Api/
COPY TournamentManager.Core/. TournamentManager.Core/
RUN dotnet publish TournamentManager.Api/TournamentManager.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TournamentManager.Api.dll"]