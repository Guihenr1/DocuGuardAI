# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/DocuGuardAI.Api/DocuGuardAI.Api.csproj"
WORKDIR "/src/src/DocuGuardAI.Api"
RUN dotnet build "DocuGuardAI.Api.csproj" -c Release -o /app/build

# Publish stage (MUST come BEFORE final)
FROM build AS publish
RUN dotnet publish "DocuGuardAI.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/* \
    
EXPOSE 8080
ENTRYPOINT ["dotnet", "DocuGuardAI.Api.dll"]

# Optional health check so Azure knows it's ready
HEALTHCHECK CMD curl --fail http://localhost:8080/health || exit 1