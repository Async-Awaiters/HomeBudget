# ===== Stage 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем csproj только нужных проектов
COPY HomeBudget.AuthService.EF/HomeBudget.AuthService.EF.csproj HomeBudget.AuthService.EF/
COPY HomeBudget.AuthService/HomeBudget.AuthService.csproj HomeBudget.AuthService/

# Восстанавливаем зависимости
RUN dotnet restore HomeBudget.AuthService/HomeBudget.AuthService.csproj

# Копируем весь код нужных проектов
COPY HomeBudget.AuthService.EF/ HomeBudget.AuthService.EF/
COPY HomeBudget.AuthService/ HomeBudget.AuthService/

# Публикуем билд
RUN dotnet publish HomeBudget.AuthService/HomeBudget.AuthService.csproj -c Release -o /app/publish

# ===== Stage 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Переменные окружения для порта и режима
ENV DOTNET_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HomeBudget.AuthService.dll"]
