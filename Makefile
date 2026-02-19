# ==========================================
# VARIABLES (Shortcuts for long paths)
# ==========================================
API_PROJ = src/CredibilityIndex.Api/CredibilityIndex.Api.csproj
INFRA_PROJ = src/CredibilityIndex.Infrastructure/CredibilityIndex.Infrastructure.csproj
APP_PROJ = src/CredibilityIndex.Application/CredibilityIndex.Application.csproj
DOMAIN_PROJ = src/CredibilityIndex.Domain/CredibilityIndex.Domain.csproj
UI_DIR = ui/credibility-ui

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
	@echo "Creating and applying database migrations..."
	dotnet ef migrations add InitialCreate --project src/CredibilityIndex.Infrastructure --startup-project $(API_PROJ) --output-dir Persistence/Migrations
	dotnet ef database update --project src/CredibilityIndex.Infrastructure --startup-project $(API_PROJ)

setup-dependencies:
	dotnet restore
	# Ensure EF Core is installed
	dotnet add src/CredibilityIndex.Infrastructure/CredibilityIndex.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Sqlite
	dotnet add src/CredibilityIndex.Infrastructure/CredibilityIndex.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
	cd ui/credibility-ui && npm install

# ==========================================
# 2. DEPENDENCY MANAGEMENT (Run after git pull)
# ==========================================

## install: Downloads external packages (NuGet and NPM)
install:
	@echo "Installing backend dependencies (NuGet)..."
	dotnet restore
	@echo "Installing frontend dependencies (NPM)..."
	cd $(UI_DIR) && npm install

# ==========================================
# 3. DEVELOPMENT & RUNNING
# ==========================================

## build: Compiles the .NET solution
build:
	dotnet build

## dev-backend: Runs the API with Hot Reload (auto-restarts on save)
dev-backend:
	dotnet watch run --project $(API_PROJ) --launch-profile https

## dev-ui: Runs the frontend development server
dev-ui:
	cd $(UI_DIR) && npm start

## dev: Runs both Backend and Frontend at the same time
dev:
	make -j 2 dev-backend dev-ui

