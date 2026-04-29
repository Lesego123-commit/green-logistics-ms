# ============================================================================
#  GLMS Web API - multi-stage Dockerfile (Linux container)
#  Reference: Microsoft. 2024. Containerize a .NET app.
#  https://learn.microsoft.com/en-us/dotnet/core/docker/build-container [Accessed 31 May 2026].
# ============================================================================

# ── Build stage ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore first (better layer caching).
COPY ["GreenLogisticsMS.Api/GreenLogisticsMS.Api.csproj", "GreenLogisticsMS.Api/"]
RUN dotnet restore "GreenLogisticsMS.Api/GreenLogisticsMS.Api.csproj"

# Copy the rest of the API source and publish.
COPY GreenLogisticsMS.Api/ GreenLogisticsMS.Api/
WORKDIR /src/GreenLogisticsMS.Api
RUN dotnet publish "GreenLogisticsMS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ── Runtime stage ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The API listens on port 8080 inside the container.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GreenLogisticsMS.Api.dll"]
