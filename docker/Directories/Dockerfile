# ===== Stage 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Клонируем репозиторий с исходниками
RUN git clone https://github.com/Async-Awaiters/HomeBudget.git .

# Восстанавливаем зависимости проекта Directories
RUN dotnet restore HomeBudget.Directories/HomeBudget.Directories.csproj

# Публикуем билд
RUN dotnet publish HomeBudget.Directories/HomeBudget.Directories.csproj -c Release -o /app/publish

# ===== Stage 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV DOTNET_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "HomeBudget.Directories.dll"]

