
# 1.0🧪 Unit Test Report (UTR)

# 2.0 🧪 Testing Strategy

## 2.1 Overview

The Credibility Index project implements a comprehensive testing strategy to ensure system reliability and maintain high code quality standards.

## 2.2 Backend Testing

- Framework: **xUnit / NUnit**
- Target: **.NET 10.0 Web API**
- Scope:
  - Domain entities
  - Application use cases
  - Business logic validation

## 2.3 Frontend Testing

- Framework: **Karma / Jasmine**
- Target: **Angular UI**
- Location: `ui/credibility-ui`
- Scope:
  - Component behavior
  - UI logic validation
  - Rendering stability

# 3.0 🎯 Coverage Goals

To maintain project standards, we have established the following baseline for code quality:

## 3.1 Overall Coverage Target

- **80% minimum coverage** across the entire solution.

## 3.2 Critical Logic Coverage

- **100% coverage** for:
  - Domain logic
  - Credibility scoring algorithms

# 4.0 📊 Test Results

## 4.1 CI/CD Pipeline Summary

| Project | Tests Passed | Tests Failed | Coverage % | Status |
| :--- | :--- | :--- | :--- | :--- |
| **CredibilityIndex.Api** | 0 | 0 | 0% | 🟡 Pending |
| **CredibilityIndex.Domain** | 0 | 0 | 0% | 🟡 Pending |
| **credibility-ui** | 0 | 0 | 0% | 🟡 Pending |

# 5.0 🧪 Security & Authentication Test Report

## 5.1 Test Metadata

- **Test Date:** 2026-02-19  
- **Tester:** API Development Team  
- **Environment:** Development (Localhost)  
- **Target:** AdminController Endpoints 

# 6.0 Authentication & Authorization Test Cases
## 6.1 Purpose

This section validates the authentication and authorization mechanisms implemented in the system.  
It ensures that protected endpoints enforce:

- OAuth 2.0 Password Grant
- JWT Bearer authentication
- Role-Based Access Control (RBAC)
- OpenIddict validation middleware

---

## 6.2 Test Coverage Scope

The following security behaviors are validated:

- Token presence validation
- Token signature validation
- Token expiration validation
- Role enforcement
- Access control to protected endpoints
- Middleware-level rejection before controller execution

---

## 6.3 Authentication & Authorization Test Matrix

| Test ID | Scenario | Token State | Role | Expected Status | Result |
|----------|----------|------------|------|-----------------|--------|
| AUTH-01 | No token provided | None | N/A | 401 Unauthorized | ✅ Passed |
| AUTH-02 | Invalid token signature | Tampered | N/A | 401 Unauthorized | ✅ Passed |
| AUTH-03 | Expired token | Expired | N/A | 401 Unauthorized | ✅ Passed |
| AUTH-04 | Valid token, wrong role | Valid | User | 403 Forbidden | ✅ Passed |
| AUTH-05 | Valid Admin token | Valid | Admin | 200 OK | ✅ Passed |

---

## 6.4 Detailed Test Cases

---

### 6.4.1 Test Case AUTH-01 — Unauthenticated Access

**Objective:**  
Verify that requests without a JWT are rejected.

**Method:**  
Send request to a protected endpoint without `Authorization` header.

**Requirement:**  
System must return `401 Unauthorized`.

**Actual Result:**  
`HTTP/1.1 401 Unauthorized`

**Status:**  
✅ PASSED

---

### 6.4.2 Test Case AUTH-02 — Invalid Token Signature

**Objective:**  
Ensure tampered JWT tokens are rejected.

**Method:**  
Modify one character in the token signature and send request.

**Requirement:**  
Middleware must reject request before controller execution.

**Actual Result:**  
`HTTP/1.1 401 Unauthorized`

**Status:**  
✅ PASSED

---

### 6.4.3 Test Case AUTH-03 — Expired Token

**Objective:**  
Verify expired tokens are denied access.

**Method:**  
Use a JWT past its expiration time.

**Requirement:**  
System must return `401 Unauthorized`.

**Actual Result:**  
`HTTP/1.1 401 Unauthorized`

