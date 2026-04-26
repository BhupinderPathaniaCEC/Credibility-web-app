FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy full repository and build frontend assets first
COPY . .
WORKDIR /src/ui/credibility-ui
RUN npm install
RUN npm run build -- --configuration production

WORKDIR /src
RUN dotnet restore CredibilityIndex.slnx
RUN dotnet publish src/CredibilityIndex.Api/CredibilityIndex.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "CredibilityIndex.Api.dll"]
