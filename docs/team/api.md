
# 1.0 📡 API Documentation (api.md)
# 2.0 🛠 Base Configuration

## 2.1 Environments
All API requests should be made to the following base URL depending on the environment:

### 2.1.1 Development
Development:http://localhost:5000/api

### 2.1.2 Production
Production: https://api.credibilityindex.com/v1

# 3.0 📂 Category Management
Endpoints related to the organization and retrieval of website categories.

## 3.1 GET /categories

### 3.1.1 Description:
Retrieves a list of all active website categories.

### 3.1.2 Auth Required: 
No

### 3.1.3 Success Response (200 OK)

JSON
```
[
  { "id": "uuid", "name": "News", "slug": "news" },
  { "id": "uuid", "name": "Technology", "slug": "tech" }
]
```
# 4.0⚖️ Credibility Scoring
Endpoints for calculating and retrieving trust scores for domains.

## 4.1 POST /scoring/evaluate

### 4.1.1 Description:
Triggers a new credibility evaluation for a specific domain.

### 4.1.2 Auth Required:
Yes (JWT)

### 4.1.3 Request Body:

JSON
```
{
  "url": "https://example.com",
  "depth": "standard"
}
```
### 4.1.4 Success Response:
201 Created

## 4.2 GET /scoring/{domain}

### 4.2.1 Description:
Returns the cached credibility score for a given domain.

### 4.2.2 Auth Required:
No

### 4.2.3 Success Response:
200 OK

JSON
```
{
  "domain": "example.com",
  "score": 85,
  "rating": "High Trust",
  "lastUpdated": "2024-05-20T10:00:00Z"
}
```
# 4.0 🌐 Website Lookup

## 4.1 GET /websites/{id}

### 4.1.1 Description:
Return detailed metadata about a single website, including its current credibility snapshot if one has been computed. Snapshot data is nullable – endpoints consuming this response should handle a null value when ratings have not yet been submitted.

### 4.1.2 Auth Required:
No

### 4.1.3 Success Response (200 OK) – example:
```json
{
  "id": 123,
  "name": "Example Site",
  "domain": "example.com",
  "description": "A sample website",
  "isActive": true,
  "category": { "id": 10, "name": "News" },
  "snapshot": {
    "score": 75,
    "avgAccuracy": 3.5,
    "avgBiasNeutrality": 4.0,
    "avgTransparency": 2.8,
    "avgSafetyTrust": 3.9,
    "ratingCount": 42,
    "computedAt": "2026-02-28T12:34:56Z"
  }
}
```

If the website has no snapshot yet the `snapshot` field will be `null`.

---

# 5.0 🛡 Error Handling

## 5.1 Standard HTTP Status Codes
The API uses standard HTTP status codes:

* **400 Bad Request:** Validation failed (e.g., invalid URL format).

* **401 Unauthorized:** Missing or invalid JWT token.

* **404 Not Found:** The requested category or domain does not exist.

* **500 Server Error:** Internal system failure.

# 6.0 Authentication Service (OpenIddict)

The system uses OpenIddict to provide secure JWT tokens. Currently, only the Password Grant is supported. NOW  Browser-based redirects and interactive logins are NOT disabled for security.

## 6.1 OpenIdToken Endpoint

**URL:** `POST /connect/token`  
**Authentication:** Password grant type 
**Format:** `application/x-www-form-urlencoded`

### 6.1.1 Request Body
| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| grant_type | string | Yes | Must be password. |
| username | string | Yes | The user's email address. |
| password | string | Yes | The user's account password. |
| scope | string | No | Optional permissions (e.g., openid profile email). |

### 6.1.2 Example Request (cURL)

```bash
curl -X POST https://api.credibility-index.com/api/auth/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=user@example.com&password=SecretPassword123"
```

### 6.1.3 Success Response (200 OK)
```json
{
    "access_token": "eyJhbG...",
    "token_type": "Bearer",
    "expires_in": 3600,
    "refresh_token": "..."
}
```
**Failure (400 Bad Request):** To prevent User Enumeration, the system returns a generic invalid_grant error regardless of whether the email is missing or the password is wrong.

