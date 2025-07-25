﻿
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY bin/Release/net8.0/publish/ .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "ChocobabiesReloaded.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ChocobabiesReloaded.csproj", "."]
RUN dotnet restore "./ChocobabiesReloaded.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "ChocobabiesReloaded.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChocobabiesReloaded.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChocobabiesReloaded.dll"]


