# Credibility Index - Backend API

This folder contains the .NET 8.0 core implementation following Clean Architecture principles.

### 🏗 Architecture Overview
* **Api**: Entry point, Controllers, and Middleware.
* **Application**: Business logic, DTOs, and Request Handlers.
* **Domain**: Entities, Enums, and Core logic (No dependencies).
* **Infrastructure**: Database context, Migrations, and External Services.
* **Shared**: Common utilities and constants.

### 🚀 Local Setup
1. **Prerequisites**: Install [.NET SDK 8.0+](https://dotnet.microsoft.com/download).
2. **Restore**: Run `dotnet restore` from this directory.
3. **Database**: 
   - Ensure your connection string in `CredibilityIndex.Api/appsettings.Development.json` is correct.
   - Run `dotnet ef database update --project CredibilityIndex.Infrastructure --startup-project CredibilityIndex.Api`
4. **Run**: `dotnet run --project CredibilityIndex.Api`