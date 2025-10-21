# ===== Stage 1: Build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

RUN git clone https://github.com/Async-Awaiters/HomeBudget.git .

RUN dotnet clean --configuration Release

RUN dotnet restore AccountManagement/AccountManagement.csproj
RUN dotnet publish AccountManagement/AccountManagement.csproj -c Release -o /app/publish

# ===== Stage 2: Runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV DOTNET_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AccountManagement.dll"]
