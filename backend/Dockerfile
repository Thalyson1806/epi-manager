FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/EpiManagement.API/EpiManagement.API.csproj", "src/EpiManagement.API/"]
COPY ["src/EpiManagement.Application/EpiManagement.Application.csproj", "src/EpiManagement.Application/"]
COPY ["src/EpiManagement.Infrastructure/EpiManagement.Infrastructure.csproj", "src/EpiManagement.Infrastructure/"]
COPY ["src/EpiManagement.Domain/EpiManagement.Domain.csproj", "src/EpiManagement.Domain/"]
RUN dotnet restore "src/EpiManagement.API/EpiManagement.API.csproj"

COPY . .
RUN dotnet publish "src/EpiManagement.API/EpiManagement.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "EpiManagement.API.dll"]
