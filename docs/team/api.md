
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
