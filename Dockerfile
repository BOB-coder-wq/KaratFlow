# Use .NET 9 SDK to match the project's target framework
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY KaratFlowAPI/KaratFlowAPI.csproj ./KaratFlowAPI/
RUN cd KaratFlowAPI && dotnet restore

# Copy everything and build
COPY KaratFlowAPI/ ./KaratFlowAPI/
RUN cd KaratFlowAPI && dotnet publish -c Release -o /app/publish --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}

ENTRYPOINT ["dotnet", "KaratFlowAPI.dll"]
