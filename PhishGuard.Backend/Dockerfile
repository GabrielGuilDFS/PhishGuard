# Estágio 1: build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PhishGuard.Backend.csproj ./
RUN dotnet restore "PhishGuard.Backend.csproj"

COPY . .
RUN dotnet publish "PhishGuard.Backend.csproj" -c Release -o /app/publish --no-restore

# Estágio 2: runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "PhishGuard.Backend.dll"]
