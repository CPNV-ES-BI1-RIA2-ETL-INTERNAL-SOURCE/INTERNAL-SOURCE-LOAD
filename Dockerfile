# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /source

# Configure a custom global package folder for NuGet
ENV NUGET_PACKAGES=/source/.nuget/packages

# Copy solution and project files
COPY INTERNAL-SOURCE-LOAD/ ./INTERNAL-SOURCE-LOAD/
COPY INTERNAL_SOURCE_LOAD_TEST/ ./INTERNAL_SOURCE_LOAD_TEST/

# Restore dependencies
WORKDIR /source/INTERNAL-SOURCE-LOAD
RUN dotnet restore --packages $NUGET_PACKAGES

# Build the app
RUN dotnet build -c Release --no-restore

# Stage 2: Test
FROM build AS test
WORKDIR /source/INTERNAL_SOURCE_LOAD_TEST
RUN dotnet test -c Release --logger:trx --no-build

# Stage 3: Publish
FROM build AS publish
WORKDIR /source/INTERNAL-SOURCE-LOAD
RUN dotnet publish -c Release -o /app/out --no-build

# Stage 4: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

COPY --from=publish /app/out .
ENTRYPOINT ["dotnet", "INTERNAL-SOURCE-LOAD.dll"]
