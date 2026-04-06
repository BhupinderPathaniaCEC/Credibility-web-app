# 1.0 Credibility Index

The **Credibility Index** is a full-stack solution designed with a **Clean Architecture backend (.NET 10)** and a modern **Angular frontend**.  
It provides secure user registration, real-time website credibility scoring, and organized category management.

This repository serves as the central hub for development, documentation, and deployment.

---

# 2.0 Development Environment

This project is optimized for development inside **WSL2 (Windows Subsystem for Linux)** with Ubuntu for high performance and Linux-based tooling compatibility.

### 2.1 Quick Start

For detailed setup instructions, see the [Developer Onboarding Guide](./docs/onboarding.md).

### 2.2 Enable WSL2 and Ubuntu

If you do not have WSL2 installed, run this command in **PowerShell (as Administrator)**

```powershell
wsl --install -d Ubuntu
```

# 3.0 Documentation Index

The following documentation provides in-depth guidance for different aspects of the project:

### 3.1 Design & Architecture

- [Software Design Document (SDD)](./docs/qms/sdd.md) – Architecture, request lifecycles, and security design.  
- [Architecture Decision Records (ADR)](./docs/decision-records/ADR-001-architecture.md) – Detailed architectural decisions.  
- [Database Schema ADR](./docs/decision-records/ADR-002-database.md) – Database design and decisions.

### 3.2 API & Integration

- [API Documentation](./docs/team/api.md) – Endpoints, authentication, and payload structures.  
- [Deployment Guide](./docs/team/deployment.md) – CI/CD and production deployment instructions.

### 3.3 Testing

- [Unit Test Report (UTR)](./docs/qms/utr.md) – Backend and frontend test coverage and results.

# 4.0 Main Features

- **User Registration & Authentication** – Secure onboarding using **OpenIddict (OAuth2/OIDC)**.  
- **Website Credibility Scoring** – Real-time domain scoring based on accuracy, transparency, and trust signals.  
- **Category Management** – Organize and classify websites for better browsing and credibility analysis.


# 5.0 Technology Stack

## 5.1 Backend

- .NET 10 Web API with Clean Architecture  
- Entity Framework Core for database access  
- SQL Server for persistence

## 5.2 Frontend

- Angular application located in `ui/credibility-ui`  

## 5.3 Authentication

- OpenIddict (OAuth2 / OIDC)  
- JWT Bearer tokens for stateless security

## 5.4 Other Tools

- CI/CD pipelines, automated tests, and code quality checks.

---

# 6.0 Getting Started

## 6.1 Prerequisites

- .NET 10 SDK  
- Node.js & npm  
- SQL Server  
- WSL2 + Ubuntu  

## 6.2 Setup & Run

### 6.2.1 Install Dependencies

```bash
make setup-dependencies
```

### 6.2.2 make build
```bash
make build
```
### 6.2.3 Run Locally:
   * Backend:  [Run BackEnd Locally](./src/README.md)- For BackEnd.
   * Frontend:  [Run FrontEnd Locally](./ui/README.md)- For FrontEnd.

# 7.0 Documentation Index

## 7.1 API Documentation
* [API Specification (OpenAPI)](./docs/team/openapi.yml)
* [API Desgin & Token Endpoint](./docs/team/api.md)

## 7.2 Software Design Documentation
* [Software Design Document (SDD)](./docs/qms/sdd.md)

## 7.3 Database Documentation
* [Database Schema](./docs/decision-records/ADR-002-database.md)

## 7.4 Registration API Design Documentation
* [Registration API Design](./docs/qms/registration-design.md)

## 7.5 Unit Test Report (UTR)
* [Unit Test Report (UTR)](./docs/qms/utr.md)

## 7.6 Security & Authentication Test Report
* [Security & Authentication Test Report](./docs/qms/utr.md)

# 8.0 Folder Structure
```
credibility-index/
├─ README.md
├─ CONTRIBUTING.md
├─ docs/
│  ├─ qms/
│  │  ├─ sdd.md
│  │  ├─ utr.md
│  ├─ team/
│  │  ├─ api.md
│  │  ├─ deployment.md
│  └─ decision-records/
│        ├─ ADR-001-architecture.md
│        └─ ADR-002-database.md
├─ src/
│  ├─ README.md
│  ├─ CredibilityIndex.Api/
│  ├─ CredibilityIndex.Application/
│  ├─ CredibilityIndex.Domain/
│  ├─ CredibilityIndex.Infrastructure/
│  └─ CredibilityIndex.Shared/
├─ ui/
│  ├─ README.md
│  └─ credibility-ui/
│     └─ README.md
├─ extension/
│  └─ chrome/
│     └─ README.md
└─ tests/
   └─ README.md
Refer to:
[Project Structure Map](./docs/workflow.md)
[`src/Documentation`](./src/README.md) 
[`ui/Credibilityui/ Documentation`](./ui/credibility-ui/README.md)
```
---

# 9.0 License

This project is licensed under the MIT License.

## Full MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

# 10.0 Contributing Guidelines

We welcome contributions! Please see [CONTRIBUTING.md](./CONTRIBUTING.md) for:

- Coding standards  
- Pull request workflow  
- Branching strategy  
- Commit message conventions

