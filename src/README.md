# Credibility Index - Backend API

This folder contains the .NET 8.0 core implementation following Clean Architecture principles.

### src folder Structure
```
src/
├── CredibilityIndex.Api/              # ASP.NET Core Web API (Composition Root)
│   ├── Controllers/                   # API Endpoints
│   ├── Middlewares/                   # Custom Request/Response handling
│   ├── Filters/                       # Action and Exception filters
│   ├── Contracts/                     # Data Transfer Objects (DTOs)
│   ├── Program.cs                     # App startup and DI configuration
│   └── appsettings.json               # Environment configurations
│
├── CredibilityIndex.Application/      # Business Logic & Use Cases
│   ├── Interfaces/                    # Contract definitions for Infra
│   ├── UseCases/                      # Core application logic
│   ├── Services/                      # Application services
│   └── Validators/                    # Input validation logic
│
├── CredibilityIndex.Domain/           # Core Enterprise Logic (No dependencies)
│   ├── Entities/                      # Database models/Domain objects
│   ├── ValueObjects/                  # Complex types without identity
│   ├── Enums/                         # Domain-specific enumerations
│   └── Events/                        # Domain events
│
├── CredibilityIndex.Infrastructure/    # External Concerns & Persistence
│   ├── Persistence/                   # Data Access Layer
│   │   ├── DbContext/                 # EF Core Context
│   │   ├── Configurations/            # Fluent API model configurations
│   │   └── Migrations/                # Database version history
│   ├── Repositories/                  # Implementation of Application interfaces
│   └── Identity/                      # Authentication and Authorization logic
│
└── CredibilityIndex.Shared/           # Optional Shared Utilities & Helpers
```

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

# Backend Implementation (Clean Architecture)

## Infrastructure Layer (`CredibilityIndex.Infrastructure`)
This layer contains the implementation of interfaces defined in the Application layer.

### Repository Implementation: Category
The `CategoryRepository` uses **Entity Framework Core** for database interactions. 

**Code Patterns Used:**
* **Async/Await:** All database operations are asynchronous to prevent blocking the thread pool.
* **Entity Tracking:** Uses `_context.Categories.FindAsync()` to leverage EF Core's built-in caching for primary key lookups.
* **State Management:** The `ToggleStatusAsync` method encapsulates the business logic of flipping the `IsActive` state, ensuring data integrity before saving.

### How to use the Category Repository
Inject `ICategoryRepository` into your Application Services or MediatR Handlers:

```csharp
public class GetActiveCategoriesHandler {
    private readonly ICategoryRepository _repository;
    
    public GetActiveCategoriesHandler(ICategoryRepository repository) {
        _repository = repository;
    }
}
```
