﻿FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Backend_online_testing.csproj", "./"]
RUN dotnet restore "Backend_online_testing.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "Backend_online_testing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Backend_online_testing.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Backend_online_testing.dll"]