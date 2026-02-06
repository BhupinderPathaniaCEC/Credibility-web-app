
# Link API to Application and Infrastructure
# Link Infrastructure to Domain and Application
# Link Application to Domain
# Link Application to Domain

setup:
	dotnet add src/CredibilityIndex.Api/CredibilityIndex.Api.csproj reference src/CredibilityIndex.Application/CredibilityIndex.Application.csproj src/CredibilityIndex.Infrastructure/CredibilityIndex.Infrastructure.csproj
	
	dotnet add src/CredibilityIndex.Infrastructure/CredibilityIndex.Infrastructure.csproj reference src/CredibilityIndex.Domain/CredibilityIndex.Domain.csproj src/CredibilityIndex.Application/CredibilityIndex.Application.csproj
	
	dotnet add src/CredibilityIndex.Application/CredibilityIndex.Application.csproj reference src/CredibilityIndex.Domain/CredibilityIndex.Domain.csproj
	
	dotnet restore



# Run this to create your database tables
db-init:
	dotnet ef migrations add InitialCreate --project src/CredibilityIndex.Infrastructure --startup-project src/CredibilityIndex.Api --output-dir Persistence/Migrations
	dotnet ef database update --project src/CredibilityIndex.Infrastructure --startup-project src/CredibilityIndex.Api


# Run only the .NET Backend
dev-backend:
	dotnet watch run --project src/CredibilityIndex.Api/CredibilityIndex.Api.csproj

# Run only the React/NPM UI
dev-ui:
	cd ui/credibility-ui && npm install && ./node_modules/.bin/ng serve --host 0.0.0.0

# Run both simultaneously (if you still want a single command)
dev:
	make -j 2 dev-backend dev-ui
