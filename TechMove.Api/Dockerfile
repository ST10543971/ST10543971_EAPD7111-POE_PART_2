FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY TechMove.Api/TechMove.Api.csproj TechMove.Api/
RUN dotnet restore TechMove.Api/TechMove.Api.csproj
COPY TechMove.Api/ TechMove.Api/
RUN dotnet publish TechMove.Api/TechMove.Api.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
RUN mkdir -p /app/Uploads && chown -R app:app /app/Uploads
COPY --from=build /app/publish .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "TechMove.Api.dll"]
