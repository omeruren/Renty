# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Renty.Server.slnx .
COPY Renty.Server.Domain/Renty.Server.Domain.csproj Renty.Server.Domain/
COPY Renty.Server.Application/Renty.Server.Application.csproj Renty.Server.Application/
COPY Renty.Server.Infrastructure/Renty.Server.Infrastructure.csproj Renty.Server.Infrastructure/
COPY Renty.Server.Persistence/Renty.Server.Persistence.csproj Renty.Server.Persistence/
COPY Renty.Server.API/Renty.Server.API.csproj Renty.Server.API/

# Restore dependencies
RUN dotnet restore Renty.Server.API/Renty.Server.API.csproj

# Copy source code
COPY . .

# Build
RUN dotnet build Renty.Server.API/Renty.Server.API.csproj -c Release --no-restore

# Publish
RUN dotnet publish Renty.Server.API/Renty.Server.API.csproj -c Release -o /app/publish --no-build

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Health checks (see docs/10-Deployment.md "Docker Rules": containers must define health checks)
# need curl, which the minimal aspnet runtime image doesn't ship with.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Security: Run as non-root user. mcr.microsoft.com/dotnet/aspnet is Debian-based, not Alpine,
# so this needs groupadd/useradd rather than BusyBox's addgroup/adduser.
RUN groupadd --system appgroup && useradd --system --gid appgroup appuser
USER appuser

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "Renty.Server.API.dll"]