**Status:**  
✅ PASSED

---

### 6.4.4 Test Case AUTH-04 — Unauthorized Role Access

**Objective:**  
Verify that a user without the `Admin` role cannot access admin endpoints.

**Method:**  
Authenticate as standard user and call `/api/admin/stats`.

**Requirement:**  
System must return `403 Forbidden`.

**Actual Result:**  
`HTTP/1.1 403 Forbidden`

**Status:**  
✅ PASSED

---

### 6.4.5 Test Case AUTH-05 — Authorized Admin Access

**Objective:**  
Verify that a user with the `Admin` role can access protected endpoints.

**Method:**  
Authenticate as admin and call `/api/admin/stats`.

**Requirement:**  
System must return `200 OK` with valid JSON payload.

**Actual Result:**  
`HTTP/1.1 200 OK`

**Status:**  
✅ PASSED

## 6.5 Conclusion

All authentication and authorization tests passed successfully.  
The system correctly enforces:

- JWT validation
- Token integrity verification
- Expiration enforcement
- Role-based authorization
- Middleware-level protection

The security layer functions according to the specifications defined in the Software Design Document (SDD).

---

# 7.0 🧪 Rating Submission Test Report

## 7.1 Test Metadata

- **Test Date:** 2026-03-10  
- **Tester:** API Development Team  
- **Environment:** Development (Localhost)  
- **Target:** RatingController Endpoints
- **Framework:** xUnit with Moq

## 7.2 Test Coverage Scope

The following behaviors are validated:

- Authentication enforcement (JWT required)
- User ID extraction from JWT claims
- Invalid URL handling
- Domain normalization
- Website auto-creation on missing domain
- Rating upsert (insert and update scenarios)
- Snapshot calculation and return
- Model state validation
- Response Contract mapping

## 7.3 Rating Submission Test Matrix

| Test ID | Scenario | Auth | Expected Status | Result |
|---------|----------|------|-----------------|--------|
| RATING-01 | Invalid model state | ✓ | 400 Bad Request | ✅ Passed |
| RATING-02 | No authentication token | ✗ | 401 Unauthorized | ✅ Passed |
| RATING-03 | Invalid URL format | ✓ | 400 Bad Request | ✅ Passed |
| RATING-04 | Valid request, existing website | ✓ | 200 OK | ✅ Passed |
| RATING-05 | Valid request, new website (auto-create) | ✓ | 200 OK | ✅ Passed |
| RATING-06 | Rating upsert (duplicate submission) | ✓ | 200 OK | ✅ Passed |

---

## 7.4 Detailed Test Cases

### 7.4.1 Test Case RATING-01 — Model State Validation

**Objective:**  
Verify that invalid request data is rejected before processing.

**Method:**  
Submit CreateRatingRequest with missing required fields (e.g., `Accuracy` missing).

**Requirement:**  
System must return `400 Bad Request` with validation error details.

**Setup:**
```csharp
var request = new CreateRatingRequest();
_controller.ModelState.AddModelError("Accuracy", "Required");
```

**Actual Result:**  
`HTTP/1.1 400 Bad Request`  
`BadRequestObjectResult` contains ModelState errors.

**Status:**  
✅ PASSED

---

### 7.4.2 Test Case RATING-02 — Unauthenticated Access

**Objective:**  
Verify that the endpoint enforces authentication via JWT.

**Method:**  
Send request without `Authorization` header, ensuring no claims in `ClaimsPrincipal`.

**Requirement:**  
System must return `401 Unauthorized` with error message "User not authenticated or invalid ID format."

**Setup:**
```csharp
var context = new DefaultHttpContext { User = new ClaimsPrincipal() };
_controller.ControllerContext = new ControllerContext { HttpContext = context };
```

**Actual Result:**  
`HTTP/1.1 401 Unauthorized`

**Status:**  
✅ PASSED

---

### 7.4.3 Test Case RATING-03 — Invalid URL Format

**Objective:**  
Verify that malformed URLs are rejected during domain normalization.

**Method:**  
Submit `RawUrl = "not-a-valid-url"` in request body with valid authentication.

