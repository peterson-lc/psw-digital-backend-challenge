# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy central package management and solution files
COPY Directory.Packages.props ./
COPY PswChallange.slnx ./

# Copy project files for restore
COPY src/PswChallenge.Api/PswChallenge.Api.csproj src/PswChallenge.Api/
COPY src/PswChallenge.Application/PswChallenge.Application.csproj src/PswChallenge.Application/
COPY src/PswChallenge.Infra/PswChallenge.Infra.csproj src/PswChallenge.Infra/

# Restore dependencies
RUN dotnet restore src/PswChallenge.Api/PswChallenge.Api.csproj

# Copy remaining source code
COPY src/ src/

# Publish the application
RUN dotnet publish src/PswChallenge.Api/PswChallenge.Api.csproj \
    -c Release \
    -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "PswChallenge.Api.dll"]

