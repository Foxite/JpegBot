FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["JpegBot/JpegBot.csproj", "JpegBot/"]
RUN dotnet restore "JpegBot/JpegBot.csproj"
COPY . .
WORKDIR "/src/JpegBot"
RUN dotnet build "JpegBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JpegBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JpegBot.dll"]
