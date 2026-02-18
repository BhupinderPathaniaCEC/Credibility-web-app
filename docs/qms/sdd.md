## 2.  (Software Design Document)
This file fulfills the specific requirements from your guide regarding architecture, request lifecycles, and security.


# # Software Design Document (SDD)

## 🏛 System Architecture
This project follows **Clean Architecture** principles to ensure the system is independent of UI, database, and external frameworks.

* **Domain**: Core entities and business logic.
* **Application**: Use cases and request handling .
* **Infrastructure**: Database persistence (EF Core) and external services.
* **API**: RESTful endpoints and entry points.

## 🔄 Data Flow (Request Lifecycle)
How a request travels from the user to the database:

1. **Angular UI**: User triggers an action; request is sent via HttpClient.
2. **.NET API**: The controller receives the request and validates the JWT.
3. **Application Layer**: The command/query is handled, and business rules are applied.
4. **Infrastructure Layer**: EF Core translates the request into a query for **SQL Server**.
5. **Database**: Data is retrieved/saved, and the response flows back up the chain.


## ⚙️ Tech Stack Rationale
* **.NET 10**: Chosen for its high performance, long-term support, and native integration with Windows/WSL environments.
* **Angular**: Chosen for its robust enterprise-grade framework and strong typing (TypeScript).

## 🛡 Security
* **Authentication**: Managed via **OpenIddict** using **JWT (JSON Web Tokens)** for secure stateless communication.
* **Data Validation**: Implemented using FluentValidation in the Application layer to ensure data integrity before it reaches the Domain.

### 🔑 Identity and Access Management
The AuthController handles both user provisioning and token issuance.

# Registration Logic:

* Checks for existing users by Email.

* Utilizes UserManager.CreateAsync for secure password hashing and persistence.

* Returns a RegisterResponse containing Id, Email, and DisplayName.

# Token Exchange Logic:

* **Implementation:** The /connect/token endpoint is manually exposed in AuthController to allow for Swagger integration and custom logging.

* **Claims Transformation:** On successful authentication, the system generates a ClaimsIdentity containing:

    * **sub:** User ID

    * **name:** Username

    * **email:** User email

* **Security Constraints:** * Destinations are restricted to AccessToken.

    * Only PasswordGrant is accepted; other grant types return a ForbidResult.

    * Anti-forgery tokens are ignored for this endpoint to support cross-origin API calls (Chrome Extension).

### Token Lifetime Configuration:
The Access Token lifetime is configurable via the application settings hierarchy (JSON, Environment Variables, or Secrets), allowing security adjustments without code changes.

# Configuration Keys

## KeyDefaultDescriptionIdentitySettings

|Key|Default|Description|
|:---|:---|:---|
|**IdentitySettings:AccessTokenLifetimeMinutes** |60 |Time in minutes until the JWT expires.|

## Example appsettings.json:
'''json
{
    "IdentitySettings": {
        "AccessTokenLifetimeMinutes": 60
    }
}

**Environment Variable Override:** IdentitySettings__AccessTokenLifetimeMinutes=30

# Security Specifications

### Identifier Claims
Every generated access token includes a `sub` (Subject) claim containing the unique `ApplicationUser.Id` from the database.

### Anti-Enumeration
The `Exchange` action in `AuthController` uses a unified `Forbid()` response for all credential failures to prevent user existence discovery.

### Flow Restriction
Browser-based flows (Authorization Code/Implicit) are disabled in the OpenIddict configuration to prevent redirect-based attacks.