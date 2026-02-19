
## 📡 API Documentation (api.md)
### 🛠 Base Configuration
All API requests should be made to the following base URL depending on the environment:

Development (WSL2): http://localhost:5000/api

Production: https://api.credibilityindex.com/v1

### 📂 Category Management
Endpoints related to the organization and retrieval of website categories.

GET /categories
Description: Retrieves a list of all active website categories.

Auth Required: No

Response:

JSON
```
[
  { "id": "uuid", "name": "News", "slug": "news" },
  { "id": "uuid", "name": "Technology", "slug": "tech" }
]
```
### ⚖️ Credibility Scoring
Endpoints for calculating and retrieving trust scores for domains.

POST /scoring/evaluate
Description: Triggers a new credibility evaluation for a specific domain.

Auth Required: Yes (JWT)

Request Body:

JSON
```
{
  "url": "https://example.com",
  "depth": "standard"
}
```
Response: 201 Created

GET /scoring/{domain}
Description: Returns the cached credibility score for a given domain.

Auth Required: No

Success Response: 200 OK

JSON
```
{
  "domain": "example.com",
  "score": 85,
  "rating": "High Trust",
  "lastUpdated": "2024-05-20T10:00:00Z"
}
```
### 🛡 Error Handling
The API uses standard HTTP status codes:

400 Bad Request: Validation failed (e.g., invalid URL format).

401 Unauthorized: Missing or invalid JWT token.

404 Not Found: The requested category or domain does not exist.

500 Server Error: Internal system failure.

## Authentication Service (OpenIddict)

The system uses OpenIddict to provide secure JWT tokens. Currently, only the Password Grant is supported. NOW  Browser-based redirects and interactive logins are NOT disabled for security.

### Token Endpoint
**URL:** `POST /connect/token`  
**Authentication:** Password grant type 
**Format:** `application/x-www-form-urlencoded`

#### Request Body
| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| grant_type | string | Yes | Must be password. |
| username | string | Yes | The user's email address. |
| password | string | Yes | The user's account password. |
| scope | string | No | Optional permissions (e.g., openid profile email). |

### Example Request (cURL)
```bash
curl -X POST https://api.credibility-index.com/api/auth/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&username=user@example.com&password=SecretPassword123"
```

#### Success Response (200 OK)
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

### Validation Behavior:

* The system checks the username against both Email and UserName fields.

* Lockout: After multiple failed attempts, the account will be temporarily locked (lockoutOnFailure: true).

* Error Response: For security, any invalid credential or user error returns a 403 Forbidden via the OpenIddict challenge.

# API Authentication Guide
This API uses **OAuth 2.0 with JWT Bearer Tokens** to secure protected endpoints.
Authentication and authorization are handled via **OpenIddict**.

To access protected resources, clients must include a valid **JSON Web Token (JWT)** in the Authorization header of every HTTP request.

## Header Format
``` https
Authorization: Bearer <YOUR_JWT_TOKEN>
```
## Example Request (curl)
``` bash
curl -X GET "http://localhost:5149/api/admin/stats" \
     -H "Authorization: Bearer eyJhbGciOiJSUzI1..."
```
# 🛠 Obtaining a Token
Tokens are obtained by sending a `POST` request to the token endpoint using the Resource Owner Password Credentials grant.

## Endpoint
```
/connect/token
```
## Headers
```https
Content-Type: application/x-www-form-urlencoded
```
## Parameters

|Parameter  |	Value|
|grant_type  |	password|
|username  |	Your email address|
|password	 |Your account password|
|client_id  |	mvp-client|
|scope  |	openid profile email roles|

## Example Token Request (curl)

```bash
curl -X POST "http://localhost:5149/connect/token" \
     -H "Content-Type: application/x-www-form-urlencoded" \
     -d "grant_type=password" \
     -d "username=your@email.com" \
     -d "password=yourpassword" \
     -d "client_id=mvp-client" \
     -d "scope=openid profile email roles"
```

## 🚦 Expected Error Responses:

* [Error Responses](./api.md) - API validates every request before it reaches the business logic layer.