```JSON
{
  "error": "invalid_grant",
  "error_description": "The username/password couple is invalid."
}
```

## 6.2 Validation Behavior:

### 6.2.1 Username Validation

The system checks the username against both Email and UserName fields.

### 6.2.2 Lockout Policy

Lockout: After multiple failed attempts, the account will be temporarily locked `(lockoutOnFailure: true)`.

### 6.6.3 Error Response Policy

Error Response: For security, any invalid credential or user error returns a `403 Forbidden` via the OpenIddict challenge.

# 7.0 API Authentication Guide

This API uses **OAuth 2.0 with JWT Bearer Tokens** to secure protected endpoints.
Authentication and authorization are handled via **OpenIddict**.

To access protected resources, clients must include a valid **JSON Web Token (JWT)** in the Authorization header of every HTTP request.

## 7.1 Header Format

``` https
Authorization: Bearer <YOUR_JWT_TOKEN>
```
## 7.2 Example Authenticated Request (cURL)
``` bash
curl -X GET "http://localhost:5149/api/admin/stats" \
     -H "Authorization: Bearer eyJhbGciOiJSUzI1..."
```
# 8.0 🛠 Obtaining a Token
Tokens are obtained by sending a `POST` request to the token endpoint using the Resource Owner Password Credentials grant.

## 8.1 Token Endpoint
```
/connect/token
```
## 8.2 Request Headers
```https
Content-Type: application/x-www-form-urlencoded
```
## 8.3 Request Parameters

|Parameter  |	Value|
|grant_type  |	password|
|username  |	Your email address|
|password	 |Your account password|
|client_id  |	mvp-client|
|scope  |	openid profile email roles|

## 8.4 Example Token Request (curl)

```bash
curl -X POST "http://localhost:5149/connect/token" \
     -H "Content-Type: application/x-www-form-urlencoded" \
     -d "grant_type=password" \
     -d "username=your@email.com" \
     -d "password=yourpassword" \
     -d "client_id=mvp-client" \
     -d "scope=openid profile email roles"
```

## 8.5🚦 Expected Error Responses:

* [Error Responses](./api.md) - API validates every request before it reaches the business logic layer.

# 9.0 📑 Category API Documentation

This section defines the interface for managing categories within the **Credibility Index**.  
These endpoints allow the **Management UI** and **Chrome Extension** to interact with the underlying `CategoryRepository`.

## 9.1 1️⃣ Endpoints Summary

| Method | Endpoint | Description | Auth Required |
|--------|----------|------------|---------------|
| GET | `/` | Retrieve all categories | No |
| GET | `/active` | Retrieve only enabled categories | No |
| POST | `/` | Create a new category | Yes (Admin) |
| PUT | `/{id}` | Update Name/Description | Yes (Admin) |
| PATCH | `/{id}/toggle` | Flip `isActive` status | Yes (Admin) |
| DELETE | `/{id}` | Permanent removal | Yes (Admin) |

---

## 9.2 2️⃣ Detailed Endpoint Specification

### 9.2.1 🔍 GET `/active`

Fetches all categories where `isActive` is `true`.  
Primarily used by the Chrome Extension for filtering content.

### 9.2.2 ✅ Response (200 OK)

```json
[
  {
    "id": 1,
    "name": "Politics",
    "description": "News related to government and policy",
    "isActive": true
  }
]
```
## 9.3 🔄 PATCH `/{id}/toggle`

### 9.3.1 Description
Toggles the current status of a category.
If `isActive` was `true`, it becomes `false`, and vice versa.
This calls the `ToggleStatusAsync` method in the infrastructure layer.

#### 9.3.2 ✅ Response (200 OK)

```json
{
  "id": 1,
  "name": "Politics",
  "newStatus": false
}
```
## 9.4 ➕ POST `/`

Creates a new category entry in the system.

### 9.4.1 📥 Request Body

``` json
{
  "name": "Science",
  "description": "Research and technological advancement"
}
```
### 9.4.2 ✅ Response (201 Created)

Returns the created object including its database-generated ID.

