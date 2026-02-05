# Run this to create your database tables
db-init:
	dotnet ef migrations add InitialCreate --project src/CredibilityIndex.Infrastructure --startup-project src/CredibilityIndex.Api --output-dir Persistence/Migrations
	dotnet ef database update --project src/CredibilityIndex.Infrastructure --startup-project src/CredibilityIndex.Api

# Run this to start the API and UI together.
dev:
	dotnet watch run --project src/CredibilityIndex.Api & cd ui/credibility-ui && npm start