# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# O contexto de build é a raiz do repositório (Render, Compose e E2E).
# Copia primeiro o projeto para aproveitar o cache de restore.
COPY PhishGuard.Backend/PhishGuard.Backend.csproj ./

RUN dotnet restore "PhishGuard.Backend.csproj"

COPY PhishGuard.Backend/ .

RUN dotnet build "PhishGuard.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhishGuard.Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio de Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhishGuard.Backend.dll"]
