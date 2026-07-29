# ==========================================
# ETAPA 1 — COMPILAÇÃO
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore "src/QuentinhasDaTininha.Api/QuentinhasDaTininha.Api.csproj"

RUN dotnet publish \
    "src/QuentinhasDaTininha.Api/QuentinhasDaTininha.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ==========================================
# ETAPA 2 — EXECUÇÃO
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "QuentinhasDaTininha.Api.dll"]