``` json
{
  "id": 5,
  "name": "Science",
  "description": "Research and technological advancement",
  "isActive": true
}
```
## 9.5 ✏️ PUT `/{id}`

Updates an existing category’s `name` and/or `description`.

### 9.5.1 📥 Request Body

``` json
{
  "name": "Updated Name",
  "description": "Updated description"
}
```
### 9.5.2 ✅ Response (200 OK)

```json
{
  "id": 1,
  "name": "Updated Name",
  "description": "Updated description",
  "isActive": true
}
```
## 9.6 ❌ DELETE `/{id}`

Permanently removes a category from the database.

### 9.6.1 ✅ Response (204 No Content)

No body returned.

## 9.73️⃣ Error Handling

The API adheres to standard HTTP status codes to communicate processing results:

| Status Code                   | Meaning            | Common Cause                                   |
|--------------------------------|--------------------|----------------------------------------------|
| **400 Bad Request**            | Validation Error   | Missing required fields (e.g., `name` is null) |
| **401 Unauthorized**           | Auth Error         | Missing or invalid Admin JWT token             |
| **404 Not Found**              | Resource Error     | The requested Category ID does not exist       |
| **500 Internal Server Error**  | Server Error       | Unexpected database failure or timeout         |

## 9.8 🔐 Authentication Notes

### 9.8.1 Admin Protection

 * Admin-protected endpoints require a valid JWT token.

 * The token must be included in the Authorization header:
 
### 9.8.2 Authorization Header Format

 ``` code
 Authorization: Bearer <your-admin-token>
 ```
# 10.0 Authorization Requirements
    `POST /api/v1/categories`

## 10.1 Requirement:
 Authenticated

## 10.2 Role:
 Admin

## 10.3 Header:
 Authorization: Bearer <JWT_TOKEN>

## 10.4 Request Specification

**Request Body (JSON)**

| Field       | Type    | Constraints                         | Description                                                     |
|------------|---------|-------------------------------------|-----------------------------------------------------------------|
| name       | string  | 3–30 chars, Required                | The display name for the category.                              |
| slug       | string  | 3–30 chars, Required                | URL-friendly unique identifier (lowercase, hyphens).            |
| description| string  | Max 500 chars                       | Metadata describing what content fits here.                     |
| isActive   | boolean | Default: true                       | Set to false to hide the category immediately.                  |

## 10.5 Example Request

```JSON
{
  "name": "Artificial Intelligence",
  "slug": "ai-news",
  "description": "Latest updates in machine learning and neural networks.",
  "isActive": true
}
```
### 10.6 Response

**Success:** `201 Created`

```JSON
{
  "id": 105,
  "name": "Artificial Intelligence",
  "slug": "ai-news",
  "description": "Latest updates in machine learning and neural networks.",
  "isActive": true
}
```

---

# 11.0 ⭐ Rating Submission and Retrieval

This section documents the endpoints for submitting user credibility assessments and retrieving aggregated ratings for domains.

## 11.1 🎯 Endpoints Summary

| Method | Endpoint | Description | Auth Required | Public |
|--------|----------|------------|---------------|--------|
| POST | `/websites` | Submit a credibility rating | Yes (JWT) | No |
| GET | `/websites/{domain}/ratings` | Retrieve paginated ratings | No | Yes |

---

## 11.2 📤 POST /websites — Submit Rating

### 11.2.1 Description

Allows authenticated users to submit credibility assessments for a website. The endpoint:

1. Validates user authentication (JWT required)
2. Auto-creates website entries if domain doesn't exist
3. Implements upsert logic (update if user already rated this domain)
4. Auto-calculates credibility snapshot (aggregate scores)
5. Returns the updated snapshot for immediate UI feedback

### 11.2.2 Authentication Required

**Yes** — Valid JWT Bearer token in `Authorization` header

```
Authorization: Bearer <JWT_TOKEN>
```

### 11.2.3 Route

```
POST /api/v1/websites
Content-Type: application/json
```

### 11.2.4 Request Body Contract (CreateRatingRequest)

