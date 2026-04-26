# ==========================================
# VARIABLES (Shortcuts for long paths)
# ==========================================
API_PROJ = src/CredibilityIndex.Api/CredibilityIndex.Api.csproj
INFRA_PROJ = src/CredibilityIndex.Infrastructure/CredibilityIndex.Infrastructure.csproj
APP_PROJ = src/CredibilityIndex.Application/CredibilityIndex.Application.csproj
DOMAIN_PROJ = src/CredibilityIndex.Domain/CredibilityIndex.Domain.csproj
UI_DIR = ui/credibility-ui

# EF Core CLI tool (dotnet-ef)
EF_VERSION ?= 10.0.3
TOOLS_MANIFEST = .config/dotnet-tools.json
DOTNET_EF = dotnet tool run dotnet-ef

# ==========================================
# 1. INITIAL SETUP (Run these once)
# ==========================================

## setup: Links the internal projects together (defines the architecture)
setup:
	@echo "Linking internal project references..."
	dotnet add $(API_PROJ) reference $(APP_PROJ) $(INFRA_PROJ)
	dotnet add $(INFRA_PROJ) reference $(DOMAIN_PROJ) $(APP_PROJ)
	dotnet add $(APP_PROJ) reference $(DOMAIN_PROJ)
	dotnet restore

## db-init: Creates the database schema and applies it
db-init:
	dotnet ef migrations add InitialCreate --project src/CredibilityIndex.Infrastructure --startup-project src/CredibilityIndex.Api --output-dir Persistence/Migrations
	dotnet ef database update --project src/CredibilityIndex.Infrastructure --startup-project src/CredibilityIndex.Api

# Run this to start the API and UI together
dev:
	dotnet watch run --project src/CredibilityIndex.Api & cd ui/credibility-ui && npm start