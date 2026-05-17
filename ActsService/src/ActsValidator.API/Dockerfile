FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/ActsValidator.API/ActsValidator.API.csproj", "src/ActsValidator.API/"]
COPY ["src/ActsValidator.Application/ActsValidator.Application.csproj", "src/ActsValidator.Application/"]
COPY ["src/ActsValidator.Domain/ActsValidator.Domain.csproj", "src/ActsValidator.Domain/"]
COPY ["src/ActsValidator.Infrastructure/ActsValidator.Infrastructure.csproj", "src/ActsValidator.Infrastructure/"]
COPY ["src/ActsValidator.Presentation/ActsValidator.Presentation.csproj", "src/ActsValidator.Presentation/"]

RUN dotnet restore "src/ActsValidator.API/ActsValidator.API.csproj"

COPY . .
WORKDIR "/src/src/ActsValidator.API"
RUN dotnet build "./ActsValidator.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ActsValidator.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY src/ActsValidator.API/etc/ /app/etc/

ENTRYPOINT ["dotnet", "ActsValidator.API.dll"]
