# 1. Aşama: Derleme (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasını kopyala ve restore et
COPY ["QradarLogSystem.Api/QradarLogSystem.Api.csproj", "QradarLogSystem.Api/"]
RUN dotnet restore "QradarLogSystem.Api/QradarLogSystem.Api.csproj"

# Tüm dosyaları kopyala ve derle
COPY . .
WORKDIR "/src/QradarLogSystem.Api"
RUN dotnet publish "QradarLogSystem.Api.csproj" -c Release -o /app/publish

# 2. Aşama: Çalıştırma (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

ENTRYPOINT ["dotnet", "QradarLogSystem.Api.dll"]
