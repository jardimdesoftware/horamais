# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . /src/

WORKDIR /src/Back.API

RUN dotnet restore "Back.sln"

RUN dotnet publish "Back.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

RUN apt-get update && apt-get upgrade -y && rm -rf /var/lib/apt/lists/*

#Versionamento no runtime
ARG VERSION=dev
ENV APP_VERSION=$VERSION

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Back.API.dll"]
