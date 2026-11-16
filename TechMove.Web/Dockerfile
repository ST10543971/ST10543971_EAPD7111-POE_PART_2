FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TechMove.Web/TechMove.csproj TechMove.Web/
RUN dotnet restore TechMove.Web/TechMove.csproj
COPY TechMove.Web/ TechMove.Web/
RUN dotnet publish TechMove.Web/TechMove.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "TechMove.dll"]
