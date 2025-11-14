FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["UserService.csproj", "."]
RUN dotnet restore
COPY . .
WORKDIR /src
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .

# DO NOT USE USER HERE (causes copy failure)

ENTRYPOINT ["dotnet", "UserService.dll"]