## 🚀 Deployment Guide (deployment.md)
# 1.0 Deployment Architecture 🏗️

The Credibility Index is a **containerized application**. This ensures consistent behavior between **WSL2 (Ubuntu) development** and production servers.

### 1.1 Application Tiers

| Tier | Description | Container |
|------|-------------|-----------|
| Frontend | Angular production files | Nginx |
| Backend | .NET 10 API including Registration API | ASP.NET Core Runtime |
| Database | SQL Server for Linux | Official SQL Server Container |

### 1.2 CI/CD Flow

- **Source Control:** GitHub  
- **Pipeline:** Builds backend and frontend Docker images, runs tests, applies migrations, and deploys containers.  
- **Validation:** Unit tests, integration tests, and security/authentication checks (see `utr.md`) are executed before release.  

# 2.0 Environment Setup 🛠

Before deployment, ensure your Ubuntu environment includes:

```bash
# Docker Engine
sudo apt install docker.io

# Docker Compose
sudo apt install docker-compose
```
## 2.1 Required Environment Variables

The following environment variables **must be set in production**:

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__Default` | SQL Server connection string | `Server=tcp:db.net...` |
| `ASPNETCORE_ENVIRONMENT` | App execution mode | `Production` |
| `Jwt__Secret` | Key for signing API tokens | `[Long-Random-String]` |
| `AUTH_DB_CONNECTION` | Connection string for Registration API | `Server=tcp:db.net...` |
| `JWT_SECRET` | Secret key for Registration API | `[Long-Random-String]` |
| `CORS_ORIGIN` | UI URL allowed to access Registration API | `https://ui.example.com` |

> ⚠️ **Security Note:** Never commit secrets to source control. Use **CI/CD secrets** or environment variable overrides for production deployments.


# 3.0 🐳 Container Configuration
We use Docker Compose to manage the three main tiers of the application:

- Frontend (Angular):

- Dockerized using an Nginx image to serve the static production files.

- Backend (.NET 10 API):

- Dockerized using the ASP.NET Core Runtime image. This is where your Registration API will live.

- Database (SQL Server):

- A containerized instance of SQL Server for Linux.

## 3.1 📝 Deployment Steps (Future Logic)
Once the Registration API is generated, the deployment flow will be:

1. Build the Images
Navigate to the root directory in WSL and run:

```Bash
docker-compose build
```
2. Start the Services
```Bash
docker-compose up -d
```
3. Verify the Registration API
Test the registration endpoint via the container:

``` URL: http://localhost:8080/api/auth/register ```

# 4.0 🔐 Registration API Setup Notes
Since you are currently generating the Registration API, remember to configure these environment variables in your deployment settings:

AUTH_DB_CONNECTION: Connection string for the user database.

JWT_SECRET: A secure key for signing registration tokens.

CORS_ORIGIN: Set to your UI URL to allow registration requests.

# 5.0. Database Migrations

Since the `CategoryRepository` relies on the `CredibilityDbContext`, any changes to the **Category** entity require a migration.

## 5.1 Create a New Migration

```bash
dotnet ef migrations add AddCategoryTable \
  --project src/CredibilityIndex.Infrastructure \
  --startup-project src/CredibilityIndex.Api
```
## 5.2 Apply Migrations in Production

Our CI/CD pipeline runs the following command during the "Release" phase to ensure the database schema is up to date:

``` bash
dotnet ef database update \
  --project src/CredibilityIndex.Infrastructure \
  --startup-project src/CredibilityIndex.Api
```

## 5.3 tools-ef: Installs/restores the dotnet-ef local tool
```make
make tools-ef
```

## 5.4 db-reset: Deletes the SQLite DB + migrations and recreates from scratch
``` make
make db-reset
```
