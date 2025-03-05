FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["AdvertisingPlatforms.API/AdvertisingPlatforms.API.csproj", "AdvertisingPlatforms.API/"]
RUN dotnet restore "AdvertisingPlatforms.API/AdvertisingPlatforms.API.csproj"
COPY . .
WORKDIR "/src/AdvertisingPlatforms.API"
RUN dotnet build "AdvertisingPlatforms.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "AdvertisingPlatforms.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdvertisingPlatforms.API.dll"]
