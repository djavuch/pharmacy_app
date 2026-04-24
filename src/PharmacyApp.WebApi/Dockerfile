FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Packages.props", "."]
COPY ["src/PharmacyApp.WebApi/PharmacyApp.WebApi.csproj", "src/PharmacyApp.WebApi/"]
COPY ["src/PharmacyApp.Application/PharmacyApp.Application.csproj", "src/PharmacyApp.Application/"]
COPY ["src/PharmacyApp.Domain/PharmacyApp.Domain.csproj", "src/PharmacyApp.Domain/"]
COPY ["src/PharmacyApp.Infrastructure/PharmacyApp.Infrastructure.csproj", "src/PharmacyApp.Infrastructure/"]
COPY ["src/PharmacyApp.Presentation/PharmacyApp.Presentation.csproj", "src/PharmacyApp.Presentation/"]

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
