# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["MusicDistributionSystem.Domain/MusicDistributionSystem.Domain.csproj", "MusicDistributionSystem.Domain/"]
COPY ["MusicDistributionSystem.Application/MusicDistributionSystem.Application.csproj", "MusicDistributionSystem.Application/"]
COPY ["MusicDistributionSystem.Infrastructure/MusicDistributionSystem.Infrastructure.csproj", "MusicDistributionSystem.Infrastructure/"]
COPY ["MusicDistributionSystem/MusicDistributionSystem.Web.csproj", "MusicDistributionSystem/"]
COPY ["MusicDistributionSystem.Tests/MusicDistributionSystem.Tests.csproj", "MusicDistributionSystem.Tests/"]

RUN dotnet restore "MusicDistributionSystem/MusicDistributionSystem.Web.csproj"

# Copy full source and build
COPY . .
WORKDIR "/src/MusicDistributionSystem"
RUN dotnet build "MusicDistributionSystem.Web.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "MusicDistributionSystem.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

# Set environment
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MusicDistributionSystem.Web.dll"]
