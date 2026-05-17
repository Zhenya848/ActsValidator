FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/UserService.API/UserService.API.csproj", "src/UserService.API/"]
COPY ["src/UserService.Application/UserService.Application.csproj", "src/UserService.Application/"]
COPY ["src/UserService.Domain/UserService.Domain.csproj", "src/UserService.Domain/"]
COPY ["src/UserService.Infrastructure/UserService.Infrastructure.csproj", "src/UserService.Infrastructure/"]
COPY ["src/UserService.Presentation/UserService.Presentation.csproj", "src/UserService.Presentation/"]
COPY ["src/PaymentMessaging.Contracts/PaymentMessaging.Contracts.csproj", "src/PaymentMessaging.Contracts/"]

RUN dotnet restore "src/UserService.API/UserService.API.csproj"

COPY . .
WORKDIR "/src/src/UserService.API"
RUN dotnet build "./UserService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY src/UserService.API/etc/ /app/etc/

ENTRYPOINT ["dotnet", "UserService.API.dll"]
