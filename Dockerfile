FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Slack-GPT-Socket/Slack-GPT-Socket.csproj", "Slack-GPT-Socket/"]
RUN dotnet restore "Slack-GPT-Socket/Slack-GPT-Socket.csproj"
COPY . .

# Set the version as a build argument
ARG APP_VERSION=1.0.0

WORKDIR "/src/Slack-GPT-Socket"
# create a version file
RUN dotnet build "Slack-GPT-Socket.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Slack-GPT-Socket.csproj" -c Release -o /app/publish
RUN echo $APP_VERSION > /app/publish/version.txt

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Slack-GPT-Socket.dll"]
