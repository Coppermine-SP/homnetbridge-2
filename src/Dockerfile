FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app
ENV DOTNET_NUGET_SIGNATURE_VERIFICATION FALSE

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out ./CloudInteractive.HomNetBridge.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install Libraries
RUN apt update -y; exit 0
RUN apt install -y tcpdump

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "HomNetBridge_2.dll"]
