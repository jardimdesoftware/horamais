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
ARG REVISION=local
ARG BUILDTIME=dev
ENV APP_VERSION=$VERSION
ENV APP_REVISION=$REVISION
ENV APP_BUILDTIME=$BUILDTIME

LABEL org.opencontainers.image.version=$VERSION
LABEL org.opencontainers.image.revision=$REVISION
LABEL org.opencontainers.image.created=$BUILDTIME

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Back.API.dll"]