| Field | Type | Range | Required | Description |
|-------|------|-------|----------|-------------|
| `rawUrl` | string | Any valid URL | Yes | The website domain to rate (e.g., `https://example.com`) |
| `websiteId` | integer | > 0 | Yes | The database ID of the website (auto-filled if new domain) |
| `accuracy` | integer | 1–5 | Yes | Rating for factual accuracy (1=low, 5=high) |
| `biasNeutrality` | integer | 1–5 | Yes | Rating for neutrality/bias (1=biased, 5=neutral) |
| `transparency` | integer | 1–5 | Yes | Rating for content transparency (1=opaque, 5=transparent) |
| `safetyTrust` | integer | 1–5 | Yes | Rating for website safety/trustworthiness (1=unsafe, 5=safe) |
| `comment` | string | 0–1000 chars | No | Optional user feedback or review (max 1000 characters) |

### 11.2.5 Example Request

```json
{
  "rawUrl": "https://techcrunch.com",
  "websiteId": 42,
  "accuracy": 4,
  "biasNeutrality": 3,
  "transparency": 4,
  "safetyTrust": 4,
  "comment": "Generally accurate tech news, occasional opinion pieces"
}
```

### 11.2.6 Success Response (200 OK)

Returns the updated credibility snapshot after processing the rating.

**Response Contract (UpdatedSnapshotResponse):**

| Field | Type | Description |
|-------|------|-------------|
| `websiteId` | integer | ID of the rated website |
| `score0to100` | double | Aggregate score (0–100 scale) calculated from all ratings |
| `avgAccuracy` | double | Average accuracy rating across all users (1.0–5.0) |
| `avgBiasNeutrality` | double | Average bias/neutrality rating (1.0–5.0) |
| `avgTransparency` | double | Average transparency rating (1.0–5.0) |
| `avgSafetyTrust` | double | Average safety/trust rating (1.0–5.0) |
| `ratingCount` | integer | Total number of ratings for this website |
| `computedAt` | ISO 8601 datetime | Timestamp when snapshot was recalculated (UTC) |

```json
{
  "websiteId": 42,
  "score0to100": 87.5,
  "avgAccuracy": 4.2,
  "avgBiasNeutrality": 3.8,
  "avgTransparency": 4.1,
  "avgSafetyTrust": 4.3,
  "ratingCount": 12,
  "computedAt": "2026-03-10T15:45:30Z"
}
```

### 11.2.7 Error Responses

| HTTP Status | Error Condition | Message |
|------------|-----------------|---------|
| **400 Bad Request** | Invalid URL format | `"Invalid URL provided."` |
| **400 Bad Request** | Rating scores out of range | Validation error from `[Range(1, 5)]` |
| **400 Bad Request** | Comment exceeds 1000 chars | `"Comment cannot exceed 1000 characters."` |
| **401 Unauthorized** | No JWT token present | `"User not authenticated or invalid ID format."` |
| **401 Unauthorized** | Invalid JWT token | Standard authentication error |
| **422 Unprocessable Entity** | Database constraint violation | Conflicting website data |
| **500 Internal Server Error** | Snapshot calculation failure | Server-side processing error |

### 11.2.8 Request/Response Lifecycle

```
1. Angular UI sends POST request with rating and JWT
   ↓
2. Middleware validates JWT (OpenIddict)
   ↓
3. [Authorize] attribute confirms authentication
   ↓
4. Controller extracts user ID from JWT "sub" claim
   ↓
5. URL is normalized to standard domain format
   ↓
6. Website lookup: Does "example.com" exist?
   - YES → Reuse existing Website entity
   - NO  → Auto-create new Website entity
   ↓
7. Rating is upserted (insert or update based on userId+websiteId)
   ↓
8. Snapshot Calculator computes aggregates:
   - Average of all accuracy ratings
   - Average of all bias/neutrality ratings
   - Average of all transparency ratings
   - Average of all safety/trust ratings
   - Overall score (0–100 scale)
   ↓
9. Updated snapshot is cached in database
   ↓
10. Return 200 OK with snapshot for immediate UI feedback
```

### 11.2.9 Validation Rules

