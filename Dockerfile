FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /BeeCollector

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore BeeCollector/BeeCollector.csproj
# Build and publish a release
RUN dotnet publish BeeCollector/BeeCollector.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/sdk:7.0
WORKDIR /BeeCollector
COPY --from=build-env /BeeCollector/out .
ENTRYPOINT ["dotnet", "BeeCollector.dll"]