FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["DiaryApp.sln", "./"]
COPY ["DiaryApp.API/DiaryApp.API.csproj", "DiaryApp.API/"]
COPY ["DiaryApp.Application/DiaryApp.Application.csproj", "DiaryApp.Application/"]
COPY ["DiaryApp.Domain/DiaryApp.Domain.csproj", "DiaryApp.Domain/"]
COPY ["DiaryApp.Infrastructure/DiaryApp.Infrastructure.csproj", "DiaryApp.Infrastructure/"]
COPY ["DiaryApp.Tests/DiaryApp.Tests.csproj", "DiaryApp.Tests/"]

RUN dotnet restore "DiaryApp.sln" --verbosity normal

COPY . .

WORKDIR "/src/DiaryApp.API"
RUN dotnet publish "DiaryApp.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "DiaryApp.API.dll"]