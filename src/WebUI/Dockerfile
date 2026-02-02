FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/WebUI/WebUI.csproj", "src/WebUI/"]

RUN dotnet restore "src/WebUI/WebUI.csproj"

COPY . .

WORKDIR "/src/src/WebUI"
RUN dotnet build "WebUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebUI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebUI.dll"]