# Admin Cleanup Approach - Implementation Guide

## Overview

The Admin Cleanup Approach allows admins to manually manage website categorization with validation and error handling. This document outlines the architecture and usage.

## Features

### 1. **Dropdown Category Selection**
- Admin can select a category from a dropdown for any website
- Only valid categories are available
- Disabled during validation to prevent duplicate submissions

### 2. **Validation & Cleanup**
- **Pre-validation**: Checks if the selected category exists
- **Category Assignment**: Updates the website's category
- **Error Removal**: Automatically clears error states on success

### 3. **Visual Feedback**
- **Status Badges**: Pending, Validating, Success, Error
- **Error Messages**: Displays what went wrong
- **Statistics Panel**: Shows count of pending, validating, success, and error states

### 4. **Error Management**
- **Individual Error Removal**: Click "×" button next to error to clear it
- **Batch Clear**: "Clear All" button removes all error states at once
- **Auto-clear Success**: Success states automatically clear after 3 seconds

## Architecture

### Components

#### AdminDashboardComponent
Main UI component that displays:
- Website list with current categories
- Dropdown to select new category
- Status and error messages
- Statistics summary

**Key Methods:**
- `onCategorySelect()`: Handles category selection
- `removeError()`: Removes error state for a website  
- `clearAllErrors()`: Clears all errors at once
- `getCleanupState()`: Retrieves cleanup status

#### AdminCleanupService
Service handling business logic:
- Website and category data fetching
- Validation logic
- State management using RxJS BehaviorSubject
- Error handling

**Key Methods:**
- `validateAndCleanupWebsite()`: Main validation-cleanup workflow
- `removeError()`: Clears error state
- `clearAllErrors()`: Batch error cleanup

### State Management

Uses RxJS BehaviorSubject for cleanup states:

```typescript
interface CleanupState {
  websiteId: string;
  status: 'pending' | 'validating' | 'success' | 'error';
  message?: string;
}
```

**States:**
- **pending**: No action taken
- **validating**: Category validation in progress
- **success**: Category assigned successfully
- **error**: Category assignment failed

## Workflow

```
1. Admin selects category from dropdown
   ↓
2. validateAndCleanupWebsite() initiates
   ↓
3. Status: 'validating'
   ↓
4. Validate category exists
   ├─ Success → Update website category
   │  ├─ Success → Status: 'success'
   │  │           Auto-clear after 3s
   │  └─ Fail → Status: 'error'
   │           Show error message
   └─ Fail → Status: 'error'
              Show "Invalid category" message
   ↓
5. Admin can:
   - Remove single error (× button)
   - Clear all errors
   - Try again with different category
```

## Minimal Error Removal Approach

The implementation follows a minimal error removal strategy:

1. **No Auto-Retry**: Users must manually attempt again
2. **Clear State Only**: Removes UI state without data modification
3. **Non-Destructive**: Doesn't delete or modify website data
4. **User Control**: Admin decides when to clear errors

## API Endpoints Used

- `GET /api/v1/admin/websites` - Fetch all websites with categories
- `GET /api/v1/categories` - Fetch available categories
- `GET /api/v1/categories/{id}` - Validate category exists
- `PUT /api/v1/admin/websites/{id}/category` - Update website category

## UI/UX Details

### Statistics Panel
Shows real-time count of:
- Pending: No action yet
- Validating: In-progress operations
- Cleaned: Successfully categorized
- Errors: Failed operations

### Color Coding
- **Gray**: Pending
- **Yellow**: Validating
- **Green**: Success
- **Red**: Error

### Responsive Design
- Mobile-friendly table with scrolling
- Touch-friendly buttons
- Clear visual hierarchy

## Example Usage

```typescript
// In component
onCategorySelect(websiteId: string, event: any) {
  const categoryId = event.target.value;
  
  // Trigger validation and cleanup
  this.cleanupService.validateAndCleanupWebsite(websiteId, categoryId)
    .subscribe({
      next: () => this.loadData(), // Refresh on success
      error: (err) => console.error('Cleanup failed:', err)
    });
}

// Remove individual error
removeError(websiteId: string) {
  this.cleanupService.removeError(websiteId);
}

// Clear all errors with confirmation
clearAllErrors() {
  if (confirm('Clear all error states?')) {
    this.cleanupService.clearAllErrors();
  }
}
```

## Future Enhancements

1. **Batch Operations**: Select multiple websites and bulk assign categories
2. **Undo Functionality**: Revert last category change
3. **History Log**: Track who changed categories and when
4. **AI Suggestions**: Recommend categories based on domain analysis
5. **Notifications**: Toast/snackbar notifications instead of alerts

## Testing Checklist

- [ ] Load all websites successfully
- [ ] Load all categories successfully
- [ ] Select category and validate assignment
- [ ] Display success status and auto-clear
- [ ] Trigger error (invalid category) and show error
- [ ] Remove individual error
- [ ] Clear all errors
- [ ] Refresh data after successful assignment
- [ ] Responsive on mobile devices

## Security Considerations

- Admin role required (enforced server-side)
- HTTPS for API communication
- CSRF protection on PUT requests
- Input validation on category IDs
- Audit logging for admin actions (recommended)
