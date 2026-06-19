# ============================================
# NixLang API - Multi-stage Dockerfile
# ============================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY NixLang.sln ./
COPY src/NixLang.Api/NixLang.Api.csproj src/NixLang.Api/
COPY src/NixLang.Application/NixLang.Application.csproj src/NixLang.Application/
COPY src/NixLang.Domain/NixLang.Domain.csproj src/NixLang.Domain/
COPY src/NixLang.Infrastructure/NixLang.Infrastructure.csproj src/NixLang.Infrastructure/

# Restore dependencies (cached unless .csproj files change)
RUN dotnet restore

# Copy all source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/NixLang.Api/NixLang.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN groupadd -r nixlang && useradd -r -g nixlang -s /bin/false nixlang

COPY --from=build /app/publish .

# Set non-root user
USER nixlang

EXPOSE 8080

ENTRYPOINT ["dotnet", "NixLang.Api.dll"]
