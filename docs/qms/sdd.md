# (Software Design Document)
This file fulfills the specific requirements from your guide regarding architecture, request lifecycles, and security.


# 1.0 Software Design Document (SDD)

# 2.0 System Architecture

## 2.1 Architectural Style

This project follows **Clean Architecture** principles to ensure the system is independent of UI, database, and external frameworks.

## 2.2 Layered Structure

### 2.2.1 Domain Layer
- Contains core entities.
- Implements business rules.
- Has no dependencies on external frameworks.

### 2.2.2 Application Layer
- Handles use cases.
- Processes commands and queries.
- Applies validation and orchestration logic.

### 2.2.3 Infrastructure Layer
- Manages database persistence using **Entity Framework Core (EF Core)**.
- Integrates external services.

### 2.2.4 API Layer
- Exposes RESTful endpoints.
- Acts as the system entry point.

# 3.0 🔄 Data Flow (Request Lifecycle)

## 3.1 End-to-End Request Flow
How a request travels from the user to the database:

1. **Angular UI**  
   The user triggers an action and a request is sent via `HttpClient`.

2. **.NET API**  
   The controller receives the request and validates the JWT.

3. **Application Layer**  
   The command/query handler applies business rules.

4. **Infrastructure Layer**  
   EF Core translates the request into SQL queries targeting **SQL Server**.

5. **Database**  
   Data is retrieved or persisted, and the response flows back through the layers.


# 4.0 ⚙️ Tech Stack Rationale

## 4.1 Backend – .NET 10
- High performance.
- Long-term support.
- Strong integration with Windows and WSL environments.

## 4.2 Frontend – Angular
- Enterprise-grade framework.
- Strong typing via TypeScript.
- Structured modular architecture.

# 5.0 🛡 Security

## 5.1 Authentication Strategy

- Managed using **OpenIddict**.
- Stateless authentication via **JWT (JSON Web Tokens)**.
- Token-based authorization for protected endpoints.

## 5.2 Data Validation

- Implemented using **FluentValidation**.
- Executed in the Application layer before domain interaction.

# 6.0 🔑 Identity and Access Management
The AuthController handles both user provisioning and token issuance.

The `AuthController` manages:

- User registration.
- Token issuance.
- Claims generation.

## 6.1 Registration Logic

### 6.1.1 User Validation
- Checks for existing users by Email.

### 6.1.2 User Creation
- Uses `UserManager.CreateAsync`.
- Performs secure password hashing.
- Persists user data.

### 6.1.3 Response Model
Returns:
- `Id`
- `Email`
- `DisplayName`

## 6.2 Token Exchange Logic

### 6.2.1 Endpoint Exposure
The `/connect/token` endpoint is manually exposed for:
- Swagger integration.
- Custom logging.

### 6.2.2 Claims Transformation

On successful authentication, a `ClaimsIdentity` is generated with:

- `sub` – User ID  
- `name` – Username  
- `email` – User Email

### 6.2.3 Security Constraints

- Destinations restricted to `AccessToken`.
- Only Password Grant is accepted.
- Other grant types return `ForbidResult`.
- Anti-forgery tokens are ignored to support cross-origin calls (e.g., Chrome Extension).

# 7.0 🔑Token Lifetime Configuration:

## 7.1 Configuration Strategy

Access token lifetime is configurable via:

- `appsettings.json`
- Environment Variables
- Secret storage

The Access Token lifetime is configurable via the application settings hierarchy (JSON, Environment Variables, or Secrets), allowing security adjustments without code changes.

## 7.2 Configuration Keys

### 7.2.1 KeyDefaultDescriptionIdentitySettings

|Key|Default|Description|
|:---|:---|:---|
|**IdentitySettings:AccessTokenLifetimeMinutes** |60 |Time in minutes until the JWT expires.|

## 7.3 Example appsettings.json:

