# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["FluyoV2/FluyoV2.csproj", "FluyoV2/"]

# Restore dependencies
RUN dotnet restore "FluyoV2/FluyoV2.csproj"

# Copy application source code
COPY . .

# Build application
WORKDIR "/src/FluyoV2"
RUN dotnet build "FluyoV2.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FluyoV2.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Copy published application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "FluyoV2.dll"]