- **URL Normalization:** `https://TechCrunch.com`, `techcrunch.com`, `TECHCRUNCH.COM` all normalize to `techcrunch.com`
- **User Identification:** Extracted from JWT `sub` (Subject) claim
- **Upsert Behavior:** If same user rates domain twice, first rating is updated (not duplicated)
- **Score Ranges:** All ratings must be integers 1–5 (inclusive)
- **Comments:** Optional, but if provided, must be <= 1000 characters

### 11.2.10 Usage Example (cURL)

```bash
curl -X POST "http://localhost:5000/api/v1/websites" \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIs..." \
  -H "Content-Type: application/json" \
  -d '{
    "rawUrl": "https://techcrunch.com",
    "websiteId": 42,
    "accuracy": 4,
    "biasNeutrality": 3,
    "transparency": 4,
    "safetyTrust": 4,
    "comment": "Good tech coverage"
  }'
```

---

## 11.3 📥 GET /websites/{domain}/ratings — Retrieve Ratings

### 11.3.1 Description

Retrieves paginated credibility ratings for a specific domain. This endpoint is public (no authentication required) to enable sharing and visibility of community assessments.

Features:
1. Domain normalization (case-insensitive, standardized to lowercase)
2. Paginated results (configurable page size)
3. Privacy-by-default (reviewers shown as "Anonymous Reviewer")
4. Fast read-only queries optimized for performance

### 11.3.2 Authentication Required

**No** — Public endpoint with `[AllowAnonymous]` attribute

### 11.3.3 Route

```
GET /api/v1/websites/{domain}/ratings?page=1&pageSize=10
```

### 11.3.4 Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `domain` | string | The domain to query ratings for (e.g., `example.com`, `techcrunch.com`) |

### 11.3.5 Query Parameters

| Parameter | Type | Default | Range | Description |
|-----------|------|---------|-------|-------------|
| `page` | integer | 1 | ≥ 1 | Which page of results to retrieve (1-indexed) |
| `pageSize` | integer | 10 | 1–100 | Number of ratings per page |

### 11.3.6 Example Requests

**Request 1 — Default Pagination (first 10 results):**
```
GET /api/v1/websites/example.com/ratings
```

**Request 2 — Custom Page and Size:**
```
GET /api/v1/websites/example.com/ratings?page=2&pageSize=5
```

**Request 3 — Large Page Size:**
```
GET /api/v1/websites/bbc.com/ratings?page=1&pageSize=50
```

### 11.3.7 Success Response (200 OK)

Returns a paginated list of ratings for the requested domain.

**Response Contract (PaginatedRatingsResponse):**

| Field | Type | Description |
|-------|------|-------------|
| `domain` | string | The normalized domain that was queried |
| `totalCount` | integer | Total number of ratings for this domain (across all pages) |
| `page` | integer | Current page number requested |
| `pageSize` | integer | Ratings per page |
| `items` | array | Array of `RatingItemResponse` objects |

**RatingItemResponse Structure:**

| Field | Type | Description |
|-------|------|-------------|
| `id` | integer | Database ID of the rating |
| `accuracy` | integer | Accuracy score (1–5) submitted by reviewer |
| `biasNeutrality` | integer | Bias/neutrality score (1–5) |
| `transparency` | integer | Transparency score (1–5) |
| `safetyTrust` | integer | Safety/trust score (1–5) |
| `comment` | string\|null | Optional review text (null if not provided) |
| `createdAt` | ISO 8601 datetime | When the rating was submitted (UTC) |
| `displayName` | string | Always "Anonymous Reviewer" for privacy |

```json
{
  "domain": "example.com",
  "totalCount": 23,
  "page": 1,
  "pageSize": 10,
  "items": [
    {
      "id": 101,
      "accuracy": 5,
      "biasNeutrality": 4,
      "transparency": 4,
      "safetyTrust": 5,
      "comment": "Excellent journalism standards",
      "createdAt": "2026-02-28T10:15:00Z",
      "displayName": "Anonymous Reviewer"
    },
    {
      "id": 102,
      "accuracy": 3,
      "biasNeutrality": 2,
      "transparency": 3,
      "safetyTrust": 4,
      "comment": "Some opinion pieces mixed in",
      "createdAt": "2026-02-28T14:22:30Z",
      "displayName": "Anonymous Reviewer"
    }
  ]
}
```

