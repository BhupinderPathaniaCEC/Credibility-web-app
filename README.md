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