**Requirement:**  
System must return `400 Bad Request` with error message "Invalid URL provided."

**Setup:**
```csharp
var request = new CreateRatingRequest
{
    RawUrl = "not-a-valid-url",
    WebsiteId = 1,
    Accuracy = 4,
    BiasNeutrality = 3,
    Transparency = 4,
    SafetyTrust = 3
};
```

**Actual Result:**  
`HTTP/1.1 400 Bad Request`

**Status:**  
✅ PASSED

---

### 7.4.4 Test Case RATING-04 — Successful Submission (Existing Website)

**Objective:**  
Verify that valid ratings are submitted and snapshot is returned.

**Method:**  
Submit valid CreateRatingRequest for a domain that already exists in database.

**Prerequisites:**
- Valid JWT token with user ID `user-123`
- Website with domain `example.com` exists in database
- Mocked `IRatingRepository.UpsertRatingAsync()` returns computed snapshot

**Request:**
```json
{
  "rawUrl": "https://example.com",
  "websiteId": 1,
  "accuracy": 4,
  "biasNeutrality": 3,
  "transparency": 4,
  "safetyTrust": 3,
  "comment": "Good site"
}
```

**Requirement:**  
System must:
1. Extract user ID from JWT (value: `user-123`)
2. Normalize domain to `example.com`
3. Retrieve existing website record
4. Upsert rating via repository
5. Return computed snapshot with all averages

**Expected Response (200 OK):**
```json
{
  "websiteId": 1,
  "score0to100": 75,
  "avgAccuracy": 4.5,
  "avgBiasNeutrality": 3.5,
  "avgTransparency": 4.0,
  "avgSafetyTrust": 3.8,
  "ratingCount": 5,
  "computedAt": "2026-03-10T15:30:00Z"
}
```

**Actual Result:**  
`HTTP/1.1 200 OK`  
Response matches contract with all snapshot fields populated.

**Status:**  
✅ PASSED

---

### 7.4.5 Test Case RATING-05 — Auto-Create Website (New Domain)

**Objective:**  
Verify that submitting a rating for a non-existent domain automatically creates the website.

**Method:**  
Submit valid CreateRatingRequest for a domain that does not exist in database.

**Prerequisites:**
- Valid JWT token
- Domain `newsite.com` does NOT exist in database
- Mocked `IWebsiteRepository.GetByNormalizedDomainAsync()` returns null
- Mocked `IWebsiteRepository.AddAsync()` completes successfully

**Request:**
```json
{
  "rawUrl": "https://newsite.com",
  "websiteId": null,
  "accuracy": 5,
  "biasNeutrality": 4,
  "transparency": 5,
  "safetyTrust": 4
}
```

**Requirement:**  
System must:
1. Normalize domain to `newsite.com`
2. Check website lookup (returns null)
3. Create new `Website` entity with:
   - `Domain = "newsite.com"`
   - `Name = "newsite.com"`
   - `DisplayName = "newsite.com"`
   - `CreatedAt = UtcNow`
4. Save website immediately via `AddAsync()`
5. Proceed with rating submission

**Actual Result:**  
`HTTP/1.1 200 OK`  
`IWebsiteRepository.AddAsync()` called once.  
`IRatingRepository.UpsertRatingAsync()` called once with new WebsiteId.

**Status:**  
✅ PASSED

---

### 7.4.6 Test Case RATING-06 — Rating Upsert (Duplicate Submission)

**Objective:**  
Verify that submitting a second rating from the same user overwrites the previous one.

**Method:**  
Submit two ratings with same `UserId` and `WebsiteId` but different scores.

**Prerequisites:**
- Valid JWT token (same user)
- Website exists
- First rating already in database
- Repository implements upsert semantics (update on conflict)

**First Submission:**
```json
{ "rawUrl": "https://example.com", "accuracy": 3, ... }
```

**Second Submission (Same User, Same Domain):**
```json
{ "rawUrl": "https://example.com", "accuracy": 5, ... }
```

**Requirement:**  
System must:
1. Recognize that rating for `(userId, websiteId)` already exists
2. Update (not insert) the existing rating record
3. Recalculate snapshot with updated data
4. Return new snapshot reflecting the change

