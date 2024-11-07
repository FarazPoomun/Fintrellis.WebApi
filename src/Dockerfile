FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.sln .
COPY ["Fintrellis.Services.Tests/*.csproj", "Fintrellis.Services.Tests/"]
COPY ["Fintrellis.MongoDb/*.csproj", "Fintrellis.MongoDb/"]
COPY ["Fintrellis.Services/*.csproj", "Fintrellis.Services/"]
COPY ["Fintrellis.WebApi/*.csproj", "Fintrellis.WebApi/"]

RUN dotnet restore
COPY . .
RUN dotnet publish "Fintrellis.WebApi/Fintrellis.WebApi.csproj" -c Release -o /app/publish/api

FROM base AS api
WORKDIR /app
COPY --from=build /app/publish/api .
ENTRYPOINT ["dotnet", "Fintrellis.WebApi.dll"]
