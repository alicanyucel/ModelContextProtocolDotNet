# Dockerfile for ModelContextProtocol.WebApi
# Targets .NET 10 (change image tags if specific preview/runtime images differ)

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore (adjust csproj filename if different)
COPY ["ModelContextProtocol.WebApi.csproj", "./"]
RUN dotnet restore "ModelContextProtocol.WebApi.csproj"

# Copy everything and publish
COPY . .
RUN dotnet publish "ModelContextProtocol.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "ModelContextProtocol.WebApi.dll"]
