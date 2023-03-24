﻿FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Slack-GPT-Socket/Slack-GPT-Socket.csproj", "Slack-GPT-Socket/"]
RUN dotnet restore "Slack-GPT-Socket/Slack-GPT-Socket.csproj"
COPY . .

# Set the version as a build argument
ARG VERSION=1.0.0

WORKDIR "/src/Slack-GPT-Socket"
RUN dotnet build "Slack-GPT-Socket.csproj" -c Release -o /app/build --version-suffix $VERSION -v detailed

FROM build AS publish
RUN dotnet publish "Slack-GPT-Socket.csproj" -c Release -o /app/publish --version-suffix $VERSION -v detailed

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Slack-GPT-Socket.dll"]
