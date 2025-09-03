# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src

# Copy project files
COPY ["FoodKeep.sln", "./"]
COPY ["FoodKeep.Domain/FoodKeep.Domain.csproj", "FoodKeep.Domain/"]
COPY ["FoodKeep.Application/FoodKeep.Application.csproj", "FoodKeep.Application/"]
COPY ["FoodKeep.Infrastructure/FoodKeep.Infrastructure.csproj", "FoodKeep.Infrastructure/"]
COPY ["FoodKeep.API/FoodKeep.API.csproj", "FoodKeep.API/"]
COPY ["FoodKeep.Tests/FoodKeep.Tests.csproj", "FoodKeep.Tests/"]

# Restore
RUN dotnet restore

# Copy everything and build
COPY . .
WORKDIR "/src/FoodKeep.API"
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app

# Install curl for health checks
RUN apk add --no-cache curl

COPY --from=publish /app/publish .

# Create non-root user (CORRECTION ICI)
RUN addgroup -g 1001 -S appuser && \
    adduser -u 1001 -S appuser -G appuser && \
    chown -R appuser:appuser /app

USER appuser

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FoodKeep.API.dll"]