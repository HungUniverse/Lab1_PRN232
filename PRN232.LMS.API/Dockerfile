FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PRN232.LMS.API/PRN232.LMS.API.csproj", "PRN232.LMS.API/"]
COPY ["PRN232.LMS.Service/PRN232.LMS.Service.csproj", "PRN232.LMS.Service/"]
COPY ["PRN232.LMS.Repository/PRN232.LMS.Repository.csproj", "PRN232.LMS.Repository/"]
RUN dotnet restore "PRN232.LMS.API/PRN232.LMS.API.csproj"

COPY . .
RUN dotnet publish "PRN232.LMS.API/PRN232.LMS.API.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PRN232.LMS.API.dll"]
