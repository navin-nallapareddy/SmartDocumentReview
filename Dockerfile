# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy everything and restore
COPY . ./
RUN dotnet restore

# Publish the app
RUN dotnet publish -c Release -o out

# Use the ASP.NET runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

# Expose default port for Blazor Server
EXPOSE 80

# Run the app
ENTRYPOINT ["dotnet", "SmartDocumentReview.dll"]