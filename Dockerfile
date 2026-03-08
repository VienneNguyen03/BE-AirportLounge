FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["AirportLounge.sln", "."]
COPY ["src/AirportLounge.API/AirportLounge.API.csproj", "src/AirportLounge.API/"]
COPY ["src/AirportLounge.Application/AirportLounge.Application.csproj", "src/AirportLounge.Application/"]
COPY ["src/AirportLounge.Domain/AirportLounge.Domain.csproj", "src/AirportLounge.Domain/"]
COPY ["src/AirportLounge.Infrastructure/AirportLounge.Infrastructure.csproj", "src/AirportLounge.Infrastructure/"]
COPY ["src/AirportLounge.Persistence/AirportLounge.Persistence.csproj", "src/AirportLounge.Persistence/"]

RUN dotnet restore "src/AirportLounge.API/AirportLounge.API.csproj"

COPY src/ src/
WORKDIR /src
RUN dotnet build "src/AirportLounge.API/AirportLounge.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/AirportLounge.API/AirportLounge.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 fonts-liberation \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "AirportLounge.API.dll"]
