# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/Services/ApiDesign/Apivia.Services.ApiDesign.csproj", "src/Services/ApiDesign/"]
COPY ["src/Shared/Common/Apivia.Shared.Common.csproj", "src/Shared/Common/"]
COPY ["src/Shared/Data/Apivia.Shared.Data.csproj", "src/Shared/Data/"]
COPY ["src/Shared/Messaging/Apivia.Shared.Messaging.csproj", "src/Shared/Messaging/"]

# Restore dependencies
RUN dotnet restore "src/Services/ApiDesign/Apivia.Services.ApiDesign.csproj"

# Copy all source code
COPY . .

# Build
WORKDIR "/src/src/Services/ApiDesign"
RUN dotnet build "Apivia.Services.ApiDesign.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Apivia.Services.ApiDesign.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Apivia.Services.ApiDesign.dll"]
