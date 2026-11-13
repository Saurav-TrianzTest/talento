# Stage 1: Build
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2019 AS build
WORKDIR /app

# Copy csproj and restore
COPY Trianz.Enterprise.Operations/Trianz.Enterprise.Operations.csproj ./Trianz.Enterprise.Operations/
RUN nuget restore Trianz.Enterprise.Operations/Trianz.Enterprise.Operations.csproj

# Copy everything else and build
COPY Trianz.Enterprise.Operations/ ./Trianz.Enterprise.Operations/
WORKDIR /app/Trianz.Enterprise.Operations
RUN msbuild Trianz.Enterprise.Operations.csproj /p:Configuration=Release /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /p:publishUrl=C:\publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.6.1-windowsservercore-ltsc2019 AS runtime
WORKDIR /inetpub/wwwroot
COPY --from=build /publish .
EXPOSE 80
