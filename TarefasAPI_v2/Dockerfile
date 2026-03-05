FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo .csproj (agora sem o caminho da pasta)
COPY ["TarefasAPI_v2.csproj", "./"]
RUN dotnet restore "TarefasAPI_v2.csproj"

# Copia todo o restante
COPY . .
RUN dotnet publish "TarefasAPI_v2.csproj" -c Release -o /app/publish

# Estágio Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TarefasAPI_v2.dll"]