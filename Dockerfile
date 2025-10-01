
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["TrabalhoP2P.csproj", "./"]
RUN dotnet restore "./TrabalhoP2P.csproj"

COPY . .
RUN dotnet publish "./TrabalhoP2P.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

COPY --from=build /app/publish .

COPY knownPeers/ ./knownPeers/

EXPOSE 5000 5001 5002

CMD ["dotnet", "TrabalhoP2P.dll"]