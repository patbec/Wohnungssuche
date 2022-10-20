FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0
LABEL org.opencontainers.image.title="Wohnungssuche"
LABEL org.opencontainers.image.description="Eine kleine Anwendung um im Raum Würzburg automatisch nach neuen Wohnungen zu suchen."
LABEL org.opencontainers.image.url="https://github.com/patbec"
LABEL org.opencontainers.image.documentation="https://github.com/patbec/wohnungssuche.git"
LABEL org.opencontainers.image.licenses="MIT"
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "wohnungssuche.dll"]