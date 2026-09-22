# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["ZHome.API.csproj", "./"]
RUN dotnet restore "ZHome.API.csproj"

# Copy full source and publish
COPY . .
RUN dotnet publish "ZHome.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Ensure uploads directory exists
RUN mkdir -p /app/wwwroot/uploads

# Expose ports
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ZHome.API.dll"]
