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
	@$(MAKE) tools-ef
	@echo "Creating and applying database migrations..."
	$(DOTNET_EF) migrations add InitialCreate --project $(INFRA_PROJ) --startup-project $(API_PROJ) --output-dir Persistence/Migrations --context CredibilityDbContext
	$(DOTNET_EF) database update --project $(INFRA_PROJ) --startup-project $(API_PROJ) --context CredibilityDbContext

## tools-ef: Installs/restores the dotnet-ef local tool
tools-ef:
	@echo "Ensuring dotnet-ef is available (local tool)..."
	@if [ ! -f $(TOOLS_MANIFEST) ]; then dotnet new tool-manifest; fi
	@dotnet tool restore
	@dotnet tool install dotnet-ef --version $(EF_VERSION) || dotnet tool update dotnet-ef --version $(EF_VERSION)
	@dotnet tool restore

## db-reset: Deletes the SQLite DB + migrations and recreates from scratch
db-reset:
	@$(MAKE) tools-ef
	@echo "Deleting SQLite database files..."
	rm -f credibility.db credibility.db-shm credibility.db-wal
	rm -f src/CredibilityIndex.Api/credibility.db src/CredibilityIndex.Api/credibility.db-shm src/CredibilityIndex.Api/credibility.db-wal
	@echo "Deleting migrations folder..."
	rm -rf src/CredibilityIndex.Infrastructure/Persistence/Migrations
	@echo "Recreating migrations and updating database..."
	$(DOTNET_EF) migrations add InitialCreate --project $(INFRA_PROJ) --startup-project $(API_PROJ) --output-dir Persistence/Migrations --context CredibilityDbContext
	$(DOTNET_EF) database update --project $(INFRA_PROJ) --startup-project $(API_PROJ) --context CredibilityDbContext

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

## local-backend: Runs the API with Development environment (local profile)
local-backend:
	dotnet watch run --project $(API_PROJ) --launch-profile local

## dev-backend: Runs the API with CI environment
dev-backend:
	dotnet watch run --project $(API_PROJ) --launch-profile dev

## prod-backend: Runs the API with Production environment
prod-backend:
	dotnet watch run --project $(API_PROJ) --launch-profile prod

## local-ui: Runs frontend using local environment.ts
local-ui:
	cd $(UI_DIR) && npm run start:local

## dev-ui: Runs frontend using environment.dev.ts
dev-ui:
	cd $(UI_DIR) && npm run start:dev

## prod-ui: Runs frontend using environment.prod.ts
prod-ui:
	cd $(UI_DIR) && npm run start:prod

## local: Runs local API (https://localhost:7222) and local Angular server in parallel
local:
	@$(MAKE) -j 2 local-backend local-ui

## dev: Runs Angular dev environment server (targets dev API)
dev:
	@$(MAKE) -j 2 dev-backend dev-ui

## prod: Runs Angular production environment server (targets prod API)
prod:
	@$(MAKE) -j 2 prod-backend prod-ui

## dev-backend-http: Runs the API on HTTP profile (legacy)
dev-backend-http:
	dotnet watch run --project $(API_PROJ) --launch-profile http

docker-build:
	docker build -f src/CredibilityIndex.Api/Dockerfile -t credibilityindex-api:latest .

docker-up:
	docker compose -f deploy/docker-compose.yml up -d --build

docker-down:
	docker compose -f deploy/docker-compose.yml down

test:
	dotnet restore
	dotnet test tests/CredibilityIndex.ApiTests/CredibilityIndex.ApiTests.csproj

