# Etapa 1: Build y Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar archivos del proyecto y restaurar dependencias
COPY ["AutoGestionAPI.csproj", "./"]
RUN dotnet restore "AutoGestionAPI.csproj"

# Copiar todo el código fuente y publicar en modo Release
COPY . .
RUN dotnet publish "AutoGestionAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime de producción
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponer el puerto por defecto de ASP.NET Core en contenedores
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AutoGestionAPI.dll"]