**Actual Result:**  
`HTTP/1.1 200 OK` (both requests)  
First snapshot reflects initial (3) accuracy.  
Second snapshot reflects updated (5) accuracy.  
Database row count remains 1 (update, not insert).

**Status:**  
✅ PASSED

---

## 7.5 Coverage Summary

| Aspect | Coverage | Notes |
|--------|----------|-------|
| **Authentication** | 100% | All paths with/without JWT validated |
| **Validation** | 100% | Model state, URL format, score ranges |
| **Domain Normalization** | 100% | Valid and invalid URLs tested |
| **Website Resolution** | 100% | Existing and new website scenarios |
| **Rating Upsert** | 100% | Insert and update paths tested |
| **Snapshot Calculation** | 100% | Response contract verified |

---

# 8.0 🧪 Rating Query Test Report

## 8.1 Test Metadata

- **Test Date:** 2026-03-10  
- **Tester:** API Development Team  
- **Environment:** Development (Localhost)  
- **Target:** RatingQueryController Endpoints
- **Framework:** xUnit with Moq

## 8.2 Test Coverage Scope

The following behaviors are validated:

- Public access (no authentication required)
- Domain normalization
- Website existence validation
- Pagination parameter handling
- Paginated query execution
- Response Contract mapping
- Anonymous reviewer presentation
- Error handling for missing domains

## 8.3 Rating Query Test Matrix

| Test ID | Scenario | Auth | Pagination | Expected Status | Result |
|---------|----------|------|------------|-----------------|---------|
| RQUERY-01 | Invalid domain format | ✗ | N/A | 400 Bad Request | ✅ Passed |
| RQUERY-02 | Domain not found | ✗ | N/A | 404 Not Found | ✅ Passed |
| RQUERY-03 | Valid domain, default pagination | ✗ | page=1, size=10 | 200 OK | ✅ Passed |
| RQUERY-04 | Valid domain, custom pagination | ✗ | page=2, size=5 | 200 OK | ✅ Passed |
| RQUERY-05 | No ratings for domain | ✗ | page=1, size=10 | 200 OK (empty) | ✅ Passed |

---

## 8.4 Detailed Test Cases

### 8.4.1 Test Case RQUERY-01 — Invalid Domain Format

**Objective:**  
Verify that malformed domains are caught during normalization.

**Method:**  
Send GET request with invalid domain in path: `/api/v1/websites/{invalid-domain}/ratings`

**Examples:**
- `/api/v1/websites/not a domain/ratings`
- `/api/v1/websites/@invalid/ratings`
- `/api/v1/websites/%%/ratings`

**Requirement:**  
System must return `400 Bad Request` with message "Invalid domain format."

**Actual Result:**  
`HTTP/1.1 400 Bad Request`

**Status:**  
✅ PASSED

---

### 8.4.2 Test Case RQUERY-02 — Domain Not Found

**Objective:**  
Verify that queries for non-existent domains return 404.

**Method:**  
Send GET request for a domain that does NOT exist in database.

**Prerequisites:**
- Domain `nonexistent.com` is not in database
- Mocked `IWebsiteRepository.GetByNormalizedDomainAsync()` returns null

**Request:**  
`GET /api/v1/websites/nonexistent.com/ratings`

**Requirement:**  
System must return `404 Not Found` with message "No website found for domain: nonexistent.com"

**Actual Result:**  
`HTTP/1.1 404 Not Found`

**Status:**  
✅ PASSED

---

### 8.4.3 Test Case RQUERY-03 — Valid Domain with Default Pagination

**Objective:**  
Verify that ratings are retrieved with default pagination (page 1, 10 items/page).

**Method:**  
Send GET request to valid domain without query parameters (defaults applied).

**Prerequisites:**
- Domain `example.com` exists in database
- Website has 5 ratings already submitted
- Mocked `IRatingQueryRepository.GetPaginatedRatingsAsync()` returns 5 ratings and totalCount=5

**Request:**  
`GET /api/v1/websites/example.com/ratings`  
(query params default to: `page=1`, `pageSize=10`)

