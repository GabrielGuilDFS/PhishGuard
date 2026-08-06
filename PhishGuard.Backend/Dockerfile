# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copiar o .csproj indicando a subpasta
COPY PhishGuard.Backend/PhishGuard.Backend.csproj PhishGuard.Backend/

# 2. Restaurar dependências apontando para o arquivo dentro da subpasta
RUN dotnet restore "PhishGuard.Backend/PhishGuard.Backend.csproj"

# 3. Copiar todo o código-fonte
COPY PhishGuard.Backend/ PhishGuard.Backend/

# 4. Entrar na pasta do projeto e compilar
WORKDIR "/src/PhishGuard.Backend"
RUN dotnet build "PhishGuard.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhishGuard.Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio de Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PhishGuard.Backend.dll"]