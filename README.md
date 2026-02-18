# Credibility Index

The Credibility Index is a full-stack solution designed with a Clean Architecture backend (.NET 8) and a modern frontend (React/Vite). This repository serves as the central hub for development, documentation, and deployment.

---

## 🛠 WSL2 & Ubuntu Environment Setup

This project is optimized for development inside **WSL2 (Windows Subsystem for Linux)** to ensure high performance and compatibility with Linux-based tooling.

### 1. Enable WSL2 and Ubuntu
If you do not have WSL2 installed, run this command in **PowerShell (as Administrator)**:
```powershell
wsl --install -d Ubuntu
```

## 📖 Documentation

Welcome to the project! Please review the following guides before starting:
* [🚀 Developer Onboarding Checklist](./docs/onboarding.md) - Setup your local environment.
* [🔄 Working Agreements & Workflow](./docs/workflow.md) - Branching, PRs, and Definition of Done.


## High-level Architecture Diagram
Detailed architecture documentation can be found here: [docs/architecture.md](./docs/decision-records/ADR-001-architecture.md).

## Main Features
* **User Registration & Auth**: Secure onboarding via OpenIddict.
* **Website Credibility**: Real-time scoring based on accuracy, transparency, and trust.
* **Category Management**: Organized browsing of domains.

## Tech Stack
* **Backend**: .NET 10.0 Web API
* **Frontend**: Angular (located in `ui/credibility-ui`)
* **Auth**: OpenIddict (OAuth2/OIDC)
* **Database**: SQL Server / EF Core

## Getting Started

### Prerequisites
* .NET 10 SDK
* Node.js & npm
* SQL Server
* WSL2
* Ubuntu Distribution

### Setup and Run
Use the included `Makefile` to automate the setup:

1.  **Install Dependencies**:
    ```bash
    make setup-dependencies
    ```
2.  **Build the Project**:
    ```bash
    make build
    ```
3.  **Run Locally**:
    * Backend:  [To Run BackEnd Locally](./src/README.md)- For BackEnd.
    * Frontend:  [To Run FrontEnd Locally](./ui/README.md)- For FrontEnd.

## Folder Structure

```text
credibility-index/
├─ docs/                # QMS, Team guides, and Architecture Decision Records (ADRs)
├─ src/                # Backend: .NET 8 Clean Architecture Projects
│  └─ CredibilityIndex.sln
├─ ui/                 # Frontend: React/TypeScript (Vite)
└─ README.md           # Main project 
```
Refer to the [Project Structure Map](./docs/workflow.md) for a full tree of the [`src/`](./src/README.md) and [`ui/`](./ui/credibility-ui/README.md) directories.

## Documentation Links
* [API Specification (OpenAPI)](./docs/team/openapi.yml)
* [Registration API Design](./docs/qms/registration-design.md)
* [Database Schema](./docs/decision-records/ADR-002-database.md)

## License
MIT License


## 📚 Documentation
- **API Specification:** See [docs/team/api.md](./docs/team/api.md) for Token Endpoint usage.
- **Software Design Document (SDD):** See [docs/qms/sdd.md](./docs/qms/sdd.md) for OpenIddict configuration.