# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo .csproj e restaura dependências
COPY . . 
RUN dotnet restore "TarefasAPI_v2.csproj"

# Copia o restante dos arquivos e compila
COPY . .
RUN dotnet publish "TarefasAPI_v2.csproj" -c Release -o /app/publish
# Estágio Final (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Define a porta que a aplicação vai rodar
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TarefasAPI_v2.dll"]