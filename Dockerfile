FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy csproj and restore dependencies
COPY src/WebTechProfiler/*.csproj ./src/WebTechProfiler/
RUN dotnet restore src/WebTechProfiler/*.csproj

# Copy everything else
COPY . ./
RUN dotnet publish src/WebTechProfiler -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "WebTechProfiler.dll"] 