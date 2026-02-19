
## 🧪 Unit Test Report (UTR)

### 🧪 Testing Strategy
The Credibility Index project implements a comprehensive testing strategy to ensure system reliability and code quality.
* **Backend Testing**: We utilize **xUnit/NUnit** for the **.NET 10.0 Web API** to verify Domain entities and Application use cases.
* **Frontend Testing**: We utilize **Karma/Jasmine** for the **Angular UI** (located in `ui/credibility-ui`) to ensure component stability.


### 🎯 Coverage Goals
To maintain project standards, we have established the following baseline for code quality:
* **Overall Coverage**: Aiming for **80% coverage** across the entire solution.
* **Critical Logic**: Aiming for **100% coverage on Domain logic** and scoring algorithms.

### 📊 Test Results
Below is the summary of the latest test execution from the CI/CD pipeline.

| Project | Tests Passed | Tests Failed | Coverage % | Status |
| :--- | :--- | :--- | :--- | :--- |
| **CredibilityIndex.Api** | 0 | 0 | 0% | 🟡 Pending |
| **CredibilityIndex.Domain** | 0 | 0 | 0% | 🟡 Pending |
| **credibility-ui** | 0 | 0 | 0% | 🟡 Pending |

## 🧪 Security & Authentication Test Report

* **Test Date:** 2026-02-19

* **Tester:** API Development Team

* **Environment:** Development (Localhost)

* **Target:** AdminController Endpoints

## 1. Test Case: Unauthenticated Access (401)
* **Objective:** Verify that requests without a valid JWT are rejected.

* **Method:** curl -i -X GET http://localhost:5149/api/admin/stats (No Header).

* **Requirement:** API must return 401 Unauthorized.

* **Actual Result:** HTTP/1.1 401 Unauthorized.
```https
WWW-Authenticate: Bearer error="invalid_token"
```

* **Status:** ✅ PASSED

## 2. Test Case: Unauthorized Role Access (403)
* **Objective:** Verify that a valid user without the "Admin" role cannot access admin data.

* **Method: 1.**  Login as user@test.com.
* **Method: 2.**  Use returned token to call GET /api/admin/users.

* **Requirement:** API must return 403 Forbidden.

* **Actual Result:** HTTP/1.1 403 Forbidden.
```json
{
  "error": "Forbidden",
  "message": "You do not have permission to access this resource."
}
```
* **Status:** ✅ PASSED

## 3. Test Case: Authorized Admin Access (200)
* **Objective:** Verify that a user with the "Admin" role can retrieve protected data.

* **Method: 1.**  Login as admin@credibility.com.
  **Mehtod: 2.**  Use returned token to call GET /api/admin/stats.

* **Requirement:** API must return 200 OK with valid JSON payload.

* **Actual Result:** HTTP/1.1 200 OK.

* **Payload:** {"totalUsers": 5, "serverStatus": "Healthy" ...}

* **Status:** ✅ PASSED

## 4. Test Case: Invalid Token/Signature Rejection
* **Objective:** Verify the middleware rejects tokens that have been tampered with.

* **Method:** Manually change one character in the JWT signature and send request.

* **Requirement:** Middleware must reject the signature before reaching the controller.

* **Actual Result:** HTTP/1.1 401 Unauthorized.

* **Status:** ✅ PASSED