### 11.3.8 Empty Results (200 OK)

If a domain exists but has no ratings yet, the endpoint returns an empty `items` array:

```json
{
  "domain": "newsite.com",
  "totalCount": 0,
  "page": 1,
  "pageSize": 10,
  "items": []
}
```

### 11.3.9 Error Responses

| HTTP Status | Error Condition | Message |
|------------|-----------------|---------|
| **400 Bad Request** | Invalid domain format | `"Invalid domain format."` |
| **404 Not Found** | Domain doesn't exist in DB | `"No website found for domain: example.com"` |
| **500 Internal Server Error** | Database query failure | Server-side error |

### 11.3.10 Request/Response Lifecycle

```
1. Angular UI or Chrome Extension sends GET request
   (No JWT required, public access)
   ↓
2. Domain from URL is normalized (e.g., "EXAMPLE.COM" → "example.com")
   ↓
3. Website lookup: Does this domain exist?
   - NO  → Return 404 Not Found
   - YES → Continue
   ↓
4. Query paginated ratings from read-only repository
   - Database: SELECT * FROM Ratings
     WHERE WebsiteId = X
     LIMIT {pageSize} OFFSET (page-1)*pageSize
   ↓
5. Map each Rating entity to RatingItemResponse
   - Set displayName to "Anonymous Reviewer"
   - Include all score fields
   - Include comment (if exists)
   ↓
6. Calculate pagination metadata:
   - Total count of all ratings for domain
   - Current page number
   - Page size
   ↓
7. Return 200 OK with PaginatedRatingsResponse
   - Contains domain, totalCount, page, pageSize, items
```

### 11.3.11 Pagination Behavior

**Scenario 1: Domain has 23 ratings, pageSize=10**

| Page | OFFSET | LIMIT | Results |
|------|--------|-------|---------|
| 1 | 0 | 10 | Items 1–10 (shown) |
| 2 | 10 | 10 | Items 11–20 (shown) |
| 3 | 20 | 10 | Items 21–23 (shown) |
| 4 | 30 | 10 | No results (404 equivalent, but return 200 with empty items) |

**totalCount always = 23** across all page requests

### 11.3.12 Privacy By Design

- **Reviewer Identity:** Never exposed
- **Display Name:** Always "Anonymous Reviewer"
- **User Relationships:** No links to user accounts in response
- **Timestamps:** Only submission time shown, not user creation time

### 11.3.13 Usage Examples (cURL)

**Retrieve first page of ratings:**
```bash
curl -X GET "http://localhost:5000/api/v1/websites/example.com/ratings" \
  -H "Accept: application/json"
```

**Retrieve second page with custom size:**
```bash
curl -X GET "http://localhost:5000/api/v1/websites/example.com/ratings?page=2&pageSize=5" \
  -H "Accept: application/json"
```

**Retrieve from publicly accessible domain:**
```bash
curl -X GET "https://api.credibilityindex.com/v1/websites/bbc.com/ratings?page=1&pageSize=20"
```

---

## 11.4 📊 Rating Aggregation via Snapshot

Both endpoints work with the **Credibility Snapshot** system:

| Component | Role |
|-----------|------|
| **POST /websites** | Triggers snapshot recalculation after each rating submission |
| **GET /websites/{domain}/ratings** | Returns individual ratings that feed the snapshot |
| **CredibilitySnapshot** entity | Stores cached aggregates: Average accuracy, bias, transparency, safety, and overall score (0–100) |

### 11.4.1 Usage Patterns

**For UI Display:**
- Show snapshot summary at the top (overall score, counts)
- Show individual ratings below for detailed reviews

**For Chrome Extension:**
- Query snapshot for quick domain assessment
- Query individual ratings for detailed breakdown

**For Analytics:**
- Track rating volume (via `totalCount`)
- Monitor snapshot score changes over time

---
