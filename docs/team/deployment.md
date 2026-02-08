## 🚀 Deployment Guide (deployment.md)
### 🏗 Deployment Strategy
The Credibility Index is designed to be a Containerized Application. This ensures that the code running in your WSL2 (Ubuntu) environment behaves exactly the same way when moved to a Production Server.

### 🛠 Environment Setup
Before deploying, ensure your Ubuntu environment has Docker installed:

Docker Engine: sudo apt install docker.io

Docker Compose: sudo apt install docker-compose

### 🐳 Container Configuration
We use Docker Compose to manage the three main tiers of the application:

Frontend (Angular):

Dockerized using an Nginx image to serve the static production files.

Backend (.NET 10 API):

Dockerized using the ASP.NET Core Runtime image. This is where your Registration API will live.

Database (SQL Server):

A containerized instance of SQL Server for Linux.

### 📝 Deployment Steps (Future Logic)
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

### 🔐 Registration API Setup Notes
Since you are currently generating the Registration API, remember to configure these environment variables in your deployment settings:

AUTH_DB_CONNECTION: Connection string for the user database.

JWT_SECRET: A secure key for signing registration tokens.

CORS_ORIGIN: Set to your UI URL to allow registration requests.