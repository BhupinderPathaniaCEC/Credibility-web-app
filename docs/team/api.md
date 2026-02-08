
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