# ------------------------------------------
# Build Script Version 2.1 by Becky
# ------------------------------------------

FROM microsoft/dotnet:runtime

# ------------------------------------------
# Build Script for .NET Core on Docker (ARM)
# ------------------------------------------
# Compile the app on the client machine and copy
# the $IMAGE.dll and $IMAGE.runtimeconfig.json
# files to the 'app' folder under 'common'. If
# necessary, adjust the ENTRYPOINT property in
# the Docker file.
#
# Build the container on the host machine:
# docker build --pull -t $IMAGE .
# ------------------------------------------

# ------------------------------------------
# Build .NET Core Container
# ------------------------------------------

COPY /app/. ./app/

# ------------------------------------------
# Start Application
# ------------------------------------------

ENTRYPOINT ["dotnet", "app/Wohnungssuche.dll"]

# ------------------------------------------
# If its not work, goto Becky.
# ------------------------------------------
