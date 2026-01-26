# Build artifacts
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/Api/VlogForge.Api.csproj", "src/Api/"]
COPY ["src/Application/VlogForge.Application.csproj", "src/Application/"]
COPY ["src/Domain/VlogForge.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/VlogForge.Infrastructure.csproj", "src/Infrastructure/"]
COPY ["Directory.Build.props", "."]

RUN dotnet restore "src/Api/VlogForge.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/Api"
RUN dotnet build "VlogForge.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VlogForge.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:80/health || exit 1

ENTRYPOINT ["dotnet", "VlogForge.Api.dll"]
