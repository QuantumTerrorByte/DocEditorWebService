FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["WordWebApplication/WordWebApplication.csproj", "WordWebApplication/"]
RUN dotnet restore "WordWebApplication/WordWebApplication.csproj"
COPY . .
WORKDIR "/src/WordWebApplication"
RUN dotnet build "WordWebApplication.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WordWebApplication.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WordWebApplication.dll"]
