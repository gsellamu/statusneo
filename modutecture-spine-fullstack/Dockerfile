# Multi-stage build: SDK to compile, slim ASP.NET runtime to serve.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY twin-service-dotnet/Spine.Twin.csproj twin-service-dotnet/
RUN dotnet restore twin-service-dotnet/Spine.Twin.csproj
COPY twin-service-dotnet/ twin-service-dotnet/
RUN dotnet publish twin-service-dotnet/Spine.Twin.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Spine.Twin.dll"]
