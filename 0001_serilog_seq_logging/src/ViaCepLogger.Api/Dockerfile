# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore
COPY src/ViaCepLogger.Api/ViaCepLogger.Api.csproj ViaCepLogger.Api/
RUN dotnet restore ViaCepLogger.Api/ViaCepLogger.Api.csproj

# Copy solution contents
COPY src/ ./

# Publish
RUN dotnet publish ViaCepLogger.Api/ViaCepLogger.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Configure ASP.NET Core to listen on 8080 by default
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ViaCepLogger.Api.dll"]
