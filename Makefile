# Run this to create your database tables
db-init:
	dotnet ef migrations add InitialCreate --project src/CredibilityIndex.Infrastructure --startup-project src/CredibilityIndex.Api --output-dir Persistence/Migrations
	dotnet ef database update --project src/CredibilityIndex.Infrastructure --startup-project src/CredibilityIndex.Api


# Run only the .NET Backend
dev-backend:
	dotnet watch run --project src/CredibilityIndex.Api

# Run only the React/NPM UI
dev-ui:
	cd ui/credibility-ui && npm start

# Run both simultaneously (if you still want a single command)
dev:
	make -j 2 dev-backend dev-ui