'''json
{
    "IdentitySettings": {
        "AccessTokenLifetimeMinutes": 60
    }
}

### 7.3.1Environment Variable Override:

```</code>
IdentitySettings__AccessTokenLifetimeMinutes=30
```

# 8.0 🔑Security Specifications

## 8.1 Identifier Claims

Every generated access token includes:
`sub` (Subject) claim containing the unique `ApplicationUser.Id` from the database.

## 8.2 Anti-Enumeration

The `Exchange` action in `AuthController` uses a unified `Forbid()` response for all credential failures to prevent user existence discovery.

## 8.3 Flow Restriction

Authorization Code and Implicit flows are disabled in OpenIddict to prevent redirect-based attacks.

# 9.0 Authentication Mechanism

## 9.1 Protocol:

* OAuth 2.0

* Resource Owner Password Credentials (ROPC)

## 9.2 Token Validation

* Handled by OpenIddict.Validation.AspNetCore middleware:

* Signature validation

* Expiration validation

* Issuer validation

## 9.3 Transport Security:

For development environments:
`DisableTransportSecurityRequirement()` is applied to allow testing over HTTP.

# 10.0 Endpoint Protection Strategy

Authorization is enforced using a layered approach:

## 10.1 Global Middleware:

Registered in pipeline:

 * `app.UseAuthentication()`

 * `app.UseAuthorization()`

All requests require a valid security context.

## 10.2 Declarative Authorization:

Protected endpoints use:

``` </> code
[Authorize]
```

## 10.3 Role-Based Access Control (RBAC):

Administrative endpoints use:

``` </> code
[Authorize(Roles = "Admin")]
```

# 11.0 Security Response Matrix (E2E Validation)

The following table defines the system's behavior for protected resources, fulfilling the requirement for standardized error responses:

|Condition |Middleware Action| HTTP Status|
|:---|:---|:---:|
|**No Token Provided** |Authentication Challenge |**401 Unauthorized**|
|**Invalid/Expired Token** |Validation Failure |**401 Unauthorized**|
|**Valid Token, Wrong Role** |Authorization Denial |**403 Forbidden**|
|**Valid Token + Correct Role** |Request Processed |**200 OK**|

# 12.0 Data Persistence Design

## 12.1 Category Management Logic

The `CategoryRepository` handles the lifecycle of categories used to classify credibility scores. It follows the Repository Pattern to decouple the Domain from the Infrastructure.

## 12.2 Key Operations

| Operation | Method | Description |
| :--- | :--- | :--- |
| **Create** | `AddAsync` | Persists a new Category entity to the database. |
| **Read** | `GetActiveCategoriesAsync` | Filters categories based on the `IsActive` flag for UI display. |
| **Update** | `ToggleStatusAsync` | A specialized update for soft-enabling/disabling categories. |
| **Delete** | `DeleteAsync` | Performs a hard delete of the category record. |

## 12.3 Repository Data Flow

1. **API Layer** receives a request.
2. **Application Layer** invokes the `ICategoryRepository` interface.
3. **Infrastructure Layer** (this file) implements the logic using `CredibilityDbContext` (Entity Framework Core).

# 13.0 Rating Submission Workflow

## 13.1 Overview

The Rating submission system allows authenticated users to submit credibility assessments for websites. Each submission triggers automatic calculation of aggregate snapshot data (scores, averages) that are cached for fast retrieval.

## 13.2 Submission Flow

### 13.2.1 User Initialization Phase

1. User must be authenticated with a valid JWT token.
2. System extracts the user ID from the OpenIddict `sub` (Subject) claim.
3. If no `sub` claim exists, fallback to `NameIdentifier` claim.
4. If user ID cannot be parsed as a GUID, request is rejected with **401 Unauthorized**.

### 13.2.2 Domain Normalization

Before processing any rating, the raw URL is normalized:

- Input: `https://example.com`, `EXAMPLE.COM`, `Example.com:443`
- Process: `DomainUtility.NormalizeDomain()` extracts and lowercases the domain
- Output: `example.com`

This ensures preventing duplicate website entries for the same domain.

### 13.2.3 Website Resolution

The system checks if the domain already exists in the database:

- **if exists**: Reuse the existing `Website` entity
- **if not exists**: Create a new `Website` entity with:
  - `Domain`: Normalized domain
  - `Name`: Default to normalized domain
  - `DisplayName`: Default to normalized domain
  - `CreatedAt`: Current UTC timestamp
  - Persist immediately to database (required for FK relationship)

### 13.2.4 Rating Entity Creation

A new `RatingEntity` is instantiated with:

| Field | Source | Purpose |
|-------|--------|---------|
| `UserId` | Extracted from JWT | Identifies the reviewer |
| `WebsiteId` | From normalized domain lookup | Links rating to website |
| `Accuracy` | Request body (1-5) | User's rating of factual accuracy |
| `BiasNeutrality` | Request body (1-5) | User's rating of neutrality |
| `Transparency` | Request body (1-5) | User's rating of content transparency |
| `SafetyTrust` | Request body (1-5) | User's rating of website safety |
| `Comment` | Request body (optional) | User's textual feedback |

### 13.2.5 Repository Upsert Operation

The `IRatingRepository.UpsertRatingAsync()` method:

1. **Upsert Logic**: Finds existing rating by `(UserId, WebsiteId)` composite key
2. **Update Case**: If rating exists, update it with new scores
3. **Insert Case**: If rating doesn't exist, create new entry
4. **Snapshot Calculation**: After upsert, automatically recalculate aggregate snapshot

### 13.2.6 Snapshot Calculation

The `SnapshortCalculator` (Application Layer) computes:

| Metric | Formula | Purpose |
|--------|---------|---------|
| `Score` (0-100) | Average of all ratings × 20 | Overall credibility score |
| `AvgAccuracy` | Mean of all user accuracy ratings | Aggregate accuracy assessment |
| `AvgBiasNeutrality` | Mean of all bias/neutrality ratings | Aggregate neutrality assessment |
| `AvgTransparency` | Mean of all transparency ratings | Aggregate transparency assessment |
| `AvgSafetyTrust` | Mean of all safety/trust ratings | Aggregate safety assessment |
| `RatingCount` | Total number of ratings | Confidence indicator |
| `ComputedAt` | Current UTC timestamp | Snapshot freshness |

The snapshot is cached in the `CredibilitySnapshot` table for O(1) lookup performance.

### 13.2.7 Response

Returns **200 OK** with `UpdatedSnapshotResponse` containing the freshly computed snapshot.

## 13.3 Data Consistency

### 13.3.1 Transaction Boundaries

- Website creation and rating upsert happen within the same repository operation
- Snapshot calculation is deterministic and idempotent
- Concurrent submissions for the same website are serialized at the database level

### 13.3.2 Validation Rules

- All rating scores must be integers between 1 and 5 (enforced via `[Range(1, 5)]` attributes)
- Comments cannot exceed 1000 characters
- URL must be valid and normalizable to a domain
- User ID must be a valid GUID

# 14.0 Rating Query Workflow

## 14.1 Overview

The Rating Query system provides read-only access to submitted ratings for a domain. It uses a dedicated read-only repository to optimize performance and maintain separation of concerns.

## 14.2 Query Flow

### 14.2.1 Domain Normalization

1. Input domain from route parameter: `/api/v1/websites/{domain}/ratings`
2. Apply `DomainUtility.NormalizeDomain()` for consistency
3. If normalization fails (empty/invalid), return **400 Bad Request**

### 14.2.2 Website Existence Check

1. Query `IWebsiteRepository.GetByNormalizedDomainAsync(normalizedDomain)`
2. If website not found, return **404 Not Found**
3. If website exists, proceed with rating retrieval

### 14.2.3 Pagination

Query parameters control pagination:
- `page` (default 1): Which page of results to return
- `pageSize` (default 10): How many ratings per page

The `IRatingQueryRepository.GetPaginatedRatingsAsync()` method:
- Accepts `WebsiteId`, `page`, and `pageSize`
- Returns tuple: `(ratings: List<RatingEntity>, totalCount: int)`
- Implements LIMIT/OFFSET at the database level for efficiency

### 14.2.4 Response Mapping

Each rating is mapped to `RatingItemResponse`:

| Field | Source | Purpose |
|-------|--------|---------|
| `Id` | Rating ID | Unique identifier |
| `Accuracy` | Rating.Accuracy | Displayed score (1-5) |
| `BiasNeutrality` | Rating.BiasNeutrality | Displayed score (1-5) |
| `Transparency` | Rating.Transparency | Displayed score (1-5) |
| `SafetyTrust` | Rating.SafetyTrust | Displayed score (1-5) |
| `Comment` | Rating.Comment | User feedback |
| `CreatedAt` | Rating.CreatedAt | Submission timestamp |
| `DisplayName` | Hardcoded | "Anonymous Reviewer" (privacy by default) |

### 14.2.5 Response

Returns **200 OK** with `PaginatedRatingsResponse` containing:
- `Domain`: Normalized domain
- `TotalCount`: Total number of ratings for this domain
- `Page`: Current page number
- `PageSize`: Ratings per page
- `Items`: Array of `RatingItemResponse`

## 14.3 Access Control

### 14.3.1 Authentication Not Required

Rating query endpoints use `[AllowAnonymous]` to enable:
- Public visibility of credibility assessments
- Chrome Extension queries without user authentication
- SEO-friendly crawlability

### 14.3.2 Privacy Considerations

- Individual reviewer identities are never exposed
- All ratings show as "Anonymous Reviewer"
- Only aggregated data (via snapshot) can be sensitive

## 14.4 Data Consistency

### 14.4.1 Read-Only Repository

`IRatingQueryRepository` is strictly read-only:
- No write operations
- Uses separate database context (optional, for optimization)
- Designed for concurrent reads without contention

### 14.4.2 Pagination Integrity

- `TotalCount` remains stable across page requests
- LIMIT/OFFSET at database level prevents skipped/duplicated records
- Sorting (if implemented) must be consistent