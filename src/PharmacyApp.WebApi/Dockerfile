FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Packages.props", "."]
COPY ["src/PharmacyApp.WebApi/PharmacyApp.WebApi.csproj", "src/PharmacyApp.WebApi/"]
COPY ["src/PharmacyApp.Application/PharmacyApp.Application.csproj", "src/PharmacyApp.Application/"]
COPY ["src/PharmacyApp.Domain/PharmacyApp.Domain.csproj", "src/PharmacyApp.Domain/"]
COPY ["src/PharmacyApp.Infrastructure/PharmacyApp.Infrastructure.csproj", "src/PharmacyApp.Infrastructure/"]

RUN dotnet restore "src/PharmacyApp.WebApi/PharmacyApp.WebApi.csproj"

COPY . .

WORKDIR "/src/src/PharmacyApp.WebApi"
RUN dotnet build "PharmacyApp.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PharmacyApp.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PharmacyApp.WebApi.dll"]
