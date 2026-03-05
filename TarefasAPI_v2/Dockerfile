# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia tudo da pasta TarefasAPI_v2 para a raiz do container
COPY TarefasAPI_v2/ .

# Restaura e publica referenciando o arquivo na raiz do container
RUN dotnet restore "TarefasAPI_v2.csproj"
RUN dotnet publish "TarefasAPI_v2.csproj" -c Release -o /app/publish

# Estágio Final (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Porta e Execução
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TarefasAPI_v2.dll"]