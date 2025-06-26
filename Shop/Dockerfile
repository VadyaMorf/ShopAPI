#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Shop/Shop.csproj", "Shop/"]
COPY ["Shop.Application/Shop.Application.csproj", "Shop.Application/"]
COPY ["Shop.Core/Shop.Core.csproj", "Shop.Core/"]
COPY ["Shop.DataAccess/Shop.DataAccess.csproj", "Shop.DataAccess/"]
COPY ["Shop.Infastracture/Shop.Infastracture.csproj", "Shop.Infastracture/"]

RUN dotnet restore "Shop/Shop.csproj"
COPY . .
WORKDIR "/src/Shop"
RUN dotnet build "Shop.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Shop.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Копируем XML файлы в контейнер
COPY ["Shop/xml_files", "/app/xml_files"]
ENTRYPOINT ["dotnet", "Shop.dll"]