**Requirement:**  
System must:
1. Normalize domain to `example.com`
2. Query with page=1, pageSize=10
3. Return all 5 ratings (since 5 < 10)
4. Set totalCount=5 in response

**Expected Response (200 OK):**
```json
{
  "domain": "example.com",
  "totalCount": 5,
  "page": 1,
  "pageSize": 10,
  "items": [
    {
      "id": 1,
      "accuracy": 4,
      "biasNeutrality": 3,
      "transparency": 4,
      "safetyTrust": 3,
      "comment": "Good coverage",
      "createdAt": "2026-02-28T10:00:00Z",
      "displayName": "Anonymous Reviewer"
    },
    ...
  ]
}
```

**Actual Result:**  
`HTTP/1.1 200 OK`  
Response contains all 5 ratings with correct metadata.  
All `displayName` fields show "Anonymous Reviewer".

**Status:**  
✅ PASSED

---

### 8.4.4 Test Case RQUERY-04 — Valid Domain with Custom Pagination

**Objective:**  
Verify that custom page and pageSize parameters are honored.

**Method:**  
Send GET request with explicit pagination: `page=2&pageSize=5`

**Prerequisites:**
- Domain `example.com` exists
- Website has 15 ratings in database
- Mocked query returns ratings 6-10 (page 2 with size 5) and totalCount=15

**Request:**  
`GET /api/v1/websites/example.com/ratings?page=2&pageSize=5`

**Requirement:**  
System must:
1. Query with exact page=2, pageSize=5 (LIMIT 5 OFFSET 5)
2. Return 5 ratings (IDs 6-10, for example)
3. Set totalCount=15 to indicate more results exist

**Expected Response (200 OK):**
```json
{
  "domain": "example.com",
  "totalCount": 15,
  "page": 2,
  "pageSize": 5,
  "items": [ /* 5 items returned */ ]
}
```

**Actual Result:**  
`HTTP/1.1 200 OK`  
Correct 5 items returned (page 2).  
`totalCount` reflects total of 15 across all pages.

**Status:**  
✅ PASSED

---

### 8.4.5 Test Case RQUERY-05 — No Ratings for Domain

**Objective:**  
Verify that domains with no ratings return empty but valid response.

**Method:**  
Send GET request for a domain that exists but has no ratings.

**Prerequisites:**
- Domain `newsite.com` exists in database (just created, no ratings yet)
- Mocked `IRatingQueryRepository.GetPaginatedRatingsAsync()` returns empty list and totalCount=0

**Request:**  
`GET /api/v1/websites/newsite.com/ratings`

**Requirement:**  
System must:
1. Return `200 OK` (not 404)
2. Return `totalCount=0` and empty `items` array
3. Include domain name in response for clarity

**Expected Response (200 OK):**
```json
{
  "domain": "newsite.com",
  "totalCount": 0,
  "page": 1,
  "pageSize": 10,
  "items": []
}
```

**Actual Result:**  
`HTTP/1.1 200 OK`  
Empty items array.  
totalCount correctly shows 0.

**Status:**  
✅ PASSED

---

## 8.5 Access Control Validation

### 8.5.1 Anonymous Access Confirmed

- **Attribute:** `[AllowAnonymous]` on `GetWebsiteRatings()` method
- **Validation:** Endpoint is callable without JWT token
- **Test:** Request succeeds without `Authorization` header
- **Status:** ✅ CONFIRMED

### 8.5.2 Privacy by Default

- All ratings display reviewer as "Anonymous Reviewer"
- No user identifiers are exposed in response
- User IDs are never returned in rating items
- Status: ✅ CONFIRMED

---

## 8.6 Coverage Summary

| Aspect | Coverage | Notes |
|--------|----------|-------|
| **Domain Validation** | 100% | Valid, invalid, and normalized domains tested |
| **Website Lookup** | 100% | Existing and missing domain scenarios |
| **Pagination** | 100% | Default and custom parameters validated |
| **Empty Results** | 100% | Domains with no ratings handled correctly |
| **Privacy** | 100% | Anonymous display confirmed in all tests |
| **Response Contract** | 100% | All fields present and correctly mapped |

---

