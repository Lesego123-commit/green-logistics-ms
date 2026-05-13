# ============================================================================
#  GLMS MVC Web client - multi-stage Dockerfile (Linux container)
#  Reference: Microsoft. 2024. Containerize a .NET app.
#  https://learn.microsoft.com/en-us/dotnet/core/docker/build-container [Accessed 31 May 2026].
# ============================================================================

# ── Build stage ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["GreenLogisticsMS.Web/GreenLogisticsMS.Web.csproj", "GreenLogisticsMS.Web/"]
RUN dotnet restore "GreenLogisticsMS.Web/GreenLogisticsMS.Web.csproj"

COPY GreenLogisticsMS.Web/ GreenLogisticsMS.Web/
WORKDIR /src/GreenLogisticsMS.Web
RUN dotnet publish "GreenLogisticsMS.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ── Runtime stage ───────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "GreenLogisticsMS.Web.dll"]
