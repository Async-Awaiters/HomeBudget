# ===== Stage 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Клонируем только нужную ветку healthCheck из репозитория
RUN git clone --branch healthCheck --single-branch https://github.com/Async-Awaiters/HomeBudget.git .

# Публикуем билд
RUN dotnet publish HomeBudget.HealthCheck/HomeBudget.HealthCheck.csproj -c Release -o /app/publish

# ===== Stage 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV DOTNET_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HomeBudget.HealthCheck.dll"]

